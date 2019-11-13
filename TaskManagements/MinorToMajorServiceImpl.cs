using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    public class MinorToMajorServiceImpl : ITransactionTypeService
    {
        private const string INSERT_QUERY = "INSERT INTO[dbo].[MinorToMajor] " +
            "([TaskId],[ARN],[CID] ,[MinorName],[Guardian],[AMC],[FolioNumber], " +
            "[ModeOfExecution]) VALUES " +
            "({0},{1},{2},'{3}','{4}',{5},'{6}','{7}')";

        private const string UPDATE_QUERY = "UPDATE[dbo].[MinorToMajor] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MinorName] = '{3}' " +
            ",[Guardian] = '{4}', [AMC] = {5} ,[FolioNumber] = '{6}'," +
            "[ModeOfExecution] = '{7}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM MinorToMajor WHERE TASKID ={0}";
        MinorToMajor minorToMajor;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Minor to Major change request transaction process start");
                minorToMajor = new MinorToMajor();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    minorToMajor = converToMinorToMajor(dr);
                }
                Logger.LogInfo("Get: Minor to Major change request transaction process completed.");
                return minorToMajor;
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
        public void SaveTransaction(TaskCard taskCard, int id)
        {
            minorToMajor = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<MinorToMajor>(taskCard.TaskTransactionType.ToString());
            minorToMajor.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                   minorToMajor.TaskId,
                   minorToMajor.Arn,
                   minorToMajor.Cid,
                   minorToMajor.MinorName,
                   minorToMajor.Guardian,
                   minorToMajor.Amc,
                   minorToMajor.FolioNumber,
                   minorToMajor.ModeOfExecution), true);
        }
        private MinorToMajor converToMinorToMajor(DataRow dr)
        {
            MinorToMajor minorToMajor = new MinorToMajor();
            minorToMajor.Id = dr.Field<int>("ID");
            minorToMajor.TaskId = dr.Field<int>("TaskId");
            minorToMajor.Cid  = dr.Field<int>("CID");
            minorToMajor.Arn = dr.Field<int>("ARN");
            minorToMajor.MinorName = dr.Field<string>("MinorName");
            minorToMajor.Guardian = dr.Field<string>("Guardian");
            minorToMajor.Amc = dr.Field<int>("AMC");
            minorToMajor.FolioNumber = dr.Field<string>("FolioNumber");            
            minorToMajor.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return minorToMajor;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            minorToMajor = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<MinorToMajor>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   taskCard.Id,
                   minorToMajor.Arn,
                   minorToMajor.Cid,
                   minorToMajor.MinorName,
                   minorToMajor.Guardian,
                   minorToMajor.Amc,
                   minorToMajor.FolioNumber,                  
                   minorToMajor.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
