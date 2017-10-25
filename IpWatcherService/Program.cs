using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace IpWatcherService {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main () {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };


            if (Environment.UserInteractive) {
                #if DEBUG
                (new Logger()).Start();
                Thread.Sleep(Timeout.Infinite);
                #else
                MessageBox.Show("Приложение должно быть установлено в виде службы Windows и не может быть запущено интерактивно.");
                #endif
            }
            else {
                ServiceBase.Run(ServicesToRun);
            }




            
        }
    }
}
