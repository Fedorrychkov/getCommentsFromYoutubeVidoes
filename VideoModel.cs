using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace ParseYoutube
{
    public class VideoModel: IEnumerable
    {
        List<VideoModel> listGroup = new List<VideoModel>();

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

        public override string ToString()
        {
            return string.Format("{0}; {1};", this.VideoId, this.VideoTitle);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return listGroup.GetEnumerator();
        }
        public IEnumerator<VideoModel> GetEnumerator()
        {
            return listGroup.GetEnumerator();
        }
    }
}
