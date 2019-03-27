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
    public class ShareService
    {

        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Shares N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Shares N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string INSERT_SHARES = "INSERT INTO SHARES VALUES (" +
            "{0},'{1}','{2}',{3},{4},{5},{6},{7},'{8}',{9},'{10}',{11},'{12}','{13}'," +
            "'{14}',{15})";

        const string UPDATE_SHARES = "UPDATE SHARES SET " +
            "[INVESTERNAME] = '{0}'," +
            "[COMPANYNAME] = '{1}'," +
            "[FACEVALUE] ={2}, NOOFSHARES ={3},MARKETPRICE = {4},CURRENTVALUE ={5}," +
            "GoalId ={6}, " +
            "[UpdatedOn] = '{7}', [UpdatedBy] ={8}," +
            "FIRSTHOLDER = '{9}', SECONDHOLDER ='{10}', NOMINEE = '{11}', INVESTMENTRETURNRATE = {12} " +
            "WHERE ID = {13} ";

        const string DELETE_Shares = "DELETE FROM SHARES WHERE ID = {0}";
        public IList<Shares> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Shares process start");
                IList<Shares> lstSharesOption = new List<Shares>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Shares mf = convertToShares(dr);
                    lstSharesOption.Add(mf);
                }
                Logger.LogInfo("Get: Shares process completed.");
                return lstSharesOption;
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


        public void Add(Shares shares)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,shares.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SHARES,
                      shares.Pid, shares.InvesterName, shares.CompanyName,
                      shares.FaceValue,
                      shares.NoOfShares, shares.MarketPrice, shares.CurrentValue,
                      shares.GoalID,
                      shares.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), shares.CreatedBy,
                      shares.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), shares.UpdatedBy,
                      shares.FirstHolder,shares.SecondHolder,shares.Nominee,
                      shares.InvestmentReturnRate), true);

                Activity.ActivitiesService.Add(ActivityType.CreateShares, EntryStatus.Success,
                         Source.Server, shares.UpdatedByUserName, "Shares", shares.MachineName);
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

        public void Update(Shares Shares)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Shares.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SHARES,
                      Shares.InvesterName,
                      Shares.CompanyName,
                      Shares.FaceValue,
                      Shares.NoOfShares,
                      Shares.MarketPrice, Shares.CurrentValue,
                      (Shares.GoalID == null) ? null : Shares.GoalID.Value.ToString(),
                      Shares.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      Shares.UpdatedBy,
                      Shares.FirstHolder,Shares.SecondHolder,Shares.Nominee,
                      Shares.InvestmentReturnRate,
                      Shares.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateShares, EntryStatus.Success,
                         Source.Server, Shares.UpdatedByUserName, "Shares", Shares.MachineName);
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

        public void Delete(Shares Shares)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Shares.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_Shares,
                      Shares.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteShares, EntryStatus.Success,
                         Source.Server, Shares.UpdatedByUserName, "Shares", Shares.MachineName);
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
        private Shares convertToShares(DataRow dr)
        {
            Shares Shares = new Shares();
            Shares.Id = dr.Field<int>("ID");
            Shares.Pid = dr.Field<int>("PID");
            Shares.InvesterName = dr.Field<string>("InvesterName");
            Shares.CompanyName = dr.Field<string>("CompanyName");
            Shares.FaceValue = float.Parse(dr["FaceValue"].ToString());
            Shares.NoOfShares = dr.Field<int>("NoOfShares");
            Shares.MarketPrice = float.Parse(dr["MarketPrice"].ToString());
            Shares.CurrentValue = double.Parse(dr["CurrentValue"].ToString());
            Shares.GoalID = dr.Field<int>("GoalId");
            Shares.UpdatedBy = dr.Field<int>("UpdatedBy");
            Shares.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            Shares.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            Shares.FirstHolder = dr.Field<string>("FirstHolder");
            Shares.SecondHolder = dr.Field<string>("SecondHolder");
            Shares.Nominee = dr.Field<string>("Nominee");
            Shares.InvestmentReturnRate = float.Parse(dr["InvestmentReturnRate"].ToString());
            return Shares;
        }
    }
}
