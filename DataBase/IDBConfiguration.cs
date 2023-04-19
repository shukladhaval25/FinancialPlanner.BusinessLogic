using System.Data;

namespace FinancialPlanner.BusinessLogic.DataBase
{
    public interface IDBConfiguration
    {
        void SetConfiguration(string configuration);
        string GetConfiguration();
        void Open();
        void Open(string connectionString);
        void Close();
        void BeginTransaction();
        void CommitTransaction();
        void RollBackTransaction();
        object GetTransaction();
        object GetConnection();
        object GetCommand();
        void ExecuteCommand(string sqlQuery);
        void ExecuteCommand(string sqlQuery, bool isTrancation);
        DataTable ExecuteDataAdaptor(string sqlQuery);
        string ExecuteCommandScalar(string sqlQuery, bool isTransaction = false);

    }
}
