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
    public class PANCardUpdateServiceImpl : ITransactionTypeService
    {
        private const string INSERT_CONTACTUPDATE = "INSERT INTO[dbo].[PANCardUpdate] " +
           "([TaskId],[ARN],[CID] ,[MemberName],[AMC],[FolioNumber],[FirstHolderPAN], " +
           "[SecondHolderPAN],[ModeOfExecution]) VALUES " +
           "({0},{1},{2},'{3}',{4},'{5}','{6}','{7}','{8}')";

        private const string UPDATE_CONTACT = "UPDATE[dbo].[PANCardUpdate] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MemberName] = '{3}' " +
            ",[AMC] = {4} ,[FolioNumber] = '{5}', [FirstHolderPAN] = '{6}' ," +
            "[SecondHolderPAN] = '{7}', [ModeOfExecution] = '{8}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM PANCardUpdate WHERE TASKID ={0}";
        PANCardUpdate panCardUpdate;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: PanCard change request transaction process start");
                panCardUpdate = new PANCardUpdate();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    panCardUpdate = converToBankchangeRequest(dr);
                }
                Logger.LogInfo("Get: Pancard change request transaction process completed.");
                return panCardUpdate;
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
            panCardUpdate = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<PANCardUpdate>(taskCard.TaskTransactionType.ToString());
            panCardUpdate.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CONTACTUPDATE,
                   panCardUpdate.TaskId,
                   panCardUpdate.Arn,
                   panCardUpdate.Cid,
                   panCardUpdate.MemberName,
                   panCardUpdate.Amc,
                   panCardUpdate.FolioNumber,                   
                   panCardUpdate.FirstHolderPAN,                   
                   panCardUpdate.SecondHolderPAN,
                   panCardUpdate.ModeOfExecution), true);
        }
        private PANCardUpdate converToBankchangeRequest(DataRow dr)
        {
            PANCardUpdate contactUpdate = new PANCardUpdate();
            contactUpdate.Id = dr.Field<int>("ID");
            contactUpdate.TaskId = dr.Field<int>("TaskId");
            contactUpdate.Cid = dr.Field<int>("CID");
            contactUpdate.Arn = dr.Field<int>("ARN");
            contactUpdate.MemberName = dr.Field<string>("MemberName");
            contactUpdate.Amc = dr.Field<int>("AMC");
            contactUpdate.FolioNumber = dr.Field<string>("FolioNumber");            
            contactUpdate.FirstHolderPAN = dr.Field<string>("FirstHolderPAN");            
            contactUpdate.SecondHolderPAN = dr.Field<string>("SecondHolderPAN");
            contactUpdate.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return contactUpdate;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            panCardUpdate = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<PANCardUpdate>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CONTACT,
                   taskCard.Id,
                   panCardUpdate.Arn,
                   panCardUpdate.Cid,
                   panCardUpdate.MemberName,
                   panCardUpdate.Amc,
                   panCardUpdate.FolioNumber,                   
                   panCardUpdate.FirstHolderPAN,                   
                   panCardUpdate.SecondHolderPAN,
                   panCardUpdate.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
