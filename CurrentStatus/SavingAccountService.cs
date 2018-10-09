using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.CurrentStatus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.CurrentStatus
{
    public class SavingAccountService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SavingAccount N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SavingAccount N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_SavingAccount= "INSERT INTO SavingAccount VALUES (" +
            "{0},'{1}','{2}','{3}','{4}',{5},{6},{7},'{8}',{9},'{10}',{11})";

        const string UPDATE_SavingAccount = "UPDATE SavingAccount SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[BANKNAME] ='{2}', BRANCH ='{3}', BALANCE = {4},INTRATE ={5}, GoalId ={6}, " +
            "[UpdatedOn] = '{7}', [UpdatedBy] ={8} " +
            "WHERE ID = {9} ";

        const string DELETE_SavingAccount = "DELETE FROM SavingAccount WHERE ID = {0}";


        public IList<SavingAccount> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: SavingAccount process start");
                IList<SavingAccount> lstSavingAccountOption = new List<SavingAccount>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SavingAccount mf = convertToSavingAccount(dr);
                    lstSavingAccountOption.Add(mf);
                }
                Logger.LogInfo("Get: SavingAccount fund process completed.");
                return lstSavingAccountOption;
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


        public SavingAccount Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: SavingAccount by id process start");
                SavingAccount SavingAccount = new SavingAccount();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SavingAccount = convertToSavingAccount(dr);
                }
                Logger.LogInfo("Get: SavingAccount by id process completed");
                return SavingAccount;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public void Add(SavingAccount SavingAccount)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SavingAccount.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SavingAccount,
                      SavingAccount.Pid, SavingAccount.InvesterName, SavingAccount.AccountNo,
                      SavingAccount.BankName,SavingAccount.Branch,
                      SavingAccount.Balance,SavingAccount.IntRate,
                      SavingAccount.GoalId,
                      SavingAccount.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), SavingAccount.CreatedBy,
                      SavingAccount.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), SavingAccount.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateSavingAccount, EntryStatus.Success,
                         Source.Server, SavingAccount.UpdatedByUserName, SavingAccount.AccountNo, SavingAccount.MachineName);
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Update(SavingAccount SavingAccount)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SavingAccount.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SavingAccount,
                      SavingAccount.InvesterName,
                      SavingAccount.AccountNo,
                      SavingAccount.BankName,
                      SavingAccount.Branch,
                      SavingAccount.Balance,
                      SavingAccount.IntRate,                      
                      (SavingAccount.GoalId == null) ? null : SavingAccount.GoalId.Value.ToString(),
                      SavingAccount.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      SavingAccount.UpdatedBy,
                      SavingAccount.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateSavingAccount, EntryStatus.Success,
                         Source.Server, SavingAccount.UpdatedByUserName, SavingAccount.AccountNo, SavingAccount.MachineName);
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Delete(SavingAccount SavingAccount)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SavingAccount.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_SavingAccount,
                      SavingAccount.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteSavingAccount, EntryStatus.Success,
                         Source.Server, SavingAccount.UpdatedByUserName, SavingAccount.AccountNo, SavingAccount.MachineName);
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
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
        private SavingAccount convertToSavingAccount(DataRow dr)
        {
            SavingAccount savingAccount = new SavingAccount();
            savingAccount.Id = dr.Field<int>("ID");
            savingAccount.Pid = dr.Field<int>("PID");
            savingAccount.InvesterName = dr.Field<string>("InvesterName");
            savingAccount.AccountNo = dr.Field<string>("AccountNo");
            savingAccount.BankName = dr.Field<string>("BankName");
            savingAccount.Branch = dr.Field<string>("Branch");
            savingAccount.Balance = Double.Parse(dr["Balance"].ToString());
            savingAccount.IntRate = float.Parse(dr["IntRate"].ToString());
            savingAccount.GoalId = dr.Field<int>("GoalId");
            savingAccount.UpdatedBy = dr.Field<int>("UpdatedBy");
            savingAccount.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            savingAccount.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            savingAccount.UpdatedBy = dr.Field<int>("UpdatedBy");
            savingAccount.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            savingAccount.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return savingAccount;
        }
    }
}
