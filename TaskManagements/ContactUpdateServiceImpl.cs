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
    public class ContactUpdateServiceImpl : ITransactionTypeService
    {
        private const string INSERT_CONTACTUPDATE = "INSERT INTO[dbo].[ContactUpdate] " +
           "([TaskId],[ARN],[CID] ,[MemberName],[AMC],[FolioNumber],[NewEmailId], " +
           "[NewMobileNo],[ModeOfExecution]) VALUES " +
           "({0},{1},{2},'{3}',{4},'{5}','{6}','{7}','{8}')";

        private const string UPDATE_CONTACT = "UPDATE[dbo].[ContactUpdate] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MemberName] = '{3}' " +
            ",[AMC] = {4} ,[FolioNumber] = '{5}', [NewEmailId] = '{6}' ," +
            "[NewMobileNo] = '{7}', [ModeOfExecution] = '{8}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM ContactUpdate WHERE TASKID ={0}";
        ContactUpdate contactUpdate;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Bank change request transaction process start");
                contactUpdate = new ContactUpdate();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    contactUpdate = converToBankchangeRequest(dr);
                }
                Logger.LogInfo("Get: Bank change request transaction process completed.");
                return contactUpdate;
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
            contactUpdate = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<ContactUpdate>(taskCard.TaskTransactionType.ToString());
            contactUpdate.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CONTACTUPDATE,
                   contactUpdate.TaskId,
                   contactUpdate.Arn,
                   contactUpdate.Cid,
                   contactUpdate.MemberName,
                   contactUpdate.Amc,
                   contactUpdate.FolioNumber,                   
                   contactUpdate.NewEmailId,                   
                   contactUpdate.NewMobileNo,
                   contactUpdate.ModeOfExecution), true);
        }
        private ContactUpdate converToBankchangeRequest(DataRow dr)
        {
            ContactUpdate contactUpdate = new ContactUpdate();
            contactUpdate.Id = dr.Field<int>("ID");
            contactUpdate.TaskId = dr.Field<int>("TaskId");
            contactUpdate.Cid = dr.Field<int>("CID");
            contactUpdate.Arn = dr.Field<int>("ARN");
            contactUpdate.MemberName = dr.Field<string>("MemberName");
            contactUpdate.Amc = dr.Field<int>("AMC");
            contactUpdate.FolioNumber = dr.Field<string>("FolioNumber");            
            contactUpdate.NewEmailId = dr.Field<string>("NewEmailId");            
            contactUpdate.NewMobileNo = dr.Field<string>("NewMobileNo");
            contactUpdate.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return contactUpdate;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            contactUpdate = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<ContactUpdate>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CONTACT,
                   taskCard.Id,
                   contactUpdate.Arn,
                   contactUpdate.Cid,
                   contactUpdate.MemberName,
                   contactUpdate.Amc,
                   contactUpdate.FolioNumber,                   
                   contactUpdate.NewEmailId,                   
                   contactUpdate.NewMobileNo,
                   contactUpdate.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
