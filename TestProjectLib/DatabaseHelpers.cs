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

        public int UpdateCurrencies(String output)
        {
            string result;
            using (var connectionToTestDb = new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]))
            {
                result= connectionToTestDb
                   .Query<String>("Exec AddCurrencies @output", new { output }).FirstOrDefault();
            }
            if (result==null)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
