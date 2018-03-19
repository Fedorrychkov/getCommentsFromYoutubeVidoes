using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace ParseYoutube
{
    /// <summary>
    /// YouTube Data API v3 sample: search by keyword.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://developers.google.com/api-client-library/dotnet/get_started
    ///
    /// Set ApiKey to the API key value from the APIs & auth > Registered apps tab of
    ///   https://cloud.google.com/console
    /// Please ensure that you have enabled the YouTube Data API for your project.
    /// </summary>
    public class Search
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("YouTube Data API: Search");
            Console.WriteLine("========================");

            try
            {
                new Search().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run()
        {
            string[] PositiveMarkers = { "молодцы", "так держать", "лучший", "крым наш", "мир", "дружба", "красавчик", "отлично", "хороший", "не вор", "добро" };
            string[] NegativeMarkers = { "бандит", "вор", "мошенник", "взяточник", "хохлы", "москали", "москаль", "хохол", "говносми", "ватный", "диванный" };

            /** Init Youtube Service */
            var baseClientService = new BaseClientService.Initializer();
            baseClientService.ApiKey = "AIzaSyDIj8I0bg1V8FsqO8nKcKc8vEvjJFkOJEc";
            baseClientService.ApplicationName = this.GetType().ToString();
            var youtubeService = new YouTubeService(baseClientService);
            /** End Init Youtube Service */
            /** Query **/
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = "Крым"; // Replace with your search term.
            searchListRequest.MaxResults = 50;
            /** Query **/
            /** Get Results **/
            var searchListResponse = await searchListRequest.ExecuteAsync();
            List<VideoModel> Videos = new List<VideoModel>();
            foreach (var searchResult in searchListResponse.Items)
            {   // Create Array
                VideoModel SingleVideo = new VideoModel();
                var videoInfo = await VideosListById(youtubeService, "statistics", searchResult.Id.VideoId, SingleVideo);
                if (searchResult.Id.Kind == "youtube#video")
                {
                    SingleVideo.VideoTitle = searchResult.Snippet.Title;
                    SingleVideo.VideoId = searchResult.Id.VideoId;
                    SingleVideo.PublishedAt = searchResult.Snippet.PublishedAt;
                    SingleVideo.DislikeCount = videoInfo.Statistics.DislikeCount.ToString();
                    SingleVideo.LikeCount = videoInfo.Statistics.LikeCount.ToString();
                    SingleVideo.VideoViews = videoInfo.Statistics.ViewCount.ToString();
                    SingleVideo.CommentsCount = videoInfo.Statistics.CommentCount;

                    if (videoInfo.Statistics.CommentCount != 0)
                    {
                        int negativeCounter = 0;
                        int positiveCounter = 0;
                        try
                        {
                            var comments = await CommentThreadsListByVideoID(youtubeService, "snippet,replies", SingleVideo.VideoId, 100);
                            if (comments != null)
                            {
                                List<string> comment = new List<string>();
                                foreach (var commentRes in comments.Items)
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
                                    SingleVideo.NegativeCount = negativeCounter;
                                    SingleVideo.PositiveCount = positiveCounter;
                                }
                                SingleVideo.Comments = comment;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Ошибка в Commets: " + ex.Message);
                            continue;
                        }
                    }
                }
                Console.WriteLine("(" + SingleVideo.VideoId + "): " + SingleVideo.VideoTitle + " | ViewsCount: " + SingleVideo.VideoViews + " | CommentsCount: " + SingleVideo.CommentsCount + " | PositiveComment " + SingleVideo.PositiveCount + " | NegativeComment " + SingleVideo.NegativeCount);
                Videos.Add(SingleVideo);
            }

            WrtiteToCSV(Videos, searchListRequest.Q);
        }
        public async Task<Video> VideosListById(YouTubeService service, string part, string id, VideoModel SingleVideo)
        {
            VideoListResponse response = new VideoListResponse();
            var call = service.Videos.List(part);
            call.Id = id;
            var res = await call.ExecuteAsync();
            return res.Items[0];
        }
        public async Task<CommentThreadListResponse> CommentThreadsListByVideoID(YouTubeService service, string part, string videoId, Int64 maxResult)
        {
            CommentThreadListResponse response = new CommentThreadListResponse();
            var call = service.CommentThreads.List(part);
            call.MaxResults = maxResult;
            call.VideoId = videoId;
            try
            {
                var res = await call.ExecuteAsync();
                response = res;
            }
            catch (IOException ex)
            {
                Console.WriteLine("ERRRROOORRR: " + ex.Message);
            }

            return response;
        }

        public void WrtiteToCSV(List<VideoModel> Videos, string fileName)
        {
            Console.WriteLine("We in WriteToCSV function");
                var sw = new StreamWriter(fileName + ".csv", false);
                sw.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", "id видео", "Название видео", "Публикация видео", "Просмотров", "Лайков", "Дизлайков", "Положительных комментариев (всего)", "Негативных комментариев (всего)", "Комментарий"));
            int index = 0;
                foreach (var item in Videos)
                {
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