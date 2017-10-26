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
        public string OldIp { get; set; }
        public string SenderAddress { get; set; }
        public string SenderName { get; set; }
        public string SenderSmtp { get; set; }
        public string SenderLogin { get; set; }
        public string SenderPass { get; set; }
        public string SenderPort { get; set; }
        public Watcher () {
            CurrentIp = this.GetIp();
            // if file not exist - return false
            enabled = this.ReadConfigurationValues();
        }

        public void Start () {
            while (enabled) {
                CurrentIp = this.GetIp();
                if (CurrentIp != OldIp) {
                    Notification();
                    OldIp = CurrentIp;
                    WriteIpToFile();
                    MakeLog();
                }
                Thread.Sleep(6000);
            }
        }
        public void Stop () {
            enabled = false;
        }
        // method receive external IP
        public string GetIp () {
            return "";
        }
        // method receive values from configuration file
        // check the values to null
        public bool ReadConfigurationValues () {
            return true;
        }
        // notify the user of a change IP
        public void Notification () {
            enabled = this.ReadConfigurationValues();
        }
        // write current IP to configuration file
        public void WriteIpToFile () {

        }
        // register events
        public void MakeLog () {
            using (StreamWriter writer = new StreamWriter("templog.txt", true)) {
                writer.WriteLine(String.Format("test"));
                writer.Flush();
            }
        }
    }
}