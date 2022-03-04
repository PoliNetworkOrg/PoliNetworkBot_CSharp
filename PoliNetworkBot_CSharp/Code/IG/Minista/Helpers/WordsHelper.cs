#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

#endregion

namespace Minista.Helpers
{
    internal class WordsHelper : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<Paragraph> GetParagraph(string text,
            TypedEventHandler<Hyperlink, HyperlinkClickEventArgs> hyperLinkAction, Color? defaultColor = null,
            Color? hyperColor = null)
        {
            var list = new List<Paragraph>();
            if (hyperColor == null)
                hyperColor = Helper.GetColorFromHex("#ff5897fc");
            if (defaultColor == null)
                defaultColor = Colors.White;
            var p = new Paragraph();

            try
            {
                var chars = text.ToCharArray();

                var parsedTextX = GetParsedPassageX(text);
                parsedTextX.Count.PrintDebug();

                parsedTextX.ForEach(item =>
                {
                    if (item.PassageType == PassageType.Text)
                    {
                        if (item.Text.Contains("@") || item.Text.Contains("#") || item.Text.Contains("http://") ||
                            item.Text.Contains("https://") || item.Text.Contains("www."))
                        {
                            var hyper = new Hyperlink
                            {
                                Foreground = new SolidColorBrush(hyperColor.Value)
                            };
                            hyper.Inlines.Add(new Run
                                { Text = item.Text, Foreground = new SolidColorBrush(hyperColor.Value) });
                            hyper.Click += hyperLinkAction;
                            p.Inlines.Add(hyper);
                            //p.Inlines.Add(new Run() { Text = " " });
                        }
                        else
                        {
                            p.Inlines.Add(new Run
                                { Text = item.Text, Foreground = new SolidColorBrush(defaultColor.Value) });
                            //p.Inlines.Add(new Run() { Text = " " });
                        }
                    }
                    else if (item.PassageType == PassageType.Space)
                    {
                        p.Inlines.Add(new Run { Text = " " });
                    }
                    else if (item.PassageType == PassageType.NewLine)
                    {
                        //p.Inlines.Add(new LineBreak());
                        list.Add(p);
                        p = new Paragraph();
                    }
                    else
                    {
                        if ( /*item.Text?.StartsWith("%NEWLINE%") ||*/ item.PassageType == PassageType.NewLine)
                        {
                            p.Inlines.Add(new LineBreak());
                        }
                        else
                        {
                            var hyper = new Hyperlink
                            {
                                Foreground = new SolidColorBrush(hyperColor.Value)
                            };
                            hyper.Inlines.Add(new Run
                            {
                                Text = item.Text,
                                Foreground = new SolidColorBrush(hyperColor.Value)
                            });

                            hyper.UnderlineStyle = UnderlineStyle.None;
                            hyper.Click += hyperLinkAction;
                            p.Inlines.Add(hyper);
                            p.Inlines.Add(new Run { Text = " " });
                        }
                    }
                });
                list.Add(p);
            }
            catch (Exception ex)
            {
                ex.PrintException("GetParagraph");
                p.Inlines.Clear();
                p.Inlines.Add(new Run { Text = text, Foreground = new SolidColorBrush(defaultColor.Value) });
                list.Clear();
                list.Add(p);
            }

            //p.Inlines.Count.PrintDebug();
            return list;
        }

        private Passage GetP(PassageType type = PassageType.Text)
        {
            return new Passage { PassageType = type };
        }

        public List<Passage> GetParsedPassageX(string text)
        {
            var parsedList = new List<Passage>();
            try
            {
                var p = GetP();

                var chars = text.ToCharArray().ToList();
                chars.ForEach(c =>
                {
                    var item = c.ToString();
                    if (item == "\r" || item == Environment.NewLine)
                    {
                        parsedList.Add(p);
                        p = GetP(PassageType.NewLine);
                        parsedList.Add(p);
                    }
                    else if (item == "@")
                    {
                        if (p.PassageType != PassageType.User)
                            p = GetP(PassageType.User);
                        p.Text += item;
                    }
                    else if (item == "#")
                    {
                        if (p.PassageType != PassageType.Hashtag)
                            p = GetP(PassageType.Hashtag);
                        p.Text += item;
                    }
                    else if (item == " ")
                    {
                        parsedList.Add(p);
                        p = GetP(PassageType.Space);
                        parsedList.Add(p);
                    }
                    else
                    {
                        if (p.PassageType != PassageType.User &&
                            p.PassageType != PassageType.Hashtag)
                            if (p.PassageType != PassageType.Text)
                                p = GetP();
                        p.Text += item;
                    }
                });
                parsedList.Add(p);
                p = GetP(PassageType.Space);
                parsedList.Add(p);
                //😱😱😂😂%NEWLINE%@mohsen.akhavan1
                //var list = new List<string>(text.Split(new string[] { " ", Environment.NewLine }, StringSplitOptions.None));
                //foreach (var item in list)
                //{
                //    if (item.StartsWith("http://") || item.StartsWith("https://") || item.StartsWith("www."))
                //        parsedList.Add(new Passage { Text = item, PassageType = PassageType.Url });
                //    else if (item.StartsWith("#"))
                //        parsedList.Add(new Passage { Text = item, PassageType = PassageType.Hashtag });
                //    else if (item.StartsWith("@"))
                //        parsedList.Add(new Passage { Text = item, PassageType = PassageType.User });
                //    else
                //    {
                //        var splitter = new List<string>(item.Split(new[] { "%NEWLINE%" }, StringSplitOptions.None));
                //        foreach (var spl in splitter)
                //        {
                //            if (spl.StartsWith("http://") || spl.StartsWith("https://") || spl.StartsWith("www."))
                //                parsedList.Add(new Passage { Text = spl, PassageType = PassageType.Url });
                //            else if (spl.StartsWith("#"))
                //                parsedList.Add(new Passage { Text = spl, PassageType = PassageType.Hashtag });
                //            else if (spl.StartsWith("@"))
                //                parsedList.Add(new Passage { Text = spl, PassageType = PassageType.User });
                //            else
                //                parsedList.Add(new Passage { Text = spl, PassageType = PassageType.Text });
                //            if (splitter.Count > 1)
                //                parsedList.Add(new Passage { PassageType = PassageType.NewLine });
                //        }

                //    }
                //}
            }
            catch (Exception ex)
            {
                ex.PrintException("GetParsedPassage");
            }

            return parsedList;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            // if (disposing)
            // {
            // }
            disposed = true;
        }
    }
}