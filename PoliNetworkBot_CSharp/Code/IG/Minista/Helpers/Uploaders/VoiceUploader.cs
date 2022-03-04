#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Minista.Helpers.Uploaders
{
    public class VoiceUploader
    {
        private static readonly Random Rnd = new();
        private readonly List<string> WaveForm = new();
        private CancellationTokenSource cts;
        private InstaDirectInboxItem DirectItem;
        public string GUID;
        private int LatestRndIndex;

        public VoiceUploader()
        {
            Name = Guid.NewGuid().ToString();
        }

        public string Name { get; }

        public string UploadId { get; private set; }
        public string ThreadId { get; private set; }

        internal string GenerateUploadId()
        {
            return DateTime.UtcNow.ToUnixTimeMiliSeconds().ToString();
        }

        private static Uri GetDIrectConfigureUri()
        {
            return new Uri("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/share_voice/",
                UriKind.RelativeOrAbsolute);
        }

        private static Uri GeFinishUri()
        {
            return new Uri("https://i.instagram.com/api/v1/media/upload_finish/", UriKind.RelativeOrAbsolute);
        }

        private static Uri GetMediaUploadUri(string uploadId, int fileHashCode)
        {
            return new Uri(string.Format("https://i.instagram.com/rupload_igvideo/{0}_0_{1}", uploadId, fileHashCode),
                UriKind.RelativeOrAbsolute);
        }

        private string GetRetryContext()
        {
            return new JObject
            {
                { "num_step_auto_retry", 0 },
                { "num_reupload", 0 },
                { "num_step_manual_retry", 0 }
            }.ToString(Formatting.None);
        }

        private int GetRnd()
        {
            var nw = Rnd.Next(100000000, 999999999);
            if (nw != 0 && nw == LatestRndIndex)
                nw = Rnd.Next(0, 9);
            LatestRndIndex = nw;
            return nw;
        }

        public async void UploadSingleVoice(StorageFile File, string threadId)
        {
            ThreadId = threadId;
            UploadId = GenerateUploadId();
            GUID = Guid.NewGuid().ToString();
            WaveForm.Clear();
            //try
            //{
            //    var deteils = await File.Properties.GetMusicPropertiesAsync();
            //    var count = deteils.Duration.TotalMilliseconds/* * 100*/;
            //    for (int i = 0; i < count; i++)
            //        WaveForm.Add($"0.{GetRnd()}");
            //}
            //catch { }
            var directItem = new InstaDirectInboxItem
            {
                ItemType = InstaDirectThreadItemType.VoiceMedia,
                VoiceMedia = new InstaVoiceMedia
                {
                    Media = new InstaVoice
                    {
                        Audio = new InstaAudio
                        {
                            AudioSource = File.Path,
                            WaveformData = new float[] { }
                        }
                    }
                },
                UserId = InstaApi.GetLoggedUser().LoggedInUser.Pk,
                TimeStamp = DateTime.UtcNow,
                SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Pending,
                ItemId = GUID
            };
            DirectItem = directItem;


            var hashCode = Path.GetFileName(File.Path ?? $"C:\\{13.GenerateRandomStringStatic()}.jpg").GetHashCode();
            var entityName = $"{UploadId}_0_{hashCode}";
            var instaUri = GetMediaUploadUri(UploadId, hashCode);

            var device = InstaApi.GetCurrentDevice();
            var httpRequestProcessor = InstaApi.HttpRequestProcessor;

            var BGU = new BackgroundUploader();

            var cookies =
                httpRequestProcessor.HttpHandler.CookieContainer.GetCookies(httpRequestProcessor.Client.BaseAddress);
            var strCookies = string.Empty;
            foreach (Cookie cook in cookies)
                strCookies += $"{cook.Name}={cook.Value}; ";
            // header haye asli in ha hastan faghat
            BGU.SetRequestHeader("Cookie", strCookies);
            var r = InstaApi.HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, device);
            foreach (var item in r.Headers) BGU.SetRequestHeader(item.Key, string.Join(" ", item.Value));


            var photoUploadParamsObj = new JObject
            {
                { "xsharing_user_ids", "[]" },
                { "is_direct_voice", "1" },
                { "upload_media_duration_ms", "0" },
                { "upload_id", UploadId },
                { "retry_context", GetRetryContext() },
                { "media_type", "11" }
            };
            var photoUploadParams = JsonConvert.SerializeObject(photoUploadParamsObj);
            var openedFile = await File.OpenAsync(FileAccessMode.Read);

            BGU.SetRequestHeader("X-Entity-Type", "audio/mp4");
            BGU.SetRequestHeader("Offset", "0");
            BGU.SetRequestHeader("X-Instagram-Rupload-Params", photoUploadParams);
            BGU.SetRequestHeader("X-Entity-Name", entityName);
            BGU.SetRequestHeader("X-Entity-Length", openedFile.AsStream().Length.ToString());
            BGU.SetRequestHeader("X_FB_VIDEO_WATERFALL_ID", ExtensionHelper.GetThreadToken());
            BGU.SetRequestHeader("Content-Transfer-Encoding", "binary");
            BGU.SetRequestHeader("Content-Type", "application/octet-stream");

            Debug.WriteLine("----------------------------------------Start upload----------------------------------");

            //var uploadX = await BGU.CreateUploadAsync(instaUri, parts, "", UploadId);
            var upload = BGU.CreateUpload(instaUri, File);
            upload.Priority = BackgroundTransferPriority.High;
            await HandleUploadAsync(upload, true);
        }

        private async Task HandleUploadAsync(UploadOperation upload, bool start)
        {
            cts = new CancellationTokenSource();
            try
            {
                LogStatus("Running: " + upload.Guid);

                var progressCallback = new Progress<UploadOperation>(UploadProgress);
                if (start)
                    // Start the upload and attach a progress handler.
                    await upload.StartAsync().AsTask(cts.Token, progressCallback);
                else
                    // The upload was already running when the application started, re-attach the progress handler.
                    await upload.AttachAsync().AsTask(cts.Token, progressCallback);

                ResponseInformation response = upload.GetResponseInformation();

                LogStatus(string.Format("Completed: {0}, Status Code: {1}", upload.Guid, response.StatusCode));
            }
            catch (TaskCanceledException)
            {
                MainPage.Current?.HideMediaUploadingUc();
                LogStatus("Canceled: " + upload.Guid);
            }
            catch (Exception)
            {
                MainPage.Current?.HideMediaUploadingUc();
                //throw;
            }
        }

        private async void UploadProgress(UploadOperation upload)
        {
            LogStatus(string.Format("Progress: {0}, Status: {1}", upload.Guid, upload.Progress.Status));

            BackgroundUploadProgress progress = upload.Progress;

            double percentSent = 100;
            if (progress.TotalBytesToSend > 0)
            {
                var bs = progress.BytesSent;
                percentSent = bs * 100 / progress.TotalBytesToSend;
            }

            SendNotify(percentSent);
            //MarshalLog(String.Format(" - Sent bytes: {0} of {1} ({2}%)",
            //  progress.BytesSent, progress.TotalBytesToSend, percentSent));

            LogStatus(string.Format(" - Sent bytes: {0} of {1} ({2}%), Received bytes: {3} of {4}",
                progress.BytesSent, progress.TotalBytesToSend, percentSent,
                progress.BytesReceived, progress.TotalBytesToReceive));

            if (progress.HasRestarted) LogStatus(" - Upload restarted");

            if (progress.HasResponseChanged)
            {
                var resp = upload.GetResponseInformation();
                // We've received new response headers from the server.
                LogStatus(" - Response updated; Header count: " + resp.Headers.Count);
                var response = upload.GetResultStreamAt(0);
                var stream = new StreamReader(response.AsStreamForRead());
                Debug.WriteLine(
                    "----------------------------------------Response from upload----------------------------------");
                var st = stream.ReadToEnd();
                Debug.WriteLine(st);
                //var res = JsonConvert.DeserializeObject<APIResponseOverride>(st);
                //await ConfigureMediaPhotoAsync(res.upload_id, Caption, null, null);
                await FinishVoiceAsync();
                //UploadCompleted?.Invoke(null, Convert.ToInt64(res.upload_id));
                // If you want to stream the response data this is a good time to start.
                // upload.GetResultStreamAt(0);
            }
        }

        private async Task<IResult<bool>> FinishVoiceAsync()
        {
            try
            {
                Debug.WriteLine(
                    "----------------------------------------FinishVoiceAsync----------------------------------");
                await Task.Delay(5000);
                var instaUri = GeFinishUri();
                var retryContext = GetRetryContext();
                var _user = InstaApi.GetLoggedUser();
                var _deviceInfo = InstaApi.GetCurrentDevice();
                var data = new JObject
                {
                    { "timezone_offset", InstaApi.GetTimezoneOffset().ToString() },
                    { "_csrftoken", _user.CsrfToken },
                    { "source_type", "4" },
                    { "_uid", _user.LoggedInUser.Pk.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "upload_id", UploadId },
                    {
                        "device", new JObject
                        {
                            { "manufacturer", _deviceInfo.HardwareManufacturer },
                            { "model", _deviceInfo.DeviceModelIdentifier },
                            { "android_release", _deviceInfo.AndroidVer.VersionNumber },
                            { "android_version", _deviceInfo.AndroidVer.APILevel }
                        }
                    }
                };


                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var request = InstaApi.HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                return await ConfigureVoiceAsync();
            }
            catch (HttpRequestException httpException)
            {
                SendNotify(102);

                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                SendNotify(102);
                MainPage.Current?.HideMediaUploadingUc();
                return Result.Fail<bool>(exception);
            }
        }

        private async Task<IResult<bool>> ConfigureVoiceAsync()
        {
            try
            {
                await Task.Delay(1500);
                Debug.WriteLine(
                    "----------------------------------------ConfigureVoiceAsync----------------------------------");
                var instaUri = GetDIrectConfigureUri();
                var retryContext = GetRetryContext();
                var _user = InstaApi.GetLoggedUser();
                var _deviceInfo = InstaApi.GetCurrentDevice();
                var waves = string.Join(",", WaveForm);
                var token = ExtensionHelper.GetThreadToken();
                var data = new Dictionary<string, string>
                {
                    { "action", "send_item" },
                    { "client_context", token },
                    { "_csrftoken", _user.CsrfToken },
                    { "device_id", _deviceInfo.DeviceId },
                    { "mutation_token", token },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "waveform", "[]" },
                    { "waveform_sampling_frequency_hz", "10" },
                    { "upload_id", UploadId },
                    { "thread_ids", $"[{ThreadId}]" }
                };


                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var request = InstaApi.HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var obj = JsonConvert.DeserializeObject<InstaDirectVoiceRespondResponse>(json);
                    if (obj != null)
                        if (DirectItem != null)
                        {
                            DirectItem.SendingType = InstagramApiSharp.Enums.InstaDirectInboxItemSendingType.Sent;
                            DirectItem.ItemId = obj.MessageMetadatas[0].ItemId;
                        }

                    //var res = ConvertersFabric.Instance.GetDirectVoiceRespondConverter(obj).Convert();
                    //Views.Main.MainView.Current?.MainVM.PostsGenerator.Items.Insert(0, new InstaPost
                    //{
                    //    Media = obj,
                    //    Type = InstagramApiSharp.Enums.InstaFeedsType.Media
                    //});
                    //ShowNotify("Your photo uploaded successfully...", 3500);
                    //NotificationHelper.ShowNotify(Caption?.Truncate(50), NotifyFile?.Path, "Media uploaded");
                }
                catch
                {
                }

                SendNotify(102);
                //RemoveThis();
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                SendNotify(102);

                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                SendNotify(102);
                return Result.Fail<bool>(exception);
            }
            finally
            {
                DirectItem = null;
            }
        }

        public void LogStatus(string Text)
        {
            Debug.WriteLine(Text);
        }

        private async void SendNotify(double value)
        {
            try
            {
                //if (MediaShare == null) return;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //MediaShare.Percentage = value;
                });
            }
            catch
            {
            }
        }
    }
}