using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using log4net;
using log4net.Config;
using TestService.Models;

namespace TestService
{
    public partial class Service1 : ServiceBase
    {
        private Exchange exchange;

        public Service1()
        {

            InitializeComponent();
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            exchange = new Exchange();
            Thread loggerThread = new Thread(exchange.Start);
            loggerThread.Start();
        }

        protected override void OnStop()
        {

            exchange.Stop();
            Thread.Sleep(1000);
        }

    }
}

