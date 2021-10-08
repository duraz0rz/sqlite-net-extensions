using System;
using SQLite;
using SQLitePCL;

namespace SQLiteNetExtensions.IntegrationTests
{
    public static class Utils
    {
        public static SQLiteConnection CreateConnection()
        {
            Batteries_V2.Init();
	        var con = new SQLiteConnection(":memory:");
	        raw.sqlite3_trace(con.Handle, Log, null);
			return con;
        }

        private static void Log(object userData, string statement)
        {
            Console.WriteLine(statement);
        }

        public static SQLiteAsyncConnection CreateAsyncConnection()
        {
            Batteries_V2.Init();
            return new(":memory:");
        }
    }
}