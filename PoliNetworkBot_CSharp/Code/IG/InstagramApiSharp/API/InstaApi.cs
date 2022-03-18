#region

using InstagramApiSharp.API.Processors;
using InstagramApiSharp.API.Services;
using InstagramApiSharp.API.Versions;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using InstagramApiSharp.Classes.SessionHandlers;
using InstagramApiSharp.Converters;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace InstagramApiSharp.API
{
    /// <summary>
    ///     Base of everything that you want.
    /// </summary>
    public class InstaApi
    {
        #region Constructor

        public InstaApi(UserSessionData user, IInstaLogger logger, AndroidDevice deviceInfo,
            IHttpRequestProcessor httpRequestProcessor, InstaApiVersionType apiVersionType)
        {
            _userAuthValidate = new UserAuthValidate();
            User = user;
            _logger = logger;
            _deviceInfo = deviceInfo;
            HttpRequestProcessor = httpRequestProcessor;
            InstaApiVersionType = apiVersionType;
            _apiVersion = InstaApiVersionList.GetApiVersionList().GetApiVersion(apiVersionType);
            HttpHelper = new HttpHelper(_apiVersion, httpRequestProcessor, this);
            RegistrationService = new RegistrationService(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
        }

        #endregion Constructor

        #region SessionHandler

        public ISessionHandler SessionHandler { get; set; }

        #endregion

        #region Variables and properties

        private IRequestDelay _delay = RequestDelay.Empty();
        private readonly IInstaLogger _logger;
        private InstaApiVersion _apiVersion;
        public HttpHelper HttpHelper { get; set; }
        public AndroidDevice _deviceInfo;
        private UserSessionData _userSession;

        internal UserSessionData User
        {
            get => _userSession;
            set
            {
                _userSession = value;
                _userAuthValidate.User = value;
            }
        }

        private readonly UserAuthValidate _userAuthValidate;
        private bool IsCustomDeviceSet;

        private string _waterfallIdReg = "", _deviceIdReg = "", _phoneIdReg = "", _guidReg = "";
        private InstaAccountRegistrationPhoneNumber _signUpPhoneNumberInfo;

        private bool _isUserAuthenticated;

        /// <summary>
        ///     Indicates whether user authenticated or not
        /// </summary>
        public bool IsUserAuthenticated
        {
            get => _isUserAuthenticated;
            internal set
            {
                _isUserAuthenticated = value;
                _userAuthValidate.IsUserAuthenticated = value;
            }
        }

        /// <summary>
        ///     Current HttpClient
        /// </summary>
        public HttpClient HttpClient => HttpRequestProcessor.Client;

        /// <summary>
        ///     Current <see cref="IHttpRequestProcessor" />
        /// </summary>
        public IHttpRequestProcessor HttpRequestProcessor { get; }

        public InstaApiVersionType InstaApiVersionType { get; private set; }

        /// <summary>
        ///     Registration Service
        /// </summary>
        public IRegistrationService RegistrationService { get; }

        /// <summary>
        ///     Gets or sets challenge login info
        /// </summary>
        public InstaChallengeLoginInfo ChallengeLoginInfo { get; set; }

        /// <summary>
        ///     Gets or sets two factor login info
        /// </summary>
        public InstaTwoFactorLoginInfo TwoFactorLoginInfo { get; set; }

        #endregion Variables and properties

        #region Processors

        /// <summary>
        ///     Live api functions.
        /// </summary>
        public ILiveProcessor LiveProcessor { get; private set; }

        /// <summary>
        ///     Discover api functions.
        /// </summary>
        public IDiscoverProcessor DiscoverProcessor { get; private set; }

        /// <summary>
        ///     Account api functions.
        /// </summary>
        public IAccountProcessor AccountProcessor { get; private set; }

        /// <summary>
        ///     Comments api functions.
        /// </summary>
        public ICommentProcessor CommentProcessor { get; private set; }

        /// <summary>
        ///     Story api functions.
        /// </summary>
        public IStoryProcessor StoryProcessor { get; private set; }

        /// <summary>
        ///     Media api functions.
        /// </summary>
        public IMediaProcessor MediaProcessor { get; private set; }

        /// <summary>
        ///     Messaging (direct) api functions.
        /// </summary>
        public IMessagingProcessor MessagingProcessor { get; private set; }

        /// <summary>
        ///     Feed api functions.
        /// </summary>
        public IFeedProcessor FeedProcessor { get; private set; }

        /// <summary>
        ///     Collection api functions.
        /// </summary>
        public ICollectionProcessor CollectionProcessor { get; private set; }

        /// <summary>
        ///     Location api functions.
        /// </summary>
        public ILocationProcessor LocationProcessor { get; private set; }

        /// <summary>
        ///     Hashtag api functions.
        /// </summary>
        public IHashtagProcessor HashtagProcessor { get; private set; }

        /// <summary>
        ///     User api functions.
        /// </summary>
        public IUserProcessor UserProcessor { get; private set; }

        /// <summary>
        ///     Helper processor for other processors
        /// </summary>
        internal HelperProcessor HelperProcessor { get; private set; }

        /// <summary>
        ///     Instagram TV api functions
        /// </summary>
        public ITVProcessor TVProcessor { get; private set; }

        /// <summary>
        ///     Business api functions
        ///     <para>Note: All functions of this interface only works with business accounts!</para>
        /// </summary>
        public IBusinessProcessor BusinessProcessor { get; private set; }

        /// <summary>
        ///     Shopping and commerce api functions
        /// </summary>
        public IShoppingProcessor ShoppingProcessor { get; private set; }

        /// <summary>
        ///     Instagram Web api functions.
        ///     <para>It's related to https://instagram.com/accounts/ </para>
        /// </summary>
        public IWebProcessor WebProcessor { get; private set; }

        #endregion Processors

        #region Register new account with Phone number and email

        /// <summary>
        ///     Check email availability
        /// </summary>
        /// <param name="email">Email to check</param>
        public async Task<IResult<InstaCheckEmailRegistration>> CheckEmailAsync(string email)
        {
            return await CheckEmail(email);
        }

        private async Task<IResult<InstaCheckEmailRegistration>> CheckEmail(string email, bool useNewWaterfall = true)
        {
            try
            {
                if (_waterfallIdReg == null || useNewWaterfall)
                    _waterfallIdReg = Guid.NewGuid().ToString();

                var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;

                var postData = new Dictionary<string, string>
                {
                    { "_csrftoken", csrftoken },
                    { "login_nonces", "[]" },
                    { "email", email },
                    { "qe_id", Guid.NewGuid().ToString() },
                    { "waterfall_id", _waterfallIdReg }
                };
                var instaUri = UriCreator.GetCheckEmailUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var obj = JsonConvert.DeserializeObject<InstaCheckEmailRegistration>(json);
                    return obj.ErrorType switch
                    {
                        "fail" => Result.UnExpectedResponse<InstaCheckEmailRegistration>(response, json),
                        "email_is_taken" => Result.Fail("Email is taken.", (InstaCheckEmailRegistration)null),
                        _ => obj.ErrorType == "invalid_email"
                            ? Result.Fail("Please enter a valid email address.", (InstaCheckEmailRegistration)null)
                            : Result.UnExpectedResponse<InstaCheckEmailRegistration>(response, json)
                    };
                }
                else
                {
                    var obj = JsonConvert.DeserializeObject<InstaCheckEmailRegistration>(json);
                    return obj.ErrorType switch
                    {
                        "fail" => Result.UnExpectedResponse<InstaCheckEmailRegistration>(response, json),
                        "email_is_taken" => Result.Fail("Email is taken.", (InstaCheckEmailRegistration)null),
                        "invalid_email" => Result.Fail("Please enter a valid email address.",
                            (InstaCheckEmailRegistration)null),
                        _ => Result.Success(obj)
                    };
                }
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaCheckEmailRegistration), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaCheckEmailRegistration>(exception);
            }
        }

        /// <summary>
        ///     Check phone number availability
        /// </summary>
        /// <param name="phoneNumber">Phone number to check</param>
        public async Task<IResult<bool>> CheckPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                _deviceIdReg = ApiRequestMessage.GenerateDeviceId();

                var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;

                var postData = new Dictionary<string, string>
                {
                    { "_csrftoken", csrftoken },
                    { "login_nonces", "[]" },
                    { "phone_number", phoneNumber },
                    { "device_id", _deviceInfo.DeviceId }
                };
                var instaUri = UriCreator.GetCheckPhoneNumberUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                return response.StatusCode != HttpStatusCode.OK
                    ? Result.UnExpectedResponse<bool>(response, json)
                    : Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }

        /// <summary>
        ///     Check username availablity.
        /// </summary>
        /// <param name="username">Username</param>
        public async Task<IResult<InstaAccountCheck>> CheckUsernameAsync(string username)
        {
            try
            {
                var instaUri = UriCreator.GetCheckUsernameUri();
                var data = new JObject
                {
                    { "_csrftoken", User.CsrfToken },
                    { "username", username }
                };
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<InstaAccountCheck>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaAccountCheck), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaAccountCheck>(exception);
            }
        }

        /// <summary>
        ///     Send sign up sms code
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        public async Task<IResult<bool>> SendSignUpSmsCodeAsync(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(_waterfallIdReg))
                    _waterfallIdReg = Guid.NewGuid().ToString();

                await CheckPhoneNumberAsync(phoneNumber);

                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;
                var postData = new Dictionary<string, string>
                {
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "phone_number", phoneNumber },
                    { "_csrftoken", csrftoken },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "waterfall_id", _waterfallIdReg }
                };
                var instaUri = UriCreator.GetSignUpSMSCodeUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var o = JsonConvert.DeserializeObject<InstaAccountRegistrationPhoneNumber>(json);

                    return Result.UnExpectedResponse<bool>(response, o.Message?.Errors?[0], json);
                }

                _signUpPhoneNumberInfo = JsonConvert.DeserializeObject<InstaAccountRegistrationPhoneNumber>(json);
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }

        /// <summary>
        ///     Verify sign up sms code
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="verificationCode">Verification code</param>
        public async Task<IResult<InstaPhoneNumberRegistration>> VerifySignUpSmsCodeAsync(string phoneNumber,
            string verificationCode)
        {
            try
            {
                if (string.IsNullOrEmpty(_waterfallIdReg))
                    throw new ArgumentException("You should call SendSignUpSmsCodeAsync function first.");

                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;
                var postData = new Dictionary<string, string>
                {
                    { "verification_code", verificationCode },
                    { "phone_number", phoneNumber },
                    { "_csrftoken", csrftoken },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "waterfall_id", _waterfallIdReg }
                };
                var instaUri = UriCreator.GetValidateSignUpSMSCodeUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var o = JsonConvert.DeserializeObject<InstaAccountRegistrationPhoneNumberVerifySms>(json);

                    return Result.Fail(o.Errors?.Nonce?[0], (InstaPhoneNumberRegistration)null);
                }

                var r = JsonConvert.DeserializeObject<InstaAccountRegistrationPhoneNumberVerifySms>(json);
                if (r.ErrorType == "invalid_nonce")
                    return Result.Fail(r.Errors?.Nonce?[0], (InstaPhoneNumberRegistration)null);

                await GetRegistrationStepsAsync();
                var obj = JsonConvert.DeserializeObject<InstaPhoneNumberRegistration>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaPhoneNumberRegistration), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaPhoneNumberRegistration>(exception);
            }
        }

        /// <summary>
        ///     Get username suggestions
        /// </summary>
        /// <param name="name">Name</param>
        public async Task<IResult<InstaRegistrationSuggestionResponse>> GetUsernameSuggestionsAsync(string name)
        {
            return await GetUsernameSuggestions(name);
        }

        public async Task<IResult<InstaRegistrationSuggestionResponse>> GetUsernameSuggestions(string name,
            bool useNewIds = true)
        {
            try
            {
                if (string.IsNullOrEmpty(_deviceIdReg))
                    _deviceIdReg = ApiRequestMessage.GenerateDeviceId();
                if (useNewIds)
                {
                    _phoneIdReg = Guid.NewGuid().ToString();
                    _waterfallIdReg = Guid.NewGuid().ToString();
                    _guidReg = Guid.NewGuid().ToString();
                }

                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;
                var postData = new Dictionary<string, string>
                {
                    { "name", name },
                    { "_csrftoken", csrftoken },
                    { "email", "" }
                };
                if (useNewIds)
                {
                    postData.Add("phone_id", _phoneIdReg);
                    postData.Add("guid", _guidReg);
                    postData.Add("device_id", _deviceIdReg);
                    postData.Add("waterfall_id", _waterfallIdReg);
                }
                else
                {
                    postData.Add("phone_id", _deviceInfo.PhoneGuid.ToString());
                    postData.Add("guid", _deviceInfo.DeviceGuid.ToString());
                    postData.Add("device_id", _deviceInfo.DeviceId);
                    postData.Add("waterfall_id", _waterfallIdReg ?? Guid.NewGuid().ToString());
                }

                var instaUri = UriCreator.GetUsernameSuggestionsUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var o = JsonConvert.DeserializeObject<InstaAccountRegistrationPhoneNumber>(json);

                    return Result.Fail(o.Message?.Errors?[0], (InstaRegistrationSuggestionResponse)null);
                }

                var obj = JsonConvert.DeserializeObject<InstaRegistrationSuggestionResponse>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaRegistrationSuggestionResponse),
                    ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaRegistrationSuggestionResponse>(exception);
            }
        }

        /// <summary>
        ///     Validate new account creation with phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="verificationCode">Verification code</param>
        /// <param name="username">Username to set</param>
        /// <param name="password">Password to set</param>
        /// <param name="firstName">First name to set</param>
        public async Task<IResult<InstaAccountCreation>> ValidateNewAccountWithPhoneNumberAsync(string phoneNumber,
            string verificationCode, string username, string password, string firstName)
        {
            try
            {
                if (string.IsNullOrEmpty(_waterfallIdReg) || _signUpPhoneNumberInfo == null)
                    throw new ArgumentException("You should call SendSignUpSmsCodeAsync function first.");

                if (_signUpPhoneNumberInfo.GdprRequired)
                {
                    var acceptGdpr = await AcceptConsentRequiredAsync(null, phoneNumber);
                    if (!acceptGdpr.Succeeded)
                        return Result.Fail(acceptGdpr.Info.Message, (InstaAccountCreation)null);
                }

                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;

                var postData = new Dictionary<string, string>
                {
                    { "allow_contacts_sync", "true" },
                    { "verification_code", verificationCode },
                    { "sn_result", "API_ERROR:+null" },
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "phone_number", phoneNumber },
                    { "_csrftoken", csrftoken },
                    { "username", username },
                    { "first_name", firstName },
                    { "adid", Guid.NewGuid().ToString() },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "sn_nonce", "" },
                    { "force_sign_up_code", "" },
                    { "waterfall_id", _waterfallIdReg },
                    { "qs_stamp", "" },
                    { "password", password },
                    { "has_sms_consent", "true" }
                };
                if (_signUpPhoneNumberInfo.GdprRequired)
                    postData.Add("gdpr_s", "[0,2,0,null]");

                var instaUri = UriCreator.GetCreateValidatedUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var o = JsonConvert.DeserializeObject<InstaAccountCreationResponse>(json);

                    return Result.Fail(o.Errors?.Username?[0], (InstaAccountCreation)null);
                }

                var r = JsonConvert.DeserializeObject<InstaAccountCreationResponse>(json);
                if (r.ErrorType == "username_is_taken")
                    return Result.Fail(r.Errors?.Username?[0], (InstaAccountCreation)null);

                var obj = JsonConvert.DeserializeObject<InstaAccountCreation>(json);
                if (obj.AccountCreated && obj.CreatedUser != null)
                    ValidateUserAsync(obj.CreatedUser, csrftoken, true, password);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaAccountCreation), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaAccountCreation>(exception);
            }
        }

        private async Task<IResult<object>> GetRegistrationStepsAsync()
        {
            try
            {
                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;
                var postData = new Dictionary<string, string>
                {
                    { "fb_connected", "false" },
                    { "seen_steps", "[]" },
                    { "phone_id", _phoneIdReg },
                    { "fb_installed", "false" },
                    { "locale", "en_US" },
                    { "timezone_offset", "16200" },
                    { "network_type", "WIFI-UNKNOWN" },
                    { "_csrftoken", csrftoken },
                    { "guid", _guidReg },
                    { "is_ci", "false" },
                    { "android_id", _deviceIdReg },
                    { "reg_flow_taken", "phone" },
                    { "tos_accepted", "false" }
                };
                var instaUri = UriCreator.GetOnboardingStepsUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var o = JsonConvert.DeserializeObject<InstaAccountRegistrationPhoneNumber>(json);

                    return Result.Fail(o.Message?.Errors?[0], (InstaRegistrationSuggestionResponse)null);
                }

                var obj = JsonConvert.DeserializeObject<InstaRegistrationSuggestionResponse>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaRegistrationSuggestionResponse),
                    ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaRegistrationSuggestionResponse>(exception);
            }
        }

        /// <summary>
        ///     Create a new instagram account [NEW FUNCTION, BUT NOT WORKING?!!!!!!!!!!]
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="email">Email</param>
        /// <param name="firstName">First name (optional)</param>
        /// <param name="delay">Delay between requests. null = 2.5 seconds</param>
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private async Task<IResult<InstaAccountCreation>> CreateNewAccountAsync(string username, string password,
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
            string email, string firstName = "", TimeSpan? delay = null)
        {
            var createResponse = new InstaAccountCreation();
            try
            {
                delay ??= TimeSpan.FromSeconds(2.5);

                var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                await firstResponse.Content.ReadAsStringAsync();
                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                var checkEmail = await CheckEmail(email, false);
                if (!checkEmail.Succeeded)
                    return Result.Fail(checkEmail.Info.Message, (InstaAccountCreation)null);

                await Task.Delay((int)delay.Value.TotalMilliseconds);
                if (checkEmail.Value.GdprRequired)
                {
                    var acceptGdpr = await AcceptConsentRequiredAsync(email);
                    if (!acceptGdpr.Succeeded)
                        return Result.Fail(acceptGdpr.Info.Message, (InstaAccountCreation)null);
                }

                await Task.Delay((int)delay.Value.TotalMilliseconds);
                if (username.Length > 6)
                {
                    await GetUsernameSuggestions(username[..4], false);
                    await Task.Delay(1000);
                    await GetUsernameSuggestions(username[..5], false);
                }
                else
                {
                    await GetUsernameSuggestions(username, false);
                    await Task.Delay(1000);
                    await GetUsernameSuggestions(username, false);
                }

                await Task.Delay((int)delay.Value.TotalMilliseconds);
                var postData = new Dictionary<string, string>
                {
                    { "allow_contacts_sync", "true" },
                    { "sn_result", "API_ERROR:+null" },
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "_csrftoken", csrftoken },
                    { "username", username },
                    { "first_name", firstName },
                    { "adid", Guid.NewGuid().ToString() },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "email", email },
                    { "sn_nonce", "" },
                    { "force_sign_up_code", "" },
                    { "waterfall_id", _waterfallIdReg ?? Guid.NewGuid().ToString() },
                    { "qs_stamp", "" },
                    { "password", password }
                };
                if (checkEmail.Value.GdprRequired)
                    postData.Add("gdpr_s", "[0,2,0,null]");

                var instaUri = UriCreator.GetCreateAccountUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaAccountCreation>(response, json);
                var obj = JsonConvert.DeserializeObject<InstaAccountCreation>(json);
                //{"account_created": false, "errors": {"email": ["Another account is using iranramtin73jokar@live.com."], "username": ["This username isn't available. Please try another."]}, "allow_contacts_sync": true, "status": "ok", "error_type": "email_is_taken, username_is_taken"}
                //{"message": "feedback_required", "spam": true, "feedback_title": "Signup Error", "feedback_message": "Sorry! There\u2019s a problem signing you up right now. Please try again later. We restrict certain content and actions to protect our community. Tell us if you think we made a mistake.", "feedback_url": "repute/report_problem/instagram_signup/", "feedback_appeal_label": "Report problem", "feedback_ignore_label": "OK", "feedback_action": "report_problem", "status": "fail", "error_type": "signup_block"}

                if (obj.AccountCreated && obj.CreatedUser != null)
                    ValidateUserAsync(obj.CreatedUser, csrftoken, true, password);

                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaAccountCreation), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaAccountCreation>(exception);
            }
        }

        /// <summary>
        ///     Create a new instagram account
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="email">Email</param>
        /// <param name="firstName">First name (optional)</param>
        /// <returns></returns>
        public async Task<IResult<InstaAccountCreation>> CreateNewAccountAsync(string username, string password,
            string email, string firstName)
        {
            var createResponse = new InstaAccountCreation();
            try
            {
                var _deviceIdReg = ApiRequestMessage.GenerateDeviceId();
                var _phoneIdReg = Guid.NewGuid().ToString();
                var _waterfallIdReg = Guid.NewGuid().ToString();
                var _guidReg = Guid.NewGuid().ToString();
                var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                await firstResponse.Content.ReadAsStringAsync();

                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;

                var postData = new Dictionary<string, string>
                {
                    { "allow_contacts_sync", "true" },
                    { "sn_result", "API_ERROR:+null" },
                    { "phone_id", _phoneIdReg },
                    { "_csrftoken", csrftoken },
                    { "username", username },
                    { "first_name", firstName },
                    { "adid", Guid.NewGuid().ToString() },
                    { "guid", _guidReg },
                    { "device_id", _deviceIdReg },
                    { "email", email },
                    { "sn_nonce", "" },
                    { "force_sign_up_code", "" },
                    { "waterfall_id", _waterfallIdReg },
                    { "qs_stamp", "" },
                    { "password", password }
                };
                var instaUri = UriCreator.GetCreateAccountUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaAccountCreation>(response, json);
                var obj = JsonConvert.DeserializeObject<InstaAccountCreation>(json);
                if (obj.AccountCreated && obj.CreatedUser != null)
                    ValidateUserAsync(obj.CreatedUser, csrftoken, true, password);

                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaAccountCreation), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaAccountCreation>(exception);
            }
        }

        /// <summary>
        ///     Accept consent require (for GDPR countries)
        /// </summary>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        private async Task<IResult<bool>> AcceptConsentRequiredAsync(string email, string phone = null)
        {
            try
            {
                var delay = TimeSpan.FromSeconds(2);

                //{"message": "consent_required", "consent_data": {"headline": "Updates to Our Terms and Data Policy", "content": "We've updated our Terms and made some changes to our Data Policy. Please take a moment to review these changes and let us know that you agree to them.\n\nYou need to finish reviewing this information before you can use Instagram.", "button_text": "Review Now"}, "status": "fail"}
                await Task.Delay((int)delay.TotalMilliseconds);
                var instaUri = UriCreator.GetConsentNewUserFlowBeginsUri();
                var data = new JObject
                {
                    { "phone_id", _deviceInfo.PhoneGuid },
                    { "_csrftoken", User.CsrfToken }
                };
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);

                await Task.Delay((int)delay.TotalMilliseconds);

                instaUri = UriCreator.GetConsentNewUserFlowUri();
                data = new JObject
                {
                    { "phone_id", _deviceInfo.PhoneGuid },
                    { "gdpr_s", "" },
                    { "_csrftoken", User.CsrfToken },
                    { "guid", _deviceInfo.DeviceGuid },
                    { "device_id", _deviceInfo.DeviceId }
                };
                if (email != null)
                {
                    data.Add("email", email);
                }
                else
                {
                    if (phone != null && !phone.StartsWith("+"))
                        phone = $"+{phone}";

                    phone ??= string.Empty;
                    data.Add("phone", phone);
                }

                request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                response = await HttpRequestProcessor.SendAsync(request);
                json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);

                await Task.Delay((int)delay.TotalMilliseconds);

                data = new JObject
                {
                    { "current_screen_key", "age_consent_two_button" },
                    { "phone_id", _deviceInfo.PhoneGuid },
                    { "gdpr_s", "[0,0,0,null]" },
                    { "_csrftoken", User.CsrfToken },
                    { "updates", "{\"age_consent_state\":\"2\"}" },
                    { "guid", _deviceInfo.DeviceGuid },
                    { "device_id", _deviceInfo.DeviceId }
                };
                request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                response = await HttpRequestProcessor.SendAsync(request);
                json = await response.Content.ReadAsStringAsync();

                return response.StatusCode != HttpStatusCode.OK
                    ? Result.UnExpectedResponse<bool>(response, json)
                    : Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex, false);
            }
        }

        #endregion Register new account with Phone number and email

        #region Authentication and challenge functions

        /// <summary>
        ///     Login using given credentials asynchronously
        /// </summary>
        /// <param name="isNewLogin"></param>
        /// <returns>
        ///     Success --> is succeed
        ///     TwoFactorRequired --> requires 2FA login.
        ///     BadPassword --> Password is wrong
        ///     InvalidUser --> User/phone number is wrong
        ///     Exception --> Something wrong happened
        ///     ChallengeRequired --> You need to pass Instagram challenge
        /// </returns>
        public async Task<IResult<InstaLoginResult>> LoginAsync(bool isNewLogin = true)
        {
            ValidateUser();
            ValidateRequestMessage();
            try
            {
                if (isNewLogin)
                {
                    User.Authorization =
                        User.RurHeader =
                            User.XMidHeader =
                                User.WwwClaim = null;

                    User.LoggedInUser = null;
                }

                var needsRelogin = false;
            ReloginLabel:
                //if (isNewLogin)
                //    await GetToken();
                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);

                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;
                var instaUri = UriCreator.GetLoginUri();
                var signature = string.Empty;
                var devid = string.Empty;
                if (!string.IsNullOrEmpty(User.Password))
                {
                    if (string.IsNullOrEmpty(User.PublicKey))
                        await SendRequestsBeforeLoginAsync();
                    var encruptedPassword = this.GetEncryptedPassword(User.Password);
                    HttpRequestProcessor.RequestMessage.EncPassword = encruptedPassword;
                }

                signature = isNewLogin
                    ? $"{HttpRequestProcessor.RequestMessage.GenerateSignature(_apiVersion, _apiVersion.SignatureKey, out devid)}.{HttpRequestProcessor.RequestMessage.GetMessageString()}"
                    : $"{HttpRequestProcessor.RequestMessage.GenerateChallengeSignature(_apiVersion, _apiVersion.SignatureKey, csrftoken, out devid)}.{HttpRequestProcessor.RequestMessage.GetChallengeMessageString(csrftoken)}";
                _deviceInfo.DeviceId = devid;
                var fields = new Dictionary<string, string>
                {
                    { InstaApiConstants.HEADER_IG_SIGNATURE, signature },
                    { InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION, InstaApiConstants.IG_SIGNATURE_KEY_VERSION }
                };
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, fields);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                AddXMidHeader(response);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var loginFailReason = JsonConvert.DeserializeObject<InstaLoginBaseResponse>(json);

                    if (loginFailReason.InvalidCredentials)
                        return Result.Fail("Invalid Credentials",
                            loginFailReason.ErrorType == "bad_password"
                                ? InstaLoginResult.BadPassword
                                : InstaLoginResult.InvalidUser);
                    if (loginFailReason.TwoFactorRequired)
                    {
                        if (loginFailReason.TwoFactorLoginInfo != null)
                            HttpRequestProcessor.RequestMessage.Username = loginFailReason.TwoFactorLoginInfo.Username;
                        TwoFactorLoginInfo = loginFailReason.TwoFactorLoginInfo;
                        //2FA is required!
                        return Result.Fail("Two Factor Authentication is required", InstaLoginResult.TwoFactorRequired);
                    }

                    switch (loginFailReason.ErrorType)
                    {
                        /* || !string.IsNullOrEmpty(loginFailReason.Message) && loginFailReason.Message == "challenge_required"*/
                        case "checkpoint_challenge_required":
                            ChallengeLoginInfo = loginFailReason.Challenge;

                            return Result.Fail("Challenge is required", InstaLoginResult.ChallengeRequired);

                        case "rate_limit_error":
                            return Result.Fail("Please wait a few minutes before you try again.",
                                InstaLoginResult.LimitError);

                        case "inactive user" or "inactive_user":
                            return Result.Fail($"{loginFailReason.Message}\r\nHelp url: {loginFailReason.HelpUrl}",
                                InstaLoginResult.InactiveUser);

                        case "checkpoint_logged_out":
                            {
                                if (needsRelogin)
                                    return Result.Fail($"{loginFailReason.ErrorType} {loginFailReason.CheckpointUrl}",
                                        InstaLoginResult.CheckpointLoggedOut);
                                needsRelogin = true;
                                goto ReloginLabel;

                                return Result.Fail($"{loginFailReason.ErrorType} {loginFailReason.CheckpointUrl}",
                                    InstaLoginResult.CheckpointLoggedOut);
                            }
                        default:
                            return Result.UnExpectedResponse<InstaLoginResult>(response, json);
                    }
                }

                var loginInfo = JsonConvert.DeserializeObject<InstaLoginResponse>(json);
                User.UserName = loginInfo.User?.UserName;
                IsUserAuthenticated = loginInfo.User != null;
                if (loginInfo.User != null)
                    HttpRequestProcessor.RequestMessage.Username = loginInfo.User.UserName;
                var converter = ConvertersFabric.Instance.GetUserShortConverter(loginInfo.User);
                User.LoggedInUser = converter.Convert();
                User.RankToken = $"{User.LoggedInUser.Pk}_{HttpRequestProcessor.RequestMessage.PhoneId}";
                if (string.IsNullOrEmpty(User.CsrfToken))
                {
                    cookies =
                        HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                            .BaseAddress);
                    User.CsrfToken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                }

                await AfterLoginAsync(response).ConfigureAwait(false);
                return Result.Success(InstaLoginResult.Success);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, InstaLoginResult.Exception, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                LogException(exception);
                return Result.Fail(exception, InstaLoginResult.Exception);
            }
            finally
            {
                InvalidateProcessors();
            }
        }

        /// <summary>
        ///     Login using cookies
        ///     <para>
        ///         Note: You won't be able to change password, if you use <see cref="IInstaApi.LoginWithCookiesAsync(string)" />
        ///         function for logging in!
        ///     </para>
        /// </summary>
        /// <param name="cookies">Cookies</param>
        public async Task<IResult<bool>> LoginWithCookiesAsync(string cookies)
        {
            try
            {
                if (cookies.Contains("Cookie:"))
                    cookies = cookies[8..];

                var parts = cookies.Split(';')
                    .Where(xx => xx.Contains('='))
                    .Select(xx => xx.Trim().Split('='))
                    .Select(xx => new { Name = xx.First(), Value = xx.Last() });

                var user = parts.FirstOrDefault(u => u.Name.ToLower() == "ds_user")?.Value?.ToLower();
                var userId = parts.FirstOrDefault(u => u.Name.ToLower() == "ds_user_id")?.Value;
                var csrfToken = parts.FirstOrDefault(u => u.Name.ToLower() == "csrftoken")?.Value;

                if (string.IsNullOrEmpty(csrfToken))
                    return Result.Fail<bool>("Cannot find 'csrftoken' in cookies!");

                if (string.IsNullOrEmpty(userId))
                    return Result.Fail<bool>("Cannot find 'ds_user_id' in cookies!");

                var uri = new Uri(InstaApiConstants.INSTAGRAM_URL);
                cookies = cookies.Replace(';', ',');
                HttpRequestProcessor.HttpHandler.CookieContainer.SetCookies(uri, cookies);
                User = UserSessionData.Empty;
                user ??= "AlakiMasalan";
                User.UserName = HttpRequestProcessor.RequestMessage.Username = user;
                User.Password = "AlakiMasalan";
                User.LoggedInUser = new InstaUserShort
                {
                    UserName = user
                };
                try
                {
                    User.LoggedInUser.Pk = long.Parse(userId);
                }
                catch
                {
                }

                User.CsrfToken = csrfToken;
                User.RankToken = $"{_deviceInfo.RankToken}_{userId}";

                IsUserAuthenticated = true;
                InvalidateProcessors();

                var us = await UserProcessor.GetUserInfoByIdAsync(long.Parse(userId));
                if (!us.Succeeded)
                {
                    IsUserAuthenticated = false;
                    return Result.Fail(us.Info, false);
                }

                User.UserName = HttpRequestProcessor.RequestMessage.Username =
                    User.LoggedInUser.UserName = us.Value.Username;
                User.LoggedInUser.FullName = us.Value.FullName;
                User.LoggedInUser.IsPrivate = us.Value.IsPrivate;
                User.LoggedInUser.IsVerified = us.Value.IsVerified;
                User.LoggedInUser.ProfilePicture = us.Value.ProfilePicUrl;
                User.LoggedInUser.ProfilePicUrl = us.Value.ProfilePicUrl;

                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                LogException(exception);
                return Result.Fail(exception, false);
            }
        }

        /// <summary>
        ///     2-Factor Authentication Login using a verification code
        ///     Before call this method, please run LoginAsync first.
        /// </summary>
        /// <param name="verificationCode">Verification Code sent to your phone number</param>
        /// <param name="verificationMethod">
        ///     Method to verify with.
        ///     0 for OTP
        ///     1 for SMS
        /// </param>
        /// <returns>
        ///     Success --> is succeed
        ///     InvalidCode --> The code is invalid
        ///     CodeExpired --> The code is expired, please request a new one.
        ///     Exception --> Something wrong happened
        /// </returns>
        public async Task<IResult<InstaLoginTwoFactorResult>> TwoFactorLoginAsync(string verificationCode,
            int verificationMethod = 1)
        {
            if (TwoFactorLoginInfo == null)
                return Result.Fail<InstaLoginTwoFactorResult>("Run LoginAsync first");

            try
            {
                var twoFactorRequestMessage = new ApiTwoFactorRequestMessage(verificationCode,
                    HttpRequestProcessor.RequestMessage.Username,
                    HttpRequestProcessor.RequestMessage.DeviceId,
                    TwoFactorLoginInfo.TwoFactorIdentifier);

                var instaUri = UriCreator.GetTwoFactorLoginUri();
                var data = new Dictionary<string, string>
                {
                    { "verification_code", verificationCode },
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "two_factor_identifier", TwoFactorLoginInfo.TwoFactorIdentifier },
                    { "username", HttpRequestProcessor.RequestMessage.Username.ToLower() },
                    { "trust_this_device", "1" },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "waterfall_id", Guid.NewGuid().ToString() },
                    { "verification_method", verificationMethod.ToString() }
                };
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);

                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var loginInfo =
                        JsonConvert.DeserializeObject<InstaLoginResponse>(json);
                    User.UserName = loginInfo.User?.UserName;
                    IsUserAuthenticated = loginInfo.User != null;
                    HttpRequestProcessor.RequestMessage.Username = loginInfo.User?.UserName;
                    var converter = ConvertersFabric.Instance.GetUserShortConverter(loginInfo.User);
                    User.LoggedInUser = converter.Convert();
                    User.RankToken = $"{User.LoggedInUser.Pk}_{HttpRequestProcessor.RequestMessage.PhoneId}";

                    InvalidateProcessors();
                    await AfterLoginAsync(response).ConfigureAwait(false);
                    return Result.Success(InstaLoginTwoFactorResult.Success);
                }

                var loginFailReason = JsonConvert.DeserializeObject<InstaLoginTwoFactorBaseResponse>(json);

                if (loginFailReason.ErrorType == "sms_code_validation_code_invalid")
                    return Result.Fail("Please check the security code.", InstaLoginTwoFactorResult.InvalidCode);

                if (!loginFailReason.Message.ToLower().Contains("challenge"))
                    return Result.Fail(
                        "This code is no longer valid, please, call LoginAsync again to request a new one",
                        InstaLoginTwoFactorResult.CodeExpired);
                ChallengeLoginInfo = loginFailReason.Challenge;

                return Result.Fail("Challenge is required", InstaLoginTwoFactorResult.ChallengeRequired);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaLoginTwoFactorResult), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                LogException(exception);
                return Result.Fail(exception, InstaLoginTwoFactorResult.Exception);
            }
        }

        /// <summary>
        ///     Get Two Factor Authentication details
        /// </summary>
        /// <returns>
        ///     An instance of TwoFactorInfo if success.
        ///     A null reference if not success; in this case, do LoginAsync first and check if Two Factor Authentication is
        ///     required, if not, don't run this method
        /// </returns>
        public async Task<IResult<InstaTwoFactorLoginInfo>> GetTwoFactorInfoAsync()
        {
            return await Task.Run(() =>
                TwoFactorLoginInfo != null
                    ? Result.Success(TwoFactorLoginInfo)
                    : Result.Fail<InstaTwoFactorLoginInfo>("No Two Factor info available."));
        }

        /// <summary>
        ///     Send requests for login flows (contact prefill, read msisdn header, launcher sync and qe sync)
        ///     <para>
        ///         Note 1: You should call this function before you calling <see cref="IInstaApi.LoginAsync(bool)" />, if you
        ///         want your account act like original instagram app.
        ///     </para>
        ///     <para>Note 2: One call per one account! No need to call while you are loading a session</para>
        /// </summary>
        public async Task<IResult<bool>> SendRequestsBeforeLoginAsync()
        {
            try
            {
                await GetToken();
                await GetContactPointPrefill();
                await LauncherSyncPrivate();
                await QeSync();
                await GetPrefillCandidates();
                await Task.Delay(500);
                await QeSync();
                await Task.Delay(2500);
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }

        /// <summary>
        ///     Send requests after you logged in successfully (Act as an real instagram user)
        /// </summary>
        public async Task<IResult<bool>> SendRequestsAfterLoginAsync()
        {
            try
            {
                await QeSync();
                await FeedProcessor.GetUserTimelineFeedAsync(PaginationParameters.MaxPagesToLoad(1));
                await StoryProcessor.GetStoryFeedAsync();
                await Task.Delay(1000);
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private async Task GetNotificationBadge()
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            try
            {
                var data = new Dictionary<string, string>
                {
                    //{"_csrftoken", _user.CsrfToken},
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "user_ids", "" }
                };
                var cookies = HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(HttpRequestProcessor.Client.BaseAddress);

                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value;
                if (!string.IsNullOrEmpty(csrftoken))
                    data.Add("_csrftoken", csrftoken);
                var instaUri = UriCreator.GetNotificationBadgeUri();
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
            }
        }

        private async Task GetContactPointPrefill()
        {
            try
            {
                var cookies = HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(HttpRequestProcessor.Client.BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value;
                //.{"phone_id":"----","usage":"prefill"}&
                var data = new Dictionary<string, string>
                {
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() }
                };

                if (!string.IsNullOrEmpty(csrftoken))
                    data.Add("_csrftoken", csrftoken);
                data.Add("usage", "prefill");
                var instaUri = UriCreator.GetContactPointPrefillUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
            }
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private async Task GetReadMsisdnHeader()
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            try
            {
                var cookies = HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(HttpRequestProcessor.Client.BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value;
                //.{"mobile_subno_usage":"default","device_id":"----"}&
                var data = new Dictionary<string, string>
                {
                    { "mobile_subno_usage", "default" },
                    { "device_id", _deviceInfo.DeviceGuid.ToString() }
                };

                if (!string.IsNullOrEmpty(csrftoken))
                    data.Add("_csrftoken", csrftoken);
                var instaUri = UriCreator.GetReadMsisdnHeaderUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
            }
        }

        private async Task GetPrefillCandidates()
        {
            try
            {
                var clientContactPoints = new JArray(new JObject
                {
                    { "type", "omnistring" },
                    { "value", User?.UserName?.ToLower() },
                    { "source", "last_login_attempt" }
                });
                var data = new Dictionary<string, string>
                {
                    { "android_device_id", _deviceInfo.DeviceId },
                    { "client_contact_points", clientContactPoints.ToString(Formatting.None) },
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "usages", "[\"account_recovery_omnibox\"]" },
                    { "logged_in_user_ids", "[]" },
                    { "device_id", _deviceInfo.DeviceGuid.ToString() }
                };

                var cookies = HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(HttpRequestProcessor.Client.BaseAddress);

                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value;
                if (!string.IsNullOrEmpty(csrftoken))
                    data.Add("_csrftoken", csrftoken);
                var instaUri = UriCreator.GetPrefillCandidatesUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
            }
        }

        private async Task LauncherSyncPrivate()
        {
            try
            {
                var data = new JObject();
                var cookies = HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(HttpRequestProcessor.Client.BaseAddress);

                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value;
                if (!string.IsNullOrEmpty(csrftoken))
                    data.Add("_csrftoken", csrftoken);
                data.Add("server_config_retrieval", "1");
                if (IsUserAuthenticated && User?.LoggedInUser != null)
                {
                    data.Add("id", User.LoggedInUser.Pk.ToString());
                    data.Add("_uid", User.LoggedInUser.Pk.ToString());
                    data.Add("_uuid", _deviceInfo.DeviceGuid.ToString());
                }
                else
                {
                    data.Add("id", _deviceInfo.DeviceGuid.ToString());
                }

                var uri = UriCreator.GetLauncherSyncUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, uri, _deviceInfo, data);

                var response = await HttpRequestProcessor.SendAsync(request);
                await AfterLoginAsync(response, true).ConfigureAwait(false);
                if (!IsUserAuthenticated)
                {
                    if (ContainsHeader(InstaApiConstants.RESPONSE_HEADER_IG_PASSWORD_ENC_PUB_KEY) &&
                        ContainsHeader(InstaApiConstants.RESPONSE_HEADER_IG_PASSWORD_ENC_KEY_ID))
                    {
                        User.PublicKey = string.Join("",
                            response.Headers.GetValues(InstaApiConstants.RESPONSE_HEADER_IG_PASSWORD_ENC_PUB_KEY));
                        User.PublicKeyId = string.Join("",
                            response.Headers.GetValues(InstaApiConstants.RESPONSE_HEADER_IG_PASSWORD_ENC_KEY_ID));
                    }

                    var mid = cookies[InstaApiConstants.COOKIES_MID]?.Value ??
                              (ContainsHeader(InstaApiConstants.RESPONSE_HEADER_IG_SET_X_MID)
                                  ? string.Join("",
                                      response.Headers.GetValues(InstaApiConstants.RESPONSE_HEADER_IG_SET_X_MID))
                                  : null);
                    var rur = cookies[InstaApiConstants.COOKIES_RUR]?.Value ??
                              (ContainsHeader(InstaApiConstants.RESPONSE_HEADER_X_IG_ORIGIN_REGION)
                                  ? string.Join("",
                                      response.Headers.GetValues(InstaApiConstants.RESPONSE_HEADER_X_IG_ORIGIN_REGION))
                                  : null);

                    if (!string.IsNullOrEmpty(mid)) User.XMidHeader = mid;

                    if (!string.IsNullOrEmpty(rur)) User.RurHeader = rur;

                    bool ContainsHeader(string head)
                    {
                        return response.Headers.Contains(head);
                    }
                }
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
            }
        }

        private async Task QeSync()
        {
            try
            {
                var data = new JObject();
                var cookies = HttpRequestProcessor.HttpHandler.CookieContainer
                    .GetCookies(HttpRequestProcessor.Client.BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value;
                if (!string.IsNullOrEmpty(csrftoken))
                    data.Add("_csrftoken", csrftoken);
                else if (!string.IsNullOrEmpty(User.CsrfToken))
                    data.Add("_csrftoken", User.CsrfToken);
                if (IsUserAuthenticated && User?.LoggedInUser != null)
                {
                    data.Add("id", _deviceInfo.DeviceGuid.ToString());
                    data.Add("_uid", _deviceInfo.DeviceGuid.ToString());
                    data.Add("server_config_retrieval", "1");
                    data.Add("experiments", InstaApiConstants. /*AFTER_*/LOGIN_EXPERIMENTS_CONFIGS);
                }
                else
                {
                    data.Add("id", _deviceInfo.DeviceGuid.ToString());
                    data.Add("server_config_retrieval", "1");
                    data.Add("experiments", InstaApiConstants.LOGIN_EXPERIMENTS_CONFIGS);
                }

                var uri = UriCreator.GetQeSyncUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, uri, _deviceInfo, data);
                request.Headers.Add("X-DEVICE-ID", _deviceInfo.DeviceGuid.ToString());
                var response = await HttpRequestProcessor.SendAsync(request);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
            }
        }

        /// <summary>
        ///     Logout from instagram asynchronously
        /// </summary>
        /// <returns>
        ///     True if logged out without errors
        /// </returns>
        public async Task<IResult<bool>> LogoutAsync()
        {
            ValidateUser();
            ValidateLoggedIn();
            try
            {
                var data = new Dictionary<string, string>
                {
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "_csrftoken", User.CsrfToken },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() }
                };
                var instaUri = UriCreator.GetLogoutUri();
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK) return Result.UnExpectedResponse<bool>(response, json);
                var logoutInfo = JsonConvert.DeserializeObject<BaseStatusResponse>(json);
                if (logoutInfo.Status == "ok")
                    IsUserAuthenticated = false;
                return Result.Success(!IsUserAuthenticated);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                LogException(exception);
                return Result.Fail(exception, false);
            }
        }

        /// <summary>
        ///     Get user lookup for recovery options
        /// </summary>
        /// <param name="usernameOrEmailOrPhoneNumber">Username or email or phone number</param>
        public async Task<IResult<InstaUserLookup>> GetRecoveryOptionsAsync(string usernameOrEmailOrPhoneNumber)
        {
            try
            {
                var csrfToken = "";
                if (!string.IsNullOrEmpty(User.CsrfToken))
                {
                    csrfToken = User.CsrfToken;
                }
                else
                {
                    var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                    var cookies =
                        HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                            .BaseAddress);
                    _logger?.LogResponse(firstResponse);
                    csrfToken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                }

                var data = new JObject
                {
                    { "_csrftoken", csrfToken },
                    { "q", usernameOrEmailOrPhoneNumber },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "directly_sign_in", "true" }
                };

                var instaUri = UriCreator.GetUsersLookupUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);

                var response = await HttpRequestProcessor.SendAsync(request);

                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<InstaUserLookupResponse>(json);
                return response.StatusCode != HttpStatusCode.OK
                    ? Result.Fail<InstaUserLookup>(obj.Message)
                    : Result.Success(ConvertersFabric.Instance.GetUserLookupConverter(obj).Convert());
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaUserLookup), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                return Result.Fail<InstaUserLookup>(exception);
            }
        }

        /// <summary>
        ///     Send recovery code by Username
        /// </summary>
        /// <param name="username">Username</param>
        public async Task<IResult<InstaRecovery>> SendRecoveryByUsernameAsync(string username)
        {
            return await SendRecoveryByEmailAsync(username);
        }

        /// <summary>
        ///     Send recovery code by Email
        /// </summary>
        /// <param name="email">Email Address</param>
        public async Task<IResult<InstaRecovery>> SendRecoveryByEmailAsync(string email)
        {
            try
            {
                var token = "";
                if (!string.IsNullOrEmpty(User.CsrfToken))
                {
                    token = User.CsrfToken;
                }
                else
                {
                    var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                    var cookies =
                        HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                            .BaseAddress);
                    _logger?.LogResponse(firstResponse);
                    token = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                }

                var postData = new JObject
                {
                    { "query", email },
                    { "adid", _deviceInfo.GoogleAdId },
                    { "device_id", ApiRequestMessage.GenerateDeviceId() },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "_csrftoken", token }
                };

                var instaUri = UriCreator.GetAccountRecoveryEmailUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);

                var response = await HttpRequestProcessor.SendAsync(request);

                var result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                    return Result.Success(JsonConvert.DeserializeObject<InstaRecovery>(result));
                var error = JsonConvert.DeserializeObject<MessageErrorsResponseRecoveryEmail>(result);
                return Result.Fail<InstaRecovery>(error.Message);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaRecovery), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                return Result.Fail<InstaRecovery>(exception);
            }
        }

        /// <summary>
        ///     Send recovery code by Phone
        /// </summary>
        /// <param name="phone">Phone Number</param>
        public async Task<IResult<InstaRecovery>> SendRecoveryByPhoneAsync(string phone)
        {
            try
            {
                var token = "";
                if (!string.IsNullOrEmpty(User.CsrfToken))
                {
                    token = User.CsrfToken;
                }
                else
                {
                    var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                    var cookies =
                        HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                            .BaseAddress);
                    _logger?.LogResponse(firstResponse);
                    token = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                }

                var postData = new JObject
                {
                    { "query", phone },
                    { "_csrftoken", User.CsrfToken }
                };

                var instaUri = UriCreator.GetAccountRecoverPhoneUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);

                var response = await HttpRequestProcessor.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var error = JsonConvert.DeserializeObject<BadStatusErrorsResponse>(result);
                    var errors = "";
                    error.Message.Errors.ForEach(errorContent => errors += errorContent + "\n");
                    return Result.Fail<InstaRecovery>(errors);
                }

                if (!result.Contains("errors"))
                    return Result.Success(JsonConvert.DeserializeObject<InstaRecovery>(result));
                {
                    var error = JsonConvert.DeserializeObject<BadStatusErrorsResponseRecovery>(result);
                    var errors = "";
                    error.PhoneNumber.Errors.ForEach(errorContent => errors += errorContent + "\n");

                    return Result.Fail<InstaRecovery>(errors);
                }
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaRecovery), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                return Result.Fail<InstaRecovery>(exception);
            }
        }

        /// <summary>
        ///     Send Two Factor Login SMS Again
        /// </summary>
        public async Task<IResult<TwoFactorLoginSMS>> SendTwoFactorLoginSMSAsync()
        {
            try
            {
                if (TwoFactorLoginInfo == null)
                    return Result.Fail<TwoFactorLoginSMS>("Run LoginAsync first");

                var postData = new Dictionary<string, string>
                {
                    { "two_factor_identifier", TwoFactorLoginInfo.TwoFactorIdentifier },
                    { "username", HttpRequestProcessor.RequestMessage.Username },
                    { "device_id", HttpRequestProcessor.RequestMessage.DeviceId },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "_csrftoken", User.CsrfToken }
                };

                var instaUri = UriCreator.GetAccount2FALoginAgainUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                var T = JsonConvert.DeserializeObject<TwoFactorLoginSMS>(result);
                if (!string.IsNullOrEmpty(T.TwoFactorInfo.TwoFactorIdentifier))
                    TwoFactorLoginInfo.TwoFactorIdentifier = T.TwoFactorInfo.TwoFactorIdentifier;
                return Result.Success(T);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(TwoFactorLoginSMS), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<TwoFactorLoginSMS>(exception);
            }
        }

        #region Challenge part

        /// <summary>
        ///     Get challenge data for logged in user
        ///     <para>This will pop-on, if some suspecious login happend</para>
        /// </summary>
        public async Task<IResult<InstaLoggedInChallengeDataInfo>> GetLoggedInChallengeDataInfoAsync()
        {
            UserAuthValidator.Validate(_userAuthValidate);

            try
            {
                var instaUri = UriCreator.GetChallengeRequireFirstUri("/challenge/", _deviceInfo.DeviceGuid.ToString(),
                    _deviceInfo.DeviceId);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaLoggedInChallengeDataInfo>(response, json);

                var obj = JsonConvert.DeserializeObject<InstaLoggedInChallengeDataInfoContainer>(json);
                return Result.Success(obj?.StepData);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaLoggedInChallengeDataInfo), ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex, (InstaLoggedInChallengeDataInfo)null);
            }
        }

        /// <summary>
        ///     Accept challlenge, it is THIS IS ME feature!!!!
        ///     <para>
        ///         You must call <see cref="IInstaApi.GetLoggedInChallengeDataInfoAsync" /> first,
        ///         if you across to <see cref="ResultInfo.ResponseType" /> equals to <see cref="ResponseType.ChallengeRequired" />
        ///         while you logged in!
        ///     </para>
        /// </summary>
        public async Task<IResult<bool>> AcceptChallengeAsync()
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetChallengeUri();

                var data = new JObject
                {
                    { "choice", "0" },
                    { "_csrftoken", User.CsrfToken },
                    { "_uid", User.LoggedInUser.Pk },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() }
                };

                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);

                var obj = JsonConvert.DeserializeObject<InstaChallengeRequireVerifyCode>(json);
                return obj.Action.ToLower() == "close" ? Result.Success(true) : Result.Success(false);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex);
            }
        }

        /// <summary>
        ///     Get challenge require (checkpoint required) options
        /// </summary>
        public async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync()
        {
            if (ChallengeLoginInfo == null)
                return Result.Fail("challenge require info is empty.\r\ntry to call LoginAsync function first.",
                    (InstaChallengeRequireVerifyMethod)null);

            try
            {
                var instaUri = UriCreator.GetChallengeRequireFirstUri(ChallengeLoginInfo.ApiPath,
                    _deviceInfo.DeviceGuid.ToString(), _deviceInfo.DeviceId);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaChallengeRequireVerifyMethod>(response, json);

                var obj = JsonConvert.DeserializeObject<InstaChallengeRequireVerifyMethod>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaChallengeRequireVerifyMethod),
                    ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex, (InstaChallengeRequireVerifyMethod)null);
            }
        }

        /// <summary>
        ///     Reset challenge require (checkpoint required) method
        /// </summary>
        public async Task<IResult<InstaChallengeRequireVerifyMethod>> ResetChallengeRequireVerifyMethodAsync()
        {
            if (ChallengeLoginInfo == null)
                return Result.Fail("challenge require info is empty.\r\ntry to call LoginAsync function first.",
                    (InstaChallengeRequireVerifyMethod)null);

            try
            {
                var instaUri = UriCreator.GetResetChallengeRequireUri(ChallengeLoginInfo.ApiPath);
                var data = new JObject
                {
                    { "_csrftoken", User.CsrfToken },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId }
                };
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var msg = "";
                    try
                    {
                        var j = JsonConvert.DeserializeObject<InstaChallengeRequireVerifyMethod>(json);
                        msg = j.Message;
                    }
                    catch
                    {
                    }

                    return Result.UnExpectedResponse<InstaChallengeRequireVerifyMethod>(response, json);
                }

                var obj = JsonConvert.DeserializeObject<InstaChallengeRequireVerifyMethod>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaChallengeRequireVerifyMethod),
                    ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex, (InstaChallengeRequireVerifyMethod)null);
            }
        }

        /// <summary>
        ///     Request verification code sms for challenge require (checkpoint required)
        /// </summary>
        /// <param name="replayChallenge">true if Instagram should resend verification code to you</param>
        public async Task<IResult<InstaChallengeRequireSMSVerify>> RequestVerifyCodeToSMSForChallengeRequireAsync(
            bool replayChallenge)
        {
            return await RequestVerifyCodeToSMSForChallengeRequire(replayChallenge);
        }

        /// <summary>
        ///     Submit phone number for challenge require (checkpoint required)
        ///     <para>
        ///         Note: This only needs , when you calling <see cref="IInstaApi.GetChallengeRequireVerifyMethodAsync" /> or
        ///         <see cref="IInstaApi.ResetChallengeRequireVerifyMethodAsync" /> and
        ///         <see cref="InstaChallengeRequireVerifyMethod.SubmitPhoneRequired" /> property is true.
        ///     </para>
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        public async Task<IResult<InstaChallengeRequireSMSVerify>> SubmitPhoneNumberForChallengeRequireAsync(
            string phoneNumber, bool replayChallenge)
        {
            return await RequestVerifyCodeToSMSForChallengeRequire(replayChallenge, phoneNumber);
        }

        private async Task<IResult<InstaChallengeRequireSMSVerify>> RequestVerifyCodeToSMSForChallengeRequire(
            bool replayChallenge, string phoneNumber = null)
        {
            if (ChallengeLoginInfo == null)
                return Result.Fail("challenge require info is empty.\r\ntry to call LoginAsync function first.",
                    (InstaChallengeRequireSMSVerify)null);

            try
            {
                Uri instaUri;

                instaUri = replayChallenge
                    ? UriCreator.GetChallengeReplayUri(ChallengeLoginInfo.ApiPath)
                    : UriCreator.GetChallengeRequireUri(ChallengeLoginInfo.ApiPath);

                var data = new JObject
                {
                    { "_csrftoken", User.CsrfToken },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId }
                };
                if (!string.IsNullOrEmpty(phoneNumber))
                    data.Add("phone_number", phoneNumber);
                else
                    data.Add("choice", "0");

                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var msg = "";
                    try
                    {
                        var j = JsonConvert.DeserializeObject<InstaChallengeRequireSMSVerify>(json);
                        msg = j.Message;
                    }
                    catch
                    {
                    }

                    return Result.Fail(msg, (InstaChallengeRequireSMSVerify)null);
                }

                var obj = JsonConvert.DeserializeObject<InstaChallengeRequireSMSVerify>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaChallengeRequireSMSVerify), ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex, (InstaChallengeRequireSMSVerify)null);
            }
        }

        /// <summary>
        ///     Request verification code email for challenge require (checkpoint required)
        /// </summary>
        /// <param name="replayChallenge">true if Instagram should resend verification code to you</param>
        public async Task<IResult<InstaChallengeRequireEmailVerify>> RequestVerifyCodeToEmailForChallengeRequireAsync(
            bool replayChallenge)
        {
            if (ChallengeLoginInfo == null)
                return Result.Fail("challenge require info is empty.\r\ntry to call LoginAsync function first.",
                    (InstaChallengeRequireEmailVerify)null);

            try
            {
                Uri instaUri;

                instaUri = replayChallenge
                    ? UriCreator.GetChallengeReplayUri(ChallengeLoginInfo.ApiPath)
                    : UriCreator.GetChallengeRequireUri(ChallengeLoginInfo.ApiPath);

                var data = new JObject
                {
                    { "choice", "1" },
                    { "_csrftoken", User.CsrfToken },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId }
                };
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var msg = "";
                    try
                    {
                        var j = JsonConvert.DeserializeObject<InstaChallengeRequireEmailVerify>(json);
                        msg = j.Message;
                    }
                    catch
                    {
                    }

                    return Result.Fail(msg, (InstaChallengeRequireEmailVerify)null);
                }

                var obj = JsonConvert.DeserializeObject<InstaChallengeRequireEmailVerify>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaChallengeRequireEmailVerify),
                    ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex, (InstaChallengeRequireEmailVerify)null);
            }
        }

        /// <summary>
        ///     Verify verification code for challenge require (checkpoint required)
        /// </summary>
        /// <param name="verifyCode">Verification code</param>
        public async Task<IResult<InstaLoginResult>> VerifyCodeForChallengeRequireAsync(string verifyCode)
        {
            if (ChallengeLoginInfo == null)
                return Result.Fail("challenge require info is empty.\r\ntry to call LoginAsync function first.",
                    InstaLoginResult.Exception);

            if (verifyCode.Length != 6)
                return Result.Fail("Verify code must be an 6 digit number.", InstaLoginResult.Exception);

            try
            {
                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;
                var instaUri = UriCreator.GetChallengeRequireUri(ChallengeLoginInfo.ApiPath);

                var data = new JObject
                {
                    { "security_code", verifyCode },
                    { "_csrftoken", User.CsrfToken },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId }
                };
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var msg = "";
                    try
                    {
                        var j = JsonConvert.DeserializeObject<InstaChallengeRequireVerifyCode>(json);
                        msg = j.Message;
                    }
                    catch
                    {
                    }

                    return Result.Fail(msg, InstaLoginResult.Exception);
                }

                var obj = JsonConvert.DeserializeObject<InstaChallengeRequireVerifyCode>(json);
                if (obj == null) return Result.Fail(obj?.Message, InstaLoginResult.Exception);
                if (obj.LoggedInUser != null)
                {
                    ValidateUserAsync(obj.LoggedInUser, csrftoken);
                    await Task.Delay(3000);
                    await AfterLoginAsync(response).ConfigureAwait(false);
                    await MessagingProcessor.GetDirectInboxAsync(PaginationParameters.MaxPagesToLoad(1));
                    await FeedProcessor.GetRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(1));

                    return Result.Success(InstaLoginResult.Success);
                }

                if (string.IsNullOrEmpty(obj.Action)) return Result.Fail(obj?.Message, InstaLoginResult.Exception);
                // we should wait at least 15 seconds and then trying to login again
                await Task.Delay(15000);
                return await LoginAsync(false);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaLoginResult), ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Result.Fail(ex, InstaLoginResult.Exception);
            }
        }

        #endregion Challenge part

        #endregion Authentication and challenge functions

        #region ORIGINAL FACEBOOK LOGIN

        private string _facebookAccessToken;

        /// <summary>
        ///     Login with Facebook access token
        /// </summary>
        /// <param name="fbAccessToken">Facebook access token</param>
        /// <param name="cookiesContainer">Cookies</param>
        /// <returns>
        ///     Success --> is succeed
        ///     TwoFactorRequired --> requires 2FA login.
        ///     BadPassword --> Password is wrong
        ///     InvalidUser --> User/phone number is wrong
        ///     Exception --> Something wrong happened
        ///     ChallengeRequired --> You need to pass Instagram challenge
        /// </returns>
        public async Task<IResult<InstaLoginResult>> LoginWithFacebookAsync(string fbAccessToken,
            string cookiesContainer)
        {
            return await LoginWithFacebookAsync(fbAccessToken, cookiesContainer, true);
        }

        public async Task<IResult<InstaLoginResult>> LoginWithFacebookAsync(string fbAccessToken,
            string cookiesContainer,
            bool dryrun = true, string username = null, string waterfallId = null, string adId = null,
            bool newToken = true)
        {
            try
            {
                _facebookAccessToken = null;
                if (newToken)
                {
                    var firstResponse = await HttpRequestProcessor.GetAsync(HttpRequestProcessor.Client.BaseAddress);
                    await firstResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    Debug.WriteLine("--------------------RELOGIN-------------------------");
                }

                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                var uri = new Uri(InstaApiConstants.INSTAGRAM_URL);

                cookiesContainer = cookiesContainer.Replace(';', ',');
                HttpRequestProcessor.HttpHandler.CookieContainer.SetCookies(uri, cookiesContainer);

                if (adId.IsEmpty())
                    adId = Guid.NewGuid().ToString();

                if (waterfallId.IsEmpty())
                    waterfallId = Guid.NewGuid().ToString();

                var instaUri = UriCreator.GetFacebookSignUpUri();

                var data = new JObject
                {
                    { "dryrun", dryrun.ToString().ToLower() },
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "_csrftoken", csrftoken },
                    { "adid", adId },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "waterfall_id", waterfallId },
                    { "fb_access_token", fbAccessToken }
                };
                if (username.IsNotEmpty())
                    data.Add("username", username);

                _facebookAccessToken = fbAccessToken;
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var loginFailReason = JsonConvert.DeserializeObject<InstaLoginBaseResponse>(json);

                    if (loginFailReason.InvalidCredentials)
                        return Result.Fail("Invalid Credentials",
                            loginFailReason.ErrorType == "bad_password"
                                ? InstaLoginResult.BadPassword
                                : InstaLoginResult.InvalidUser);
                    if (loginFailReason.TwoFactorRequired)
                    {
                        TwoFactorLoginInfo = loginFailReason.TwoFactorLoginInfo;
                        HttpRequestProcessor.RequestMessage.Username = TwoFactorLoginInfo.Username;
                        HttpRequestProcessor.RequestMessage.DeviceId = _deviceInfo.DeviceId;
                        return Result.Fail("Two Factor Authentication is required", InstaLoginResult.TwoFactorRequired);
                    }

                    switch (loginFailReason.ErrorType)
                    {
                        case "checkpoint_challenge_required":
                            ChallengeLoginInfo = loginFailReason.Challenge;

                            return Result.Fail("Challenge is required", InstaLoginResult.ChallengeRequired);

                        case "rate_limit_error":
                            return Result.Fail("Please wait a few minutes before you try again.",
                                InstaLoginResult.LimitError);

                        case "inactive user" or "inactive_user":
                            return Result.Fail($"{loginFailReason.Message}\r\nHelp url: {loginFailReason.HelpUrl}",
                                InstaLoginResult.InactiveUser);

                        case "checkpoint_logged_out":
                            return Result.Fail($"{loginFailReason.ErrorType} {loginFailReason.CheckpointUrl}",
                                InstaLoginResult.CheckpointLoggedOut);

                        default:
                            return Result.UnExpectedResponse<InstaLoginResult>(response, json);
                    }
                }

                var fbUserId = string.Empty;
                InstaUserShortResponse loginInfoUser = null;
                if (json.Contains("\"account_created\""))
                {
                    var rmt = JsonConvert.DeserializeObject<InstaFacebookRegistrationResponse>(json);
                    if (rmt?.AccountCreated != null)
                    {
                        fbUserId = rmt?.FbUserId;
                        if (rmt.AccountCreated.Value)
                        {
                            loginInfoUser = JsonConvert.DeserializeObject<InstaFacebookLoginResponse>(json)
                                ?.CreatedUser;
                        }
                        else
                        {
                            var desireUsername = rmt?.UsernameSuggestionsWithMetadata?.Suggestions?.LastOrDefault()
                                ?.Username;
                            await Task.Delay(4500);
                            await GetFacebookOnboardingStepsAsync();
                            await Task.Delay(12000);

                            return await LoginWithFacebookAsync(fbAccessToken, cookiesContainer, false, desireUsername,
                                waterfallId, adId, false);
                        }
                    }
                }

                if (loginInfoUser == null)
                {
                    var obj = JsonConvert.DeserializeObject<InstaFacebookLoginResponse>(json);
                    fbUserId = obj?.FbUserId;
                    loginInfoUser = obj?.LoggedInUser;
                }

                IsUserAuthenticated = true;
                var converter = ConvertersFabric.Instance.GetUserShortConverter(loginInfoUser);
                User.LoggedInUser = converter.Convert();
                User.RankToken = $"{User.LoggedInUser.Pk}_{HttpRequestProcessor.RequestMessage.PhoneId}";
                User.CsrfToken = csrftoken;
                User.FacebookUserId = fbUserId;
                User.UserName = User.LoggedInUser.UserName;
                User.FacebookAccessToken = fbAccessToken;
                User.Password = "ALAKIMASALAN";

                InvalidateProcessors();

                User.RankToken = $"{User.LoggedInUser.Pk}_{HttpRequestProcessor.RequestMessage.PhoneId}";
                if (string.IsNullOrEmpty(User.CsrfToken))
                {
                    cookies =
                        HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                            .BaseAddress);
                    User.CsrfToken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                }

                await AfterLoginAsync(response).ConfigureAwait(false);
                return Result.Success(InstaLoginResult.Success);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, InstaLoginResult.Exception, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                LogException(exception);
                return Result.Fail(exception, InstaLoginResult.Exception);
            }
        }

        private async Task<IResult<object>> GetFacebookOnboardingStepsAsync()
        {
            try
            {
                var cookies =
                    HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                        .BaseAddress);
                var csrftoken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                User.CsrfToken = csrftoken;

                //{
                //  "fb_connected": "true",
                //  "seen_steps": "[]",
                //  "phone_id": "d46328c2-01af-4457-9da2-bc60637abde6",
                //  "fb_installed": "false",
                //  "locale": "en_US",
                //  "timezone_offset": "12600",
                //  "_csrftoken": "2YmsoSkHtIknBA8maAqb1QSk92nrM6xo",
                //  "network_type": "WIFI-UNKNOWN",
                //  "_uid": "9013775990",
                //  "guid": "6324ecb2-e663-4dc8-a3a1-289c699cc876",
                //  "_uuid": "6324ecb2-e663-4dc8-a3a1-289c699cc876",
                //  "is_ci": "false",
                //  "android_id": "android-21c311d494a974fe",
                //  "reg_flow_taken": "facebook",
                //  "tos_accepted": "false"
                //}

                var postData = new Dictionary<string, string>
                {
                    { "fb_connected", "true" },
                    { "seen_steps", "[]" },
                    { "phone_id", _deviceInfo.PhoneGuid.ToString() },
                    { "fb_installed", "false" },
                    { "locale", InstaApiConstants.ACCEPT_LANGUAGE.Replace("-", "_") },
                    { "timezone_offset", InstaApiConstants.TIMEZONE_OFFSET.ToString() },
                    { "_csrftoken", csrftoken },
                    { "network_type", "WIFI-UNKNOWN" },
                    { "guid", _deviceInfo.DeviceGuid.ToString() },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "is_ci", "false" },
                    { "android_id", _deviceInfo.DeviceId },
                    { "reg_flow_taken", "facebook" },
                    { "tos_accepted", "false" }
                };

                var instaUri = UriCreator.GetOnboardingStepsUri();
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, postData);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var o = JsonConvert.DeserializeObject<InstaAccountRegistrationPhoneNumber>(json);

                    return Result.Fail(o.Message?.Errors?[0], (InstaRegistrationSuggestionResponse)null);
                }

                var obj = JsonConvert.DeserializeObject<InstaRegistrationSuggestionResponse>(json);
                return Result.Success(obj);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(InstaRegistrationSuggestionResponse),
                    ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaRegistrationSuggestionResponse>(exception);
            }
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private async Task<IResult<bool>> AcceptFacebookConsentRequiredAsync(string email, string phone = null)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            try
            {
                var delay = TimeSpan.FromSeconds(2);

                //{"message": "consent_required", "consent_data": {"headline": "Updates to Our Terms and Data Policy", "content": "We've updated our Terms and made some changes to our Data Policy. Please take a moment to review these changes and let us know that you agree to them.\n\nYou need to finish reviewing this information before you can use Instagram.", "button_text": "Review Now"}, "status": "fail"}
                await Task.Delay((int)delay.TotalMilliseconds);
                var instaUri = UriCreator.GetConsentNewUserFlowBeginsUri();
                var data = new JObject
                {
                    { "phone_id", _deviceInfo.PhoneGuid },
                    { "_csrftoken", User.CsrfToken }
                };
                var request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);

                await Task.Delay((int)delay.TotalMilliseconds);

                instaUri = UriCreator.GetConsentNewUserFlowUri();
                data = new JObject
                {
                    { "phone_id", _deviceInfo.PhoneGuid },
                    { "gdpr_s", "" },
                    { "_csrftoken", User.CsrfToken },
                    { "guid", _deviceInfo.DeviceGuid },
                    { "device_id", _deviceInfo.DeviceId }
                };
                if (email != null)
                {
                    data.Add("email", email);
                }
                else
                {
                    if (phone != null && !phone.StartsWith("+"))
                        phone = $"+{phone}";

                    phone ??= string.Empty;
                    data.Add("phone", phone);
                }

                request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                response = await HttpRequestProcessor.SendAsync(request);
                json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);

                await Task.Delay((int)delay.TotalMilliseconds);

                data = new JObject
                {
                    { "current_screen_key", "age_consent_two_button" },
                    { "phone_id", _deviceInfo.PhoneGuid },
                    { "gdpr_s", "[0,0,0,null]" },
                    { "_csrftoken", User.CsrfToken },
                    { "updates", "{\"age_consent_state\":\"2\"}" },
                    { "guid", _deviceInfo.DeviceGuid },
                    { "device_id", _deviceInfo.DeviceId }
                };
                request = HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                response = await HttpRequestProcessor.SendAsync(request);
                json = await response.Content.ReadAsStringAsync();

                return response.StatusCode != HttpStatusCode.OK
                    ? Result.UnExpectedResponse<bool>(response, json)
                    : Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex, false);
            }
        }

        #endregion ORIGINAL FACEBOOK LOGIN

        #region Other public functions

        /// <summary>
        ///     Get current API version info (signature key, api version info, app id)
        /// </summary>
        public InstaApiVersion GetApiVersionInfo()
        {
            return _apiVersion;
        }

        /// <summary>
        ///     Get user agent of current <see cref="IInstaApi" />
        /// </summary>
        public string GetUserAgent()
        {
            return _deviceInfo.GenerateUserAgent(_apiVersion);
        }

        /// <summary>
        ///     Set timeout to <see cref="HttpClient" />
        ///     <para>Note: Set timeout more than 100 seconds!</para>
        /// </summary>
        /// <param name="timeout">Timeout (set more than 100 seconds!)</param>
        public void SetTimeout(TimeSpan? timeout)
        {
            timeout ??= TimeSpan.FromSeconds(350);

            HttpClient.Timeout = timeout.Value;
        }

        /// <summary>
        ///     Set custom HttpClientHandler to be able to use certain features, e.g Proxy and so on
        /// </summary>
        /// <param name="handler">HttpClientHandler</param>
        public void UseHttpClientHandler(HttpClientHandler handler)
        {
            HttpRequestProcessor.SetHttpClientHandler(handler);
        }

        /// <summary>
        ///     Sets user credentials
        /// </summary>
        public void SetUser(string username, string password)
        {
            User.UserName = username;
            User.Password = password;

            HttpRequestProcessor.RequestMessage.Username = username;
            HttpRequestProcessor.RequestMessage.Password = password;
        }

        /// <summary>
        ///     Sets user credentials
        /// </summary>
        public void SetUser(UserSessionData user)
        {
            SetUser(user.UserName, user.Password);
        }

        /// <summary>
        ///     Gets current device
        /// </summary>
        public AndroidDevice GetCurrentDevice()
        {
            return _deviceInfo;
        }

        /// <summary>
        ///     Gets logged in user
        /// </summary>
        public UserSessionData GetLoggedUser()
        {
            return User;
        }

        /// <summary>
        ///     Get currently logged in user info asynchronously
        /// </summary>
        /// <returns>
        ///     <see cref="T:InstagramApiSharp.Classes.Models.InstaCurrentUser" />
        /// </returns>
        public async Task<IResult<InstaCurrentUser>> GetCurrentUserAsync()
        {
            ValidateUser();
            ValidateLoggedIn();
            return await UserProcessor.GetCurrentUserAsync();
        }

        /// <summary>
        ///     Get Accept Language
        /// </summary>
        public static string GetAcceptLanguage()
        {
            try
            {
                return InstaApiConstants.ACCEPT_LANGUAGE;
            }
            catch (Exception exception)
            {
                return Result.Fail<string>(exception).Value;
            }
        }

        /// <summary>
        ///     Get current time zone
        ///     <para>Returns something like: Asia/Tehran</para>
        /// </summary>
        /// <returns>Returns something like: Asia/Tehran</returns>
        public static string GetTimezone()
        {
            return InstaApiConstants.TIMEZONE;
        }

        /// <summary>
        ///     Get current time zone offset
        ///     <para>Returns something like this: 16200</para>
        /// </summary>
        /// <returns>Returns something like this: 16200</returns>
        public static int GetTimezoneOffset()
        {
            return InstaApiConstants.TIMEZONE_OFFSET;
        }

        /// <summary>
        ///     Set delay between requests. Useful when API supposed to be used for mass-bombing.
        /// </summary>
        /// <param name="delay">Timespan delay</param>
        public void SetRequestDelay(IRequestDelay delay)
        {
            delay ??= RequestDelay.Empty();
            _delay = delay;
            HttpRequestProcessor.Delay = _delay;
        }

        internal IRequestDelay GetRequestDelay()
        {
            return _delay;
        }

        /// <summary>
        ///     Set instagram api version (for user agent version)
        /// </summary>
        /// <param name="apiVersion">Api version</param>
        public void SetApiVersion(InstaApiVersionType apiVersion)
        {
            InstaApiVersionType = apiVersion;
            _apiVersion = InstaApiVersionList.GetApiVersionList().GetApiVersion(apiVersion);
            HttpHelper._apiVersion = _apiVersion;
        }

        /// <summary>
        ///     Set custom android device.
        ///     <para>
        ///         Note 1: If you want to use this method, you should call it before you calling
        ///         <seealso cref="IInstaApi.LoadStateDataFromStream(Stream)" /> or
        ///         <seealso cref="IInstaApi.LoadStateDataFromString(string)" />
        ///     </para>
        ///     <para>Note 2: this is optional, if you didn't set this, InstagramApiSharp will choose random device.</para>
        /// </summary>
        /// <param name="device">Android device</param>
        public void SetDevice(AndroidDevice device)
        {
            IsCustomDeviceSet = false;
            if (device == null)
                return;
            _deviceInfo = device;
            IsCustomDeviceSet = true;
        }

        /// <summary>
        ///     Set Accept Language
        /// </summary>
        /// <param name="languageCodeAndCountryCode">
        ///     Language Code and Country Code. For example:
        ///     <para>en-US for united states</para>
        ///     <para>fa-IR for IRAN</para>
        /// </param>
        public static bool SetAcceptLanguage(string languageCodeAndCountryCode)
        {
            try
            {
                InstaApiConstants.ACCEPT_LANGUAGE = languageCodeAndCountryCode;
                return true;
            }
            catch (Exception exception)
            {
                return Result.Fail<bool>(exception).Value;
            }
        }

        /// <summary>
        ///     Set time zone
        ///     <para>I.e: Asia/Tehran for Iran</para>
        /// </summary>
        /// <param name="timezone">
        ///     time zone
        ///     <para>I.e: Asia/Tehran for Iran</para>
        /// </param>
        public static void SetTimezone(string timezone)
        {
            if (string.IsNullOrEmpty(timezone))
                return;
            InstaApiConstants.TIMEZONE = timezone;
        }

        /// <summary>
        ///     Set time zone offset
        ///     <para>I.e: 16200 for Iran/Tehran</para>
        /// </summary>
        /// <param name="timezoneOffset">
        ///     time zone offset
        ///     <para>I.e: 16200 for Iran/Tehran</para>
        /// </param>
        public static void SetTimezoneOffset(int timezoneOffset)
        {
            InstaApiConstants.TIMEZONE_OFFSET = timezoneOffset;
        }

        /// <summary>
        ///     Send get request
        /// </summary>
        /// <param name="uri">Desire uri (must include https://i.instagram.com/api/v...) </param>
        public async Task<IResult<string>> SendGetRequestAsync(Uri uri)
        {
            try
            {
                if (uri == null)
                    return Result.Fail("Uri cannot be null!", default(string));

                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, uri, _deviceInfo);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                return response.StatusCode != HttpStatusCode.OK
                    ? Result.UnExpectedResponse<string>(response, json)
                    : Result.Success(json);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(string), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail(exception, default(string));
            }
        }

        /// <summary>
        ///     Send signed post request (include signed signature)
        /// </summary>
        /// <param name="uri">Desire uri (must include https://i.instagram.com/api/v...) </param>
        /// <param name="data">Data to post</param>
        public async Task<IResult<string>> SendSignedPostRequestAsync(Uri uri, Dictionary<string, string> data)
        {
            return await SendSignedPostRequest(uri, null, data);
        }

        /// <summary>
        ///     Send signed post request (include signed signature)
        /// </summary>
        /// <param name="uri">Desire uri (must include https://i.instagram.com/api/v...) </param>
        /// <param name="data">Data to post</param>
        public async Task<IResult<string>> SendSignedPostRequestAsync(Uri uri, JObject data)
        {
            return await SendSignedPostRequest(uri, data, null);
        }

        private async Task<IResult<string>> SendSignedPostRequest(Uri uri, JObject JData,
            Dictionary<string, string> DicData)
        {
            try
            {
                if (uri == null)
                    return Result.Fail("Uri cannot be null!", default(string));

                HttpRequestMessage request;
                if (JData != null)
                {
                    JData.Add("_uuid", _deviceInfo.DeviceGuid.ToString());
                    JData.Add("_uid", User.LoggedInUser.Pk.ToString());
                    JData.Add("_csrftoken", User.CsrfToken);
                    request = HttpHelper.GetSignedRequest(HttpMethod.Post, uri, _deviceInfo, JData);
                }
                else
                {
                    DicData.Add("_uuid", _deviceInfo.DeviceGuid.ToString());
                    DicData.Add("_uid", User.LoggedInUser.Pk.ToString());
                    DicData.Add("_csrftoken", User.CsrfToken);
                    request = HttpHelper.GetSignedRequest(HttpMethod.Post, uri, _deviceInfo, DicData);
                }

                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                return response.StatusCode != HttpStatusCode.OK
                    ? Result.UnExpectedResponse<string>(response, json)
                    : Result.Success(json);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(string), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail(exception, default(string));
            }
        }

        /// <summary>
        ///     Send post request
        /// </summary>
        /// <param name="uri">Desire uri (must include https://i.instagram.com/api/v...) </param>
        /// <param name="data">Data to post</param>
        public async Task<IResult<string>> SendPostRequestAsync(Uri uri, Dictionary<string, string> data)
        {
            try
            {
                if (uri == null)
                    return Result.Fail("Uri cannot be null!", default(string));

                data.Add("_uuid", _deviceInfo.DeviceGuid.ToString());
                data.Add("_uid", User.LoggedInUser.Pk.ToString());
                data.Add("_csrftoken", User.CsrfToken);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Post, uri, _deviceInfo, data);
                var response = await HttpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                return response.StatusCode != HttpStatusCode.OK
                    ? Result.UnExpectedResponse<string>(response, json)
                    : Result.Success(json);
            }
            catch (HttpRequestException httpException)
            {
                _logger?.LogException(httpException);
                return Result.Fail(httpException, default(string), ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail(exception, default(string));
            }
        }

        #endregion Other public functions

        #region State data

        /// <summary>
        ///     Get current state info as Memory stream
        /// </summary>
        /// <returns>
        ///     State data
        /// </returns>
        public Stream GetStateDataAsStream()
        {
            var state = GetStateDataAsObject();
            return SerializationHelper.SerializeToStream(state);
        }

        /// <summary>
        ///     Get current state info as Json string
        /// </summary>
        /// <returns>
        ///     State data
        /// </returns>
        public string GetStateDataAsString()
        {
            var state = GetStateDataAsObject();
            return SerializationHelper.SerializeToString(state);
        }

        /// <summary>
        ///     Get current state as StateData object
        /// </summary>
        /// <returns>
        ///     State data object
        /// </returns>
        public StateData GetStateDataAsObject()
        {
            var Cookies =
                HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(new Uri(InstaApiConstants.INSTAGRAM_URL));
            var RawCookiesList = new List<Cookie>();
            foreach (Cookie cookie in Cookies) RawCookiesList.Add(cookie);

            var state = new StateData
            {
                DeviceInfo = _deviceInfo,
                IsAuthenticated = IsUserAuthenticated,
                UserSession = User,
                Cookies = HttpRequestProcessor.HttpHandler.CookieContainer,
                RawCookies = RawCookiesList,
                InstaApiVersion = InstaApiVersionType
            };
            return state;
        }

        /// <summary>
        ///     Get current state info as Memory stream asynchronously
        /// </summary>
        /// <returns>
        ///     State data
        /// </returns>
        public async Task<Stream> GetStateDataAsStreamAsync()
        {
            return await Task<Stream>.Factory.StartNew(() =>
            {
                var state = GetStateDataAsStream();
                Task.Delay(1000);
                return state;
            });
        }

        /// <summary>
        ///     Get current state info as Json string asynchronously
        /// </summary>
        /// <returns>
        ///     State data
        /// </returns>
        public async Task<string> GetStateDataAsStringAsync()
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                var state = GetStateDataAsString();
                Task.Delay(1000);
                return state;
            });
        }

        /// <summary>
        ///     Loads the state data from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void LoadStateDataFromStream(Stream stream)
        {
            var data = SerializationHelper.DeserializeFromStream<StateData>(stream);
            LoadStateDataFromObject(data);
        }

        /// <summary>
        ///     Set state data from provided json string
        /// </summary>
        public void LoadStateDataFromString(string json)
        {
            var data = SerializationHelper.DeserializeFromString<StateData>(json);
            LoadStateDataFromObject(data);
        }

        private async Task GetToken(bool fromBaseUri = true)
        {
            try
            {
                await HttpRequestProcessor.SendAsync(HttpHelper.GetDefaultRequest(HttpMethod.Get, !fromBaseUri
                    ? UriCreator.GetLoginUri()
                    :
                    /*_httpRequestProcessor.Client.BaseAddress*/
                    UriCreator.GetTokenUri(), _deviceInfo));
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Set state data from StateData object
        /// </summary>
        /// <param name="stateData"></param>
        public void LoadStateDataFromObject(StateData stateData)
        {
            if (!IsCustomDeviceSet)
                _deviceInfo = stateData.DeviceInfo;
            User = stateData.UserSession;

            //Load Stream Edit
            HttpRequestProcessor.RequestMessage.Username = stateData.UserSession.UserName;
            HttpRequestProcessor.RequestMessage.Password = stateData.UserSession.Password;

            HttpRequestProcessor.RequestMessage.DeviceId = stateData.DeviceInfo.DeviceId;
            HttpRequestProcessor.RequestMessage.PhoneId = stateData.DeviceInfo.PhoneGuid.ToString();
            HttpRequestProcessor.RequestMessage.Guid = stateData.DeviceInfo.DeviceGuid;
            HttpRequestProcessor.RequestMessage.AdId = stateData.DeviceInfo.AdId.ToString();

            foreach (var cookie in stateData.RawCookies)
                HttpRequestProcessor.HttpHandler.CookieContainer.Add(new Uri(InstaApiConstants.INSTAGRAM_URL), cookie);

            stateData.InstaApiVersion ??= InstaApiVersionType.Version180;
            InstaApiVersionType = stateData.InstaApiVersion.Value;
            _apiVersion = InstaApiVersionList.GetApiVersionList().GetApiVersion(InstaApiVersionType);
            HttpHelper = new HttpHelper(_apiVersion, HttpRequestProcessor, this);

            IsUserAuthenticated = stateData.IsAuthenticated;
            InvalidateProcessors();
        }

        /// <summary>
        ///     Set state data from provided stream asynchronously
        /// </summary>
        public async Task LoadStateDataFromStreamAsync(Stream stream)
        {
            await Task.Factory.StartNew(() =>
            {
                LoadStateDataFromStream(stream);
                Task.Delay(1000);
            });
        }

        /// <summary>
        ///     Set state data from provided json string asynchronously
        /// </summary>
        public async Task LoadStateDataFromStringAsync(string json)
        {
            await Task.Factory.StartNew(() =>
            {
                LoadStateDataFromString(json);
                Task.Delay(1000);
            });
        }

        #endregion State data

        #region private part

        internal async Task AfterLoginAsync(HttpResponseMessage response, bool dontCallLauncherSync = false)
        {
            try
            {
                if (ContainsHeader(InstaApiConstants.HEADER_RESPONSE_X_WWW_CLAIM))
                {
                    var wwwClaimHeader = response.Headers.GetValues(InstaApiConstants.HEADER_RESPONSE_X_WWW_CLAIM);
                    if (wwwClaimHeader != null &&
                        string.Join("", wwwClaimHeader) is string wwwClaim &&
                        !string.IsNullOrEmpty(wwwClaim))
                        User.WwwClaim = wwwClaim;
                }

                if (ContainsHeader(InstaApiConstants.HEADER_X_FB_TRIP_ID))
                {
                    var fbTripIdHeader = response.Headers.GetValues(InstaApiConstants.HEADER_X_FB_TRIP_ID);
                    if (fbTripIdHeader != null &&
                        string.Join("", fbTripIdHeader) is string fbTripId &&
                        !string.IsNullOrEmpty(fbTripId))
                        User.FbTripId = fbTripId;
                }

                if (ContainsHeader(InstaApiConstants.HEADER_RESPONSE_AUTHORIZATION))
                {
                    var authorizationHeader =
                        response.Headers.GetValues(InstaApiConstants.HEADER_RESPONSE_AUTHORIZATION);
                    if (authorizationHeader != null &&
                        string.Join("", authorizationHeader) is string authorization &&
                        !string.IsNullOrEmpty(authorization) &&
                        authorization != InstaApiConstants.HEADER_BEARER_IGT_2_VALUE)
                        User.Authorization = authorization;
                }

                if (!dontCallLauncherSync) await LauncherSyncPrivate( /*false, true*/).ConfigureAwait(false);

                bool ContainsHeader(string head)
                {
                    return response.Headers.Contains(head);
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
        }

        internal void AddXMidHeader(HttpResponseMessage response)
        {
            var mid = response.Headers.Contains(InstaApiConstants.RESPONSE_HEADER_IG_SET_X_MID)
                ? string.Join("", response.Headers.GetValues(InstaApiConstants.RESPONSE_HEADER_IG_SET_X_MID))
                : null;

            if (!string.IsNullOrEmpty(mid)) User.XMidHeader = mid;
        }

        private void InvalidateProcessors()
        {
            HashtagProcessor = new HashtagProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            LocationProcessor = new LocationProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            CollectionProcessor = new CollectionProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            MediaProcessor = new MediaProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate,
                this, HttpHelper);
            UserProcessor = new UserProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate,
                this, HttpHelper);
            StoryProcessor = new StoryProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate,
                this, HttpHelper);
            CommentProcessor = new CommentProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            MessagingProcessor = new MessagingProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            FeedProcessor = new FeedProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate,
                this, HttpHelper);

            LiveProcessor = new LiveProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate,
                this, HttpHelper);
            DiscoverProcessor = new DiscoverProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            AccountProcessor = new AccountProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            HelperProcessor = new HelperProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate,
                this, HttpHelper);
            TVProcessor = new TVProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate, this,
                HttpHelper);
            BusinessProcessor = new BusinessProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            ShoppingProcessor = new ShoppingProcessor(_deviceInfo, User, HttpRequestProcessor, _logger,
                _userAuthValidate, this, HttpHelper);
            WebProcessor = new WebProcessor(_deviceInfo, User, HttpRequestProcessor, _logger, _userAuthValidate, this,
                HttpHelper);
        }

        public void ValidateUserAsync(InstaUserShortResponse user, string csrfToken, bool validateExtra = true,
            string password = null)
        {
            try
            {
                var converter = ConvertersFabric.Instance.GetUserShortConverter(user);
                User.LoggedInUser = converter.Convert();
                if (password != null)
                    User.Password = password;
                User.UserName = User.UserName;
                if (!validateExtra) return;
                User.RankToken = $"{User.LoggedInUser.Pk}_{HttpRequestProcessor.RequestMessage.PhoneId}";
                User.CsrfToken = csrfToken;
                if (string.IsNullOrEmpty(User.CsrfToken))
                {
                    var cookies =
                        HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
                            .BaseAddress);
                    User.CsrfToken = cookies[InstaApiConstants.CSRFTOKEN]?.Value ?? string.Empty;
                }

                IsUserAuthenticated = true;
                InvalidateProcessors();
            }
            catch
            {
            }
        }

        private void ValidateUser()
        {
            if (string.IsNullOrEmpty(User.UserName) || string.IsNullOrEmpty(User.Password))
                throw new ArgumentException("user name and password must be specified");
        }

        private void ValidateLoggedIn()
        {
            if (!IsUserAuthenticated)
                throw new ArgumentException("user must be authenticated");
        }

        private void ValidateRequestMessage()
        {
            if (HttpRequestProcessor.RequestMessage == null || HttpRequestProcessor.RequestMessage.IsEmpty())
                throw new ArgumentException("API request message null or empty");
        }

        private void LogException(Exception exception)
        {
            _logger?.LogException(exception);
        }

        #endregion
    }
}