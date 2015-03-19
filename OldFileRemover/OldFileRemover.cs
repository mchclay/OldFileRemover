using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace OldFileRemover
{
    public partial class OldFileRemover : ServiceBase
    {
        System.Timers.Timer Timer;
        public string DirectoryToMonitor { get; set; }
        public int DaysToKeep { get; set; }
        public string[] ListOfFiles { get; set; }
        public string ErrorLogLocation { get; set; }
        public string DeletedFilesLogLocation { get; set; }
        public double TimerInterval { get; set; }
        public string ActivityLogLocation { get; set; }

        public OldFileRemover()
        {
            //Set up the timer
            GetTimerInterval();
            Timer = new System.Timers.Timer(this.TimerInterval);
            Timer.Enabled = true;
            Timer.Elapsed += new System.Timers.ElapsedEventHandler(MyHandler);

            InitializeComponent();
        }

        private void MyHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                GetErrorLogLocation();
                GetActivityLocation();
                GetDeletedFilesLogLocation();
                GetDirectoryToMonitor();
                GetDaysToKeep();
                GetFilesInDirectoryToMonitor();
                DeleteFilesOlderThanDaysToKeep();
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter(this.ErrorLogLocation, true))
                {
                    writer.WriteLine("{0} : {1}", System.DateTime.Now, ex.Message.ToString());
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            //TODO Log Service start
            Timer.Start();
        }

        protected override void OnStop()
        {
            //TODO Log Service Stop
            Timer.Stop();
        }

        private void GetDirectoryToMonitor()
        {
           this.DirectoryToMonitor = ConfigurationManager.AppSettings["DirectoryToMonitor"];
        }

        private void GetErrorLogLocation()
        {
            this.ErrorLogLocation = ConfigurationManager.AppSettings["ErrorLogLocation"]; 
        }

        private void GetDaysToKeep()
        {
            this.DaysToKeep = Convert.ToInt32(ConfigurationManager.AppSettings["DaysToKeep"]);
        }

        private void GetFilesInDirectoryToMonitor()
        {
            this.ListOfFiles = Directory.GetFiles(this.DirectoryToMonitor);
        }

        private void GetDeletedFilesLogLocation()
        {
            this.DeletedFilesLogLocation = ConfigurationManager.AppSettings["DeletedFilesLogLocation"];
        }

        private void GetTimerInterval()
        {
            this.TimerInterval = Convert.ToDouble(ConfigurationManager.AppSettings["TimerInterval"]);
        }

        private void GetActivityLocation()
        {
            this.ActivityLogLocation = ConfigurationManager.AppSettings["ServiceLogLocation"];
        }

        private void DeleteFilesOlderThanDaysToKeep()
        {
            foreach (string file in this.ListOfFiles)
            {
                FileInfo info = new FileInfo(file);

                int daysold = ((System.DateTime.Now.Subtract(info.CreationTime).Days));

                if (daysold > this.DaysToKeep)
                {
                    using (StreamWriter sr = new StreamWriter(this.DeletedFilesLogLocation, true))
                    {
                        sr.WriteLine("{0} : {1}", System.DateTime.Now, info.FullName);
                        sr.Flush();
                    }

                    info.Delete();
                }
            }
        }
    }
}
