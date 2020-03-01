using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Timers;
using System.IO;
using System.Threading;
using Scrape.Entity;

namespace KickOff.App
{
    public class ScrapeUtility
    {


        const string KickOffAIHomePage = "http://kickoff.ai";
        const string MatchSection = "matches";
        const string LogFileName = "Log.txt";
        const string DateFormat = "dd_MM_yy";



        public enum Type { Fixture, Result }

        private static string LogFile() => ProcessHtml.Info().FullName + "/" + LogFileName;




        public static void ScrapeByType(Type type)
        {
            string date = DateTime.Now.ToString(DateFormat);

            DirectoryInfo info = ProcessHtml.Info();
            using (var enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Thread.Sleep(1000);
                    Process(info, date, ((int)enumerator.Current).ToString(), type);
                }
            }
        }


        static IEnumerator<Competition> GetEnumerator()
        {
            return Enum.GetValues(typeof(Competition))
                .Cast<Competition>()
                .Where(_ => _ != Competition.WorldCup)
                .GetEnumerator();
        }


        private static void Process(DirectoryInfo info, string date, string competition, Type type)
        {
            string competitionURL = string.Join("/", new[] { KickOffAIHomePage, MatchSection, competition });

            string directoryPath = info + "/" + competition;
            System.IO.Directory.CreateDirectory(directoryPath);
            string fileName = directoryPath + "/" + date;

            HtmlDocument htmlCode = new HtmlWeb().Load(competitionURL);
            System.IO.File.WriteAllText(fileName, htmlCode.ParsedText);
            Console.WriteLine(competitionURL);
            try
            {
                if (type == Type.Fixture)
                    ProcessFixtures(htmlCode, int.Parse(competition));
                else if (type == Type.Result)
                    ProcessResults(htmlCode, int.Parse(competition));
                else
                    throw new ArgumentException($"type {type} not accounted for.");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllLines(LogFile(),new[] { ex.Message });
                Console.ReadLine();
            }
        }

        public static void ProcessFixtures(HtmlDocument document, int competition)
        {
            using (var connection = Db.GetConnection())
            {
                var fixtures = ProcessHtml.GetFixturePredictions(document, competition).ToArray();

                var arr = connection.Table<ScrapeFixture>().ToArray();
                var exception = fixtures.Except(arr);
                connection.InsertAll(exception);
            }
        }

        public static void ProcessResults(HtmlDocument document, int competition)
        {

            using (var connection = Db.GetConnection())
            {
                var results = ProcessHtml.GetResultPredictions(document, competition).ToArray();

                var arr = connection.Table<ScrapeResult>().ToArray();
                var exception = results.Except(arr);
                connection.InsertAll(exception);
            }

        }
    }
}
