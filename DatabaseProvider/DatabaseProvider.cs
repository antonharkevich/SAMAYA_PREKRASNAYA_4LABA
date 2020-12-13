using System;
using System.Collections.Generic;
using System.IO;
using Options;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class DatabaseProvider
    {
        public WorkWithDatabase databaseManager;

        private string targetPath;

        private IXMLGenerator stringGenerator;



        public DatabaseProvider(ImportantOptions config)
        {
            databaseManager = new WorkWithDatabase(config.ConnectionOptions.ConnectionStrings, config.ConnectionOptions.TypeOfDatabase);

            targetPath = config.ConnectionOptions.DataDirectory;

            stringGenerator = new XMLGenerator();
        }

        public void CreateXMLFiles()
        {
            List<string> tableNames = databaseManager.GetTableNames();

            foreach (var tableName in tableNames)
            {
                string fileText = stringGenerator.ConvertTableToXMLString(tableName,
                    databaseManager.GetTableValues(tableName));


                string name = tableName;
                int Index = name.IndexOf('.');
                name = name.Substring(Index + 1);

                string targetFile = Path.Combine(targetPath, tableName);
                targetFile = Path.ChangeExtension(targetFile, ".xml");

                using (StreamWriter sw = File.CreateText(targetFile))
                {
                    sw.WriteLine(fileText);
                }

                using (var streamWriter = new StreamWriter(
                      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logger.txt"),
                      true, Encoding.Default))
                {
                    streamWriter.WriteLine(DateTime.Now.ToString("G") + $"Create {name}.xml at {targetPath}");
                }
            }
        }


    }
}