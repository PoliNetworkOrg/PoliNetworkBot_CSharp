#region

using System;
using Windows.UI.Xaml.Documents;

#endregion

namespace Minista.Helpers
{
    internal static class HyperLinkHelper
    {
        public static void HyperLinkClick(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            if (sender == null)
                return;
            try
            {
                if (sender.Inlines.Count > 0)
                    if (sender.Inlines[0] is Run run && run != null)
                    {
                        var text = run.Text;
                        text = text.ToLower();
                        run.Text.PrintDebug();
                        if (text.StartsWith("http://") || text.StartsWith("https://") || text.StartsWith("www."))
                        {
                            UriHelper.HandleUri(run.Text);
                        }
                        else
                        {
                            if (text.Contains("@"))
                            {
                                if (text.Replace(" ", "").StartsWith("@"))
                                {
                                    text = text.Replace("@", "").Trim();
                                    if (!string.IsNullOrEmpty(text))
                                        Helper.OpenProfile(text);
                                }
                                else
                                {
                                    text.OpenEmail();
                                }
                            }
                            else if (text.Contains("#"))
                            {
                                text = text.Replace("#", "").Trim();
                                NavigationService.Navigate(typeof(Views.Infos.HashtagView), text);
                            }
                            //MainPage.Current?.PushSearch(text);
                        }
                    }
            }
            catch (Exception ex)
            {
                ex.PrintException("CaptionHyperLinkClick");
            }
        }
    }
}