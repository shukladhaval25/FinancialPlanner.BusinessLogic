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
    class SwitchTransaactionServiceImpl : ITransactionTypeService
    {
        private const string INSERT_SWITCH = "INSERT INTO SWITCH VALUES ({0},{1},{2},'{3}',{4},'{5}',{6}," +
           "'{7}',{8},'{9}',{10},'{11}','{12}','{13}')";

        private const string UPDATE_SWITCH = "UPDATE SWITCH SET ARN = {0}," +
            "CID = {1},MEMBERNAME ='{2}', AMC ={3},FOLIONUMBER ='{4}'," +
            "FROMSCHEMEID = {5},[FROMOPTION] = '{6}',TOSCHEMEID = {7}, TOOPTION = '{8}', " + 
            "AMOUNT = {9},TRANSACTIONDATE = '{10}',MODEOFEXECUTION ='{11}'," +
            "REMARK = '{12}' WHERE TASKID = {13}";

        private const string SELECT_BY_ID = "SELECT * FROM SWITCH WHERE TASKID ={0}";
        SwitchOpt  switchOpt;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Switch transaction process start");
                SwitchOpt switchOpt = new SwitchOpt();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    switchOpt = convertToSwitch(dr);
                }
                Logger.LogInfo("Get: Fresh purchase transaction process completed.");
                return switchOpt;
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

        private SwitchOpt convertToSwitch(DataRow dr)
        {
            SwitchOpt switchOpt = new SwitchOpt();
            switchOpt.Id = dr.Field<int>("ID");
            switchOpt.TaskId = dr.Field<int>("TaskId");
            switchOpt.Arn = dr.Field<int>("ARN");
            switchOpt.Cid = dr.Field<int>("CID");
            switchOpt.MemberName = dr.Field<string>("MemberName");
            switchOpt.Amc = dr.Field<int>("AMC");
            switchOpt.FolioNumber = dr.Field<string>("FolioNumber");
            switchOpt.Scheme = dr.Field<int>("ToSchemeId");
            switchOpt.Options = dr.Field<string>("ToOption");
            switchOpt.FromSchemeId = dr.Field<int>("FromSchemeId");
            switchOpt.FromOptions = dr.Field<string>("FromOption");
            switchOpt.Amount = dr.Field<long>("Amount");
            switchOpt.TransactionDate = dr.Field<DateTime>("TransactionDate");
            switchOpt.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            switchOpt.Remark = dr.Field<string>("Remark");
            return switchOpt;
        }

        public void SaveTransaction(TaskCard taskCard, int id)
        {
            switchOpt = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SwitchOpt>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SWITCH,
                   id,
                   switchOpt.Arn,
                   switchOpt.Cid,
                   switchOpt.MemberName,
                   switchOpt.Amc,
                   switchOpt.FolioNumber,
                   switchOpt.Scheme,
                   switchOpt.Options,
                   switchOpt.FromSchemeId,
                   switchOpt.FromOptions,
                   switchOpt.Amount,
                   switchOpt.TransactionDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   switchOpt.ModeOfExecution,
                   switchOpt.Remark), true);
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            switchOpt = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SwitchOpt>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SWITCH,
                   switchOpt.Arn,
                   switchOpt.Cid,
                   switchOpt.MemberName,
                   switchOpt.Amc,
                   switchOpt.FolioNumber,
                   switchOpt.Scheme,
                   switchOpt.Options,
                   switchOpt.FromSchemeId,
                   switchOpt.FromOptions,
                   switchOpt.Amount,
                   switchOpt.TransactionDate.ToString("yyyy-MM-dd hh:mm:ss"),
                   switchOpt.ModeOfExecution,
                   switchOpt.Remark,
                   taskCard.Id), true);
        }
    }
}
