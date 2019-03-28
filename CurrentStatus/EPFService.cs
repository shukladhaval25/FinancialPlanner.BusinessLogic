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
    public class EPFService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM EPF N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM EPF N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_EPF= "INSERT INTO EPF VALUES (" +
            "{0},'{1}','{2}','{3}',{4},{5},{6},'{7}',{8},'{9}',{10})";

        const string UPDATE_EPF = "UPDATE EPF SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[PARTICULAR] ='{2}',AMOUNT ={3}, INVESTMENTRETURNRATE = {4}, GoalId ={5}, " +
            "[UpdatedOn] = '{6}', [UpdatedBy] ={7} " +
            "WHERE ID = {8} ";

        const string DELETE_EPF = "DELETE FROM EPF WHERE ID = {0}";


        public IList<EPF> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: EPF process start");
                IList<EPF> lstEPFOption = new List<EPF>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    EPF mf = convertToEPF(dr);
                    lstEPFOption.Add(mf);
                }
                Logger.LogInfo("Get: EPF fund process completed.");
                return lstEPFOption;
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


        public EPF Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: EPF by id process start");
                EPF EPF = new EPF();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    EPF = convertToEPF(dr);
                }
                Logger.LogInfo("Get: EPF by id process completed");
                return EPF;
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

        public void Add(EPF EPF)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,EPF.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_EPF,
                      EPF.Pid, EPF.InvesterName, EPF.AccountNo,
                      EPF.Particular,
                      EPF.Amount,                    
                      EPF.GoalId,
                      EPF.InvestmentReturnRate,
                      EPF.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), EPF.CreatedBy,
                      EPF.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), EPF.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateEPF, EntryStatus.Success,
                         Source.Server, EPF.UpdatedByUserName, EPF.AccountNo, EPF.MachineName);
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

        public void Update(EPF EPF)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,EPF.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_EPF,
                      EPF.InvesterName,
                      EPF.AccountNo,
                      EPF.Particular,                     
                      EPF.Amount,
                      EPF.InvestmentReturnRate,
                      (EPF.GoalId == null) ? null : EPF.GoalId.Value.ToString(),
                      EPF.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      EPF.UpdatedBy,
                      EPF.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateEPF, EntryStatus.Success,
                         Source.Server, EPF.UpdatedByUserName, EPF.AccountNo, EPF.MachineName);
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

        public void Delete(EPF EPF)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,EPF.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_EPF,
                      EPF.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteEPF, EntryStatus.Success,
                         Source.Server, EPF.UpdatedByUserName, EPF.AccountNo, EPF.MachineName);
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
        private EPF convertToEPF(DataRow dr)
        {
            EPF EPF = new EPF();
            EPF.Id = dr.Field<int>("ID");
            EPF.Pid = dr.Field<int>("PID");
            EPF.InvesterName = dr.Field<string>("InvesterName");
            EPF.AccountNo = dr.Field<string>("AccountNo");
            EPF.Particular = dr.Field<string>("Particular");
            EPF.Amount = Double.Parse(dr["Amount"].ToString());
            EPF.InvestmentReturnRate = float.Parse(dr["INVESTMENTRETURNRATE"].ToString());
            EPF.GoalId = dr.Field<int>("GoalId");
            EPF.UpdatedBy = dr.Field<int>("UpdatedBy");
            EPF.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            EPF.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            EPF.UpdatedBy = dr.Field<int>("UpdatedBy");
            EPF.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            EPF.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return EPF;
        }
    }
}
