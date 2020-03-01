using System;
using System.Collections.Generic;
using System.Linq;

namespace KickOff
{

    public class Bet365Row
    {
        public string Season { get; set; }
        public long Ticks { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public int HomeOdd { get; set; }
        public int DrawOdd { get; set; }
        public int AwayOdd { get; set; }
        public double HomeProfit() => (HomeScore > AwayScore ? DrawOdd / 100d : 0) - 1;
        public double DrawProfit() => (HomeScore == AwayScore ? HomeOdd / 100d : 0) - 1;
        public double AwayProfit() => (HomeScore < AwayScore ? AwayOdd / 100d : 0) - 1;
        public string FirstCharacterHome(int i = 1) => string.Concat(HomeTeam.Take(i));
        public string FirstCharacterAway(int i = 1) => string.Concat(AwayTeam.Take(i));
        public DateTime Date() => new DateTime(Ticks).Date;
    }

    class Bet365Db
    {
        public IEnumerable<Bet365Row> GetRows()
        {
            SQLite.SQLiteConnection ty = new SQLite.SQLiteConnection("/home/declan/Projects/FootballData/fuzzy/Data/FootballData.sqlite");
            return ty.Table<Bet365Row>().ToArray();
        }
    }
}
