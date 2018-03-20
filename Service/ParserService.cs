using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Models.ParseYoutube;
using ParseYoutube.Service;
using System;
using System.Collections.Generic;
using System.IO;

namespace Service.ParseYoutube
{
    public class ParserService : IParserService
    {
        public const string apiKey = "AIzaSyDIj8I0bg1V8FsqO8nKcKc8vEvjJFkOJEc";

        private readonly string[] PositiveMarkers = { "молодцы", "так держать", "лучший", "крым наш", "мир", "дружба", "красавчик", "отлично", "хороший", "не вор", "добро" };
        private readonly string[] NegativeMarkers = { "бандит", "вор", "мошенник", "взяточник", "хохлы", "москали", "москаль", "хохол", "говносми", "ватный", "диванный" };
        private readonly string searchQuestion = "Обещания Путина";

        public void ExportCommentToCsvFile()
        {
            /** Init Youtube Service */
            var baseClientService = new BaseClientService.Initializer();
            baseClientService.ApiKey = apiKey;
            var youtubeService = new YouTubeService(baseClientService);
            /** End Init Youtube Service */
            /** Query **/
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = searchQuestion; // Replace with your search term.
            searchListRequest.MaxResults = 50;
            /** Query **/
            /** Get Results **/
            var searchListResponse = searchListRequest.Execute();
            var videos = new List<VideoModel>();
            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind != "youtube#video")
                {
                    continue;
                }

                var singleVideo = new VideoModel();
                var videoInfo = VideosListById(youtubeService, "statistics", searchResult.Id.VideoId, singleVideo);
                if (videoInfo.Statistics.CommentCount == 0)
                {
                    continue;
                }

                singleVideo.VideoTitle = searchResult.Snippet.Title;
                singleVideo.VideoId = searchResult.Id.VideoId;
                singleVideo.PublishedAt = searchResult.Snippet.PublishedAt;
                singleVideo.DislikeCount = videoInfo.Statistics.DislikeCount.ToString();
                singleVideo.LikeCount = videoInfo.Statistics.LikeCount.ToString();
                singleVideo.VideoViews = videoInfo.Statistics.ViewCount.ToString();
                singleVideo.CommentsCount = videoInfo.Statistics.CommentCount;               

                int negativeCounter = 0;
                int positiveCounter = 0;
                var comments = CommentThreadsListByVideoID(youtubeService, "snippet,replies", singleVideo.VideoId, 100);
                if (comments == null)
                {
                    continue;
                }

                List<string> comment = new List<string>();
                foreach (var commentRes in comments.Items)
                {
                    if (commentRes.Snippet.TopLevelComment.Snippet.TextDisplay.Length > 20 && commentRes.Snippet.TopLevelComment.Snippet.TextDisplay.Length < 150)
                    {
                        comment.Add(commentRes.Snippet.TopLevelComment.Snippet.TextDisplay);
                        var str = commentRes.Snippet.TopLevelComment.Snippet.TextDisplay;
                        for (var i = 0; i < PositiveMarkers.Length; i++)
                        {
                            if (str.Contains(PositiveMarkers[i]))
                            {
                                positiveCounter++;
                            }
                            if (str.Contains(NegativeMarkers[i]))
                            {
                                negativeCounter++;
                            }
                        }
                        singleVideo.NegativeCount = negativeCounter;
                        singleVideo.PositiveCount = positiveCounter;
                    }
                    singleVideo.Comments = comment;
                }

                var resultStr = $"({singleVideo.VideoId}) {singleVideo.VideoTitle} | ViewsCount: {singleVideo.VideoViews } | CommentsCount: {singleVideo.CommentsCount} | PositiveComment: {singleVideo.PositiveCount} | NegativeComment: {singleVideo.NegativeCount}";

                Console.WriteLine(resultStr);
                videos.Add(singleVideo);
            }

            WrtiteToCSV(videos, searchListRequest.Q);
        }

        private Video VideosListById(YouTubeService service, string part, string id, VideoModel SingleVideo)
        {
            VideoListResponse response = new VideoListResponse();
            var call = service.Videos.List(part);
            call.Id = id;
            var res = call.Execute();
            return res.Items[0];
        }

        private CommentThreadListResponse CommentThreadsListByVideoID(YouTubeService service, string part, string videoId, Int64 maxResult)
        {
            CommentThreadListResponse response = new CommentThreadListResponse();
            var call = service.CommentThreads.List(part);
            call.MaxResults = maxResult;
            call.VideoId = videoId;

            try
            {
                var res = call.Execute();
                response = res;

                return response;
            }
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
            
        }

        private void WrtiteToCSV(List<VideoModel> Videos, string fileName)
        {
            Console.WriteLine("We in WriteToCSV function");
            var sw = new StreamWriter(fileName + ".csv", false);
            sw.WriteLine("id видео;Название видео;Публикация видео;Просмотров;Лайков;Дизлайков;Положительных комментариев (всего);Негативных комментариев (всего);Комментарий");
            int index = 0;
            foreach (var item in Videos)
            {
                if (item.Comments == null)
                {
                    continue;
                }
                foreach (var comment in item.Comments)
                {
                    index++;
                    Console.WriteLine("items:" + index);
                    //if (item != null && comment != null)
                    //{
                    try
                    {
                        sw.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", item.VideoId, item.VideoTitle, item.PublishedAt, item.VideoViews, item.LikeCount, item.DislikeCount, item.PositiveCount, item.NegativeCount, comment));
                        //Console.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", item.VideoId, item.VideoTitle, item.PublishedAt, item.VideoViews, item.LikeCount, item.DislikeCount, item.PositiveCount, item.NegativeCount, comment));
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Ошибка при построении CSV: " + ex.Message);
                        continue;
                    }
                    //}
                }
            }
            sw.Close();

        }
    }
}
