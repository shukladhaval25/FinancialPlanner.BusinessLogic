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
    class SIPBankChangeServiceImpl : ITransactionTypeService
    {
        private const string INSERT_QUERY = "INSERT INTO SIPBankChange " +
           "([TaskId],[ARN],[CID],[MemberName],[AMC],[FolioNumber],[SchemeId],[Option],[Amount],[NewBankId]," + 
            "[NewBankAccountNo],[ModeOfExecution],[Remark]) " +
           "VALUES ({0},{1},{2},'{3}',{4},'{5}',{6},'{7}',{8},{9},'{10}','{11}','{12}')";

        private const string UPDATE_QUERY = "UPDATE [SIPBankChange] SET [TaskId] = {0} " +
            ",[ARN] = {1} " +
            ",[CID] = {2} " +
            ",[MemberName] = '{3}'" +
            ",[AMC] = {4}" +
            ",[FolioNumber] = '{5}'" +
            ",[SchemeId] = {6} " +
            ",[Option] = '{7}'" +
            ",[Amount] = {8}" +
            ",[NewBankId] = {9}" +
            ",[ModeOfExecution] = '{10}' " +
            ",[Remark] = '{11}'" +
            ",[NewBankAccountNo] ='{12}'" +
            " WHERE [TaskId] = {0}"; 

        private const string SELECT_BY_ID = "SELECT * FROM SIPBankChange WHERE TASKID ={0}";
        SIPBankChange sipBankChange;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Fresh purchase transaction process start");
                SIPBankChange sip = new SIPBankChange();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    sip = converToSIPBankChange(dr);
                }
                Logger.LogInfo("Get: Fresh purchase transaction process completed.");
                return sip;
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

        private SIPBankChange  converToSIPBankChange(DataRow dr)
        {
            SIPBankChange sipBankChange = new SIPBankChange();
            sipBankChange.Id =  dr.Field<int>("ID");
            sipBankChange.TaskId = dr.Field<int>("TaskId");
            sipBankChange.Arn = dr.Field<int>("ARN");
            sipBankChange.Cid = dr.Field<int>("CID");
            sipBankChange.MemberName = dr.Field<string>("MemberName");            
            sipBankChange.Amc = dr.Field<int>("AMC");
            sipBankChange.FolioNumber = dr.Field<string>("FolioNumber");
            sipBankChange.Scheme = dr.Field<int>("SchemeId");
            sipBankChange.Options = dr.Field<string>("Option");
            sipBankChange.Amount = dr.Field<long>("Amount");
            sipBankChange.NewBankId = dr.Field<int>("NewBankId");
            sipBankChange.NewBankAccountNo = dr.Field<string>("NewBankAccountNo");
            sipBankChange.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            sipBankChange.Remark = dr.Field<string>("Remark");
            return sipBankChange;
        }

        public void SaveTransaction(TaskCard taskCard, int id)
        {
            sipBankChange = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SIPBankChange>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                   id,
                   sipBankChange.Arn,
                   sipBankChange.Cid,
                   sipBankChange.MemberName,
                   sipBankChange.Amc,
                   sipBankChange.FolioNumber,
                   sipBankChange.Scheme,
                   sipBankChange.Options,
                   sipBankChange.Amount,
                   sipBankChange.NewBankId,
                   sipBankChange.NewBankAccountNo,
                   sipBankChange.ModeOfExecution,
                   sipBankChange.Remark), true);
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            sipBankChange = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SIPBankChange>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                    taskCard.Id,
                   sipBankChange.Arn,
                   sipBankChange.Cid,
                   sipBankChange.MemberName,                  
                   sipBankChange.Amc,
                   sipBankChange.FolioNumber,
                   sipBankChange.Scheme,
                   sipBankChange.Options,
                   sipBankChange.Amount,
                   sipBankChange.NewBankId,
                   sipBankChange.ModeOfExecution,
                   sipBankChange.Remark,
                   sipBankChange.NewBankAccountNo), true);
        }
    }
}
