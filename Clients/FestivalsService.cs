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

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class FestivalsService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Festivals C1, USERS U WHERE C1.UPDATEDBY = U.ID";
        //private const string SELECT_ALL_BY_CLIENT_ID_AND_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Festivals C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.ID = {0} AND C1.CID = {1}";

        private const string INSERT_QUERY = "INSERT INTO Festivals VALUES ('{0}','{1}','{2}',{3},'{4}',{5})";
        private const string UPDATE_QUERY = "UPDATE Festivals SET RELIGION = '{0}', NAME ='{1}',UPDATEDON ='{6}',UPDATEDBY ={7} WHERE NAME ={1}";

        private const string DELETE_BY_ID = "DELETE FROM Festivals WHERE NAME ='{0}'";

        public IList<Festivals> Get()
        {
            try
            {
                Logger.LogInfo("Get: Festival process start");
                IList<Festivals> lstFestivals = new List<Festivals>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Festivals Festivals = convertToFestivalsObject(dr);
                    lstFestivals.Add(Festivals);
                }
                Logger.LogInfo("Get: Festival process completed.");
                return lstFestivals;
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

        public void Add(Festivals Festivals)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Festivals.Cid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   Festivals.Religion, Festivals.Name,
                   Festivals.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Festivals.CreatedBy,
                   Festivals.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Festivals.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateFestivals, EntryStatus.Success,
                         Source.Server, Festivals.UpdatedByUserName, Festivals.Name, Festivals.MachineName);
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
     
        public void Delete(Festivals Festivals)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, Festivals.Name));
                Activity.ActivitiesService.Add(ActivityType.DeleteFestivals, EntryStatus.Success,
                         Source.Server, Festivals.UpdatedByUserName, Festivals.Name, Festivals.MachineName);
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

        private Festivals convertToFestivalsObject(DataRow dr)
        {
            Festivals festivals = new Festivals ();
            festivals.Religion = dr.Field<string>("Religion");           
            festivals.Name = dr.Field<string>("Name");            
            return festivals;
        }
    }
}
