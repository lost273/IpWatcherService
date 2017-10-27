﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
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
        string configurationFile ="config.txt";
        string logFile = "templog.txt";
        string[,] configurationValues = { 
            { "OLD_IP =", "SENDER_ADDRESS =", "SENDER_NAME =", "SENDER_SMTP =", "SENDER_LOGIN =", "SENDER_PASS =", "SENDER_PORT =", "RECIPIENT =","END"},
        };
        public List<string> recipientsList;
        public string CurrentIp { get; set; }
        public string OldIp { get; set; }
        public string SenderAddress { get; set; }
        public string SenderName { get; set; }
        public string SenderSmtp { get; set; }
        public string SenderLogin { get; set; }
        public string SenderPass { get; set; }
        public string SenderPort { get; set; }
        public void Start () {
            // if file not exist, create them, return false
            enabled = ReadConfigurationValues();
            while (enabled) {
                CurrentIp = this.GetIp();
                //Notification every morning in 08:00
                if ((DateTime.Now.ToShortTimeString() == "8:00") && (ReadConfigurationValues())) { Notification(); }
                //Notification if Ip was change and not got the error message and read all correct values from the file
                if ((CurrentIp != OldIp) && (CurrentIp != "error") && (ReadConfigurationValues())){
                    //Notification();
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
            StreamReader reader;
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpWebResponse;
            Mutex getip = new Mutex();

            getip.WaitOne();
            try {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://checkip.dyndns.org");
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                reader = new StreamReader(httpWebResponse.GetResponseStream());
                getip.ReleaseMutex();
                return System.Text.RegularExpressions.Regex.Match(reader.ReadToEnd(), @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})").Groups[1].Value;
            }
            catch {
                getip.ReleaseMutex();
                return "error";
            }
        }
        // method receive values from configuration file
        // check the values to null/empty_string
        public bool ReadConfigurationValues () {
            if ((new FileInfo(configurationFile)).Exists) {
                using (StreamReader sr = new StreamReader(configurationFile)) {
                    while (sr.Peek() >= 0) {
                        if(ValuesChoicer(sr.ReadLine()) == false) return false;
                    }
                }
                return true;
            }
            else {
                // create a configuration file without values but with a standards fields
                using (StreamWriter sw = new StreamWriter(configurationFile, false, System.Text.Encoding.Default)) {
                    for (int i = 0; configurationValues[0,i] != "END"; i++) {
                        sw.WriteLine(configurationValues[0,i]);
                    }
                }
                return false;
            }
        }
        // notify the user of a change IP
        public void Notification () {
            foreach (string recipient in recipientsList) {
                // set the address of the sender and the name displayed in the letter
                MailAddress from = new MailAddress(SenderAddress, SenderName);
                // set the address of the recipient
                MailAddress to = new MailAddress(recipient);
                // create object of the message
                MailMessage m = new MailMessage(from, to);
                // subject of the mail
                m.Subject = "Изменение IP для доступа к рабочему серверу";
                // content of the mail
                m.Body = "<font size = \"3\" color = \"red\" face = \"Tahoma\"> Уважаемые пользователи, изменились коды доступа к серверам: </font>"
                       + "<br><font size = \"3\" color = \"green\" face = \"Tahoma\"> <u>MegaServer</u>  </font>" + CurrentIp + ":50005"
                       + "<br><font size = \"3\" color = \"green\" face = \"Tahoma\"> <u>Server</u>  </font>" + CurrentIp + ":50006";
                // set html code in mail
                m.IsBodyHtml = true;
                // set smtp-server's address and port
                SmtpClient smtp = new SmtpClient(SenderSmtp, Convert.ToInt32(SenderPort, 10));
                // логин и пароль
                //
                smtp.Credentials = new NetworkCredential(SenderLogin, SenderPass);
                smtp.EnableSsl = true;
                smtp.Send(m);
            }
        }
        // write current IP to configuration file
        public void WriteIpToFile () {

        }
        // register events
        public void MakeLog () {
            using (StreamWriter writer = new StreamWriter(logFile, true)) {
                writer.WriteLine(String.Format(DateTime.Now.ToString()));
                writer.Flush();
            }
        }
        public bool ValuesChoicer (string valueFromFile) {

            return false;
        }
    }
}