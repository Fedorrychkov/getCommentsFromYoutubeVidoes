using System;
using Service.ParseYoutube;

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
        static void Main(string[] args)
        {
            Console.WriteLine("YouTube Data API: Search");
            Console.WriteLine("========================");

            var parserService = new ParserService();
            parserService.ExportCommentToCsvFile();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        
    }
}