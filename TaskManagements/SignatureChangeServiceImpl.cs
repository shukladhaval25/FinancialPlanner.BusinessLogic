using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    public class SignatureChangeServiceImpl : ITransactionTypeService
    {
        private const string INSERT_QUERY = "INSERT INTO[dbo].[SignatureChange] " +
           "([TaskId],[ARN],[CID] ,[MemberName],[AMC],[FolioNumber],[signaturechangeof], " +
           "[ModeOfExecution]) VALUES " +
           "({0},{1},{2},'{3}',{4},'{5}','{6}','{7}')";

        private const string UPDATE_QUERY = "UPDATE[dbo].[SignatureChange] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MemberName] = '{3}' " +
            ",[AMC] = {4} ,[FolioNumber] = '{5}', [SignatureOf] = '{6}' ," +
            "[ModeOfExecution] = '{7}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM SignatureChange WHERE TASKID ={0}";
        SignatureChange signatureChange;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: PanCard change request transaction process start");
                signatureChange = new SignatureChange();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    signatureChange = converToBankchangeRequest(dr);
                }
                Logger.LogInfo("Get: Pancard change request transaction process completed.");
                return signatureChange;
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
            signatureChange = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SignatureChange>(taskCard.TaskTransactionType.ToString());
            signatureChange.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                   signatureChange.TaskId,
                   signatureChange.Arn,
                   signatureChange.Cid,
                   signatureChange.MemberName,
                   signatureChange.Amc,
                   signatureChange.FolioNumber,                   
                   signatureChange.SignatureChangeOf,                   
                   signatureChange.ModeOfExecution), true);
        }
        private SignatureChange converToBankchangeRequest(DataRow dr)
        {
            SignatureChange contactUpdate = new SignatureChange();
            contactUpdate.Id = dr.Field<int>("ID");
            contactUpdate.TaskId = dr.Field<int>("TaskId");
            contactUpdate.Cid = dr.Field<int>("CID");
            contactUpdate.Arn = dr.Field<int>("ARN");
            contactUpdate.MemberName = dr.Field<string>("MemberName");
            contactUpdate.Amc = dr.Field<int>("AMC");
            contactUpdate.FolioNumber = dr.Field<string>("FolioNumber");            
            contactUpdate.SignatureChangeOf = dr.Field<string>("SignatureChangeOf");            
            contactUpdate.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return contactUpdate;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            signatureChange = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SignatureChange>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   taskCard.Id,
                   signatureChange.Arn,
                   signatureChange.Cid,
                   signatureChange.MemberName,
                   signatureChange.Amc,
                   signatureChange.FolioNumber,                   
                   signatureChange.SignatureChangeOf,                   
                   signatureChange.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
