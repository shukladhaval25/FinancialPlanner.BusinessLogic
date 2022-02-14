using FinancialPlanner.BusinessLogic.Process;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ProspectClients
{
    public class ProspectClientService
    {
        private const string INSERT_PROSPECTCLIENT_QUERY = "INSERT INTO PROSPECTCLIENT VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9},'{10}',{11},'{12}','{13}','{14}','{15}',{16},null)";

        private const string SELECT_ALL = "SELECT U1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PROSPECTCLIENT U1, USERS U WHERE U1.UPDATEDBY = U.ID AND U1.IsConvertedToClient = 'FALSE' ORDER BY U1.NAME ASC";
        private const string SELECT_BY_ID = "SELECT U1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PROSPECTCLIENT U1, USERS U WHERE U1.UPDATEDBY = U.ID and U1.ID = {0}";
        private const string SELECT_BY_NAME = "SELECT U1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PROSPECTCLIENT U1, USERS U WHERE U1.UPDATEDBY = U.ID and U1.NAME LIKE '%{0}%'";
        private const string SELECT_MAX_ID = "SELECT MAX(ID) FROM PROSPECTCLIENT";
        

        private const string UPDATE_PROSPECTCLIENT_QUERY = "UPDATE PROSPECTCLIENT SET NAME = '{0}',PHONENO ='{1}',EMAIL ='{2}',OCCUPATION ='{3}'," +
                "EVENT = '{4}', EVENTDATE = '{5}',REFEREDBY ='{6}',UPDATEDON ='{7}',UPDATEDBY = {8},ISCONVERTEDTOCLIENT ='{9}', STOPSENDINGEMAIL ='{10}',Remarks ='{11}'," +
            "INTRODUCTIONCOMPLETED = '{12}', INTRODUCTIONCOMPLETEDDATE = '{13}',CLIENTASSIGNTO = {14},CLIENTID = {15} WHERE ID= {16}";

        private const string DELETE_QUERY = "DELETE FROM PROSPECTCLIENT WHERE ID = {0}";
        private const string DELETE_CONVERSATION_QUERY = "DELETE FROM PROSPECTCLIENTCONVERSATION WHERE PROSPECTCLIENTID = {0}";


        #region "Client Process SQL Query"
        private const string INSERT_CLIENT_PROCESS = "INSERT INTO CLIENTPROCESS (ClientId," +
            "PrimaryStepId,LinkSubStepId,Status,IsProspectClient) VALUES ({0},{1},{2},'{3}',{4})";
        private const string SELECT_MAX_CLIENTPROCESS_ID = "SELECT MAX(ID) FROM CLIENTPROCESS";

        private const string SELECT_PRIMARY_STEP = "select PrimaryStep.*, Users.Id AS UserId from PrimaryStep, Users where StepNo = {0} and Users.DesignationId = PrimaryStep.PrimaryResponsibility";

        private const string INSERT_CLIENT_PROCESS_DETAILS = "INSERT INTO ClientProcessDetail VALUES ({0},{1},'{2}','{3}','{4}','Null')";

        private const string SELECT_PRIMARY_LINKSUBSTEP_ID = "SELECT TOP(1) P.[Id] as PrimaryStepId," +
            " L.[Id] as LinkSubStepId FROM[PrimaryStep] P LEFT join LinkSubStep L on  P.Id = L.PrimaryStepId" +
            " Where P.[Id] = {0} order by P.StepNo, L.StepNo";

        #endregion

        public IList<ProspectClient> GetByName(string name)
        {
            IList<ProspectClient> lstProspClient = new List<ProspectClient>();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_NAME, name));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                ProspectClient prospClient = convertToProspectClientject(dr);
                lstProspClient.Add(prospClient);
            }
            return lstProspClient;
        }

        public ProspectClient GetById(int id)
        {
            ProspectClient prospClient = new ProspectClient();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                prospClient = convertToProspectClientject(dr);
                ProspectClientConversationService prospClientConvService = new ProspectClientConversationService();
                prospClient.ProspectClientConversationList = (List<ProspectClientConversation>)prospClientConvService.GetByProspectClientId(prospClient.ID);
            }
            return prospClient;
        }

        public IList<ProspectClient> GetAll()
        {
            IList<ProspectClient> lstProspClient = new List<ProspectClient>();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_ALL);
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                ProspectClient prospClient = convertToProspectClientject(dr);
                lstProspClient.Add(prospClient);
            }
            return lstProspClient;
        }

        public void AddProspectClient(ProspectClient prospectClient)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_PROSPECTCLIENT_QUERY,
                     prospectClient.Name, prospectClient.PhoneNo, prospectClient.Email,
                     prospectClient.Occupation, prospectClient.Event, prospectClient.EventDate.ToString("yyyy-MM-dd hh:mm:ss"),
                     prospectClient.ReferedBy, prospectClient.Remarks,
                     prospectClient.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), prospectClient.CreatedBy,
                     prospectClient.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), prospectClient.UpdatedBy,
                     prospectClient.IsConvertedToClient, prospectClient.StopSendingEmail,
                     prospectClient.IntroductionCompleted,
                     prospectClient.IntroductionCompletedDate.ToString("yyyy-MM-dd hh:mm:ss"),
                     prospectClient.ClientAssignTo));


                //DataTable dtPrimaryStep = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PRIMARY_STEP, 1));
                DataTable dtStepNo = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PRIMARY_LINKSUBSTEP_ID,1));
                foreach (DataRow dr in dtStepNo.Rows)
                {
                    
                    int maxClientId = int.Parse(DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_ID));
                    ClientProcessService clientProcessService = new ClientProcessService();
                    ClientProcess clientProcess = new ClientProcess();
                    clientProcess.ClientId = maxClientId;
                    clientProcess.PrimaryStepId = dr.Field<int>("PrimaryStepId");
                    clientProcess.LinkStepId = 0;
                    clientProcess.Status = "C";
                    clientProcess.IsProcespectClient = true;
                    if (dr["LinkSubStepId"] != DBNull.Value)
                    {
                        clientProcess.LinkStepId = dr.Field<int>("LinkSubStepId");
                    }
                    DataTable dtPrimaryStep = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PRIMARY_STEP, clientProcess.PrimaryStepId));
                    int assignTo = 0;
                    if (dtPrimaryStep.Rows.Count > 0)
                    {
                        int.TryParse(dtPrimaryStep.Rows[0]["UserId"].ToString(),out assignTo);
                    }
                    
                    clientProcessService.Add(clientProcess, assignTo);
                    
                }
                //DataTable dtPrimaryStep = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PRIMARY_STEP, 1));
                //foreach (DataRow dr in dtPrimaryStep.Rows)
                //{
                //    int maxId = int.Parse(DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_ID));

                //    //Add entry into process step
                //    DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_PROCESS,
                //        maxId, 1, 0, 'C', 1));

                //    int maxProcessId = int.Parse(DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_CLIENTPROCESS_ID));

                //    DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_PROCESS_DETAILS,
                //        maxProcessId,
                //        dr["UserId"].ToString(),
                //        DateTime.Now.ToString("yyyy-MM-dd"),
                //        DateTime.Now.ToString("yyyy-MM-dd"),
                //        DateTime.Now.ToString("yyyy-MM-dd")
                //        ));
                //}


                Activity.ActivitiesService.Add(ActivityType.CreateProspectClient, EntryStatus.Success,
                         Source.Client, prospectClient.UpdatedByUserName, prospectClient.Name, prospectClient.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
                throw ex;
            }
        }

        public void UpdateProspectClient(ProspectClient prospectClient)
        {
            try
            {
                bool isConvertedToClien = false;
                bool isIntroductionCompleted = false;
                DataTable dtPropClient = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, prospectClient.ID));
                foreach (DataRow dr in dtPropClient.Rows)
                {
                    isConvertedToClien = bool.Parse(dr["IsConvertedToClient"].ToString());
                    isIntroductionCompleted = bool.Parse(dr["IntroductionCompleted"].ToString());
                }


                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_PROSPECTCLIENT_QUERY,
                 prospectClient.Name, prospectClient.PhoneNo, prospectClient.Email,
                 prospectClient.Occupation, prospectClient.Event, prospectClient.EventDate.ToString("yyyy-MM-dd hh:mm:ss"),
                 prospectClient.ReferedBy,
                 prospectClient.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), prospectClient.UpdatedBy,
                 prospectClient.IsConvertedToClient, prospectClient.StopSendingEmail,
                 prospectClient.Remarks,
                 prospectClient.IntroductionCompleted,
                 prospectClient.IntroductionCompletedDate.ToString("yyyy-MM-dd hh:mm:ss"),
                 prospectClient.ClientAssignTo,
                 prospectClient.ClientId,
                 prospectClient.ID));

                //Converted to client from prospect list.
                if (!isConvertedToClien.Equals(prospectClient.IsConvertedToClient) && prospectClient.IsConvertedToClient)
                {
                    // Add code for task to convert to client.
                    //DataTable dtPrimaryStep = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PRIMARY_LINKSUBSTEP_ID, 2));

                    DataTable dtStepNo = DataBase.DBService.ExecuteCommand(
                        string.Format(SELECT_PRIMARY_LINKSUBSTEP_ID,2));

                    foreach (DataRow dr in dtStepNo.Rows)
                    {
                        //int maxId = int.Parse(DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_ID));

                        //Add entry into process step
                        ClientProcessService clientProcessService = new ClientProcessService();
                        ClientProcess clientProcess = new ClientProcess();
                        clientProcess.ClientId = prospectClient.ClientId;
                        clientProcess.PrimaryStepId = dr.Field<int>("PrimaryStepId");
                        clientProcess.LinkStepId = 1;
                        clientProcess.Status = "P";
                        clientProcess.IsProcespectClient = false;
                        if (dr["LinkSubStepId"] != DBNull.Value)
                        {
                            clientProcess.LinkStepId = dr.Field<int>("LinkSubStepId");
                        }

                        int assignTo = 0;
                        DataTable dtPrimaryStep = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PRIMARY_STEP, clientProcess.PrimaryStepId));

                        if (dtPrimaryStep.Rows.Count > 0)
                        {
                            int.TryParse(dtPrimaryStep.Rows[0]["UserId"].ToString(), out assignTo);
                        }
                        if (dtPrimaryStep.Rows.Count > 0)
                        {
                            int.TryParse(dtPrimaryStep.Rows[0]["UserId"].ToString(), out assignTo);
                        }
                        clientProcessService.Add(clientProcess, prospectClient.ClientAssignTo);


                        //DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_PROCESS,
                        //    prospectClient.ID, 2, 1, 'C', 1));

                        //int maxProcessId = int.Parse(DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_CLIENTPROCESS_ID));

                        //DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_PROCESS_DETAILS,
                        //    maxProcessId,
                        //    dr["UserId"].ToString(),
                        //    DateTime.Now.ToString("yyyy-MM-dd"),
                        //    DateTime.Now.ToString("yyyy-MM-dd"),
                        //    DateTime.Now.ToString("yyyy-MM-dd")
                        //    ));
                    }

                }

                if (!isIntroductionCompleted.Equals(prospectClient.IntroductionCompleted) && prospectClient.IntroductionCompleted)
                {
                    // Add code for tas to introduction completed.
                }


                Activity.ActivitiesService.Add(ActivityType.UpdateProspectClient, EntryStatus.Success,
                         Source.Client, prospectClient.UpdatedByUserName, prospectClient.Name, prospectClient.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
                throw ex;
            }
        }

        public void DeleteProspectClient(ProspectClient prospectClient)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_CONVERSATION_QUERY, prospectClient.ID), true); ;
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY, prospectClient.ID), true);
                DataBase.DBService.CommitTransaction();

                Activity.ActivitiesService.Add(ActivityType.DeleteProspectClient, EntryStatus.Success,
                           Source.Client, prospectClient.UpdatedByUserName, prospectClient.Name, prospectClient.MachineName);
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
            }
        }

        private ProspectClient convertToProspectClientject(DataRow dr)
        {
            ProspectClient prospClient = new ProspectClient();
            prospClient.ID = dr.Field<int>("ID");
            prospClient.Name = dr.Field<string>("Name");
            prospClient.PhoneNo = dr.Field<string>("PhoneNo");
            prospClient.Email = dr.Field<string>("Email");
            prospClient.Occupation = dr.Field<string>("Occupation");
            prospClient.Event = dr.Field<string>("Event");
            prospClient.EventDate = dr.Field<DateTime>("EventDate");
            prospClient.ReferedBy = dr.Field<string>("ReferedBy");
            prospClient.Remarks = dr.Field<string>("Remarks");
            prospClient.CreatedOn = dr.Field<DateTime>("CreatedOn");
            prospClient.CreatedBy = dr.Field<int>("CreatedBy");
            prospClient.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            prospClient.UpdatedBy = dr.Field<int>("UpdatedBy");
            prospClient.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            prospClient.IsConvertedToClient = dr.Field<bool>("IsConvertedToClient");
            prospClient.StopSendingEmail = dr.Field<bool>("StopSendingEmail");
            prospClient.IntroductionCompleted = dr.Field<bool>("IntroductionCompleted");
            if (dr["IntroductionCompletedDate"] != DBNull.Value)
            {
                prospClient.IntroductionCompletedDate = dr.Field<DateTime>("IntroductionCompletedDate");
            }
            if (dr["ClientAssignTo"] != DBNull.Value)
            {
                prospClient.ClientAssignTo = dr.Field<int>("ClientAssignTo");
            }   
            if (dr["ClientId"] != DBNull.Value)
            {
                prospClient.ClientId = dr.Field<int>("ClientId");
            }
            return prospClient;
        }
    }
}