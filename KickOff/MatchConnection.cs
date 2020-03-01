using Scrape.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace KickOff
{
    [Description("KickOff")]
    public class MatchConnection : Scrape.Entity.DefaultMatchConnection
    {
        SQLite.SQLiteConnection connection;

        public MatchConnection() : base(DbConnection.GetConnectionString())
        {
            this.connection = new SQLite.SQLiteConnection(DbConnection.GetConnectionString());
        }

    }

    public class DbConnection
    {
        const string connectionString = "../../../Data/KickOffAI.sqlite";

        const string databaseName = "KickOffAI.sqlite";

        public static SQLite.SQLiteConnection Get()
        {

            if (!System.IO.File.Exists(connectionString))
            {
                System.IO.Directory.CreateDirectory("../../../Data");
            }

            return new SQLite.SQLiteConnection(connectionString);
        }


        public static string GetConnectionString()
        {
            string path = connectionString;

            if (!System.IO.File.Exists(connectionString))
            {
                path = @"C:\Users\rytal\Documents\Visual Studio 2019\Projects\MasterProject\Scrape\SiteProjects\Kickoff\KickOff.App\Data\KickOffAI.sqlite";
            }

            return path;
        }


    }
}
