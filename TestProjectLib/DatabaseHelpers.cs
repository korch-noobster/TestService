using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProjectLib.Models;

namespace TestProjectLib
{
    public class DatabaseHelpers
    {
        private SqlConnection connectionToTestDb;
        private SqlCommand commandsForDb;

        public DatabaseHelpers()
        {
            connectionToTestDb = new SqlConnection(ConfigurationSettings.AppSettings["connectionString"]);
            commandsForDb = new SqlCommand
            {
                Connection = connectionToTestDb,
                CommandType = CommandType.Text,
                CommandTimeout = 10000
            };
        }

        public CurrencyModel GetCurrencyInfo()
        {
            CurrencyModel currentCurrencies = null;
            try
            {
                commandsForDb.CommandText = "SELECT FromCurr , ToCurr FROM CurrencyExchange ";
                connectionToTestDb.Open();
                SqlDataReader comReader = commandsForDb.ExecuteReader();
                comReader.Read();
                currentCurrencies = new CurrencyModel
                {
                    fromCurr = comReader[0].ToString(),
                    toCurr = comReader[1].ToString()
                };
            }
            finally
            {
                connectionToTestDb.Close();
            }
            return currentCurrencies;
        }

        public void SetDataToStoryTable(CurrencyModel currenciesToInsert, float ExRate, DateTime LastRefreshed)
        {
            try
            {
                commandsForDb = new SqlCommand
                {
                    Connection = connectionToTestDb,
                    CommandType = CommandType.Text,
                    CommandTimeout = 10000
                };
                commandsForDb.CommandText = "INSERT INTO CurrencyExchangeStory (FromCurr,ToCurr,Rate,UpdateDate) VALUES (@FromCurr,@ToCurr,@rateValue,@dateValue) ";
                commandsForDb.Parameters.Add("@rateValue", SqlDbType.Float).Value = ExRate;
                commandsForDb.Parameters.Add("@datevalue", SqlDbType.DateTime).Value = LastRefreshed;
                commandsForDb.Parameters.Add("@FromCurr", SqlDbType.NVarChar).Value = currenciesToInsert.fromCurr;
                commandsForDb.Parameters.Add("@ToCurr", SqlDbType.NVarChar).Value = currenciesToInsert.toCurr;
                connectionToTestDb.Open();
                commandsForDb.ExecuteNonQuery();
            }
            finally
            {
                connectionToTestDb.Close();
            }
        }
    }
}
