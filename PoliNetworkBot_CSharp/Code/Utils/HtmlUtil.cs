using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class HtmlUtil
    {
        internal static List<HtmlNode> getElementsByTagAndClassName(HtmlNode doc, string tag = "", string className = "", int? limit = null)
        {
            if (doc == null)
                return null;

            List<HtmlNode> lst = new List<HtmlNode>();
            bool empty_tag = String.IsNullOrEmpty(tag);
            bool empty_cn = String.IsNullOrEmpty(className);
            if (empty_tag && empty_cn) return null;

            if (limit != null && limit.Value <= 0)
                return null;

            List<HtmlNode> result = new List<HtmlNode>();

            if (empty_tag && limit == null)
            {
                lst.Add(doc);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        continue;
                    }

                    if (lst[i].GetClasses().Contains(className))
                    {
                        result.Add(lst[i]);
                    }


                    var childcollection = lst[i].ChildNodes;
                    if (childcollection != null)
                    {
                        foreach (var child in childcollection)
                        {
                            lst.Add(child);
                        }
                    }


                }

                return result;
            }
            else if (empty_cn && limit == null)
            {
                lst.Add(doc);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        continue;
                    }

                    if (lst[i].Name == tag)
                    {
                        result.Add(lst[i]);
                    }


                    var childcollection = lst[i].ChildNodes;
                    if (childcollection != null)
                    {
                        foreach (var child in childcollection)
                        {
                            lst.Add(child);
                        }
                    }


                }

                return result;
            }
            else if (empty_cn == false && empty_tag == false && limit == null)
            {

                lst.Add(doc);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        continue;
                    }

                    if (lst[i].GetClasses().Contains(className) && lst[i].Name == tag)
                    {
                        result.Add(lst[i]);
                    }


                    var childcollection = lst[i].ChildNodes;
                    if (childcollection != null)
                    {
                        foreach (var child in childcollection)
                        {
                            lst.Add(child);
                        }
                    }


                }

                return result;
            }
            else if (empty_tag && limit != null)
            {

                lst.Add(doc);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        continue;
                    }

                    if (lst[i].GetClasses().Contains(className))
                    {
                        result.Add(lst[i]);

                        if (result.Count == limit.Value)
                            return result;
                    }


                    var childcollection = lst[i].ChildNodes;
                    if (childcollection != null)
                    {
                        foreach (var child in childcollection)
                        {
                            lst.Add(child);
                        }
                    }


                }

                return result;
            }
            else if (empty_cn && limit != null)
            {
                lst.Add(doc);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        continue;
                    }

                    if (lst[i].Name == tag)
                    {
                        result.Add(lst[i]);


                        if (result.Count == limit.Value)
                            return result;
                    }


                    var childcollection = lst[i].ChildNodes;
                    if (childcollection != null)
                    {
                        foreach (var child in childcollection)
                        {
                            lst.Add(child);
                        }
                    }


                }

                return result;
            }

            else if (empty_cn == false && empty_tag == false && limit != null)
            {

                lst.Add(doc);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null)
                    {
                        continue;
                    }

                    if (lst[i].GetClasses().Contains(className) && lst[i].Name == tag)
                    {
                        result.Add(lst[i]);

                        if (result.Count == limit.Value)
                            return result;
                    }


                    var childcollection = lst[i].ChildNodes;
                    if (childcollection != null)
                    {
                        foreach (var child in childcollection)
                        {
                            lst.Add(child);
                        }
                    }


                }

                return result;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}