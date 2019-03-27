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
    public class NPSService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NPS N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NPS N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_NPS= "INSERT INTO NPS VALUES (" +
            "{0},'{1}','{2}','{3}',{4},{5},{6},{7},{8},{9},{10},'{11}',{12},'{13}',{14},{15})";

        const string UPDATE_NPS = "UPDATE NPS SET " +
            "[INVESTERNAME] = '{0}'," +
            "[SCHEMENAME] = '{1}'," +
            "[NAV] ={2}, UNITS ={3},EquityRatio = {4},GoldRatio ={5},DebtRatio = {6}," +
            "SIP ={7},GoalId ={8}, " +
            "[UpdatedOn] = '{9}', [UpdatedBy] ={10},FOLIONO = '{11}',INVESTMENTRETURNRATE ={12} " +
            "WHERE ID = {13} ";

        const string DELETE_NPS = "DELETE FROM NPS WHERE ID = {0}";


        public IList<NPS> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: NPS process start");
                IList<NPS> lstNPSOption = new List<NPS>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    NPS mf = convertToNPS(dr);
                    lstNPSOption.Add(mf);
                }
                Logger.LogInfo("Get: NPS fund process completed.");
                return lstNPSOption;
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



        public NPS Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: NPS by id process start");
                NPS NPS = new NPS();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    NPS = convertToNPS(dr);
                }
                Logger.LogInfo("Get: NPS by id process completed");
                return NPS;
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

        public void Add(NPS NPS)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,NPS.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_NPS,
                      NPS.Pid, NPS.InvesterName, NPS.SchemeName,
                      NPS.FolioNo,
                      NPS.Nav, NPS.Units, NPS.EquityRatio,
                      NPS.GoldRatio, NPS.DebtRatio, NPS.SIP,NPS.GoalID,
                      NPS.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), NPS.CreatedBy,
                      NPS.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), NPS.UpdatedBy,
                      NPS.InvestmentReturnRate), true);

                Activity.ActivitiesService.Add(ActivityType.CreateNPS, EntryStatus.Success,
                         Source.Server, NPS.UpdatedByUserName, "NPS", NPS.MachineName);
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

        public void Update(NPS NPS)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,NPS.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_NPS,
                      NPS.InvesterName,
                      NPS.SchemeName,
                      NPS.Nav,
                      NPS.Units,
                      NPS.EquityRatio, NPS.GoldRatio, NPS.DebtRatio,
                      NPS.SIP,                    
                      (NPS.GoalID == null) ? null : NPS.GoalID.Value.ToString(),
                      NPS.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      NPS.UpdatedBy, NPS.FolioNo,
                      NPS.InvestmentReturnRate,
                      NPS.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateNPS, EntryStatus.Success,
                         Source.Server, NPS.UpdatedByUserName, "NPS", NPS.MachineName);
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

        public void Delete(NPS NPS)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,NPS.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_NPS,
                      NPS.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteNPS, EntryStatus.Success,
                         Source.Server, NPS.UpdatedByUserName, "NPS", NPS.MachineName);
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
        private NPS convertToNPS(DataRow dr)
        {
            NPS NPS = new NPS();
            NPS.Id = dr.Field<int>("ID");
            NPS.Pid = dr.Field<int>("PID");
            NPS.InvesterName = dr.Field<string>("InvesterName");
            NPS.SchemeName = dr.Field<string>("SchemeName");
            NPS.FolioNo = dr.Field<string>("FolioNo");
            NPS.Nav = float.Parse(dr["NAV"].ToString());
            NPS.Units = dr.Field<int>("units");
            NPS.EquityRatio = float.Parse(dr["EquityRatio"].ToString());
            NPS.GoldRatio = float.Parse(dr["GoldRatio"].ToString());
            NPS.DebtRatio = float.Parse(dr["DebtRatio"].ToString());
            NPS.SIP = double.Parse(dr["SIP"].ToString());        
            NPS.GoalID = dr.Field<int>("GoalId");

            NPS.UpdatedBy = dr.Field<int>("UpdatedBy");
            NPS.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            NPS.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            NPS.GoalID = dr.Field<int>("GoalId");
            NPS.UpdatedBy = dr.Field<int>("UpdatedBy");
            NPS.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            NPS.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            NPS.InvestmentReturnRate = float.Parse(dr["InvestmentReturnRate"].ToString());
            return NPS;
        }
    }
}
