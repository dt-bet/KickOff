using System;
using System.IO;
using System.Linq;
using KickOff;

namespace KickOff.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("KickOff");

            try
            {
                SQLitePCL.Batteries.Init();
             
                if (args.Length == 0)
                {
                    try
                    {
                        ScrapeUtility.ScrapeByType(ScrapeUtility.Type.Fixture);
                        ScrapeUtility.ScrapeByType(ScrapeUtility.Type.Result);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                        Console.ReadLine();
                    }
                }
                else if (args[0] == "evaluate")
                {
                    Main2(args);
                }
                else if (args[0] == "process")
                {
                    Main3(args);
                }
                else if (args[0] == "process_failures")
                {
                    ScrapeUtility.ProcessFailures();
                }
                else if (args[0] == "process_all")
                {
                    ProcessAll();
                }
                else if (args[0] == "modify_dates")
                {
                    ScrapeUtility.ModifyAllFiles();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllLines(ScrapeUtility.LogFile(), new[] { ex.StackTrace });
                Console.WriteLine(ex.StackTrace);
            }

            Console.Read();
        }



        public static void Main2(string[] args)
        {
            //Evaluate();
            PredictionEvaluation.Evaluate();
            Console.WriteLine("Finished");
            Console.ReadKey();
        }


        public static void Main3(string[] args)
        {

            ProcessHtml.ProcessResults((int)Scrape.Entity.League.EnglandPremierLeague);
            ProcessHtml.ProcessResults((int)Scrape.Entity.League.FranceLigue1);
            ProcessHtml.ProcessResults((int)Scrape.Entity.League.SpainPrimeraDivisión);
            ProcessHtml.ProcessResults((int)Scrape.Entity.League.GermanyBundesliga);
            ProcessHtml.ProcessResults((int)Scrape.Entity.League.WorldFriendlies);
            ProcessHtml.ProcessResults((int)Scrape.Entity.League.ItalySerieA);
            //ProcessHtml.ProcessFixtures();
            Console.WriteLine("Finished");
            Console.ReadKey();


        }


        public static void ProcessAll()
        {

            ProcessHtml.ProcessAllResults((int)Scrape.Entity.League.EnglandPremierLeague);
            ProcessHtml.ProcessAllResults((int)Scrape.Entity.League.FranceLigue1);
            ProcessHtml.ProcessAllResults((int)Scrape.Entity.League.SpainPrimeraDivisión);
            ProcessHtml.ProcessAllResults((int)Scrape.Entity.League.GermanyBundesliga);
            ProcessHtml.ProcessAllResults((int)Scrape.Entity.League.WorldFriendlies);
            ProcessHtml.ProcessAllResults((int)Scrape.Entity.League.ItalySerieA);
            //ProcessHtml.ProcessFixtures();
            Console.WriteLine("Finished");
            Console.ReadKey();


        }

        //public static void Evaluate()
        //{
        //    using (var connection = Db.GetConnection())
        //    {
        //        var results = connection.Table<Scrape.Entity.ScrapeResult>().ToArray();
        //        var fixtures = connection.Table<Scrape.Entity.ScrapeFixture>().ToArray();

        //        var ty = from result in results
        //                 join fixture in fixtures on
        //                 result.Home + result.Away + new DateTime(result.Ticks).Date.ToString("ddMMyy")
        //                 equals fixture.Home + fixture.Away + new DateTime(fixture.Ticks).Date.ToString("ddMMyy")
        //                 select Scrape.Entity.Evaluator.GetAccuracy(result, fixture);
        //        var yc = ty.ToArray();
        //        ;
        //        var average = yc.Where(_ => _.Value > 0).Select(_ => _.Value).Average();

        //        var xxx = yc.Where(_ => _.Value < 0).ToArray();
        //        ; ;
        //    }
        //}
    }
}
