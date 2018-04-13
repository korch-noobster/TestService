using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

 
namespace Lib
{
    class Exchange
    {
        string ApiKey = "B20Y1R1P96ROXFGT";
        string from = "USD";
        string to = "RUB";

        // SqlConnection connection;
        // SqlCommand command;

        object obj = new object();
        bool enabled = true;
        public Exchange()
        {
            //     connection = new SqlConnection();
            //   connection.ConnectionString = @"Server=KOZHEMYAKON;Database=test";
        }


        public void Start()
        {

            while (enabled)
            {
                GetExRate(from, to, ApiKey);
                Thread.Sleep(10000);
            }
        }
        public void Stop()
        {

            enabled = false;
        }

        public void GetExRate(string from, string to, string ApiKey)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=" + from + "&to_currency=" + to + "&apikey=" + ApiKey);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                RecordEntry(reader.ReadToEnd());
            }
        }
        public void RecordEntry(string info)
        {
            lock (obj)
            {
                using (StreamWriter writer = new StreamWriter("D:\\templog.txt", true))
                {
                    var objects = JArray.Parse(info);
                    foreach (JObject root in objects)
                    {
                        foreach (KeyValuePair<String, JToken> data in root)
                        {
                            var value = (String)data.Value["5. Exchange Rate"];

                            writer.WriteLine(String.Format("{0} ", info));
                            writer.Flush();
                        }
                    }

                }
            }
        }
    }
}
