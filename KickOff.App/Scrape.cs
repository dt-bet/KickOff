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
            using (var enumerator = GetCompetitionEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Thread.Sleep(1000);
                    Process(info, date, ((int)enumerator.Current).ToString(), type);
                }
            }
        }

        public static void ProcessFailures()
        {

            DirectoryInfo info = ProcessHtml.Info();
            using (var enumerator = GetCompetitionEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ProcessFailures(info, (int)enumerator.Current);
                }
            }
        }


        static IEnumerator<Competition> GetCompetitionEnumerator()
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
                System.IO.File.AppendAllLines(LogFile(), new[] { ex.Message });
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }


        private static void ProcessFailures(DirectoryInfo info, int comp)
        {

            string directoryPath = info + "/" + comp.ToString();

            Record[] records = null;
            using (var connection = Db.GetConnection())
            {
                connection.CreateTable<Record>();
                records = connection.Table<Record>().Where(a => a.CompetitionId == comp).ToArray();
            }

            Console.WriteLine(comp);

            foreach (var path in from path in Directory.GetFiles(directoryPath)
                                 let file = Path.GetFileNameWithoutExtension(path)
                                 let split = file.Split('_')
                                 orderby string.Join('_',split[2], split[1], split[0])
                                 join r in records
                                 on file  equals r.Date 
                                 into temp 
                                 where temp.Any() == false
                                 select path)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(path));

                try
                {
                    ProcessFixtures(doc, comp);
                    ProcessResults(doc,comp);
                    using (var connection = Db.GetConnection())
                    {
                        int inserted = connection.Insert(new Record(comp, path));
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllLines(LogFile(), new[] { ex.Message });
                    Console.WriteLine(ex.StackTrace);
                    Console.ReadLine();
                }
            }
        }


        public static void ProcessFixtures(HtmlDocument document, int competition)
        {
            using (var connection = Db.GetConnection())
            {


                var fixtures = ProcessHtml.GetFixturePredictions(document, competition).ToArray();

                var arr = connection.Table<ScrapeFixture>().ToArray();
                var exception = fixtures.Except(arr);
                int processed = connection.InsertAll(exception);
                Console.WriteLine("Inserted Fixtures " + processed);
            }
        }

        public static void ProcessResults(HtmlDocument document, int competition)
        {

            using (var connection = Db.GetConnection())
            {
                var results = ProcessHtml.GetResultPredictions(document, competition).ToArray();

                var arr = connection.Table<ScrapeResult>().ToArray();
                var exception = results.Except(arr);
                int processed= connection.InsertAll(exception);
                Console.WriteLine("Inserted Results " + processed);
            }

        }

        private class Record : IEquatable<Record>
        {
            public Record()
            {
            }

            public Record(int competitionId, string date)
            {
                CompetitionId = competitionId;
                Date = date;
            }

            public int CompetitionId { get; set; }

            public string Date { get; set; }



            public bool Equals(Record other)
            {
                return CompetitionId == other.CompetitionId && this.Date == other.Date;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj as Record);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(CompetitionId, Date);
            }

            public override string ToString()
            {
                return CompetitionId + " " + Date;
            }

            public static bool operator ==(Record left, Record right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Record left, Record right)
            {
                return !(left == right);
            }
        }
    }
}
