#region

using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class HtmlUtil
{
    internal static List<HtmlNode?>? GetElementsByTagAndClassName(HtmlNode? doc, string tag = "",
        string className = "", long? limit = null)
    {
        if (doc == null)
            return null;

        var lst = new List<HtmlNode?>();
        var emptyTag = string.IsNullOrEmpty(tag);
        var emptyCn = string.IsNullOrEmpty(className);
        if (emptyTag && emptyCn) return null;

        if (limit is <= 0)
            return null;

        var result = new List<HtmlNode?>();

        if (emptyTag && limit == null)
        {
            lst.Add(doc);
            for (var i = 0; i < lst.Count; i++)
            {
                var htmlNode = lst[i];
                if (htmlNode == null) continue;

                if (htmlNode.GetClasses().Contains(className)) result.Add(htmlNode);

                var childCollection = htmlNode.ChildNodes;
                if (childCollection == null) continue;
                lst.AddRange(childCollection);
            }

            return result;
        }

        switch (emptyCn)
        {
            case true when limit == null:
            {
                lst.Add(doc);
                for (var i = 0; i < lst.Count; i++)
                {
                    var htmlNode = lst[i];
                    if (htmlNode == null) continue;

                    if (htmlNode.Name == tag) result.Add(htmlNode);

                    var childcollection = htmlNode.ChildNodes;
                    if (childcollection == null) continue;
                    lst.AddRange(childcollection);
                }

                return result;
            }
            case false when emptyTag == false && limit == null:
            {
                lst.Add(doc);
                for (var i = 0; i < lst.Count; i++)
                {
                    var htmlNode = lst[i];
                    if (htmlNode == null) continue;

                    if (htmlNode.GetClasses().Contains(className) && htmlNode.Name == tag) result.Add(htmlNode);

                    var childcollection = htmlNode.ChildNodes;
                    if (childcollection == null) continue;
                    lst.AddRange(childcollection);
                }

                return result;
            }
        }

        if (emptyTag && limit != null)
        {
            lst.Add(doc);
            for (var i = 0; i < lst.Count; i++)
            {
                var htmlNode = lst[i];
                if (htmlNode == null) continue;

                if (htmlNode.GetClasses().Contains(className))
                {
                    result.Add(htmlNode);

                    if (result.Count == limit.Value)
                        return result;
                }

                var childCollection = htmlNode.ChildNodes;
                if (childCollection == null) continue;
                lst.AddRange(childCollection);
            }

            return result;
        }

        switch (emptyCn)
        {
            case true when limit != null:
            {
                lst.Add(doc);
                for (var i = 0; i < lst.Count; i++)
                {
                    var htmlNode = lst[i];
                    if (htmlNode == null) continue;

                    if (htmlNode.Name == tag)
                    {
                        result.Add(htmlNode);

                        if (result.Count == limit.Value)
                            return result;
                    }

                    var childcollection = htmlNode.ChildNodes;
                    if (childcollection == null) continue;
                    lst.AddRange(childcollection);
                }

                return result;
            }
            case false when emptyTag == false && limit != null:
            {
                lst.Add(doc);
                for (var i = 0; i < lst.Count; i++)
                {
                    var htmlNode = lst[i];
                    if (htmlNode == null) continue;

                    if (htmlNode.GetClasses().Contains(className) && htmlNode.Name == tag)
                    {
                        result.Add(htmlNode);

                        if (result.Count == limit.Value)
                            return result;
                    }

                    var childcollection = htmlNode.ChildNodes;
                    if (childcollection == null) continue;
                    lst.AddRange(childcollection);
                }

                return result;
            }
            default:
                throw new ArgumentException();
        }
    }
}