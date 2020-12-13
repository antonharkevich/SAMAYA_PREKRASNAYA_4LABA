using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcher
{
    public partial class FileWatcher : ServiceBase
    {
        ManagerFiles fileManager;

        public FileWatcher()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            fileManager = new ManagerFiles();
            Thread loggerThread = new Thread(new ThreadStart(fileManager.Start));
            loggerThread.Start();
        }

        protected override void OnStop()
        {
            fileManager.Stop();
            Thread.Sleep(1000);
        }
    }
}
