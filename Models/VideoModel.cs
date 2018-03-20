using System;
using System.Collections.Generic;

namespace Models.ParseYoutube
{
    public class VideoModel
    {
        public string VideoId { get; set; }
        public string VideoTitle { get; set; }
        public string VideoViews { get; set; }
        public string LikeCount { get; set; }
        public string DislikeCount { get; set; }
        public int PositiveCount { get; set; }
        public int NegativeCount { get; set; }
        public DateTime? PublishedAt { get; set; }
        public ulong? CommentsCount { get; set; }
        public List<string> Comments { get; set; }
    }
}
