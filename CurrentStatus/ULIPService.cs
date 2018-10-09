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
    public class ULIPService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM ULIP N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM ULIP N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_ULIP= "INSERT INTO ULIP VALUES (" +
            "{0},'{1}','{2}','{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},'{13}',{14},'{15}',{16},'{17}','{18}','{19}')";

        const string UPDATE_ULIP = "UPDATE ULIP SET " +
            "[INVESTERNAME] = '{0}'," +
            "[SCHEMENAME] = '{1}'," +
            "[NAV] ={2}, UNITS ={3},EquityRatio = {4},GoldRatio ={5},DebtRatio = {6}," +
            "SIP ={7},FreeUnits ={8},REDUMPTIONAMOUNT ={9},GoalId ={10}, " +
            "[UpdatedOn] = '{11}', [UpdatedBy] ={12},FOLIONO = '{13}',FIRSTHOLDER = '{14}', SECONDHOLDER ='{15}', NOMINEE = '{16}' " +
            "WHERE ID = {17} ";

        const string DELETE_ULIP = "DELETE FROM ULIP WHERE ID = {0}";


        public IList<ULIP> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Mutual fund process start");
                IList<ULIP> lstULIPOption = new List<ULIP>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ULIP mf = convertToULIP(dr);
                    lstULIPOption.Add(mf);
                }
                Logger.LogInfo("Get: Mutual fund process completed.");
                return lstULIPOption;
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
        
        public ULIP Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: Mutual fund by id process start");
                ULIP ULIP = new ULIP();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ULIP = convertToULIP(dr);
                }
                Logger.LogInfo("Get: Mutual fund by id process completed");
                return ULIP;
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

        public void Add(ULIP ULIP)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,ULIP.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_ULIP,
                      ULIP.Pid, ULIP.InvesterName, ULIP.SchemeName,
                      ULIP.FolioNo,
                      ULIP.Nav, ULIP.Units, ULIP.EquityRatio,
                      ULIP.GoldRatio, ULIP.DebtRatio, ULIP.SIP, ULIP.FreeUnit,
                      ULIP.RedumptionAmount, ULIP.GoalID,
                      ULIP.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ULIP.CreatedBy,
                      ULIP.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ULIP.UpdatedBy,
                      ULIP.FirstHolder, ULIP.SecondHolder, ULIP.Nominee), true);

                Activity.ActivitiesService.Add(ActivityType.CreateULIP, EntryStatus.Success,
                         Source.Server, ULIP.UpdatedByUserName, "ULIP", ULIP.MachineName);
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

        public void Update(ULIP ULIP)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,ULIP.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_ULIP,
                      ULIP.InvesterName,
                      ULIP.SchemeName,
                      ULIP.Nav,
                      ULIP.Units,
                      ULIP.EquityRatio, ULIP.GoldRatio, ULIP.DebtRatio,
                      ULIP.SIP,
                      ULIP.FreeUnit,
                      ULIP.RedumptionAmount,
                      (ULIP.GoalID == null) ? null : ULIP.GoalID.Value.ToString(),
                      ULIP.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      ULIP.UpdatedBy, ULIP.FolioNo,
                      ULIP.FirstHolder, ULIP.SecondHolder, ULIP.Nominee,
                      ULIP.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateULIP, EntryStatus.Success,
                         Source.Server, ULIP.UpdatedByUserName, "ULIP", ULIP.MachineName);
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

        public void Delete(ULIP ULIP)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,ULIP.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_ULIP,
                      ULIP.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteULIP, EntryStatus.Success,
                         Source.Server, ULIP.UpdatedByUserName, "ULIP", ULIP.MachineName);
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
        private ULIP convertToULIP(DataRow dr)
        {
            ULIP ULIP = new ULIP();
            ULIP.Id = dr.Field<int>("ID");
            ULIP.Pid = dr.Field<int>("PID");
            ULIP.InvesterName = dr.Field<string>("InvesterName");
            ULIP.SchemeName = dr.Field<string>("SchemeName");
            ULIP.FolioNo = dr.Field<string>("FolioNo");
            ULIP.Nav = float.Parse(dr["NAV"].ToString());
            ULIP.Units = dr.Field<int>("units");
            ULIP.EquityRatio = float.Parse(dr["EquityRatio"].ToString());
            ULIP.GoldRatio = float.Parse(dr["GoldRatio"].ToString());
            ULIP.DebtRatio = float.Parse(dr["DebtRatio"].ToString());
            ULIP.SIP = double.Parse(dr["SIP"].ToString());
            ULIP.FreeUnit = dr.Field<int>("FreeUnits");
            ULIP.RedumptionAmount = double.Parse(dr["RedumptionAmount"].ToString());
            ULIP.GoalID = dr.Field<int>("GoalId");

            ULIP.UpdatedBy = dr.Field<int>("UpdatedBy");
            ULIP.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            ULIP.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");

            ULIP.FirstHolder = dr.Field<string>("FirstHolder");
            ULIP.SecondHolder = dr.Field<string>("SecondHolder");
            ULIP.Nominee = dr.Field<string>("Nominee");
            return ULIP;
        }
    }
}
