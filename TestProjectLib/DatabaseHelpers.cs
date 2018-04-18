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
        private IDbConnection connectionToTestDb;

        public DatabaseHelpers()
        {
            connectionToTestDb = new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]);
        }

        public CurrencyModel GetCurrencyInfo()
        {
            CurrencyModel currentCurrencies = connectionToTestDb.Query<CurrencyModel>("SELECT * FROM CurrencyExchange ").FirstOrDefault();
            return currentCurrencies;
        }

        public void SetDataToStoryTable(CurrencyModel currenciesToInsert, float exRate, DateTime lastRefreshed)
        {
            var sqlQuery = "INSERT INTO CurrencyExchangeStory(FromCurr, ToCurr, Rate, UpdateDate) VALUES(@FromCurr, @ToCurr, @rateValue, @dateValue)";
            connectionToTestDb.Execute(sqlQuery,
                new {FromCurr=currenciesToInsert.fromCurr, ToCurr=currenciesToInsert.toCurr,rateValue= exRate,dateValue= lastRefreshed, });
        }
    }
}
