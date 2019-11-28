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
    public class NominationServiceImpl : ITransactionTypeService
    {
        private const string INSERT_QUERY = "INSERT INTO[dbo].[Nomination] " +
            "([TaskId],[ARN],[CID] ,[MemberName],[AMC],[FolioNumber], " +
            "[Nominee1],[Nominee2],[AllocationForNominee1],[AllocationForNominee2]," +
            "[ModeOfExecution]) VALUES " +
            "({0},{1},{2},'{3}','{4}','{5}','{6}','{7}',{8},{9},'{10}')";

        private const string UPDATE_QUERY = "UPDATE[dbo].[Nomination] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MemberName] = '{3}', " +
            "[AMC] = {4},[FolioNumber] = '{5}',[Nominee1] ='{6}',[Nominee2] ='{7}'," +
            "[AllocationForNominee1] = {8},[AllocationForNominee2] ={9},"+
            "[ModeOfExecution] = '{10}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM Nomination WHERE TASKID ={0}";
        Nomination nomination;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Minor to Major change request transaction process start");
                nomination = new Nomination();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    nomination = converToNomination(dr);
                }
                Logger.LogInfo("Get: Minor to Major change request transaction process completed.");
                return nomination;
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
            nomination = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<Nomination>(taskCard.TaskTransactionType.ToString());
            nomination.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                   nomination.TaskId,
                   nomination.Arn,
                   nomination.Cid,
                   nomination.MemberName,
                   nomination.Amc,
                   nomination.FolioNumber,
                   nomination.Nominee1,
                   nomination.Nominee2,
                   nomination.AllocationForNominee1,
                   nomination.AllocationForNominee2,
                   nomination.ModeOfExecution), true);
        }
        private Nomination converToNomination(DataRow dr)
        {
            Nomination minorToMajor = new Nomination();
            minorToMajor.Id = dr.Field<int>("ID");
            minorToMajor.TaskId = dr.Field<int>("TaskId");
            minorToMajor.Cid  = dr.Field<int>("CID");
            minorToMajor.Arn = dr.Field<int>("ARN");
            minorToMajor.MemberName = dr.Field<string>("MemberName");
            minorToMajor.Nominee1 = dr.Field<string>("Nominee1");
            minorToMajor.Nominee2 = dr.Field<string>("Nominee2");
            minorToMajor.AllocationForNominee1 = double.Parse(dr["AllocationForNominee1"].ToString());
            minorToMajor.AllocationForNominee2 = double.Parse(dr["AllocationForNominee2"].ToString());
            minorToMajor.Amc = dr.Field<int>("AMC");
            minorToMajor.FolioNumber = dr.Field<string>("FolioNumber");            
            minorToMajor.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return minorToMajor;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            nomination = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<Nomination>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   taskCard.Id,
                   nomination.Arn,
                   nomination.Cid,
                   nomination.MemberName,
                   nomination.Amc,
                   nomination.FolioNumber,
                   nomination.Nominee1,
                   nomination.Nominee2,
                   nomination.AllocationForNominee1,
                   nomination.AllocationForNominee2,
                   nomination.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
