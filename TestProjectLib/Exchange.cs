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
        private bool _enabled = true;
        private readonly DatabaseHelpers _workingWithDb;
        private string _path;
        private readonly System.Timers.Timer _tmrExecutor = new System.Timers.Timer();
        public static NamedPipeClientStream Client;

        public Exchange()
        {
            _workingWithDb = new DatabaseHelpers();
            Client = new NamedPipeClientStream(".", "FileName", PipeDirection.Out, PipeOptions.Asynchronous);
        }


        public void Start()
        {
            _tmrExecutor.Elapsed += new ElapsedEventHandler(NewFileWithRates);
            _tmrExecutor.Interval = 120000;
            _tmrExecutor.Enabled = true;
            _tmrExecutor.Start();
        }

        private void NewFileWithRates(object sender, ElapsedEventArgs e)
        {
            _path = String.Format("D:\\Values\\{0}_{1}_{2}_{3}_{4}.csv", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute);

            var ts = new CancellationTokenSource();
            var ct = ts.Token;
            try
            {
                while (_enabled)
                {
                    Parallel.For(272, 275, new ParallelOptions { CancellationToken = ct }, GetExRate);
                    Thread.Sleep(60000);
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
            _enabled = false;
        }

        private void GetExRate(int id)
        {

            var currencies = new CurrencyModel();
            currencies = _workingWithDb.GetCurrencies(id);
            var fromCurrency = _workingWithDb.GetCurrencyInfo(currencies.FromCurr).Trim();
            var toCurrency = _workingWithDb.GetCurrencyInfo(currencies.ToCurr).Trim();
            var request = (HttpWebRequest)WebRequest.Create(
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
        private void RecordEntry(string info, CurrencyModel currencies)
        {
            lock (LockObj)
            {
                
                var objects = JObject.Parse(info);
                foreach (KeyValuePair<string, JToken> data in objects)
                {
                    Logger.Log.Info(data);
                    if (data.Key != "Realtime Currency Exchange Rate")
                    {
                        return;
                    }
                    using (TextWriter writer = new StreamWriter(_path, true))
                    {
                        var output = new CsvWriter(writer);
                        output.WriteField(currencies.Id);
                        output.WriteField(data.Value["5. Exchange Rate"].ToString());
                        output.WriteField(data.Value["6. Last Refreshed"].ToString());
                        output.NextRecord();
                    }
                }
            }
        }
    }
}
