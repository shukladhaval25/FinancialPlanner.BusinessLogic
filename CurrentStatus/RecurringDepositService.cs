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
    public class RecurringDepositService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM RecurringDeposit N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM RecurringDeposit N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_RecurringDeposit= "INSERT INTO RecurringDeposit VALUES (" +
            "{0},'{1}','{2}','{3}','{4}',{5},{6},'{7}', {8},'{9}',{10},{11},'{12}',{13},'{14}',{15})";

        const string UPDATE_RecurringDeposit = "UPDATE RecurringDeposit SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[BANKNAME] ='{2}', BRANCH ='{3}', BALANCE = {4},INTRATE ={5}, GoalId ={6}, " +
            "[UpdatedOn] = '{7}', [UpdatedBy] ={8},DEPOSITDATE ='{9}',MATURITYAMT ={10},MATURITYDATE ='{11}', " +
            "MONTHLYINSTALLMENT = {12} " +
            "WHERE ID = {13} ";

        const string DELETE_RecurringDeposit = "DELETE FROM RecurringDeposit WHERE ID = {0}";


        public IList<RecurringDeposit> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: RecurringDeposit process start");
                IList<RecurringDeposit> lstRecurringDepositOption = new List<RecurringDeposit>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    RecurringDeposit mf = convertToRecurringDeposit(dr);
                    lstRecurringDepositOption.Add(mf);
                }
                Logger.LogInfo("Get: RecurringDeposit fund process completed.");
                return lstRecurringDepositOption;
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


        public RecurringDeposit Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: RecurringDeposit by id process start");
                RecurringDeposit RecurringDeposit = new RecurringDeposit();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    RecurringDeposit = convertToRecurringDeposit(dr);
                }
                Logger.LogInfo("Get: RecurringDeposit by id process completed");
                return RecurringDeposit;
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

        public void Add(RecurringDeposit RecurringDeposit)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,RecurringDeposit.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_RecurringDeposit,
                      RecurringDeposit.Pid, RecurringDeposit.InvesterName, RecurringDeposit.AccountNo,
                      RecurringDeposit.BankName, RecurringDeposit.Branch,
                      RecurringDeposit.Balance, RecurringDeposit.IntRate,
                      RecurringDeposit.DepositDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      RecurringDeposit.MaturityAmt,
                      RecurringDeposit.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      RecurringDeposit.MonthlyInstallment,
                      RecurringDeposit.GoalId,
                      RecurringDeposit.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), RecurringDeposit.CreatedBy,
                      RecurringDeposit.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), RecurringDeposit.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateRecurringDeposit, EntryStatus.Success,
                         Source.Server, RecurringDeposit.UpdatedByUserName, RecurringDeposit.AccountNo, RecurringDeposit.MachineName);
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

        public void Update(RecurringDeposit RecurringDeposit)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,RecurringDeposit.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_RecurringDeposit,
                      RecurringDeposit.InvesterName,
                      RecurringDeposit.AccountNo,
                      RecurringDeposit.BankName,
                      RecurringDeposit.Branch,
                      RecurringDeposit.Balance,
                      RecurringDeposit.IntRate,
                      (RecurringDeposit.GoalId == null) ? null : RecurringDeposit.GoalId.Value.ToString(),
                      RecurringDeposit.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      RecurringDeposit.UpdatedBy,
                      RecurringDeposit.DepositDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      RecurringDeposit.MaturityAmt,
                      RecurringDeposit.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      RecurringDeposit.MonthlyInstallment,
                      RecurringDeposit.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateRecurringDeposit, EntryStatus.Success,
                         Source.Server, RecurringDeposit.UpdatedByUserName, RecurringDeposit.AccountNo, RecurringDeposit.MachineName);
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

        public void Delete(RecurringDeposit RecurringDeposit)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,RecurringDeposit.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_RecurringDeposit,
                      RecurringDeposit.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteRecurringDeposit, EntryStatus.Success,
                         Source.Server, RecurringDeposit.UpdatedByUserName, RecurringDeposit.AccountNo, RecurringDeposit.MachineName);
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
        private RecurringDeposit convertToRecurringDeposit(DataRow dr)
        {
            RecurringDeposit RecurringDeposit = new RecurringDeposit();
            RecurringDeposit.Id = dr.Field<int>("ID");
            RecurringDeposit.Pid = dr.Field<int>("PID");
            RecurringDeposit.InvesterName = dr.Field<string>("InvesterName");
            RecurringDeposit.AccountNo = dr.Field<string>("AccountNo");
            RecurringDeposit.BankName = dr.Field<string>("BankName");
            RecurringDeposit.Branch = dr.Field<string>("Branch");
            RecurringDeposit.Balance = Double.Parse(dr["Balance"].ToString());
            RecurringDeposit.IntRate = float.Parse(dr["IntRate"].ToString());
            RecurringDeposit.DepositDate = dr.Field<DateTime>("DepositDate");
            RecurringDeposit.MaturityAmt = Double.Parse(dr["MaturityAmt"].ToString());
            RecurringDeposit.MaturityDate = dr.Field<DateTime>("MaturityDate");
            RecurringDeposit.MonthlyInstallment = Double.Parse(dr["MonthlyInstallment"].ToString());                            
            RecurringDeposit.GoalId = dr.Field<int>("GoalId");
            RecurringDeposit.UpdatedBy = dr.Field<int>("UpdatedBy");
            RecurringDeposit.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            RecurringDeposit.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            RecurringDeposit.UpdatedBy = dr.Field<int>("UpdatedBy");
            RecurringDeposit.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            RecurringDeposit.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return RecurringDeposit;
        }
    }
}
