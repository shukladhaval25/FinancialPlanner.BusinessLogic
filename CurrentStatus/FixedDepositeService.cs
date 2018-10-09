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
    public class FixedDepositService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM FixedDeposit N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM FixedDeposit N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_FixedDeposit= "INSERT INTO FixedDeposit VALUES (" +
            "{0},'{1}','{2}','{3}','{4}',{5},{6},'{7}', {8},'{9}',{10},'{11}',{12},'{13}',{14})";

        const string UPDATE_FixedDeposit = "UPDATE FixedDeposit SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[BANKNAME] ='{2}', BRANCH ='{3}', BALANCE = {4},INTRATE ={5}, GoalId ={6}, " +
            "[UpdatedOn] = '{7}', [UpdatedBy] ={8},DEPOSITDATE ='{9}',MATURITYAMT ={10},MATURITYDATE ='{11}' " +
            "WHERE ID = {12} ";

        const string DELETE_FixedDeposit = "DELETE FROM FixedDeposit WHERE ID = {0}";


        public IList<FixedDeposit> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: FixedDeposit process start");
                IList<FixedDeposit> lstFixedDepositOption = new List<FixedDeposit>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    FixedDeposit mf = convertToFixedDeposit(dr);
                    lstFixedDepositOption.Add(mf);
                }
                Logger.LogInfo("Get: FixedDeposit fund process completed.");
                return lstFixedDepositOption;
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


        public FixedDeposit Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: FixedDeposit by id process start");
                FixedDeposit FixedDeposit = new FixedDeposit();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    FixedDeposit = convertToFixedDeposit(dr);
                }
                Logger.LogInfo("Get: FixedDeposit by id process completed");
                return FixedDeposit;
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

        public void Add(FixedDeposit FixedDeposit)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,FixedDeposit.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_FixedDeposit,
                      FixedDeposit.Pid, FixedDeposit.InvesterName, FixedDeposit.AccountNo,
                      FixedDeposit.BankName, FixedDeposit.Branch,
                      FixedDeposit.Balance, FixedDeposit.IntRate,
                      FixedDeposit.DepositDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      FixedDeposit.MaturityAmt,
                      FixedDeposit.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      FixedDeposit.GoalId,
                      FixedDeposit.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), FixedDeposit.CreatedBy,
                      FixedDeposit.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), FixedDeposit.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateFixedDeposit, EntryStatus.Success,
                         Source.Server, FixedDeposit.UpdatedByUserName, FixedDeposit.AccountNo, FixedDeposit.MachineName);
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

        public void Update(FixedDeposit FixedDeposit)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,FixedDeposit.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_FixedDeposit,
                      FixedDeposit.InvesterName,
                      FixedDeposit.AccountNo,
                      FixedDeposit.BankName,
                      FixedDeposit.Branch,
                      FixedDeposit.Balance,
                      FixedDeposit.IntRate,
                      (FixedDeposit.GoalId == null) ? null : FixedDeposit.GoalId.Value.ToString(),
                      FixedDeposit.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      FixedDeposit.UpdatedBy,
                      FixedDeposit.DepositDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      FixedDeposit.MaturityAmt,
                      FixedDeposit.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      FixedDeposit.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateFixedDeposit, EntryStatus.Success,
                         Source.Server, FixedDeposit.UpdatedByUserName, FixedDeposit.AccountNo, FixedDeposit.MachineName);
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

        public void Delete(FixedDeposit FixedDeposit)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,FixedDeposit.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_FixedDeposit,
                      FixedDeposit.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteFixedDeposit, EntryStatus.Success,
                         Source.Server, FixedDeposit.UpdatedByUserName, FixedDeposit.AccountNo, FixedDeposit.MachineName);
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
        private FixedDeposit convertToFixedDeposit(DataRow dr)
        {
            FixedDeposit FixedDeposit = new FixedDeposit();
            FixedDeposit.Id = dr.Field<int>("ID");
            FixedDeposit.Pid = dr.Field<int>("PID");
            FixedDeposit.InvesterName = dr.Field<string>("InvesterName");
            FixedDeposit.AccountNo = dr.Field<string>("AccountNo");
            FixedDeposit.BankName = dr.Field<string>("BankName");
            FixedDeposit.Branch = dr.Field<string>("Branch");
            FixedDeposit.Balance = Double.Parse(dr["Balance"].ToString());
            FixedDeposit.IntRate = float.Parse(dr["IntRate"].ToString());
            FixedDeposit.DepositDate = dr.Field<DateTime>("DepositDate");
            FixedDeposit.MaturityAmt = Double.Parse(dr["MaturityAmt"].ToString());
            FixedDeposit.MaturityDate = dr.Field<DateTime>("MaturityDate");
            FixedDeposit.GoalId = dr.Field<int>("GoalId");
            FixedDeposit.UpdatedBy = dr.Field<int>("UpdatedBy");
            FixedDeposit.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            FixedDeposit.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            FixedDeposit.UpdatedBy = dr.Field<int>("UpdatedBy");
            FixedDeposit.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            FixedDeposit.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return FixedDeposit;
        }
    }
}
