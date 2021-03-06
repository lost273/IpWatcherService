﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
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
        // get a full path for service process !!!
        static string fullPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
        string configurationFile = fullPath + "\\config.txt";
        string logFile = fullPath + "\\templog.txt";
        string keyForEncryption = "!*eRcR12D";
        string[,] configurationValues = { 
            { "OLD_IP=", "SENDER_ADDRESS=", "SENDER_NAME=", "SENDER_SMTP=", "SENDER_LOGIN=", "SENDER_PASS=", "SENDER_PORT=", "RECIPIENTS=","DISPATCH_TIMER=","END"},
            {"","","","","","","","","","" }
        };
        public List<string> recipientsList = new List<string>();
        public string CurrentIp { get; set; }
        public string OldIp { get; set; }
        public string SenderAddress { get; set; }
        public string SenderName { get; set; }
        public string SenderSmtp { get; set; }
        public string SenderLogin { get; set; }
        public string SenderPass { get; set; }
        public string SenderPort { get; set; }
        public string DispatchTimer { get; set; }
        public void Start () {
            // if file not exist, create them, return false
            enabled = ReadConfigurationValues();
            while (enabled) {
                CurrentIp = this.GetIp();
                //Notification at a specific time
                if ((DateTime.Now.ToShortTimeString() == DispatchTimer) && (ReadConfigurationValues())) {
                    Notification();
                    MakeLog("Morning dispatch");
                }
                //Notification if Ip has changed and not got the error message and read all correct values from the file
                if ((CurrentIp != "error") && (CurrentIp != OldIp)){
                    string mailError = "";
                    if (ReadConfigurationValues()) {
                        Notification();
                    }
                    else mailError = ",but not sent to email - errors in configuration file";

                    OldIp = CurrentIp;
                    ChangeValueInFile(configurationValues[0, 0], CurrentIp);
                    MakeLog("Ip has changed" + mailError);
                }
                Thread.Sleep(60000);
            }
        }
        public void Stop () {
            enabled = false;
        }
        // method receive an external IP
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
        // method receive a values from configuration file
        public bool ReadConfigurationValues () {
            if ((new FileInfo(configurationFile)).Exists) {
                using (StreamReader sr = new StreamReader(configurationFile)) {
                    while (sr.Peek() >= 0) {
                        if(ValuesChoicer(sr.ReadLine()) == false) return false;
                    }
                }
                // check a values to empty_string and fill a properties
                for (int i = 0; configurationValues[0, i] != "END"; i++) {
                    // if value is empty
                    if (configurationValues[1, i] == "") {
                        return false;
                    }
                }
                OldIp = configurationValues[1, 0];
                SenderAddress = configurationValues[1, 1];
                SenderName = configurationValues[1, 2];
                SenderSmtp = configurationValues[1, 3];
                SenderLogin = configurationValues[1, 4];
                // implement Encryption Algorithm for password
                if (configurationValues[1, 5].StartsWith("#ENC#")) {
                    SenderPass = DecryptTextFrom3DES(configurationValues[1, 5].Replace("#ENC#", ""), keyForEncryption);
                }
                else {
                    SenderPass = configurationValues[1, 5];
                    configurationValues[1, 5] = EncryptTextTo3DES(configurationValues[1, 5], keyForEncryption);
                    ChangeValueInFile(configurationValues[0, 5], "#ENC#" + configurationValues[1, 5]);
                }
                SenderPort = configurationValues[1, 6];
                recipientsList.Clear();
                string[] emails = configurationValues[1, 7].Split(new char[] { ',' });
                foreach (string s in emails) {
                    recipientsList.Add(s);
                }
                DispatchTimer = configurationValues[1, 8];
                return true;
            }
            else {
                // create a configuration file without values but with a standard fields
                using (StreamWriter sw = new StreamWriter(configurationFile, false, System.Text.Encoding.Default)) {
                    for (int i = 0; configurationValues[0,i] != "END"; i++) {
                        sw.WriteLine(configurationValues[0,i]);
                    }
                }
                return false;
            }
        }
        // notify a user of a change IP
        public void Notification () {
            foreach (string recipient in recipientsList) {
                // set an address of sender and a name displayed in letter
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
                //set login and pass
                smtp.Credentials = new NetworkCredential(SenderLogin, SenderPass);
                smtp.EnableSsl = true;
                smtp.Send(m);
            }
        }
        // write current IP to configuration file
        public void ChangeValueInFile (string indexString, string changeString) {
            object obj = new object();
            lock (obj) {
                // read all file in an array
                string[] stringsFromFile = File.ReadAllLines(configurationFile);
                using (StreamWriter sw = new StreamWriter(configurationFile, false, System.Text.Encoding.Default)) {
                    foreach (string s in stringsFromFile) {
                        if (s.StartsWith(indexString)) {
                            sw.WriteLine($"{indexString}{changeString}");
                        }
                        else {
                            sw.WriteLine(s);
                        }
                    }
                }
            }
        }
        // register an events
        public void MakeLog (string logMessage) {
            object obj = new object();
            lock (obj) {
                using (StreamWriter writer = new StreamWriter(logFile, true)) {
                    logMessage = $"{DateTime.Now.ToString()} - {logMessage} - {CurrentIp}.";
                    writer.WriteLine(String.Format(logMessage));
                    writer.Flush();
                }
            }
        }
        // fill configurationValues file
        public bool ValuesChoicer (string valueFromFile) {
            // delete a space between words
            valueFromFile = valueFromFile.Replace(" ","");
            // fill
            for (int i = 0; configurationValues[0, i] != "END"; i++) {
                if (valueFromFile.StartsWith(configurationValues[0, i])) {
                    configurationValues[1, i] = valueFromFile.Replace(configurationValues[0, i],"");
                    // if value is empty
                    if (configurationValues[1, i] == "") {
                        return false; }
                }
            }
            return true;
        }
        //Triple Data Encryption Algorithm
        private static TripleDES Create3DES(string key) {
            MD5 md5 = new MD5CryptoServiceProvider();
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = md5.ComputeHash(Encoding.Unicode.GetBytes(key));
            des.IV = new byte[des.BlockSize / 8];
            return des;
        }
        // encrypt
        public static string EncryptTextTo3DES(string plainText, string key) {
            TripleDES des = Create3DES(key);
            ICryptoTransform ct = des.CreateEncryptor();
            byte[] input = Encoding.Unicode.GetBytes(plainText);
            byte[] resArr = ct.TransformFinalBlock(input, 0, input.Length);
            string result = Convert.ToBase64String(resArr);
            return result;
        }

        // decrypt
        public static string DecryptTextFrom3DES(string cypherText, string key) {
            byte[] b = Convert.FromBase64String(cypherText);
            TripleDES des = Create3DES(key);
            ICryptoTransform ct = des.CreateDecryptor();
            byte[] output = ct.TransformFinalBlock(b, 0, b.Length);
            return Encoding.Unicode.GetString(output);
        }
    }
}