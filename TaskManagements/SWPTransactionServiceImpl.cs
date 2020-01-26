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
    class SWPTransactionServiceImpl : ITransactionTypeService
    {
        private const string INSERT_SWP = "INSERT INTO SWP VALUES ({0},{1},{2},'{3}',{4},'{5}',{6}," +
          "'{7}',{8},{9},'{10}','{11}','{12}')";

        private const string UPDATE_SWP = "UPDATE SWP SET ARN = {0}," +
            "CID = {1},MEMBERNAME ='{2}', AMC ={3},FOLIONUMBER ='{4}'," +
            "SCHEMEID = {5}, [OPTION] = '{6}', " +
            "AMOUNT = {7},DURATION = {8},FREQUENCY ='{9}',MODEOFEXECUTION ='{10}'," +
            "REMARK = '{11}' WHERE TASKID = {12}";

        private const string SELECT_BY_ID = "SELECT * FROM SWP WHERE TASKID ={0}";
        SWP swp;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: SWP transaction process start");
                SWP swp = new SWP();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    swp = convertToSWP(dr);
                }
                Logger.LogInfo("Get: SWP purchase transaction process completed.");
                return swp;
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

        private SWP convertToSWP(DataRow dr)
        {
            SWP SWP = new SWP();
            SWP.Id = dr.Field<int>("ID");
            SWP.TaskId = dr.Field<int>("TaskId");
            SWP.Arn = dr.Field<int>("ARN");
            SWP.Cid = dr.Field<int>("CID");
            SWP.MemberName = dr.Field<string>("MemberName");
            SWP.Amc = dr.Field<int>("AMC");
            SWP.FolioNumber = dr.Field<string>("FolioNumber");
            SWP.Scheme = dr.Field<int>("SchemeId");
            SWP.Options = dr.Field<string>("Option");           
            SWP.Amount = dr.Field<long>("Amount");
            SWP.Duration = dr.Field<int>("Duration");
            SWP.Frequency = dr.Field<string>("Frequency");
            SWP.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            SWP.Remark = dr.Field<string>("Remark");
            return SWP;
        }

        public void SaveTransaction(TaskCard taskCard, int id)
        {
            swp = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SWP>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SWP,
                   id,
                   swp.Arn,
                   swp.Cid,
                   swp.MemberName,
                   swp.Amc,
                   swp.FolioNumber,
                   swp.Scheme,
                   swp.Options,                  
                   swp.Amount,
                   swp.Duration,
                   swp.Frequency,
                   swp.ModeOfExecution,
                   swp.Remark), true);
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            swp = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SWP>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SWP,
                   swp.Arn,
                   swp.Cid,
                   swp.MemberName,
                   swp.Amc,
                   swp.FolioNumber,
                   swp.Scheme,
                   swp.Options,                  
                   swp.Amount,
                   swp.Duration,
                   swp.Frequency,
                   swp.ModeOfExecution,
                   swp.Remark,
                   taskCard.Id), true);
        }
    }
}
