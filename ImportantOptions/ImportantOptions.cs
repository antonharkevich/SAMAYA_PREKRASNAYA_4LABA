using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Options
{

    public class DirectoryOptions
    {
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
    }

    public class ConnectionOptions
    {
        public string TypeOfDatabase { get; set; }
        public string ConnectionStrings { get; set; }

        public string DataDirectory { get; set; }
    }


    public class ArchivationOptions
    {
        public string ZipName { get; set; }
    }


    public class CryptOptions
    {
        public string Key { get; set; }
    }
    public class ImportantOptions
    {
        public DirectoryOptions DirectoryOptions { get; set; }
        public CryptOptions CryptOptions { get; set; }

        public ConnectionOptions ConnectionOptions { get; set; }

        public ArchivationOptions ArchivationOptions { get; set; }

    }






}
