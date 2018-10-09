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
    public class SukanyaSamrudhiService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SukanyaSamrudhi N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SukanyaSamrudhi N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_SukanyaSamrudhi= "INSERT INTO SukanyaSamrudhi VALUES (" +
            "{0},'{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},'{10}',{11})";

        const string UPDATE_SukanyaSamrudhi = "UPDATE SukanyaSamrudhi SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[BANK] ='{2}', OPENINGDATE ='{3}', MATURITYDATE = '{4}',CURRENTVALUE ={5}, GoalId ={6}, " +
            "[UpdatedOn] = '{7}', [UpdatedBy] ={8} " +
            "WHERE ID = {9} ";

        const string DELETE_SukanyaSamrudhi = "DELETE FROM SukanyaSamrudhi WHERE ID = {0}";


        public IList<SukanyaSamrudhi> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: SukanyaSamrudhi process start");
                IList<SukanyaSamrudhi> lstSukanyaSamrudhiOption = new List<SukanyaSamrudhi>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SukanyaSamrudhi mf = convertToSukanyaSamrudhi(dr);
                    lstSukanyaSamrudhiOption.Add(mf);
                }
                Logger.LogInfo("Get: SukanyaSamrudhi fund process completed.");
                return lstSukanyaSamrudhiOption;
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


        public SukanyaSamrudhi Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: SukanyaSamrudhi by id process start");
                SukanyaSamrudhi SukanyaSamrudhi = new SukanyaSamrudhi();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SukanyaSamrudhi = convertToSukanyaSamrudhi(dr);
                }
                Logger.LogInfo("Get: SukanyaSamrudhi by id process completed");
                return SukanyaSamrudhi;
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

        public void Add(SukanyaSamrudhi SukanyaSamrudhi)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SukanyaSamrudhi.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SukanyaSamrudhi,
                      SukanyaSamrudhi.Pid, SukanyaSamrudhi.InvesterName, SukanyaSamrudhi.AccountNo,
                      SukanyaSamrudhi.Bank,
                      SukanyaSamrudhi.OpeningDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SukanyaSamrudhi.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SukanyaSamrudhi.CurrentValue,
                      SukanyaSamrudhi.GoalId,
                      SukanyaSamrudhi.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), SukanyaSamrudhi.CreatedBy,
                      SukanyaSamrudhi.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), SukanyaSamrudhi.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateSukanyaSamrudhi, EntryStatus.Success,
                         Source.Server, SukanyaSamrudhi.UpdatedByUserName, SukanyaSamrudhi.AccountNo, SukanyaSamrudhi.MachineName);
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

        public void Update(SukanyaSamrudhi SukanyaSamrudhi)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SukanyaSamrudhi.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SukanyaSamrudhi,
                      SukanyaSamrudhi.InvesterName,
                      SukanyaSamrudhi.AccountNo,
                      SukanyaSamrudhi.Bank,
                      SukanyaSamrudhi.OpeningDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SukanyaSamrudhi.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SukanyaSamrudhi.CurrentValue,
                      (SukanyaSamrudhi.GoalId == null) ? null : SukanyaSamrudhi.GoalId.Value.ToString(),
                      SukanyaSamrudhi.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      SukanyaSamrudhi.UpdatedBy,
                      SukanyaSamrudhi.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateSukanyaSamrudhi, EntryStatus.Success,
                         Source.Server, SukanyaSamrudhi.UpdatedByUserName, SukanyaSamrudhi.AccountNo, SukanyaSamrudhi.MachineName);
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

        public void Delete(SukanyaSamrudhi SukanyaSamrudhi)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SukanyaSamrudhi.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_SukanyaSamrudhi,
                      SukanyaSamrudhi.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteSukanyaSamrudhi, EntryStatus.Success,
                         Source.Server, SukanyaSamrudhi.UpdatedByUserName, SukanyaSamrudhi.AccountNo, SukanyaSamrudhi.MachineName);
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
        private SukanyaSamrudhi convertToSukanyaSamrudhi(DataRow dr)
        {
            SukanyaSamrudhi SukanyaSamrudhi = new SukanyaSamrudhi();
            SukanyaSamrudhi.Id = dr.Field<int>("ID");
            SukanyaSamrudhi.Pid = dr.Field<int>("PID");
            SukanyaSamrudhi.InvesterName = dr.Field<string>("InvesterName");
            SukanyaSamrudhi.AccountNo = dr.Field<string>("AccountNo");
            SukanyaSamrudhi.Bank = dr.Field<string>("Bank");
            SukanyaSamrudhi.OpeningDate = dr.Field<DateTime>("OpeningDate");
            SukanyaSamrudhi.CurrentValue = Double.Parse(dr["CurrentValue"].ToString());
            SukanyaSamrudhi.MaturityDate = dr.Field<DateTime>("MaturityDate");
            SukanyaSamrudhi.GoalId = dr.Field<int>("GoalId");
            SukanyaSamrudhi.UpdatedBy = dr.Field<int>("UpdatedBy");
            SukanyaSamrudhi.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            SukanyaSamrudhi.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            SukanyaSamrudhi.UpdatedBy = dr.Field<int>("UpdatedBy");
            SukanyaSamrudhi.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            SukanyaSamrudhi.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return SukanyaSamrudhi;
        }
    }
}
