using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Data;
using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace TestService
{
    public partial class Service1 : ServiceBase
    {
        Exchange exchange;
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
        class Exchange
        {
            string ApiKey = "B20Y1R1P96ROXFGT";
            string from = "USD";
            string to = "RUB";

            //private SqlConnection connection;
            //private SqlCommand command;

            private static object LockObj = new object();
            bool enabled = true;
            public Exchange()
            {

            }


            public void Start()
            {

                while (enabled)
                {
                    GetExRate(ApiKey);
                    Thread.Sleep(10000);
                }
            }
            public void Stop()
            {

                enabled = false;
            }

            private void GetExRate(string apiKey)
            {
                using (var connection = new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]))
                {

                    var command = new SqlCommand
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandTimeout = 10000
                    };


                    command.CommandText = "SELECT TOP 1 FromCurr , ToCurr FROM CurrencyExchange ORDER BY Id DESC";
                    connection.Open();
                    SqlDataReader comReader = command.ExecuteReader();
                    comReader.Read();
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                        "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=" +
                        comReader[0] +
                        "&to_currency=" + comReader[1] + "&apikey=" + apiKey);

                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        RecordEntry(reader.ReadToEnd());
                    }

                }
            }

            private void RecordEntry(string info)
            {
                lock (LockObj)
                {
                    using (StreamWriter writer = new StreamWriter("D:\\templog.txt", true))
                    {
                        var objects = JObject.Parse(info);
                        foreach (KeyValuePair<String, JToken> data in objects)
                        {
                            var rateValue = (String)data.Value["5. Exchange Rate"];
                            var dateValue = (String)data.Value["6. Last Refreshed"];
                            writer.WriteLine("{0} {1}  ", rateValue, dateValue);
                            //using (var connection =
                            //    new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]))
                            //{

                            //    var command = new SqlCommand
                            //    {
                            //        Connection = connection,
                            //        CommandType = CommandType.Text,
                            //        CommandTimeout = 10000
                            //    };


                            //    command.CommandText = "SELECT * FROM CurrencyExchange";
                            //    connection.Open();
                            //    SqlDataReader reader = command.ExecuteReader();
                            //    while (reader.Read())
                            //    {
                            //        writer.WriteLine("{0} ", reader[1]);
                            //    }

                            writer.Flush();
                            //  }
                        }
                    }
                }
            }
        }
    }
}

