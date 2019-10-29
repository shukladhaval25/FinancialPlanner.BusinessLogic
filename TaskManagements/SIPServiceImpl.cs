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
    public class SIPServiceImpl : ITransactionTypeService
    {
        private const string INSERT_SIP = "INSERT INTO SIP VALUES ({0},{1},'{2}','{3}','{4}','{5}','{6}'," +
            "{7},'{8}',{9},'{10}',{11},'{12}',{13},'{14}','{15}','{16}','{17}','{18}')";

        private const string UPDATE_SIP = "UPDATE SIP SET AccountType = '{0}'," +
            "CID = {1},MEMBERNAME ='{2}',SECONDHOLDER = '{3}',THIRDHOLDER ='{4}'," +
            "NOMINEE = '{5}',GUARDIAN = '{6}',AMC ={7},FOLIONUMBER ='{8}'," +
            "SCHEMEID = {9},[OPTION] = '{10}',AMOUNT = {11},TRANSACTIONDATE = '{12}',SIPDate ={13}," +
            "SIPSTARTDATE = '{14}',SIPENDDATE = '{15}',MODEOFEXECUTION = '{16}',"+
            "REMARK = '{17}' WHERE TASKID = {18}";

        private const string SELECT_BY_ID = "SELECT * FROM SIP WHERE TASKID ={0}";
        SIP sip;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: SIP transaction process start");
                sip = new SIP();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    sip = converToSIP(dr);
                }
                Logger.LogInfo("Get: SIP transaction process completed.");
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
        public void SaveTransaction(TaskCard taskCard, int id)
        {
            sip = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SIP>(taskCard.TaskTransactionType.ToString());
            sip.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SIP,
                   sip.TaskId,
                   sip.CID,
                   sip.MemberName,
                   sip.SecondHolder,
                   sip.ThirdHolder,
                   sip.Nominee,
                   sip.Guardian,
                   sip.AMC,
                   sip.FolioNo,
                   sip.SchemeId,
                   sip.Option,
                   sip.Amount,
                   sip.AccounType,
                   sip.SIPDayOn,
                   sip.TransactionDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   sip.SIPStartDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   sip.SIPEndDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   sip.ModeOfExecution,
                   sip.Remark), true);
        }
        private SIP converToSIP(DataRow dr)
        {
            SIP sip = new SIP();
            sip.Id = dr.Field<int>("ID");
            sip.TaskId = dr.Field<int>("TaskId");
            sip.CID = dr.Field<int>("CID");
            sip.MemberName = dr.Field<string>("MemberName");
            sip.SecondHolder = dr.Field<string>("SecondHolder");
            sip.ThirdHolder = dr.Field<string>("ThirdHolder");
            sip.Nominee = dr.Field<string>("Nominee");
            sip.Guardian = dr.Field<string>("Guardian");
            sip.AMC = dr.Field<int>("AMC");
            sip.FolioNo = dr.Field<string>("FolioNumber");
            sip.SchemeId = dr.Field<int>("SchemeId");
            sip.Option = dr.Field<string>("Option");
            sip.Amount = dr.Field<long>("Amount");
            sip.AccounType = dr.Field<string>("AccountType");
            sip.SIPDayOn = dr.Field<int>("SIPDate");
            sip.TransactionDate = dr.Field<DateTime>("TransactionDate");
            sip.SIPStartDate = dr.Field<DateTime>("SIPStartDate");
            sip.SIPEndDate = dr.Field<DateTime>("SIPEndDate");
            sip.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            sip.Remark = dr.Field<string>("Remark");
            return sip;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            sip = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SIP>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SIP,
                   sip.AccounType,
                   sip.CID,
                   sip.MemberName,
                   sip.SecondHolder,
                   sip.ThirdHolder,
                   sip.Nominee,
                   sip.Guardian,
                   sip.AMC,
                   sip.FolioNo,
                   sip.SchemeId,
                   sip.Option,
                   sip.Amount,
                   sip.TransactionDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   sip.SIPDayOn,                   
                   sip.SIPStartDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   sip.SIPEndDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   sip.ModeOfExecution,
                   sip.Remark,
                   taskCard.Id), true);
        }
    }
}
