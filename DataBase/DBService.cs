using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.DataBase
{
    public static class DBService
    {
        private static IDBConfiguration _dbConfiguration;

        public static void setDataBaseConfig(IDBConfiguration config)
        {
            _dbConfiguration = config;
        }

        public static void ExecuteCommandString(string sqlQuery)
        {
            _dbConfiguration.ExecuteCommand(sqlQuery);            
        }
        public static void ExecuteCommandString(string sqlQuery,bool isTransaction)
        {
            _dbConfiguration.ExecuteCommand(sqlQuery,isTransaction);
        }
        public static string ExecuteCommandScalar(string sqlQuery,bool isTransaction = false)
        {
           return _dbConfiguration.ExecuteCommandScalar(sqlQuery,isTransaction);
        }

        public static DataTable ExecuteCommand(string sqlQuery)
        {
            return _dbConfiguration.ExecuteDataAdaptor(sqlQuery);
        }

        public static void OpenConnection()
        {
            _dbConfiguration.Open();
        }

        public static void CloseConnection()
        {
            _dbConfiguration.Close();
        }

        public static string GetConnectionString()
        {
            return _dbConfiguration.GetConfiguration();            
        }

        public static Object GetCommand()
        {
            return  _dbConfiguration.GetCommand();
        }

        public static void BeginTransaction()
        {
            _dbConfiguration.BeginTransaction();
        }
        public static void CommitTransaction()
        {
            _dbConfiguration.CommitTransaction();
        }
        public static void RollbackTransaction()
        {
            _dbConfiguration.RollBackTransaction();
        }
    }
}
