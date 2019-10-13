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
    class STPTransactionServiceImpl : ITransactionTypeService
    {
        private const string INSERT_SWITCH = "INSERT INTO STP VALUES ({0},{1},{2},'{3}',{4},'{5}',{6}," +
          "'{7}',{8},'{9}',{10},{11},'{12}','{13}','{14}')";

        private const string UPDATE_SWITCH = "UPDATE STP SET ARN = {0}," +
            "CID = {1},MEMBERNAME ='{2}', AMC ={3},FOLIONUMBER ='{4}'," +
            "FROMSCHEMEID = {5},[FROMOPTION] = '{6}',TOSCHEMDID = {7}, TOOPTION = '{8}', " +
            "AMOUNT = {9},DURATION = {10},FREQUENCY ='{11}',MODEOFEXECUTION ='{12}'," +
            "REMARK = '{13}' WHERE TASKID = {14}";

        private const string SELECT_BY_ID = "SELECT * FROM STP WHERE TASKID ={0}";
        STP stp;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: STP transaction process start");
                STP stp = new STP();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    stp = convertToSTP(dr);
                }
                Logger.LogInfo("Get: STP purchase transaction process completed.");
                return stp;
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

        private STP convertToSTP(DataRow dr)
        {
            STP stp = new STP();
            stp.Id = dr.Field<int>("ID");
            stp.TaskId = dr.Field<int>("TaskId");
            stp.Arn = dr.Field<int>("ARN");
            stp.Cid = dr.Field<int>("CID");
            stp.MemberName = dr.Field<string>("MemberName");
            stp.Amc = dr.Field<int>("AMC");
            stp.FolioNumber = dr.Field<string>("FolioNumber");
            stp.Scheme = dr.Field<int>("ToSchemeId");
            stp.Options = dr.Field<string>("ToOption");
            stp.FromSchemeId = dr.Field<int>("FromSchemeId");
            stp.FromOptions = dr.Field<string>("FromOption");
            stp.Amount = dr.Field<long>("Amount");
            stp.Duration = dr.Field<int>("Duration");
            stp.Frequency = dr.Field<string>("Frequency");
            stp.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            stp.Remark = dr.Field<string>("Remark");
            return stp;
        }

        public void SaveTransaction(TaskCard taskCard, int id)
        {
            stp = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<STP>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SWITCH,
                   id,
                   stp.Arn,
                   stp.Cid,
                   stp.MemberName,
                   stp.Amc,
                   stp.FolioNumber,
                   stp.Scheme,
                   stp.Options,
                   stp.FromSchemeId,
                   stp.FromOptions,
                   stp.Amount,
                   stp.Duration,
                   stp.Frequency,
                   stp.ModeOfExecution,
                   stp.Remark), true);
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            stp = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<STP>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SWITCH,
                   stp.Arn,
                   stp.Cid,
                   stp.MemberName,
                   stp.Amc,
                   stp.FolioNumber,
                   stp.Scheme,
                   stp.Options,
                   stp.FromSchemeId,
                   stp.FromOptions,
                   stp.Amount,
                   stp.Duration,
                   stp.Frequency,
                   stp.ModeOfExecution,
                   stp.Remark,
                   taskCard.Id), true);
        }        
    }
}
