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
    public class PPFService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PPF N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PPF N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_PPF= "INSERT INTO PPF VALUES (" +
            "{0},'{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},'{10}',{11},{12},'{13}')";

        const string UPDATE_PPF = "UPDATE PPF SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[BANK] ='{2}', OPENINGDATE ='{3}', MATURITYDATE = '{4}',CURRENTVALUE ={5}, GoalId ={6}, " +
            "[UpdatedOn] = '{7}', [UpdatedBy] ={8},[InvestmentReturnRate] ={9},[AccountExtended] = '{10}' " +
            "WHERE ID = {11} ";

        const string DELETE_PPF = "DELETE FROM PPF WHERE ID = {0}";

        const string SELECT_PPF_MATURITY = "SELECT PPF.INVESTERNAME, Client.Name, PPF.AccountNo, PPF.MaturityDate, PPF.Bank, PPF.ACCOUNTEXTENDED FROM Client INNER JOIN Planner ON Client.ID = Planner.ClientId AND Client.ID = Planner.ClientId RIGHT OUTER JOIN PPF ON Planner.ID = PPF.PID AND Planner.ID = PPF.PID WHERE (PPF.MaturityDate BETWEEN '{0}' AND '{1}')";

        public IList<PPF> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: PPF process start");
                IList<PPF> lstPPFOption = new List<PPF>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    PPF mf = convertToPPF(dr);
                    lstPPFOption.Add(mf);
                }
                Logger.LogInfo("Get: PPF fund process completed.");
                return lstPPFOption;
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

        public IList<PPFMaturity> GetPPFMaturity(DateTime from, DateTime to)
        {
            try
            {
                Logger.LogInfo("Get: PPF maturity process start");
                IList<PPFMaturity> lstPPFOption = new List<PPFMaturity>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PPF_MATURITY, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd")));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    PPFMaturity mf = convertToPPFMaturity(dr);
                    lstPPFOption.Add(mf);
                }
                Logger.LogInfo("Get: PPF maturity process completed.");
                return lstPPFOption;
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

        private PPFMaturity convertToPPFMaturity(DataRow dr)
        {
            PPFMaturity maturity = new PPFMaturity();
            maturity.InvesterName = dr.Field<string>("InvesterName");
            maturity.ClientName = dr.Field<string>("Name");
            maturity.AccountNo = dr.Field<string>("AccountNo");
            maturity.BankName = dr.Field<string>("Bank");
            maturity.MaturityDate = dr.Field<DateTime>("MaturityDate");
            //maturity.IsAccountExtended = (dr["IsAccountExtended"] == null) ? false : bool.Parse(dr.Field<string>("IsAccountExtended"));

            return maturity;
        }

        public PPF Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: PPF by id process start");
                PPF PPF = new PPF();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    PPF = convertToPPF(dr);
                }
                Logger.LogInfo("Get: PPF by id process completed");
                return PPF;
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

        public void Add(PPF PPF)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,PPF.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_PPF,
                      PPF.Pid, PPF.InvesterName, PPF.AccountNo,
                      PPF.Bank, 
                      PPF.OpeningDate.ToString("yyyy-MM-dd hh:mm:ss"),                     
                      PPF.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      PPF.CurrentValue,
                      PPF.GoalId,
                      PPF.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), PPF.CreatedBy,
                      PPF.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), PPF.UpdatedBy,
                      PPF.InvestmentReturnRate,
                      PPF.IsAccountExtended), true);

                Activity.ActivitiesService.Add(ActivityType.CreatePPF, EntryStatus.Success,
                         Source.Server, PPF.UpdatedByUserName, PPF.AccountNo, PPF.MachineName);
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

        public void Update(PPF ppf)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,ppf.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_PPF,
                      ppf.InvesterName,
                      ppf.AccountNo,
                      ppf.Bank,
                      ppf.OpeningDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      ppf.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      ppf.CurrentValue,                  
                      (ppf.GoalId == null) ? null : ppf.GoalId.Value.ToString(),
                      ppf.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      ppf.UpdatedBy,ppf.InvestmentReturnRate,
                      ppf.IsAccountExtended,
                      ppf.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdatePPF, EntryStatus.Success,
                         Source.Server, ppf.UpdatedByUserName, ppf.AccountNo, ppf.MachineName);
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

        public void Delete(PPF ppf)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,ppf.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_PPF,
                      ppf.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeletePPF, EntryStatus.Success,
                         Source.Server, ppf.UpdatedByUserName, ppf.AccountNo, ppf.MachineName);
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
        private PPF convertToPPF(DataRow dr)
        {
            PPF PPF = new PPF();
            PPF.Id = dr.Field<int>("ID");
            PPF.Pid = dr.Field<int>("PID");
            PPF.InvesterName = dr.Field<string>("InvesterName");
            PPF.AccountNo = dr.Field<string>("AccountNo");
            PPF.Bank = dr.Field<string>("Bank");
            PPF.OpeningDate = dr.Field<DateTime>("OpeningDate");            
            PPF.CurrentValue = Double.Parse(dr["CurrentValue"].ToString());
            PPF.MaturityDate = dr.Field<DateTime>("MaturityDate");
            PPF.GoalId = dr.Field<int>("GoalId");
            PPF.UpdatedBy = dr.Field<int>("UpdatedBy");
            PPF.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            PPF.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            PPF.UpdatedBy = dr.Field<int>("UpdatedBy");
            PPF.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            PPF.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            PPF.InvestmentReturnRate = float.Parse(dr["InvestmentReturnRate"].ToString());
            PPF.IsAccountExtended = dr.Field<bool>("AccountExtended");
            return PPF;
        }
    }
}
