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
    public class ClientClientFestivalservice
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM ClientFestival C1, USERS U WHERE C1.UPDATEDBY = U.ID";
        private const string SELECT_ALL_BY_CLIENT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM ClientFestival C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.CID = {0}";

        private const string INSERT_QUERY = "INSERT INTO ClientFestival VALUES ({0},'{1}','{2}',{3},'{4}',{5})";
        private const string UPDATE_QUERY = "UPDATE ClientFestival SET RELIGION = '{0}', NAME ='{1}',UPDATEDON ='{6}',UPDATEDBY ={7} WHERE NAME ={1}";

        private const string DELETE_BY_ID = "DELETE FROM ClientFestival WHERE CID ={0}";


        public IList<ClientFestivals> Get(int clientId)
        {
            try
            {
                Logger.LogInfo("Get: Client festival process start");
                IList<ClientFestivals> lstClientFestivals = new List<ClientFestivals>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_CLIENT_ID,clientId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ClientFestivals ClientFestivals = convertToClientFestivalsObject(dr);
                    lstClientFestivals.Add(ClientFestivals);
                }
                Logger.LogInfo("Get: Client festival process completed.");
                return lstClientFestivals;
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

        private ClientFestivals convertToClientFestivalsObject(DataRow dr)
        {
            ClientFestivals clientFestivals = new ClientFestivals ();
            clientFestivals.Id = dr.Field<int>("ID");
            clientFestivals.Cid = dr.Field<int>("CID");
            clientFestivals.Festival = dr.Field<string>("Festival");            
            return clientFestivals;
        }

        public void Add(IList<ClientFestivals> festivals)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,0));
                if (festivals != null && festivals.Count > 0)
                {
                    DataBase.DBService.BeginTransaction();

                    DataBase.DBService.ExecuteCommandString(string.Format(DELETE_BY_ID, festivals[0].Cid), true);

                    foreach (ClientFestivals festival in festivals)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                           festival.Cid, festival.Festival,
                           festival.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), festival.CreatedBy,
                           festival.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), festival.UpdatedBy), true);

                        // Activity.ActivitiesService.Add(ActivityType.CreateFestivals, EntryStatus.Success,
                        //          Source.Server, festival.UpdatedByUserName, festival.Festival, festival.MachineName);
                    }
                    DataBase.DBService.CommitTransaction();
                }
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
    }
}
