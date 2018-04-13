using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TestService.Models;

namespace TestService
{
    class Exchange
    {

        private static object LockObj = new object();
        bool enabled = true;
        private CurrencyModel currencies;
        private DatabaseHelpers workingWithDb;
        public Exchange()
        {
            workingWithDb = new DatabaseHelpers();
        }


        public void Start()
        {
            try
            {

            while (enabled)
            {
                GetExRate();
                Thread.Sleep(10000);
            }

            }
            catch (Exception e)
            { 
            using (StreamWriter writer = new StreamWriter("D:\\templog.txt", true))
            {
                writer.WriteLine(e.Message);

            }

            throw;
            }
        }
        public void Stop()
        {

            enabled = false;
        }

        private void GetExRate()
        {
            currencies = new CurrencyModel();
            currencies = workingWithDb.GetCurrencyInfo();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=" +
                currencies.fromCurr +
                "&to_currency=" + currencies.toCurr + "&apikey=" + ConfigurationSettings.AppSettings["APIKey"]);

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                RecordEntry(reader.ReadToEnd(), currencies);
            }

        }
        private void RecordEntry(string info, CurrencyModel currencies)
        {
            lock (LockObj)
            {
                using (StreamWriter writer = new StreamWriter("D:\\templog.txt", true))
                {
                    var objects = JObject.Parse(info);
                    foreach (KeyValuePair<String, JToken> data in objects)
                    {
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";

                        float exRate = float.Parse(data.Value["5. Exchange Rate"].ToString(), NumberStyles.Any, ci);
                        DateTime lastRefreshed = DateTime.Parse(data.Value["6. Last Refreshed"].ToString());
                        workingWithDb.SetDataToStoryTable(currencies, exRate, lastRefreshed);

                    }
                }
            }
        }
    }
}
