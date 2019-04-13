using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.Masters;
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
    public class ClientRatingService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM ClientRating C1, USERS U WHERE C1.UPDATEDBY = U.ID";

        private const string INSERT_QUERY = "INSERT INTO ClientRating VALUES ('{0}','{1}',{2},'{3}',{4})";

        private const string DELETE_BY_ID = "DELETE FROM ClientRating WHERE RATING ='{0}'";

        public IList<ClientRating> Get()
        {
            try
            {
                Logger.LogInfo("Get: ClientRating process start");
                IList<ClientRating> lstClientRating = new List<ClientRating>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ClientRating ClientRating = convertToClientRatingObject(dr);
                    lstClientRating.Add(ClientRating);
                }
                Logger.LogInfo("Get: ClientRating process completed.");
                return lstClientRating;
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

        public void Add(ClientRating ClientRating)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   ClientRating.Rating,
                   ClientRating.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ClientRating.CreatedBy,
                   ClientRating.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ClientRating.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateClientRating, EntryStatus.Success,
                         Source.Server, ClientRating.UpdatedByUserName, ClientRating.Rating, ClientRating.MachineName);
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

        public void Delete(ClientRating ClientRating)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, ClientRating.Rating));
                Activity.ActivitiesService.Add(ActivityType.DeleteClientRating, EntryStatus.Success,
                         Source.Server, ClientRating.UpdatedByUserName, ClientRating.Rating, ClientRating.MachineName);
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

        private ClientRating convertToClientRatingObject(DataRow dr)
        {
            ClientRating ClientRating = new ClientRating ();
            ClientRating.Rating = dr.Field<string>("Rating");
            return ClientRating;
        }
    }
}
