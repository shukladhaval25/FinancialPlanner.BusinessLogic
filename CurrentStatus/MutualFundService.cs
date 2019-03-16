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
    public class MutualFundService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM MutualFund N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM MutualFund N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_MutualFund= "INSERT INTO MUTUALFUND VALUES (" +
            "{0},'{1}','{2}','{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},'{13}',{14},'{15}',{16},'{17}','{18}','{19}')";

        const string UPDATE_MutualFund = "UPDATE MUTUALFUND SET " +
            "[INVESTERNAME] = '{0}'," +
            "[SCHEMENAME] = '{1}'," +
            "[NAV] ={2}, UNITS ={3},EquityRatio = {4},GoldRatio ={5},DebtRatio = {6}," +
            "SIP ={7},FreeUnits ={8},REDUMPTIONAMOUNT ={9},GoalId ={10}, " +
            "[UpdatedOn] = '{11}', [UpdatedBy] ={12},FOLIONO = '{13}',FIRSTHOLDER = '{14}', SECONDHOLDER ='{15}', NOMINEE = '{16}' " +
            "WHERE ID = {17} ";

        const string DELETE_MutualFund = "DELETE FROM MutualFund WHERE ID = {0}";


        public IList<MutualFund> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Mutual fund process start");
                IList<MutualFund> lstMutualFundOption = new List<MutualFund>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    MutualFund mf = convertToMutualFund(dr);
                    lstMutualFundOption.Add(mf);
                }
                Logger.LogInfo("Get: Mutual fund process completed.");
                return lstMutualFundOption;
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

       

        public MutualFund Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: Mutual fund by id process start");
                MutualFund MutualFund = new MutualFund();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    MutualFund = convertToMutualFund(dr);
                }
                Logger.LogInfo("Get: Mutual fund by id process completed");
                return MutualFund;
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

        public void Add(MutualFund mutualFund)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,mutualFund.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_MutualFund,
                      mutualFund.Pid, mutualFund.InvesterName,mutualFund.SchemeName,
                      mutualFund.FolioNo,
                      mutualFund.Nav,mutualFund.Units,mutualFund.EquityRatio,
                      mutualFund.GoldRatio,mutualFund.DebtRatio,mutualFund.SIP,mutualFund.FreeUnit,
                      mutualFund.RedumptionAmount,mutualFund.GoalID,
                      mutualFund.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), mutualFund.CreatedBy,
                      mutualFund.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), mutualFund.UpdatedBy,
                      mutualFund.FirstHolder,mutualFund.SecondHolder,mutualFund.Nominee), true);

                Activity.ActivitiesService.Add(ActivityType.CreateMutualFund, EntryStatus.Success,
                         Source.Server, mutualFund.UpdatedByUserName, "MutualFund", mutualFund.MachineName);
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

        public void Update(MutualFund mutualFund)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,mutualFund.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_MutualFund,
                      mutualFund.InvesterName,
                      mutualFund.SchemeName,
                      mutualFund.Nav,
                      mutualFund.Units,
                      mutualFund.EquityRatio,mutualFund.GoldRatio,mutualFund.DebtRatio,
                      mutualFund.SIP,
                      mutualFund.FreeUnit,
                      mutualFund.RedumptionAmount,
                      (mutualFund.GoalID == null) ? null : mutualFund.GoalID.Value.ToString(),
                      mutualFund.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                      mutualFund.UpdatedBy,mutualFund.FolioNo,
                      mutualFund.FirstHolder,mutualFund.SecondHolder,mutualFund.Nominee,
                      mutualFund.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateMutualFund, EntryStatus.Success,
                         Source.Server, mutualFund.UpdatedByUserName, "MutualFund", mutualFund.MachineName);
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

        public void Delete(MutualFund mutualFund)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,mutualFund.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_MutualFund,
                      mutualFund.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteMutualFund, EntryStatus.Success,
                         Source.Server, mutualFund.UpdatedByUserName, "MutualFund", mutualFund.MachineName);
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
        private MutualFund convertToMutualFund(DataRow dr)
        {
            MutualFund mutualFund = new MutualFund();
            mutualFund.Id = dr.Field<int>("ID");
            mutualFund.Pid = dr.Field<int>("PID");
            mutualFund.InvesterName = dr.Field<string>("InvesterName");
            mutualFund.SchemeName = dr.Field<string>("SchemeName");
            mutualFund.FolioNo = dr.Field<string>("FolioNo");
            mutualFund.Nav = float.Parse(dr["NAV"].ToString());
            mutualFund.Units = double.Parse(dr["units"].ToString());
            mutualFund.EquityRatio = float.Parse(dr["EquityRatio"].ToString());
            mutualFund.GoldRatio = float.Parse(dr["GoldRatio"].ToString());
            mutualFund.DebtRatio = float.Parse(dr["DebtRatio"].ToString());
            mutualFund.SIP = double.Parse(dr["SIP"].ToString());
            mutualFund.FreeUnit = dr.Field<int>("FreeUnits");
            mutualFund.RedumptionAmount = double.Parse(dr["RedumptionAmount"].ToString());
            mutualFund.GoalID = dr.Field<int>("GoalId");

            mutualFund.UpdatedBy = dr.Field<int>("UpdatedBy");
            mutualFund.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            mutualFund.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
           
            mutualFund.FirstHolder  = dr.Field<string>("FirstHolder");
            mutualFund.SecondHolder = dr.Field<string>("SecondHolder");
            mutualFund.Nominee = dr.Field<string>("Nominee");
            return mutualFund;
        }
    }
}
