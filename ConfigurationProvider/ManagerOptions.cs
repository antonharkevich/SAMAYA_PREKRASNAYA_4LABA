using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Logger;
using System.Threading.Tasks;

namespace ConfigurationProvider
{
    public class ManagerOptions
    {

        private readonly string path = null;
        private ManagerOfLogging managerOfLogging;
        public ManagerOptions(string path)
        {
            managerOfLogging.Setup();
            if (File.Exists(path))
            {
                this.path = (Path.GetExtension(path) == ".xml"
                    || Path.GetExtension(path) == ".json") ? path : null;
            }
            else if (Directory.Exists(path))
            {
                var fileEntries = from file in Directory.GetFiles(path)
                                  where
Path.GetExtension(file) == ".xml" ||
Path.GetExtension(file) == ".json"
                                  select file;

                if (fileEntries.Count() != 0)
                {
                    Random random = new Random();
                    int value = random.Next(0, 1);
                    if (value == 0)
                    {
                        this.path = fileEntries.Last();
                    }
                    else
                    {
                        this.path = fileEntries.First();
                    }
                }
            }
        }

        public T GetConfigurations<T>() where T : class
        {
            managerOfLogging = new ManagerOfLogging();
            if (path is null)
            {
                if (managerOfLogging.setuped == true)
                {
                    managerOfLogging.LoggerMessage("File of configurations not found.");
                }

                return null;
            }

            IConfigurationParser<T> configurationParser = null;
            switch (Path.GetExtension(path))
            {
                case ".xml":
                    Help.flg = true;
                    break;
                case ".json":
                    break;

            }
            configurationParser = new Parser<T>(path);
            return configurationParser.Parse();
        }
    }
}
