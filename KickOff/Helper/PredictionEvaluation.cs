using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Collections;
using Scrape.Entity;
using KickOff;

namespace KickOff.App
{

    public class PredictionEvaluation
    {
        class CustomEqualityComparer
        {
            public static bool AreEqual(ScrapeResult object1, Bet365Row object2)
            {
                return object1.Date() == object2.Date() && 
                    object1.AwayScore == object2.AwayScore &&
                    object1.HomeScore == object2.HomeScore &&
                    object1.FirstCharacterHome(3) == object2.FirstCharacterHome(3) &&
                    object1.FirstCharacterAway(3) == object2.FirstCharacterAway(3)
                    ;

            }
        }

        public class Grouping : IGrouping<DateTime, (ScrapeResult, Bet365Row)>
        {
            public List<(ScrapeResult, Bet365Row)> List = new List<(ScrapeResult, Bet365Row)>();
            public DateTime Key { get; }

            public IEnumerator<(ScrapeResult, Bet365Row)> GetEnumerator() => List.GetEnumerator();
      
            IEnumerator IEnumerable.GetEnumerator()=> List.GetEnumerator();
       
        }

        public static void Evaluate()
        {


            var pai = Db.GetConnection().Table<ScrapeResult>().ToArray().GroupBy(_ => _.Date());
            var pair = Db.GetConnection().Table<ScrapeFixture>().ToArray().GroupBy(_ => _.Date());
            var b365 = new Bet365Db().GetRows().GroupBy(_ => _.Date());

           var x2= pai.Join(b365, _ => _.Key, _ => _.Key, (agroup, bgroup) =>
                   {
                       
                       foreach (var a in agroup)
                           foreach (var b in bgroup)
                           {
                               if (CustomEqualityComparer.AreEqual(a, b))
                               {
                                  return (new Grouping { List = new List<(ScrapeResult, Bet365Row)> { (a, b) } });
                               }

                           }
                       return null;
                   }).Where(_=>_!=null);

            var xx=x2.Join(pair,_=>_.Key,_=>_.Key,(agroup,bgroup)=>
            {
                List<(ScrapeResult, Bet365Row,ScrapeFixture)> list = new List<(ScrapeResult, Bet365Row, ScrapeFixture)>();
                foreach (var a in agroup)
                    foreach (var b in bgroup)
                    {
                        if (a.Item2.Equals(b))
                        {
                            list.Add((a.Item1, a.Item2,b));
                        }

                    }
                return list;
            });

            List<double> homeProfit = new List<double>();
            List<double> drawProfit = new List<double>();
            List<double> awayProfit = new List<double>();
            List<double> sum = new List<double>();
            List<double> alldaysum = new List<double>();
            sum.Add(100);
            homeProfit.Add(100);
            drawProfit.Add(100);
            awayProfit.Add(100);

            foreach (var ls in xx)
            {
                foreach (var l in ls)
                {
                    Console.WriteLine($"{l.Item1.Home} {l.Item2.HomeTeam}");
                    Console.WriteLine($"{l.Item1.Away} {l.Item2.AwayTeam}");
                    double home=(l.Item3.HomePrediction * l.Item2.HomeProfit());
                    double draw = (l.Item3.DrawPrediction * l.Item2.DrawProfit());
                    double away = (l.Item3.AwayPrediction * l.Item2.AwayProfit());
                    double premium = 100d / l.Item2.AwayOdd + 100d / l.Item2.HomeOdd + 100d / l.Item2.DrawOdd;
                    double daysum = home + draw + away;
                  /*  Console.WriteLine(premium);
                    Console.WriteLine(home);
                    Console.WriteLine(draw);
                    Console.WriteLine(away);
                    Console.WriteLine(daysum);
                    alldaysum.Add(daysum);
                    Console.WriteLine(alldaysum.Sum());*/

                    if (premium < 1.03)
                    {
                        homeProfit.Add(home);
                        drawProfit.Add(draw );
                        awayProfit.Add(away);
                        
                    }
                    double last = sum.Last();
                    //if(premium<1.02)
                    sum.Add(last + last*(home + draw + away)*0.000001);
                    Console.WriteLine(last);
                }
            }

            Console.WriteLine("Finished");
            Console.WriteLine("home profit av. " + homeProfit.Average());
            Console.WriteLine("home profit sum " + homeProfit.Sum());
            Console.WriteLine("draw profit av. " + drawProfit.Average());
            Console.WriteLine("draw profit sum " + drawProfit.Sum());
            Console.WriteLine("away profit av. " + awayProfit.Average());
            Console.WriteLine("away profit sum " + awayProfit.Sum());
            Console.WriteLine("sum " + sum.Last());
            Console.WriteLine("count " + homeProfit.Count());
            Console.ReadKey();
        }
    }



  

}
