using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json.Linq;
using TestProjectLib.Models;
using System.Timers;
using System.IO.Pipes;
using System.Text;

namespace TestProjectLib
{
    public class Exchange
    {

        private static readonly object LockObj = new object();
        bool enabled = true;
        private readonly DatabaseHelpers workingWithDb;
        String path;
        System.Timers.Timer tmrExecutor = new System.Timers.Timer();
        public static NamedPipeClientStream client;
     
        public Exchange()
        {
            workingWithDb = new DatabaseHelpers();
            client = new NamedPipeClientStream(".", "FileName", PipeDirection.Out, PipeOptions.Asynchronous);
        }


        public void Start()
        {
            tmrExecutor.Elapsed += new ElapsedEventHandler(NewFileWithRates);
            tmrExecutor.Interval = 120000;
            tmrExecutor.Enabled = true;
            tmrExecutor.Start();
        }
      
        private void NewFileWithRates(object sender, ElapsedEventArgs e)
        {
            path = String.Format("D:\\Values\\{0}_{1}_{2}_{3}_{4}.csv", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute);

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
            catch (Exception ex)
            {
                Logger.Log.Info(ex);
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
                    using (TextWriter writer = new StreamWriter(path, true))
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
