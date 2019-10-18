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
    class SIPCancellationTransactionServiceImpl : ITransactionTypeService
    {
        private const string INSERT_SIPCANCELLATION = "INSERT INTO SIPCancellation VALUES ({0},{1},'{2}',{3},'{4}',{5},'{6}'," +
            "{7},'{8}','{9}',{10},{11},'{12}','{13}','{14}',{15})";

        private const string UPDATE_SIPCANCELLATION = "UPDATE SIPCancellation SET CID = {0}," + 
            "MEMBERNAME ='{1}',AMC ={2},FOLIONUMBER ='{3}'," +
            "SCHEMEID = {4},[OPTION] = '{5}',AMOUNT = {6}, SIPStartDate ='{7}'," +
            "SIPEndDate ='{8}',SIPDate ={9}, BankName= {10}, AccountNo = '{11}'," +
            "MODEOFEXECUTION = '{12}'," +
            "REMARK = '{13}',ARN = {14} WHERE TASKID = {15}";

        private const string SELECT_BY_ID = "SELECT * FROM SIPCANCELLATION WHERE TASKID ={0}";
        SIPCancellation sipCancellation;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: SIP cancellation transaction process start");
                sipCancellation = new SIPCancellation();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    sipCancellation = converToSIPCancellation(dr);
                }
                Logger.LogInfo("Get: SIP cancellation transaction process completed.");
                return sipCancellation;
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
            try
            {
                sipCancellation = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SIPCancellation>(taskCard.TaskTransactionType.ToString());
                sipCancellation.TaskId = id;
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SIPCANCELLATION,
                       sipCancellation.TaskId,
                       sipCancellation.Cid,
                       sipCancellation.MemberName,
                       sipCancellation.Amc,
                       sipCancellation.FolioNumber,
                       sipCancellation.SchemeId,
                       sipCancellation.Options,
                       sipCancellation.Amount,
                       sipCancellation.SipStartDate,
                       sipCancellation.SipEndDate,
                       sipCancellation.SipDate,
                       sipCancellation.BankId,
                       sipCancellation.AccountNo,
                       sipCancellation.ModeOfExecution,
                       sipCancellation.Remark,
                       sipCancellation.Arn), true);
            }
            catch(Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        private SIPCancellation converToSIPCancellation(DataRow dr)
        {
            SIPCancellation sipCancellation = new SIPCancellation();
            sipCancellation.Id = dr.Field<int>("ID");
            sipCancellation.TaskId = dr.Field<int>("TaskId");
            sipCancellation.Cid = dr.Field<int>("CID");
            sipCancellation.MemberName = dr.Field<string>("MemberName");            
            sipCancellation.Amc = dr.Field<int>("AMC");
            sipCancellation.FolioNumber = dr.Field<string>("FolioNumber");
            sipCancellation.SchemeId = dr.Field<int>("SchemeId");
            sipCancellation.Options = dr.Field<string>("Option");
            sipCancellation.Amount = dr.Field<long>("Amount");
            sipCancellation.SipStartDate = dr.Field<DateTime>("SipStartDate");
            sipCancellation.SipEndDate = dr.Field<DateTime>("SipEndDate");
            sipCancellation.SipDate = dr.Field<int>("SipDate");
            sipCancellation.BankId = int.Parse(dr["BankName"].ToString());
            sipCancellation.AccountNo = dr.Field<String>("AccountNo");
            sipCancellation.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            sipCancellation.Remark = dr.Field<string>("Remark");
            sipCancellation.Arn = dr.Field<int>("ARN");

            return sipCancellation;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            sipCancellation = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SIPCancellation>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SIPCANCELLATION,
                   sipCancellation.Cid,
                   sipCancellation.MemberName,
                   sipCancellation.Amc,
                   sipCancellation.FolioNumber,
                   sipCancellation.SchemeId,
                   sipCancellation.Options,
                   sipCancellation.Amount,
                   sipCancellation.SipStartDate,
                   sipCancellation.SipEndDate,
                   sipCancellation.SipDate,
                   sipCancellation.BankId,
                   sipCancellation.AccountNo,
                   sipCancellation.ModeOfExecution,
                   sipCancellation.Remark,
                   sipCancellation.Arn,
                   taskCard.Id), true);
        }
    }
}
