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
    public class EmploymentService
    {
        private const string SELECT_CLIENT_EMPLOYMENT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENTEMPLOYMENT C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.CID = {0}";
        private const string SELECT_SPOUSE_EMPLOYMENT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SPOUSEEMPLOYMENT C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.CID = {0}";
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";

        private const string GET_CLIENT_EMPLOYMENT_RECORD = "SELECT COUNT(*) FROM CLIENTEMPLOYMENT WHERE CID ={0}";
        private const string GET_SPOUSE_EMPLOYMENT_RECORD = "SELECT COUNT(*) FROM SPOUSEEMPLOYMENT WHERE CID ={0}";

        private const string INSERT_CLIENT_EMPLOYMENT_QUERY = "INSERT INTO CLIENTEMPLOYMENT VALUES ({0},'{1}','{2}','{3}','{4}'," +
            "'{5}','{6}','{7}','{8}',{9},'{10}',{11})";
        private const string INSERT_SPOUSE_EMPLOYMENT_QUERY = "INSERT INTO SPOUSEEMPLOYMENT VALUES ({0},'{1}','{2}','{3}','{4}'," +
            "'{5}','{6}','{7}','{8}',{9},'{10}',{11})";

        private const string UPDATE_CLIENT_EMPLOYMENT_QUERY = "UPDATE CLIENTEMPLOYMENT SET DESIGNATION = '{0}',"+
            "EMPLOYERNAME = '{1}', ADDRESS = '{2}',STREET ='{3}'," +
            "CITY = '{4}', PIN ='{5}', INCOME ='{6}',UPDATEDON = '{7}', UPDATEDBY = {8} WHERE CID= {9}";
        private const string UPDATE_SPOUSE_EMPLOYMENT_QUERY = "UPDATE SPOUSEEMPLOYMENT SET DESIGNATION = '{0}',"+
            "EMPLOYERNAME = '{1}', ADDRESS = '{2}',STREET ='{3}'," +
            "CITY = '{4}', PIN ='{5}', INCOME ='{6}',UPDATEDON = '{7}', UPDATEDBY = {8} WHERE CID= {9}";

        public Employment Get(int Id)
        {
            try
            {
                Logger.LogInfo("GET: Employment process start.");
                Employment employment = new Employment();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_CLIENT_EMPLOYMENT_ID,Id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    employment.ClientEmployment = convertToClientEmploymentObject(dr);
                }

                dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_SPOUSE_EMPLOYMENT_ID, Id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    employment.SpouseEmployment = convertToSpouseEmploymentObject(dr);
                }
                return employment;
                Logger.LogInfo("GET: Employment process completed.");
            }
            catch (Exception ex)
            {
                DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
                debuggerInfo.ClassName = this.GetType().Name;
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                debuggerInfo.Method = currentMethodName.Name;
                debuggerInfo.ExceptionInfo = ex;
                Logger.LogDebug(debuggerInfo);
                return null;
            }
        }

        private ClientEmployment convertToClientEmploymentObject(DataRow dr)
        {
            ClientEmployment clientEmployment = new ClientEmployment();
            clientEmployment.Id = dr.Field<int>("ID");
            clientEmployment.Cid = dr.Field<int>("CID");
            clientEmployment.Designation = dr.Field<string>("Designation");
            clientEmployment.EmployerName = dr.Field<string>("EmployerName");
            clientEmployment.Address = dr.Field<string>("Address");
            clientEmployment.Street = dr.Field<string>("Street");
            clientEmployment.City = dr.Field<string>("City");
            clientEmployment.Pin = dr.Field<string>("pin");
            clientEmployment.Income = dr.Field<string>("Income");            
            return clientEmployment;
        }

        private SpouseEmployment convertToSpouseEmploymentObject(DataRow dr)
        {
            SpouseEmployment spouseEmployment = new SpouseEmployment();

            spouseEmployment.Id = dr.Field<int>("ID");
            spouseEmployment.Cid = dr.Field<int>("CID");
            spouseEmployment.Designation = dr.Field<string>("Designation");
            spouseEmployment.EmployerName = dr.Field<string>("EmployerName");
            spouseEmployment.Address = dr.Field<string>("Address");
            spouseEmployment.Street = dr.Field<string>("Street");
            spouseEmployment.City = dr.Field<string>("City");
            spouseEmployment.Pin = dr.Field<string>("pin");
            spouseEmployment.Income = dr.Field<string>("Income");
            return spouseEmployment;            
        }

        public void Update(Employment employment)
        {
            try
            {
                updateClientEmployment(employment.ClientEmployment);
                updateSpouseEmployment(employment.SpouseEmployment);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex.Message);
            }
        }

        private void updateClientEmployment(ClientEmployment clientEmployment)
        {
            Logger.LogInfo("UPDATE: Client Employment process start.");
            string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,clientEmployment.Cid));
            string value = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_EMPLOYMENT_RECORD,clientEmployment.Cid));
            bool isRecordExist = (value.Equals("0")) ? false : true;
            if (isRecordExist)
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_CLIENT_EMPLOYMENT_QUERY,
                        clientEmployment.Designation, clientEmployment.EmployerName, clientEmployment.Address,
                        clientEmployment.Street, clientEmployment.City, clientEmployment.Pin, clientEmployment.Income,
                        clientEmployment.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientEmployment.UpdatedBy, clientEmployment.Cid));
                Activity.ActivitiesService.Add(ActivityType.UpdateEmployment, EntryStatus.Success,
                         Source.Server, clientEmployment.UpdatedByUserName, clientName, clientEmployment.MachineName);
            }
            else
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_EMPLOYMENT_QUERY,
                        clientEmployment.Cid, clientEmployment.Designation, clientEmployment.EmployerName,
                        clientEmployment.Address, clientEmployment.Street, clientEmployment.City, clientEmployment.Pin,
                        clientEmployment.Income,
                        clientEmployment.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientEmployment.CreatedBy,
                        clientEmployment.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientEmployment.UpdatedBy));
                Activity.ActivitiesService.Add(ActivityType.UpdateEmployment, EntryStatus.Success,
                         Source.Server, clientEmployment.UpdatedByUserName, clientName, clientEmployment.MachineName);
            }
            Logger.LogInfo("UPDATE: Client Employment process completed.");
        }

        private void updateSpouseEmployment(SpouseEmployment spouseEmployment)
        {
            Logger.LogInfo("UPDATE: Spouse Employment process start.");
            string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,spouseEmployment.Cid));
            string value = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_SPOUSE_EMPLOYMENT_RECORD,spouseEmployment.Cid));
            bool isRecordExist = (value.Equals("0")) ? false : true;
            if (isRecordExist)
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_SPOUSE_EMPLOYMENT_QUERY,
                        spouseEmployment.Designation, spouseEmployment.EmployerName, spouseEmployment.Address,
                        spouseEmployment.Street, spouseEmployment.City, spouseEmployment.Pin, spouseEmployment.Income,
                        spouseEmployment.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), spouseEmployment.UpdatedBy, spouseEmployment.Cid));
                Activity.ActivitiesService.Add(ActivityType.UpdateEmployment, EntryStatus.Success,
                         Source.Server, spouseEmployment.UpdatedByUserName, clientName, spouseEmployment.MachineName);
            }
            else
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_SPOUSE_EMPLOYMENT_QUERY,
                        spouseEmployment.Cid, spouseEmployment.Designation, spouseEmployment.EmployerName,
                        spouseEmployment.Address, spouseEmployment.Street, spouseEmployment.City, spouseEmployment.Pin,
                        spouseEmployment.Income,
                        spouseEmployment.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), spouseEmployment.CreatedBy,
                        spouseEmployment.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), spouseEmployment.UpdatedBy));
                Activity.ActivitiesService.Add(ActivityType.UpdateEmployment, EntryStatus.Success,
                         Source.Server, spouseEmployment.UpdatedByUserName, clientName, spouseEmployment.MachineName);
            }
            Logger.LogInfo("UPDATE: Spouse Employment process completed.");
        }
    }
}
