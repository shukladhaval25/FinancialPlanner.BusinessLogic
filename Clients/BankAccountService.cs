using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.CurrentStatus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class BankAccountService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL_BY_CLIENT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM BankAccount C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.CID = {0}";
        private const string SELECT_ALL_BY_CLIENT_ID_AND_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM BankAccount C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.ID = {0} AND C1.CID = {1}";

        private const string INSERT_QUERY = "INSERT INTO BankAccount VALUES ({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9},'{10}',{11},'{12}',{13})";
        private const string UPDATE_QUERY = "UPDATE BankAccount SET ACCOUNTHOLDERID ={0}, BANKNAME = '{1}'," +
            "ACCOUNTNO ='{2}',ACCOUNTTYPE ='{3}',ADDRESS ='{4}',CONTACTNO ='{5}', ISJOINACCOUNT = '{6}'," +
            "JOINHOLDERNAME = '{7}',MINREQUIREBALANCE = {8}, UPDATEDON ='{9}',UPDATEDBY ={10} WHERE CID ={11} AND ID ={12}";

        private const string DELETE_BY_ID = "DELETE FROM BankAccount WHERE ID ={0}";

        public IList<BankAccountDetail> Get(int clientId)
        {
            try
            {
                Logger.LogInfo("Get: Bank account details information process start");
                IList<BankAccountDetail> lstBankAccount = new List<BankAccountDetail>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_CLIENT_ID,clientId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    BankAccountDetail BankAccount = convertToBankAccountObject(dr);
                    lstBankAccount.Add(BankAccount);
                }
                Logger.LogInfo("Get: Bank account details information process completed.");
                return lstBankAccount;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public BankAccountDetail Get(int id, int clientId)
        {
            try
            {
                Logger.LogInfo("Get: Bank account information process start");
                BankAccountDetail BankAccount = new BankAccountDetail();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_CLIENT_ID_AND_ID,id,clientId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    BankAccount = convertToBankAccountObject(dr);
                }
                Logger.LogInfo("Get: Bank account information process completed.");
                return BankAccount;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public void Add(BankAccountDetail BankAccount)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,BankAccount.Cid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   BankAccount.Cid, BankAccount.AccountHolderID, 
                   BankAccount.BankName, BankAccount.AccountNo, BankAccount.AccountType,
                   BankAccount.Address,BankAccount.ContactNo,
                   BankAccount.IsJoinAccount, BankAccount.JoinHolderName,
                   BankAccount.MinRequireBalance,
                   BankAccount.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), BankAccount.CreatedBy,
                   BankAccount.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), BankAccount.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateBankAccount, EntryStatus.Success,
                         Source.Server, BankAccount.UpdatedByUserName, BankAccount.AccountNo, BankAccount.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        public void Update(BankAccountDetail BankAccount)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   BankAccount.AccountHolderID,
                   BankAccount.BankName, BankAccount.AccountNo,
                   BankAccount.AccountType, BankAccount.Address, BankAccount.ContactNo,
                   BankAccount.IsJoinAccount,BankAccount.JoinHolderName,
                   BankAccount.MinRequireBalance,
                   BankAccount.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   BankAccount.UpdatedBy, BankAccount.Cid, BankAccount.Id));

                Activity.ActivitiesService.Add(ActivityType.UpdateBankAccount, EntryStatus.Success,
                         Source.Server, BankAccount.UpdatedByUserName, BankAccount.AccountNo, BankAccount.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Delete(BankAccountDetail BankAccount)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, BankAccount.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteBankAccount, EntryStatus.Success,
                         Source.Server, BankAccount.UpdatedByUserName, BankAccount.AccountNo, BankAccount.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
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

        private BankAccountDetail convertToBankAccountObject(DataRow dr)
        {
            BankAccountDetail BankAccount = new BankAccountDetail();
            BankAccount.Id = dr.Field<int>("ID");
            BankAccount.Cid = dr.Field<int>("CID");
            BankAccount.AccountHolderID = dr.Field<int>("AccountHolderId");
            BankAccount.BankName = dr.Field<string>("BankName");
            BankAccount.AccountNo = dr.Field<string>("AccountNo");
            BankAccount.AccountType = dr.Field<string>("AccountType");
            BankAccount.Address = dr.Field<string>("Address");
            BankAccount.ContactNo = dr.Field<string>("ContactNo");
            BankAccount.IsJoinAccount = dr.Field<bool>("IsJoinAccount");
            BankAccount.JoinHolderName = dr.Field<string>("JoinHolderName");
            BankAccount.MinRequireBalance = double.Parse(dr["MinRequireBalance"].ToString());
            return BankAccount;
        }
    }
}
