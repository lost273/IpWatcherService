using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace IpWatcherService {
    public partial class Service1 : ServiceBase {
        Watcher watcher;
        public Service1 () {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart (string[] args) {
            watcher = new Watcher();
            Thread watcherThread = new Thread(new ThreadStart(watcher.Start));
            watcherThread.Start();
        }

        protected override void OnStop () {
            watcher.Stop();
            Thread.Sleep(1000);
        }
    }

    class Watcher {
        bool enabled = true;
        FileInfo configurationFile = new FileInfo(@"config.cfg");
        public List<string> recipientsList;
        public string CurrentIp { get; set; }
        
        public Watcher () {
            CurrentIp = this.GetIp();
            /* 
            if (IpAdress in file NOT exist) {
                
                SendIpOnMail();
                WriteIpInFile();
            }
            */
        }

        public void Start () {
            while (enabled) {
                /*GetIp();
                if(CompareIp() == false) Notification();
                */
                using (StreamWriter writer = new StreamWriter("D:\\templog.txt", true)) {
                    writer.WriteLine(String.Format("test"));
                    writer.Flush();
                }
                Thread.Sleep(6000);
            }
        }
        public void Stop () {
            enabled = false;
        }
        // Method receive external IP
        public string GetIp () {
            return "";
        }
    }
}