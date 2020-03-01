using Scrape.Entity;
using System;

namespace KickOff
{
    public class Db
    {
        const string databaseName = "KickOffAI.sqlite";
        public static SQLite.SQLiteConnection GetConnection()
        {
            var directoryInfo = System.IO.Directory.CreateDirectory("../../../Data");

            var xx = new SQLite.SQLiteConnection(System.IO.Path.Combine(directoryInfo.FullName, databaseName));

            xx.CreateTable<ScrapeFixture>();
            xx.CreateTable<ScrapeResult>();
            return xx;
        }
    }
}
