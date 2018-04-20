using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using TestProjectLib.Models;

namespace TestProjectLib
{
    public class DatabaseHelpers
    {

        //private IDbConnection connectionToTestDb;

        public CurrencyModel GetCurrencies(int id)
        {
            using (var connectionToTestDb = new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]))
            {
                CurrencyModel currentCurrencies = connectionToTestDb
                    .Query<CurrencyModel>("SELECT * FROM CurrencyExchange where id=@id ", new { id }).FirstOrDefault();
                return currentCurrencies;
            }
        }

        public string GetCurrencyInfo(int id)
        {
           
            using (var connectionToTestDb = new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]))
            {
                String currency = connectionToTestDb
                    .Query<String>("SELECT Currency_code FROM Currencies where id=@id ", new { id }).FirstOrDefault();
                return currency;
            }
        }


        //public void SetDataToStoryTable(CurrencyModel currenciesToInsert, float exRate, DateTime lastRefreshed)
        //{
          
        //    using (var connectionToTestDb = new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]))
        //    {
        //        var sqlQuery =
        //            "INSERT INTO CurrencyExchangeStory(exchange_id,Rate, UpdateDate) VALUES(@exchange_id, @rateValue, @dateValue)";
        //        connectionToTestDb.Execute(sqlQuery,
        //            new
        //            {
        //                exchange_id = currenciesToInsert.Id,
        //                rateValue = exRate,
        //                dateValue = lastRefreshed,
        //            });
        //    }
        //}
    }
}
