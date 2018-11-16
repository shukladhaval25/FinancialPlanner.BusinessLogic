using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
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
    public class AreaService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Areas C1, USERS U WHERE C1.UPDATEDBY = U.ID";

        private const string INSERT_QUERY = "INSERT INTO Areas VALUES ('{0}','{1}',{2},'{3}',{4})";

        private const string DELETE_BY_ID = "DELETE FROM Areas WHERE NAME ='{0}'";

        public IList<Area> Get()
        {
            try
            {
                Logger.LogInfo("Get: CRM group process start");
                IList<Area> lstArea = new List<Area>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Area Area = convertToAreaObject(dr);
                    lstArea.Add(Area);
                }
                Logger.LogInfo("Get: CRM group process completed.");
                return lstArea;
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

        public void Add(Area Area)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   Area.Name,
                   Area.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Area.CreatedBy,
                   Area.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Area.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateArea, EntryStatus.Success,
                         Source.Server, Area.UpdatedByUserName, Area.Name, Area.MachineName);
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

        public void Delete(Area Area)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, Area.Name));
                Activity.ActivitiesService.Add(ActivityType.DeleteArea, EntryStatus.Success,
                         Source.Server, Area.UpdatedByUserName, Area.Name, Area.MachineName);
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

        private Area convertToAreaObject(DataRow dr)
        {
            Area Area = new Area ();
            Area.Name = dr.Field<string>("Name");
            return Area;
        }
    }
}
