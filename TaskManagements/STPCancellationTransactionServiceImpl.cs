using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;


namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    class STPCancellationCancellationTransactionServiceImpl : ITransactionTypeService
    {
        private const string INSERT_STPCancellation = "INSERT INTO STPCancellation VALUES ({0},{1},{2},'{3}',{4},'{5}',{6}," +
          "'{7}',{8},'{9}',{10},{11},'{12}','{13}','{14}')";

        private const string UPDATE_STPCancellation = "UPDATE STPCancellation SET ARN = {0}," +
            "CID = {1},MEMBERNAME ='{2}', AMC ={3},FOLIONUMBER ='{4}'," +
            "FROMSCHEMEID = {5},[FROMOPTION] = '{6}',TOSCHEMEID = {7}, TOOPTION = '{8}', " +
            "AMOUNT = {9},STPDATE = {10},TRANSACTIONDATE ='{11}',MODEOFEXECUTION ='{12}'," +
            "REMARK = '{13}' WHERE TASKID = {14}";

        private const string SELECT_BY_ID = "SELECT * FROM STPCancellation WHERE TASKID ={0}";
        STPCancellation stpCancellation;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: STPCancellation transaction process start");
                STPCancellation STPCancellation = new STPCancellation();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    STPCancellation = convertToSTPCancellation(dr);
                }
                Logger.LogInfo("Get: STPCancellation purchase transaction process completed.");
                return STPCancellation;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        private STPCancellation convertToSTPCancellation(DataRow dr)
        {
            STPCancellation STPCancellation = new STPCancellation();
            STPCancellation.Id = dr.Field<int>("ID");
            STPCancellation.TaskId = dr.Field<int>("TaskId");
            STPCancellation.Arn = dr.Field<int>("ARN");
            STPCancellation.Cid = dr.Field<int>("CID");
            STPCancellation.MemberName = dr.Field<string>("MemberName");
            STPCancellation.Amc = dr.Field<int>("AMC");
            STPCancellation.FolioNumber = dr.Field<string>("FolioNumber");
            STPCancellation.Scheme = dr.Field<int>("ToSchemeId");
            STPCancellation.Options = dr.Field<string>("ToOption");
            STPCancellation.FromSchemeId = dr.Field<int>("FromSchemeId");
            STPCancellation.FromOptions = dr.Field<string>("FromOption");
            STPCancellation.Amount = dr.Field<long>("Amount");
            STPCancellation.StpDate = dr.Field<int>("STPDate");
            STPCancellation.TransactionDate = dr.Field<DateTime>("TransactionDate");
            STPCancellation.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            STPCancellation.Remark = dr.Field<string>("Remark");
            return STPCancellation;
        }

        public void SaveTransaction(TaskCard taskCard, int id)
        {
            stpCancellation = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<STPCancellation>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_STPCancellation,
                   id,
                   stpCancellation.Arn,
                   stpCancellation.Cid,
                   stpCancellation.MemberName,
                   stpCancellation.Amc,
                   stpCancellation.FolioNumber,
                   stpCancellation.Scheme,
                   stpCancellation.Options,
                   stpCancellation.FromSchemeId,
                   stpCancellation.FromOptions,
                   stpCancellation.Amount,
                   stpCancellation.StpDate,
                   stpCancellation.TransactionDate,
                   stpCancellation.ModeOfExecution,
                   stpCancellation.Remark), true);
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            stpCancellation = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<STPCancellation>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_STPCancellation,
                   stpCancellation.Arn,
                   stpCancellation.Cid,
                   stpCancellation.MemberName,
                   stpCancellation.Amc,
                   stpCancellation.FolioNumber,
                   stpCancellation.Scheme,
                   stpCancellation.Options,
                   stpCancellation.FromSchemeId,
                   stpCancellation.FromOptions,
                   stpCancellation.Amount,
                   stpCancellation.StpDate,
                   stpCancellation.TransactionDate,
                   stpCancellation.ModeOfExecution,
                   stpCancellation.Remark,
                   taskCard.Id), true);
        }
    }
}
