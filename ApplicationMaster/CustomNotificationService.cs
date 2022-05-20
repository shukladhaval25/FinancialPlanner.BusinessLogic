using FinancialPlanner.Common.Model.CustomNotifier;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ApplicationMaster
{
    public class CustomNotificationService
    {

        private const string DOB_QUERY = "SELECT  Client.Name, Client.DOB, " +
            "ClientSpouse.Name AS Spouse, ClientSpouse.DOB AS [SpouseDOB] " +
            " FROM Client INNER JOIN  ClientSpouse ON Client.ID = ClientSpouse.CID " +
            "WHERE(Client.DOB >= CONVERT(DATETIME, '{0}', 102) and Client.DOB <= CONVERT(DATETIME, '{1}', 102)) or " +
            "(ClientSpouse.DOB >= CONVERT(DATETIME, '{0}', 102) and ClientSpouse.DOB <= CONVERT(DATETIME, '{1}', 102))";


        public IList<ClientDOB> ClientDOB(DateTime fromDate, DateTime toDate)
        {
            IList<ClientDOB> lstClients = new List<ClientDOB>();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(DOB_QUERY,fromDate,toDate));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                ClientDOB client = convertToClientObject(dr);
                lstClients.Add(client);
            }
            return lstClients;
        }

        private ClientDOB convertToClientObject(DataRow dr)
        {
            ClientDOB client = new ClientDOB();
            client.ClientName = dr.Field<string>("Name");
            client.DOB = dr.Field<DateTime>("DOB");
            client.SpouseName = dr.Field<string>("Spouse");
            client.DOB = dr.Field<DateTime>("SpouseDOB");
            return client;
        }
    }
}
