﻿using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class ClientContactService
    {
        private const string SELECT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENTCONTACT C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.CID = {0}";
        private const string UPDATE_QUERY = "UPDATE CLIENTCONTACT SET ADD1 = '{0}',STREET ='{1}'," +
            "CITY = '{2}', STATE ='{3}',PIN ='{4}',EMAIL ='{5}',SPOUSEEMAIL ='{6}', " +
            "PRIMARYEMAIL ='{7}',MOBILENO = '{8}', SPOUSEMOBILENO ='{9}'," +
            "PRIMARYMOBILENO ='{10}',UPDATEDON = '{11}', UPDATEDBY = {12} , AREA = '{13}'," +
            "PREFEREDTIME = '{14}', COUNTRY = '{16}' WHERE CID= {15}";
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string IS_RECORD_EXIST = "SELECT COUNT(*) FROM CLIENTCONTACT WHERE CID = {0}";
        private const string INSERT_QUERY = "INSERT INTO CLIENTCONTACT VALUES ({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}'," +
            "'{8}','{9}','{10}','{11}','{12}',{13},'{14}',{15},'{16}','{17}','{18}')";

        private const string GET_CLIENT_PRIMARY_CONTACT = "SELECT  Client.ID, Client.Name, ClientContact.PrimaryEmail, ClientContact.PrimaryMobileNo FROM Client " +
            " INNER JOIN ClientContact ON Client.ID = ClientContact.CID" +
            " INNER JOIN Planner ON CLIENT.ID = Planner.ClientId " +
            " And (client.IsActive = 1 AND Planner.IsDeleted = 0  AND " +
            "(DATEADD(D,-1, DATEADD(M, 3, Planner.StartDate)) = '{0}' OR" +
            " DATEADD(D,-1, DATEADD(M, 6, Planner.StartDate)) = '{0}' OR" +
            " DATEADD(D,-1, DATEADD(M, 9, Planner.StartDate)) = '{0}'))";

        private const string GET_ANNUALREVIEW_CLIENT_PRIMARY_CONTACT = "SELECT  Client.ID, Client.Name, ClientContact.PrimaryEmail, ClientContact.PrimaryMobileNo FROM Client " +
        " INNER JOIN ClientContact ON Client.ID = ClientContact.CID" +
        " INNER JOIN Planner ON CLIENT.ID = Planner.ClientId " +
        " And (client.IsActive = 1 AND Planner.IsDeleted = 0  AND " +
        "(DATEADD(D,-1, DATEADD(M, 12, Planner.StartDate)) = '{0}'))";

        public ClientContact Get(int id)
        {
            ClientContact clientContact = new ClientContact();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                clientContact = convertToClientContactObject(dr);
            }
            return clientContact;
        }

        private ClientContact convertToClientContactObject(DataRow dr)
        {
            ClientContact clientContact = new ClientContact();
            clientContact.Id = dr.Field<int>("ID");
            clientContact.Cid = dr.Field<int>("CID");
            clientContact.Add1 = dr.Field<string>("Add1");
            clientContact.Street = dr.Field<string>("Street");
            clientContact.City = dr.Field<string>("City");
            clientContact.State = dr.Field<string>("State");
            clientContact.Area = dr.Field<string>("Area");
            clientContact.Pin = dr.Field<string>("Pin");
            clientContact.Email = dr.Field<string>("Email");
            clientContact.SpouseEmail = dr.Field<string>("SpouseEmail");
            clientContact.PrimaryEmail = dr.Field<string>("PrimaryEmail");
            clientContact.Mobile = dr.Field<string>("MobileNo");
            clientContact.Spousemobile = dr.Field<string>("SpouseMobileNo");
            clientContact.PrimaryMobile = dr.Field<string>("PrimaryMobileNo");
            clientContact.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            clientContact.UpdatedBy = dr.Field<int>("UpdatedBy");
            clientContact.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            clientContact.PreferedTime = dr.Field<string>("PreferedTime");
            clientContact.Country = dr.Field<string>("Country");
            return clientContact;
        }
       
        public void Update(ClientContact clientContact)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,clientContact.Cid));
                string value = DataBase.DBService.ExecuteCommandScalar(string.Format(IS_RECORD_EXIST,clientContact.Cid));
                bool isRecordExist = (value.Equals("0")) ? false : true;
                if (isRecordExist)
                {
                    DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                            clientContact.Add1, clientContact.Street, clientContact.City,
                            clientContact.State, clientContact.Pin,
                            clientContact.Email, clientContact.SpouseEmail, clientContact.PrimaryEmail,
                            clientContact.Mobile, clientContact.Spousemobile, clientContact.PrimaryMobile,
                            clientContact.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientContact.UpdatedBy,
                            clientContact.Area, clientContact.PreferedTime,                             
                            clientContact.Cid,clientContact.Country));
                    Activity.ActivitiesService.Add(ActivityType.UpdateClientContact, EntryStatus.Success,
                             Source.Server, clientContact.UpdatedByUserName, clientName, clientContact.MachineName);
                }
                else
                {
                    DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                            clientContact.Cid,
                            clientContact.Add1.Replace("'","''"), 
                            clientContact.Street.Replace("'","''"), clientContact.City,
                            clientContact.State, clientContact.Pin,
                            clientContact.Email, clientContact.SpouseEmail, clientContact.PrimaryEmail,
                            clientContact.Mobile, clientContact.Spousemobile, clientContact.PrimaryMobile,
                            clientContact.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientContact.CreatedBy,
                            clientContact.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientContact.UpdatedBy,
                            clientContact.Area,clientContact.PreferedTime.Replace("'","''"),
                            clientContact.Country));
                    Activity.ActivitiesService.Add(ActivityType.UpdateClientContact, EntryStatus.Success,
                             Source.Server, clientContact.UpdatedByUserName, clientName, clientContact.MachineName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex.Message);
            }
        }

        public IList<ClientPrimaryContact> GetPrimaryContact(DateTime dateTime,bool IsAnnualReview = false)
        {
            IList<ClientPrimaryContact> lstClients = new List<ClientPrimaryContact>();

            DataTable dtAppConfig = (IsAnnualReview) ?
                 DataBase.DBService.ExecuteCommand(
                string.Format(GET_ANNUALREVIEW_CLIENT_PRIMARY_CONTACT, dateTime.ToString("yyyy-MM-dd")))
                : DataBase.DBService.ExecuteCommand(
                string.Format(GET_CLIENT_PRIMARY_CONTACT,dateTime.ToString("yyyy-MM-dd")));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                ClientPrimaryContact client = convertToClientObject(dr);
                lstClients.Add(client);
            }
            return lstClients;
        }

        private ClientPrimaryContact convertToClientObject(DataRow dr)
        {
            ClientPrimaryContact client = new ClientPrimaryContact();
            client.Id = dr.Field<int>("ID");
            client.Name = dr.Field<string>("Name");
            client.PrimaryEmail = dr.Field<string>("PrimaryEmail");
            client.PrimaryMobileNo = dr.Field<string>("PrimaryMobileNo");
            return client;
        }
    }
}
