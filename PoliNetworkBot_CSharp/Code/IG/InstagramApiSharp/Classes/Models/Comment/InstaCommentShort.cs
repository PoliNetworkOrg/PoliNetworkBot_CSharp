#region

using System;
using System.ComponentModel;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaCommentShort : INotifyPropertyChanged
    {
        private bool _haslikedcm;
        public InstaContentType ContentType { get; set; }

        public InstaUserShort User { get; set; }

        public long Pk { get; set; }

        public string Text { get; set; }

        public int Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public long MediaId { get; set; }

        public string Status { get; set; }

        public long ParentCommentId { get; set; }

        public bool HasLikedComment
        {
            get => _haslikedcm;
            set
            {
                _haslikedcm = value;
                Update("HasLikedComment");
            }
        }

        public int CommentLikeCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Update(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
    }
}