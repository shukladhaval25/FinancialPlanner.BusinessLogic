using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.CurrentStatus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.CurrentStatus
{
    public class NSCService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NSC N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NSC N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_NSC= "INSERT INTO NSC VALUES (" +
            "{0},'{1}','{2}','{3}',{4},'{5}',{6},{7},{8},{9},'{10}',{11},'{12}',{13})";

        const string UPDATE_NSC = "UPDATE NSC SET " +
            "[INVESTERNAME] = '{0}'," +            
            "[PostOfficeBranch] ='{1}', DOCUMENTNO ='{2}', RATE = {3}, MATURITYDATE = '{4}'," +
            "UNITS = {5}, VALUEOFONE ={6}, CURRENTVALUE ={7}, GoalId ={8}, " +
            "[UpdatedOn] = '{9}', [UpdatedBy] ={10} " +
            "WHERE ID = {11} ";

        const string DELETE_NSC = "DELETE FROM NSC WHERE ID = {0}";


        public IList<NSC> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: NSC process start");
                IList<NSC> lstNSCOption = new List<NSC>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    NSC mf = convertToNSC(dr);
                    lstNSCOption.Add(mf);
                }
                Logger.LogInfo("Get: NSC fund process completed.");
                return lstNSCOption;
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


        public NSC Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: NSC by id process start");
                NSC NSC = new NSC();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    NSC = convertToNSC(dr);
                }
                Logger.LogInfo("Get: NSC by id process completed");
                return NSC;
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

        public void Add(NSC NSC)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,NSC.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_NSC,
                      NSC.Pid, NSC.InvesterName, NSC.PostOfficeBranch,
                      NSC.DocumentNo,
                      NSC.Rate,
                      NSC.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      NSC.Units,
                      NSC.ValueOfOne,
                      NSC.CurrentValue,
                      NSC.GoalId,
                      NSC.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), NSC.CreatedBy,
                      NSC.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), NSC.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateNSC, EntryStatus.Success,
                         Source.Server, NSC.UpdatedByUserName, NSC.DocumentNo, NSC.MachineName);
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

        public void Update(NSC NSC)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,NSC.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_NSC,
                      NSC.InvesterName,
                      NSC.PostOfficeBranch,
                      NSC.DocumentNo,
                      NSC.Rate,                      
                      NSC.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      NSC.Units,
                      NSC.ValueOfOne,
                      NSC.CurrentValue,
                      (NSC.GoalId == null) ? null : NSC.GoalId.Value.ToString(),
                      NSC.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      NSC.UpdatedBy,
                      NSC.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateNSC, EntryStatus.Success,
                         Source.Server, NSC.UpdatedByUserName, NSC.DocumentNo, NSC.MachineName);
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

        public void Delete(NSC NSC)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,NSC.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_NSC,
                      NSC.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteNSC, EntryStatus.Success,
                         Source.Server, NSC.UpdatedByUserName, NSC.DocumentNo, NSC.MachineName);
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
        private NSC convertToNSC(DataRow dr)
        {
            NSC NSC = new NSC();
            NSC.Id = dr.Field<int>("ID");
            NSC.Pid = dr.Field<int>("PID");
            NSC.InvesterName = dr.Field<string>("InvesterName");
            NSC.PostOfficeBranch = dr.Field<string>("PostOfficeBranch");
            NSC.DocumentNo = dr.Field<string>("DocumentNo");
            NSC.Rate = float.Parse( dr["Rate"].ToString());
            NSC.MaturityDate = dr.Field<DateTime>("MaturityDate");
            NSC.Units = dr.Field<int>("Units");
            NSC.ValueOfOne = float.Parse(dr["ValueOfOne"].ToString());
            NSC.CurrentValue = Double.Parse(dr["CurrentValue"].ToString());            
            NSC.GoalId = dr.Field<int>("GoalId");
            NSC.UpdatedBy = dr.Field<int>("UpdatedBy");
            NSC.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            NSC.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            NSC.UpdatedBy = dr.Field<int>("UpdatedBy");
            NSC.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            NSC.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return NSC;
        }
    }
}
