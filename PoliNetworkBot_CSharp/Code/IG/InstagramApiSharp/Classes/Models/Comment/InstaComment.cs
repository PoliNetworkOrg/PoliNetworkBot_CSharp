#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Comment;

public class InstaComment : INotifyPropertyChanged
{
    private bool _haslikedcm;
    public int Type { get; set; }

    public int BitFlags { get; set; }

    public long UserId { get; set; }

    public string Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int LikesCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public InstaContentType ContentType { get; set; }
    public InstaUserShort User { get; set; }
    public long Pk { get; set; }
    public string Text { get; set; }

    public bool DidReportAsSpam { get; set; }

    public bool HasLikedComment
    {
        get => _haslikedcm;
        set
        {
            _haslikedcm = value;
            Update("HasLikedComment");
        }
    }

    public int ChildCommentCount { get; set; }

    //public int NumTailChildComments { get; set; }

    public bool HasMoreTailChildComments { get; set; }

    public bool HasMoreHeadChildComments { get; set; }

    //public string NextMaxChildCursor { get; set; }
    public List<InstaCommentShort> PreviewChildComments { get; set; } = new();

    public List<InstaUserShort> OtherPreviewUsers { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    private void Update(string pName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
    }

    private bool Equals(InstaComment comment)
    {
        return Pk == comment?.Pk;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as InstaComment);
    }

    public override int GetHashCode()
    {
        return Pk.GetHashCode();
    }
}