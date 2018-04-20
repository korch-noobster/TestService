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
using CsvHelper;
using log4net;
using Newtonsoft.Json.Linq;
using TestProjectLib.Models;

namespace TestProjectLib
{
    public class Exchange
    {

        private static readonly object LockObj = new object();
        bool enabled = true;
        private readonly DatabaseHelpers workingWithDb;
        public Exchange()
        {
            workingWithDb = new DatabaseHelpers();
        }


        public void Start()
        {
            var ts = new CancellationTokenSource();
            CancellationToken ct = ts.Token;
            try
            {
                while (enabled)
                {
                    Parallel.For(272, 275, new ParallelOptions { CancellationToken = ct }, GetExRate);
                    Thread.Sleep(10000);
                }

                ts.Cancel();
            }
            catch (Exception e)
            {
                Logger.Log.Info(e);
                throw;
            }
            finally
            {
                ts.Dispose();
            }
        }
        public void Stop()
        {
            enabled = false;
        }

        private void GetExRate(int id)
        {

            CurrencyModel currencies = new CurrencyModel();
            currencies = workingWithDb.GetCurrencies(id);
            string fromCurrency = workingWithDb.GetCurrencyInfo(currencies.fromCurr).Trim();
            string toCurrency = workingWithDb.GetCurrencyInfo(currencies.toCurr).Trim();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=" + fromCurrency +
                "&to_currency=" + toCurrency + "&apikey=" + ConfigurationSettings.AppSettings["APIKey"]);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                RecordEntry(reader.ReadToEnd(), currencies);
            }

        }
        private int RecordEntry(string info, CurrencyModel currencies)
        {
            lock (LockObj)
            {
                var objects = JObject.Parse(info);
                foreach (KeyValuePair<String, JToken> data in objects)
                {
                    if (data.Key != "Realtime Currency Exchange Rate")
                    {
                        return 1;
                    }
                    //CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    //ci.NumberFormat.CurrencyDecimalSeparator = ".";
                    //float exRate = float.Parse(data.Value["5. Exchange Rate"].ToString(), NumberStyles.Any, ci);
                    //DateTime lastRefreshed = DateTime.Parse(data.Value["6. Last Refreshed"].ToString());
                    // workingWithDb.SetDataToStoryTable(currencies, exRate, lastRefreshed);
                    using (TextWriter writer = new StreamWriter("D:\\output.csv", true))
                    {
                        var output = new CsvWriter(writer);
                        output.WriteField(currencies.Id);
                        output.WriteField(data.Value["5. Exchange Rate"].ToString());
                        output.WriteField(data.Value["6. Last Refreshed"].ToString());
                        output.NextRecord();
                    }
                }
            }
            return 0;
        }
    }
}
