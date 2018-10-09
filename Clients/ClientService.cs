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
    public class ClientService
    {
        private const string INSERT_QUERY = "INSERT INTO CLIENT VALUES (" +
            "'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}'," +
            "'{11}','{12}',{13},'{14}',{15})";

        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENT C1, USERS U WHERE C1.UPDATEDBY = U.ID";
        private const string SELECT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENT C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.ID = {0}";
        
        private const string UPDATE_QUERY = "UPDATE CLIENT SET  NAME = '{0}'," +
                "FATHERNAME = '{1}', MOTHERNAME = '{2}',GENDER ='{3}',DOB ='{4}',PAN ='{5}', AADHAR = '{6}'," +
                "PLACEOFBIRTH ='{7}',Married ='{8}',MARRIAGEANNIVERSARY ='{9}', Occupation = '{10}'," +
                "INCOMESLAB = '{11}', UPDATEDON = '{12}', UPDATEDBY = {13} WHERE ID= {14}";
        private const string DELETE_QUERY = "DELETE FROM USERS WHERE ID = {0}";

        public IList<Client> Get()
        {
            IList<Client> lstClients = new List<Client>();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(SELECT_ALL);
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                Client client = convertToClientObject(dr);
                lstClients.Add(client);
            }
            return lstClients;
        }

        public Client Get(int id)
        {
            Client client = new Client();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                client = convertToClientObject(dr);               
            }
            return  client;
        }

        private Client convertToClientObject(DataRow dr)
        {
            Client client = new Client();
            client.ID = dr.Field<int>("ID");
            client.Name = dr.Field<string>("Name");
            client.DOB = dr.Field<DateTime>("DOB");
            client.Gender = dr.Field<string>("Gender");
            client.PAN = dr.Field<string>("PAN");
            client.Aadhar = dr.Field<string>("AADHAR");
            client.PlaceOfBirth = dr.Field<string>("PlaceOfBirth");
            client.IsMarried = dr.Field<bool>("Married");
            client.MarriageAnniversary = dr.Field<DateTime>("MarriageAnniversary");
            client.FatherName = dr.Field<string>("FatherName");
            client.MotherName = dr.Field<string>("MotherName");
            client.Occupation = dr.Field<string>("Occupation");
            client.IncomeSlab = dr.Field<string>("IncomeSlab");
            client.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            client.UpdatedBy = dr.Field<int>("UpdatedBy");
            client.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return client;
        }

        public void Add(Client client)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                    client.Name, client.FatherName,client.MotherName,
                    client.Gender,client.DOB.ToString("yyyy-MM-dd"),client.PAN,
                    client.Aadhar,client.PlaceOfBirth,client.IsMarried,
                    ((client.MarriageAnniversary == null) ? null : client.MarriageAnniversary.Value.ToString("yyyy-MM-dd")),client.Occupation,client.IncomeSlab,
                    client.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), client.CreatedBy, client.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), client.UpdatedBy),true);


                Activity.ActivitiesService.Add(ActivityType.CreateClient, EntryStatus.Success,
                         Source.Server, client.UpdatedByUserName, client.Name, client.MachineName);
                DataBase.DBService.CommitTransaction(); 
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
            }
        }
        public void Update(Client client)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                            client.Name, client.FatherName, client.MotherName,
                            client.Gender, client.DOB.ToString("yyyy-MM-dd"), client.PAN,
                            client.Aadhar, client.PlaceOfBirth, client.IsMarried,
                            ((client.MarriageAnniversary == null) ? null : client.MarriageAnniversary.Value.ToString("yyyy-MM-dd")),
                            client.Occupation, client.IncomeSlab,
                            client.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), client.UpdatedBy, client.ID));

                Activity.ActivitiesService.Add(ActivityType.UpdateClient, EntryStatus.Success,
                         Source.Server, client.UpdatedByUserName, client.Name, client.MachineName);
            }
            catch(Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
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
