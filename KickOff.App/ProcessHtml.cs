using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack.CssSelectors.NetCore;
using System.IO;
using Scrape.Entity;

namespace KickOff.App
{
    public class ProcessHtml
    {

        public static DirectoryInfo Info() => System.IO.Directory.CreateDirectory("../../Data");


        public static void ProcessResults(int league)
        {
            var connection = Db.GetConnection();
            connection.CreateTable<ScrapeResult>();


            //var rp = GetResultPredictions(GetHtmlDocument(league), league).ToArray();

            //var results = connection.Table<ScrapeResult>().ToArray();
            //connection.InsertAll(rp.OfType<ScrapeResult>().Except(results));

            //var fixtures = connection.Table<ScrapeFixture>().ToArray();
            //connection.InsertAll(rp.OfType<ScrapeFixture>().Except(fixtures));
        }



        public static void ProcessAllResults(int league)
        {
            var connection = Db.GetConnection();
            connection.CreateTable<ScrapeResult>();
            connection.CreateTable<ScrapeFixture>();
            var results = connection.Table<ScrapeResult>().ToList();
            var fixtures = connection.Table<ScrapeFixture>().ToList();


            foreach (var (file, date) in from file in Directory.EnumerateFiles("../../../Data/" + league, "??_??_??")
                                         let name = Path.GetFileNameWithoutExtension(file)
                                         let date = GetDate(name)
                                         orderby date
                                         select (file, date))
            {
                var rp = GetResultPredictions(GetHtmlDocument(file), date, league).ToArray();

                var newResults = rp.OfType<ScrapeResult>();
                var newFixtures = rp.OfType<ScrapeFixture>();
                var exepResults = newResults.Except(results).ToArray();
                var exepFixtures = newFixtures.Except(fixtures).ToArray();

                connection.InsertAll(exepResults);
                connection.InsertAll(exepFixtures);

                results.AddRange(exepResults);
                fixtures.AddRange(exepFixtures);

                Console.WriteLine(new FileInfo(file).Name);
            }
        }


        public static IEnumerable<ScrapeMatch> GetResultPredictions(HtmlDocument document, DateTime year, int competition = 0)
        {

            foreach (var node in GetResultNodes(document))
            {
                bool b = false;
                string homeTeam = "";
                string awayTeam = "";
                DateTime date = default(DateTime);
                int[] score = null;
                int[] prediction = null;

                try
                {
                    homeTeam = GetHomeTeam(node);
                    awayTeam = GetAwayTeam(node);
                    date = GetResultDate(node, year);
                    score = GetResult(node);
                    prediction = GetPrediction(node);
                    b = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if (b)
                {
                    yield return new ScrapeResult
                    {
                        Home = homeTeam,
                        Away = awayTeam,
                        Ticks = date.Ticks,
                        HomeScore = score[0],
                        AwayScore = score[1],
                        League = (League)Enum.ToObject(typeof(League), competition)
                    };
                    yield return new ScrapeFixture
                    {
                        Home = homeTeam,
                        Away = awayTeam,
                        HomePrediction = prediction[0],
                        DrawPrediction = prediction[1],
                        AwayPrediction = prediction[2],
                        Ticks = date.Ticks,
                        League = (League)Enum.ToObject(typeof(League), competition)
                    };
                }
            }
        }

        public static IEnumerable<ScrapeFixture> GetFixturePredictions(HtmlDocument document, DateTime year, int competition)
        {
            foreach (var node in GetFixtureNodes(document))
            {
                bool b = false;
                string homeTeam = "";
                string awayTeam = "";
                DateTime date = default(DateTime);
                int[] prediction = null;

                try
                {
                    homeTeam = GetHomeTeam(node);
                    awayTeam = GetAwayTeam(node);
                    date = GetFixtureDate(node, year);
                    prediction = GetPrediction(node);
                    b = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if (b)
                    yield return new ScrapeFixture
                    {
                        Home = homeTeam,
                        Away = awayTeam,
                        HomePrediction = prediction[0],
                        DrawPrediction = prediction[1],
                        AwayPrediction = prediction[2],
                        Ticks = date.Ticks,
                        League = (Scrape.Entity.League)competition
                    };
            }
        }

        private static DateTime GetDate(string fileName)
        {
            return DateTime.Parse(fileName.Replace('_', '-'));
                           
        }

        static string GetHomeTeam(HtmlNode node)
        {
            return node.QuerySelector("div.team-home > span.team-name").InnerText;
        }

        static DateTime GetFixtureDate(HtmlNode node, DateTime fileDate)
        {
            var cc = node.QuerySelector("a div.match-time-list").InnerText.Replace('\n', ' ').Trim();

            DateTime.TryParseExact(cc,
                   "HH:mm dd MMMM",
               CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
               out DateTime dateTime);

            dateTime = dateTime.AddYears(fileDate.Year - dateTime.Year);

            if (dateTime.DayOfYear < fileDate.DayOfYear)
                dateTime = dateTime.AddYears(1);

            return dateTime;

        }
        static DateTime GetResultDate(HtmlNode node, DateTime fileDate)
        {
            var cc = node.QuerySelector("a div.match-time-list").InnerText.Replace('\n', ' ').Trim();

            bool success = DateTime.TryParseExact(cc,
                   "HH:mm dd MMMM",
               CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
               out DateTime dateTime);

            if(success==false)
            {

            }

            dateTime= dateTime.AddYears(fileDate.Year - dateTime.Year);

            if (dateTime.DayOfYear > fileDate.DayOfYear)
                dateTime = dateTime.AddYears(-1);
            else
            {

            }

            return dateTime;

        }

        static string GetAwayTeam(HtmlNode node)
        {
            return node.QuerySelector("div.team-away > span.team-name").InnerText;
        }

        static int[] GetResult(HtmlNode node)
        {
            return node.QuerySelector("div.result").InnerText
            .Replace(" ", "")
                .Split(':')
            .Select(_ => int.Parse(_))
                .ToArray();

        }

        static int[] GetPrediction(HtmlNode node)
        {
            var nodes = node.QuerySelectorAll("div.progress > div");

            return nodes
            .Select(_ => _.Attributes["aria-valuenow"])
            .Select(_ => int.Parse(_.Value))
            .ToArray();

        }

        static IList<HtmlNode> GetResultNodes(HtmlDocument document)
        {
            return document.QuerySelectorAll("div.prediction.prediction-result");

        }

        static IList<HtmlNode> GetFixtureNodes(HtmlDocument document)
        {
            return document.QuerySelectorAll("div.prediction.prediction-fixture");

        }


        static HtmlDocument GetHtmlDocument(int competition)
        {
            //Path exists on laptop 
            //string path = "/home/declan/Documents/Kickoffai_Predictions.html";


            string path = "../../../Data/Html/";
            switch (competition)
            {
                case (int)League.ItalySerieA:
                    path = path + "IT Serie A.htm";
                    break;
                case (int)League.GermanyBundesliga:
                    path = path + "DE Bundesliga.htm";
                    break;
                case (int)League.SpainPrimeraDivisión:
                    path = path + "ES La Liga.htm";
                    break;
                case (int)League.FranceLigue1:
                    path = path + "FR Ligue 1.htm";
                    break;
                case (int)League.WorldFriendlies:
                    path = path + "Friendlies.htm";
                    break;
                case (int)League.EnglandPremierLeague:
                    path = path + "UK Premier League.htm";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var doc = new HtmlDocument();

            doc.Load(path);

            return doc;
        }

        static HtmlDocument GetHtmlDocument(string path)
        {
            var doc = new HtmlDocument();

            doc.Load(path);

            return doc;
        }
    }






}
