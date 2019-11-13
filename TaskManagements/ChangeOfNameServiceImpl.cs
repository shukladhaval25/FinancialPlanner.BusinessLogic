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
    public class ChangeOfNameServiceImpl : ITransactionTypeService
    {
        private const string INSERT_QUERY = "INSERT INTO[dbo].[ChangeOfName] " +
            "([TaskId],[ARN],[CID] ,[FromName],[ToName],[AMC],[FolioNumber], " +
            "[ModeOfExecution],[IsSignatureChange]" +
            ") VALUES " +
            "({0},{1},{2},'{3}','{4}',{5},'{6}','{7}','{8}')";

        private const string UPDATE_QUERY = "UPDATE[dbo].[ChangeOfName] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [FromName] = '{3}' " +
            ",[ToName] = '{4}', [AMC] = {5} ,[FolioNumber] = '{6}'," +
            "[ModeOfExecution] = '{7}',[IsSignatureChange] ='{8}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM ChangeOfName WHERE TASKID ={0}";
        ChangeOfName changeOfName;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Minor to Major change request transaction process start");
                changeOfName = new ChangeOfName();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    changeOfName = converToChangeOfName(dr);
                }
                Logger.LogInfo("Get: Minor to Major change request transaction process completed.");
                return changeOfName;
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
            changeOfName = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<ChangeOfName>(taskCard.TaskTransactionType.ToString());
            changeOfName.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                   changeOfName.TaskId,
                   changeOfName.Arn,
                   changeOfName.Cid,
                   changeOfName.FromMemberName,
                   changeOfName.ToMemberName,
                   changeOfName.Amc,
                   changeOfName.FolioNumber,
                   changeOfName.ModeOfExecution,
                   changeOfName.IsSignatureChanged), true);
        }
        private ChangeOfName converToChangeOfName(DataRow dr)
        {
            ChangeOfName changeOfName = new ChangeOfName();
            changeOfName.Id = dr.Field<int>("ID");
            changeOfName.TaskId = dr.Field<int>("TaskId");
            changeOfName.Cid  = dr.Field<int>("CID");
            changeOfName.Arn = dr.Field<int>("ARN");
            changeOfName.FromMemberName = dr.Field<string>("FromName");
            changeOfName.ToMemberName = dr.Field<string>("ToName");
            changeOfName.Amc = dr.Field<int>("AMC");
            changeOfName.IsSignatureChanged = bool.Parse(dr["IsSignatureChange"].ToString());
            changeOfName.FolioNumber = dr.Field<string>("FolioNumber");            
            changeOfName.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return changeOfName;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            changeOfName = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<ChangeOfName>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   taskCard.Id,
                   changeOfName.Arn,
                   changeOfName.Cid,
                   changeOfName.FromMemberName,
                   changeOfName.ToMemberName,
                   changeOfName.Amc,
                   changeOfName.FolioNumber,                  
                   changeOfName.ModeOfExecution,
                   changeOfName.IsSignatureChanged), true);
        }
    }
}
