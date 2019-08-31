using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ApplicationMaster
{
    public class AMCService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM AMC C1, USERS U WHERE C1.UPDATEDBY = U.ID";

        private const string INSERT_QUERY = "INSERT INTO AMC VALUES ('{0}','{1}',{2},'{3}',{4})";
        private const string UPDATE_QUERY = "UPDATE AMC SET NAME ='{0}',[UpdatedOn] = '{1}', [UpdatedBy] = {2} WHERE ID = {3}";

        private const string DELETE_BY_ID = "DELETE FROM AMC WHERE ID ='{0}'";

        public IList<AMC> Get()
        {
            try
            {
                Logger.LogInfo("Get: AMC process start");
                IList<AMC> lstAMC = new List<AMC>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    AMC AMC = convertToAMCObject(dr);
                    lstAMC.Add(AMC);
                }
                Logger.LogInfo("Get: AMC process completed.");
                return lstAMC;
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

        public void Add(AMC AMC)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   AMC.Name,
                   AMC.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), AMC.CreatedBy,
                   AMC.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), AMC.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateAMC, EntryStatus.Success,
                         Source.Server, AMC.UpdatedByUserName, AMC.Name, AMC.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Update(AMC AMC)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   AMC.Name,
                   AMC.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), AMC.UpdatedBy,
                   AMC.Id));

                Activity.ActivitiesService.Add(ActivityType.UpdateAMC, EntryStatus.Success,
                         Source.Server, AMC.UpdatedByUserName, AMC.Name, AMC.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Delete(AMC AMC)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, AMC.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteAMC, EntryStatus.Success,
                         Source.Server, AMC.UpdatedByUserName, AMC.Name, AMC.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
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

        private AMC convertToAMCObject(DataRow dr)
        {
            AMC AMC = new AMC();
            AMC.Id = dr.Field<int>("ID");
            AMC.Name = dr.Field<string>("Name");
            AMC.UpdatedBy = dr.Field<int>("UpdatedBy");
            AMC.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            AMC.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            AMC.CreatedBy = dr.Field<int>("CreatedBy");
            AMC.CreatedOn = dr.Field<DateTime>("CreatedOn");
            AMC.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return AMC;
        }
    }
}
