using System;
using Database;
using System.IO;
using System.Text;

namespace Logger
{
    public interface ILogger
    {
        void Setup();
    }
    public class ManagerOfLogging : ILogger
    {

        WorkWithDatabase databaseWorker;

        private const string table = "Loggers";

        private string[] columnsTypes = { "Time varchar(50)", "Message varchar(54)" };

        public bool setuped = false;

        private string[] columnsNames = { "Time", "Message" };


        public ManagerOfLogging(WorkWithDatabase databaseWorker)
        {
            this.databaseWorker = databaseWorker;

            if (!databaseWorker.GetTableNames().Contains("dbo." + table))
            {
                databaseWorker.CreateTable(table, columnsTypes);
            }


        }

        public ManagerOfLogging()
        {
        }


        public void LoggerMessage(string message)
        {
            using (var streamWriter = new StreamWriter(
                      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logger.txt"),
                      true, Encoding.Default))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("G") + message);
            }
        }

        public void LoggerDatabaseMessage(string message)
        {
            databaseWorker.InsertValue(table, columnsNames, new string[] { DateTime.Now.ToString("y/M/d h.m.ss"), message });
        }

        public void Setup()
        {
            setuped = true;
        }
    }
}
