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
    public class SCSSService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SCSS N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SCSS N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_SCSS= "INSERT INTO SCSS VALUES (" +
            "{0},'{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},'{10}',{11})";

        const string UPDATE_SCSS = "UPDATE SCSS SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[BANK] ='{2}', OPENINGDATE ='{3}', MATURITYDATE = '{4}',CURRENTVALUE ={5}, GoalId ={6}, " +
            "[UpdatedOn] = '{7}', [UpdatedBy] ={8} " +
            "WHERE ID = {9} ";

        const string DELETE_SCSS = "DELETE FROM SCSS WHERE ID = {0}";


        public IList<SCSS> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: SCSS process start");
                IList<SCSS> lstSCSSOption = new List<SCSS>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SCSS mf = convertToSCSS(dr);
                    lstSCSSOption.Add(mf);
                }
                Logger.LogInfo("Get: SCSS fund process completed.");
                return lstSCSSOption;
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


        public SCSS Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: SCSS by id process start");
                SCSS SCSS = new SCSS();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SCSS = convertToSCSS(dr);
                }
                Logger.LogInfo("Get: SCSS by id process completed");
                return SCSS;
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

        public void Add(SCSS SCSS)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SCSS.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SCSS,
                      SCSS.Pid, SCSS.InvesterName, SCSS.AccountNo,
                      SCSS.Bank,
                      SCSS.OpeningDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SCSS.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SCSS.CurrentValue,
                      SCSS.GoalId,
                      SCSS.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), SCSS.CreatedBy,
                      SCSS.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), SCSS.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateSCSS, EntryStatus.Success,
                         Source.Server, SCSS.UpdatedByUserName, SCSS.AccountNo, SCSS.MachineName);
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

        public void Update(SCSS SCSS)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SCSS.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SCSS,
                      SCSS.InvesterName,
                      SCSS.AccountNo,
                      SCSS.Bank,
                      SCSS.OpeningDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SCSS.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      SCSS.CurrentValue,
                      (SCSS.GoalId == null) ? null : SCSS.GoalId.Value.ToString(),
                      SCSS.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      SCSS.UpdatedBy,
                      SCSS.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateSCSS, EntryStatus.Success,
                         Source.Server, SCSS.UpdatedByUserName, SCSS.AccountNo, SCSS.MachineName);
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

        public void Delete(SCSS SCSS)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,SCSS.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_SCSS,
                      SCSS.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteSCSS, EntryStatus.Success,
                         Source.Server, SCSS.UpdatedByUserName, SCSS.AccountNo, SCSS.MachineName);
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
        private SCSS convertToSCSS(DataRow dr)
        {
            SCSS SCSS = new SCSS();
            SCSS.Id = dr.Field<int>("ID");
            SCSS.Pid = dr.Field<int>("PID");
            SCSS.InvesterName = dr.Field<string>("InvesterName");
            SCSS.AccountNo = dr.Field<string>("AccountNo");
            SCSS.Bank = dr.Field<string>("Bank");
            SCSS.OpeningDate = dr.Field<DateTime>("OpeningDate");
            SCSS.CurrentValue = Double.Parse(dr["CurrentValue"].ToString());
            SCSS.MaturityDate = dr.Field<DateTime>("MaturityDate");
            SCSS.GoalId = dr.Field<int>("GoalId");
            SCSS.UpdatedBy = dr.Field<int>("UpdatedBy");
            SCSS.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            SCSS.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            SCSS.UpdatedBy = dr.Field<int>("UpdatedBy");
            SCSS.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            SCSS.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return SCSS;
        }
    }
}
