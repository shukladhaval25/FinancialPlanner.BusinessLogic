using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.Masters;
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
    public class ARNService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM ARN C1, USERS U WHERE C1.UPDATEDBY = U.ID";

        private const string INSERT_QUERY = "INSERT INTO ARN VALUES ('{0}','{1}','{2}',{3},'{4}',{5})";
        private const string UPDATE_QUERY = "UPDATE ARN SET ARNNumber = '{0}', NAME ='{1}',[UpdatedOn] = '{2}', [UpdatedBy] = {3} WHERE ID = {4}";

        private const string DELETE_BY_ID = "DELETE FROM ARN WHERE ID ='{0}'";

        public IList<ARN> Get()
        {
            try
            {
                Logger.LogInfo("Get: ARN process start");
                IList<ARN> lstARN = new List<ARN>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ARN ARN = convertToARNObject(dr);
                    lstARN.Add(ARN);
                }
                Logger.LogInfo("Get: ARN process completed.");
                return lstARN;
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

        public void Add(ARN ARN)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   ARN.ArnNumber,
                   ARN.Name,
                   ARN.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ARN.CreatedBy,
                   ARN.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ARN.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateARN, EntryStatus.Success,
                         Source.Server, ARN.UpdatedByUserName, ARN.Name, ARN.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Update(ARN ARN)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   ARN.ArnNumber,
                   ARN.Name,
                   ARN.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ARN.UpdatedBy,
                   ARN.Id));

                Activity.ActivitiesService.Add(ActivityType.UpdateARN, EntryStatus.Success,
                         Source.Server, ARN.UpdatedByUserName, ARN.Name, ARN.MachineName);
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

        public void Delete(ARN ARN)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, ARN.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteARN, EntryStatus.Success,
                         Source.Server, ARN.UpdatedByUserName, ARN.Name, ARN.MachineName);
            }
            catch (Exception ex)
            {
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

        private ARN convertToARNObject(DataRow dr)
        {
            ARN arn = new ARN ();
            arn.Id = dr.Field<int>("ID");
            arn.ArnNumber = dr.Field<string>("ARNNo");
            arn.Name = dr.Field<string>("Name");
            arn.UpdatedBy = dr.Field<int>("UpdatedBy");
            arn.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            arn.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            arn.CreatedBy = dr.Field<int>("CreatedBy");
            arn.CreatedOn = dr.Field<DateTime>("CreatedOn");
            arn.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return arn;
        }
    }
}
