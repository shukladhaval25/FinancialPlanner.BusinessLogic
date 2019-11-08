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
    public class AddressChangeServiceImpl : ITransactionTypeService
    {
        private const string INSERT_CONTACTUPDATE = "INSERT INTO[dbo].[AddressChange] " +
           "([TaskId],[ARN],[CID] ,[MemberName],[AMC],[FolioNumber],[Address], " +
           "[ModeOfExecution]) VALUES " +
           "({0},{1},{2},'{3}',{4},'{5}','{6}','{7}')";

        private const string UPDATE_CONTACT = "UPDATE[dbo].[AddressChange] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MemberName] = '{3}' " +
            ",[AMC] = {4} ,[FolioNumber] = '{5}', [Address] = '{6}' ," +
            "[ModeOfExecution] = '{7}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM AddressChange WHERE TASKID ={0}";
        AddressChange addressChange;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Address change request transaction process start");
                addressChange = new AddressChange();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    addressChange = converToBankchangeRequest(dr);
                }
                Logger.LogInfo("Get: Address change request transaction process completed.");
                return addressChange;
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
            addressChange = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<AddressChange>(taskCard.TaskTransactionType.ToString());
            addressChange.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CONTACTUPDATE,
                   addressChange.TaskId,
                   addressChange.Arn,
                   addressChange.Cid,
                   addressChange.MemberName,
                   addressChange.Amc,
                   addressChange.FolioNumber,                   
                   addressChange.Address,                   
                   addressChange.ModeOfExecution), true);
        }
        private AddressChange converToBankchangeRequest(DataRow dr)
        {
            AddressChange contactUpdate = new AddressChange();
            contactUpdate.Id = dr.Field<int>("ID");
            contactUpdate.TaskId = dr.Field<int>("TaskId");
            contactUpdate.Cid = dr.Field<int>("CID");
            contactUpdate.Arn = dr.Field<int>("ARN");
            contactUpdate.MemberName = dr.Field<string>("MemberName");
            contactUpdate.Amc = dr.Field<int>("AMC");
            contactUpdate.FolioNumber = dr.Field<string>("FolioNumber");            
            contactUpdate.Address = dr.Field<string>("Address");            
            contactUpdate.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return contactUpdate;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            addressChange = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<AddressChange>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CONTACT,
                   taskCard.Id,
                   addressChange.Arn,
                   addressChange.Cid,
                   addressChange.MemberName,
                   addressChange.Amc,
                   addressChange.FolioNumber,                   
                   addressChange.Address,                   
                   addressChange.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
