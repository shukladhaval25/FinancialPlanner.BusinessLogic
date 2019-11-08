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
    public class BankChangeRequestService : ITransactionTypeService
    {
        private const string INSERT_BANKCHANGEREQUEST = "INSERT INTO[dbo].[BankChangeRequest] " +
            "([TaskId],[ARN],[CID] ,[MemberName],[AMC],[FolioNumber],[OldBankId],[OldBankAccountNo], " +
            "[NewBankId],[NewBankAccountNo],[ModeOfExecution]) VALUES " +
            "({0},{1},{2},'{3}',{4},'{5}',{6},'{7}',{8},'{9}','{10}')";

        private const string UPDATE_BANKCHANGEREQUEST = "UPDATE[dbo].[BankChangeRequest] " +
            "SET [TaskId] = {0}, [ARN] = {1}, [CID] = {2}, [MemberName] = '{3}' " +
            ",[AMC] = {4} ,[FolioNumber] = '{5}', [OldBankId] = {6} ," +
            "[OldBankAccountNo] = '{7}' ,[NewBankId] ={8},[NewBankAccountNo] = '{9}', " +
            "[ModeOfExecution] = '{10}' WHERE [TaskId] = {0}";

        private const string SELECT_BY_ID = "SELECT * FROM BankChangeRequest WHERE TASKID ={0}";
        BankChangeRequest bankChangeRequest;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Bank change request transaction process start");
                bankChangeRequest = new BankChangeRequest();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    bankChangeRequest = converToBankchangeRequest(dr);
                }
                Logger.LogInfo("Get: Bank change request transaction process completed.");
                return bankChangeRequest;
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
            bankChangeRequest = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<BankChangeRequest>(taskCard.TaskTransactionType.ToString());
            bankChangeRequest.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_BANKCHANGEREQUEST,
                   bankChangeRequest.TaskId,
                   bankChangeRequest.Arn,
                   bankChangeRequest.Cid,
                   bankChangeRequest.MemberName,                   
                   bankChangeRequest.Amc,
                   bankChangeRequest.FolioNumber,
                   bankChangeRequest.OldBankId,
                   bankChangeRequest.OldBankAcNo,
                   bankChangeRequest.NewBankId,
                   bankChangeRequest.NewBankAcNo,
                   bankChangeRequest.ModeOfExecution), true);
        }
        private BankChangeRequest converToBankchangeRequest(DataRow dr)
        {
            BankChangeRequest bankChangeRequest = new BankChangeRequest();
            bankChangeRequest.Id = dr.Field<int>("ID");
            bankChangeRequest.TaskId = dr.Field<int>("TaskId");
            bankChangeRequest.Cid  = dr.Field<int>("CID");
            bankChangeRequest.Arn = dr.Field<int>("ARN");
            bankChangeRequest.MemberName = dr.Field<string>("MemberName");           
            bankChangeRequest.Amc = dr.Field<int>("AMC");
            bankChangeRequest.FolioNumber = dr.Field<string>("FolioNumber");
            bankChangeRequest.OldBankId = dr.Field<int>("OldBankId");
            bankChangeRequest.OldBankAcNo = dr.Field<string>("OldBankAccountNo");
            bankChangeRequest.NewBankId = dr.Field<int>("NewBankId");
            bankChangeRequest.NewBankAcNo = dr.Field<string>("NewBankAccountNo");
            bankChangeRequest.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            return bankChangeRequest;
        }

        public void UpdateTransaction(TaskCard taskCard)
        {
            bankChangeRequest = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<BankChangeRequest>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_BANKCHANGEREQUEST,
                   taskCard.Id,
                   bankChangeRequest.Arn,
                   bankChangeRequest.Cid,
                   bankChangeRequest.MemberName,                 
                   bankChangeRequest.Amc,
                   bankChangeRequest.FolioNumber,
                   bankChangeRequest.OldBankId,
                   bankChangeRequest.OldBankAcNo,
                   bankChangeRequest.NewBankId,
                   bankChangeRequest.NewBankAcNo,
                   bankChangeRequest.ModeOfExecution,
                   taskCard.Id), true);
        }
    }
}
