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
    public class TransmissionAfterDeathServiceImpl : ITransactionTypeService
    {
        private const string INSERT_CONTACTUPDATE = "INSERT INTO[dbo].[TransmissionAfterDeath] " +
           "([TaskId],[ARN],[CID] ,[MemberName],[AMC],[FolioNumber],[BeneficiaryName], " +
           "[IsJoinHolder],[ModeOfExecution]) VALUES " +
           "({0},{1},{2},'{3}',{4},'{5}','{6}','{7}','{8}')";

        private const string UPDATE_CONTACT = "UPDATE[dbo].[TransmissionAfterDeath] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MemberName] = '{3}' " +
            ",[AMC] = {4} ,[FolioNumber] = '{5}', [BeneficiaryName] = '{6}' ," +
            "[IsJoinHolder] = '{7}', [ModeOfExecution] = '{8}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM TransmissionAfterDeath WHERE TASKID ={0}";
        TransmissionAfterDeath panCardUpdate;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Transmission after death change request transaction process start");
                panCardUpdate = new TransmissionAfterDeath();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    panCardUpdate = converToBankchangeRequest(dr);
                }
                Logger.LogInfo("Get: Transmission after death change request transaction process completed.");
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
            panCardUpdate = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<TransmissionAfterDeath>(taskCard.TaskTransactionType.ToString());
            panCardUpdate.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CONTACTUPDATE,
                   panCardUpdate.TaskId,
                   panCardUpdate.Arn,
                   panCardUpdate.Cid,
                   panCardUpdate.MemberName,
                   panCardUpdate.Amc,
                   panCardUpdate.FolioNumber,                   
                   panCardUpdate.BeneficiaryName,                   
                   panCardUpdate.IsJoinHolder,
                   panCardUpdate.ModeOfExecution), true);
        }
        private TransmissionAfterDeath converToBankchangeRequest(DataRow dr)
        {
            TransmissionAfterDeath contactUpdate = new TransmissionAfterDeath();
            contactUpdate.Id = dr.Field<int>("ID");
            contactUpdate.TaskId = dr.Field<int>("TaskId");
            contactUpdate.Cid = dr.Field<int>("CID");
            contactUpdate.Arn = dr.Field<int>("ARN");
            contactUpdate.MemberName = dr.Field<string>("MemberName");
            contactUpdate.Amc = dr.Field<int>("AMC");
            contactUpdate.FolioNumber = dr.Field<string>("FolioNumber");            
            contactUpdate.BeneficiaryName = dr.Field<string>("BeneficiaryName");            
            contactUpdate.IsJoinHolder = dr.Field<bool>("IsJoinHolder");
            contactUpdate.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return contactUpdate;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            panCardUpdate = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<TransmissionAfterDeath>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CONTACT,
                   taskCard.Id,
                   panCardUpdate.Arn,
                   panCardUpdate.Cid,
                   panCardUpdate.MemberName,
                   panCardUpdate.Amc,
                   panCardUpdate.FolioNumber,                   
                   panCardUpdate.BeneficiaryName,                   
                   panCardUpdate.IsJoinHolder,
                   panCardUpdate.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
