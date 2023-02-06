using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using FinancialPlanner.Common;

namespace FinancialPlanner.BusinessLogic.DataBase
{
    public class SQLConfiguration : IDBConfiguration
    {
        private string _connectionString;
        private SqlConnection _sqlConnection;
        private SqlCommand _sqlCommand;
        private SqlTransaction _sqlTransaction;
        private SqlDataAdapter _sqlDataAdapter;
        private readonly string INVALID_CONNECTION_STRING = "Invalid connection string for Database connection.";


        public SQLConfiguration(string connectionString)
        {
            _sqlConnection = new SqlConnection();
            _sqlCommand = new SqlCommand();
            _connectionString = connectionString;
        }
        public string GetConfiguration()
        {
            _connectionString = getConnectionString();
            return _connectionString;
        }

        private string getConnectionString()
        {
            return _connectionString;
            //return @"Data Source=SERVER-ASCENT;Initial Catalog=FinancialPlanner;Integrated Security=SSPI;Connection Timeout=60";
            //return @"Data Source=AABRD0046\SQLEXPRESS;Initial Catalog=INDUSTRY;Integrated Security=SSPI;Connection Timeout=60";
            //return System.Configuration.ConfigurationSettings.AppSettings.Get("connectionString");
        }

        public void SetConfiguration(string configuration)
        {
            _connectionString = configuration;
        }

        public void Open(string connectionString)
        {
            _connectionString = connectionString;
            Open();
        }

        public void Open()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    Logger.LogInfo(INVALID_CONNECTION_STRING);
                    throw new NullReferenceException(INVALID_CONNECTION_STRING);
                }

                _sqlConnection.ConnectionString = _connectionString;
                _sqlConnection.Open();
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex);
                throw ex;
            }
        }

        public void Close()
        {
            if (_sqlConnection.State == System.Data.ConnectionState.Open)
                _sqlConnection.Close();
        }

        public void BeginTransaction()
        {
            if (_sqlConnection.State != System.Data.ConnectionState.Open)
            {
                _sqlConnection.ConnectionString = getConnectionString();
                Open();
            }
            _sqlTransaction = _sqlConnection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _sqlTransaction.Commit();
            Close();
        }

        public void RollBackTransaction()
        {
            _sqlTransaction.Rollback();
            Close();
        }

        public object GetTransaction()
        {
            return _sqlTransaction;
        }

        public object GetConnection()
        {
            return _sqlConnection;
        }

        public object GetCommand()
        {
            if (_sqlConnection.State != System.Data.ConnectionState.Open)
                Open();

            _sqlCommand.Connection = _sqlConnection;
            return _sqlCommand;
        }

        public void ExecuteCommand(string sqlQuery)
        {
            if (_sqlConnection.State != System.Data.ConnectionState.Open)
            {
                _sqlConnection.ConnectionString = getConnectionString();
                Open();
            }

            _sqlCommand.Connection = _sqlConnection;
            _sqlCommand.CommandText = sqlQuery;
            _sqlCommand.ExecuteNonQuery();
            Close();
        }

        public void ExecuteCommand(string sqlQuery, bool isTransaction)
        {
            if (_sqlConnection.State != System.Data.ConnectionState.Open)
            {
                _sqlConnection.ConnectionString = getConnectionString();
                _sqlConnection.Open();
            }

            _sqlCommand.Connection = _sqlConnection;
            _sqlCommand.Transaction = _sqlTransaction;
            _sqlCommand.CommandText = sqlQuery;
            _sqlCommand.ExecuteNonQuery();
        }

      
        public DataTable ExecuteDataAdaptor(string sqlQuery)
        {
            DataTable dt = new DataTable();
            try
            {
                _sqlConnection = null;
                _sqlCommand = null;
                _sqlConnection = new SqlConnection();
                _sqlCommand = new SqlCommand();
                if (_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    _sqlConnection.ConnectionString = getConnectionString();
                    Open();
                }

                
                _sqlDataAdapter = new SqlDataAdapter(sqlQuery, _sqlConnection);
                _sqlDataAdapter.Fill(dt);
                Close();
            }
            catch(Exception ex)
            {
                Logger.LogDebug(ex);
                throw ex;
            }
            return dt;
        }

        public string ExecuteCommandScalar(string sqlQuery)
        {
            if (_sqlConnection.State != System.Data.ConnectionState.Open)
            {
                _sqlConnection.ConnectionString = getConnectionString();
                Open();
            }

            string value;
            _sqlCommand.CommandText = sqlQuery;
            _sqlCommand.Connection = _sqlConnection;
            value = (_sqlCommand.ExecuteScalar() == null) ? string.Empty : _sqlCommand.ExecuteScalar().ToString();
            Close();
            return value;
        }
    }
}
