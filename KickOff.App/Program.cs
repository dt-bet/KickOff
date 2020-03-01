using System;
using System.Linq;
using KickOff;

namespace KickOff.App
{
    public class Program
    {
        public static void Main(string[] args)
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
