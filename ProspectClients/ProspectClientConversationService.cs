using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ProspectClients
{
    public class ProspectClientConversationService
    {
        private const string SELECT_BY_PROSPECTCLIENT_ID = "SELECT U1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PROSPECTCLIENTCONVERSATION U1, " +
            "USERS U WHERE U1.UPDATEDBY = U.ID and U1.PROSPECTCLIENTID = {0} ORDER BY CONVERSATIONDATE";
        
        private const string INSERT_PROSPECTCLIENTCONVERSATION_QUERY =
            "INSERT INTO PROSPECTCLIENTCONVERSATION VALUES ({0},'{1}','{2}','{3}','{4}',{5},'{6}',{7})";

        private const string UPDATE_PROSPECTCLIENT_CONVERSATION_QUERY ="UPDATE PROSPECTCLIENTCONVERSATION SET CONVERSATIONDATE ='{0}', CONVERSATIONBY = '{1}',Remarks ='{2}'," +
            "UPDATEDON ='{3}',UPDATEDBY ={4} WHERE ID = {5}";

        private const string DELETE_CONVERSATION_QUERY = "DELETE FROM PROSPECTCLIENTCONVERSATION WHERE ID = {0}";

        public object GetByProspectClientId(int id)
        {
            IList<ProspectClientConversation> lstProspClient = new List<ProspectClientConversation>();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_PROSPECTCLIENT_ID,id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                ProspectClientConversation prospClientConversation = convertToProspectClientConversationject(dr);
                lstProspClient.Add(prospClientConversation);
            }
            return lstProspClient;
        }

        public void AddProspectClientConversation(ProspectClientConversation prospectClientConversation)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_PROSPECTCLIENTCONVERSATION_QUERY,
                     prospectClientConversation.ProspectClientId, prospectClientConversation.ConversationDate.ToString("yyyy-MM-dd hh:mm:ss"), prospectClientConversation.ConversationBy,
                     prospectClientConversation.Remarks,
                     prospectClientConversation.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), prospectClientConversation.CreatedBy,
                     prospectClientConversation.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), prospectClientConversation.UpdatedBy));

                var obj = (ProspectClient) new ProspectClientService().GetById(prospectClientConversation.ProspectClientId);

                Activity.ActivitiesService.Add(ActivityType.CreateProspectClientConversation, EntryStatus.Success,
                         Source.Client, prospectClientConversation.UpdatedByUserName, obj.Name, prospectClientConversation.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
            }
        }
        public void UpdateProspectClientConversation(ProspectClientConversation prospectClientConversation)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_PROSPECTCLIENT_CONVERSATION_QUERY,
                     prospectClientConversation.ConversationDate.ToString("yyyy-MM-dd hh:mm:ss"),
                     prospectClientConversation.ConversationBy,
                     prospectClientConversation.Remarks,
                     prospectClientConversation.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                     prospectClientConversation.UpdatedBy,
                     prospectClientConversation.ID));
                                
                var obj = (ProspectClient) new ProspectClientService().GetById(prospectClientConversation.ProspectClientId);

                Activity.ActivitiesService.Add(ActivityType.UpdateProspectClientConversation, EntryStatus.Success,
                         Source.Client, prospectClientConversation.UpdatedByUserName, obj.Name, prospectClientConversation.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
                throw ex;
            }
        }        
        public void DeleteConversation(ProspectClientConversation prospectClientConversation)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_CONVERSATION_QUERY,
                    prospectClientConversation.ID));
                var obj = (ProspectClient) new ProspectClientService().GetById(prospectClientConversation.ProspectClientId);
                Activity.ActivitiesService.Add(ActivityType.DeleteProspectClientConversation, EntryStatus.Success,
                         Source.Client, prospectClientConversation.UpdatedByUserName, obj.Name, prospectClientConversation.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
                throw ex;
            }
        }
        private ProspectClientConversation convertToProspectClientConversationject(DataRow dr)
        {
            ProspectClientConversation prospClientConv = new ProspectClientConversation();
            prospClientConv.ID = dr.Field<int>("ID");
            prospClientConv.ProspectClientId = dr.Field<int>("ProspectClientId");
            prospClientConv.ConversationDate = dr.Field<DateTime>("ConversationDate");
            prospClientConv.ConversationBy = dr.Field<string>("ConversationBy");
            prospClientConv.Remarks = dr.Field<string>("Remarks");
            prospClientConv.CreatedOn = dr.Field<DateTime>("CreatedOn");
            prospClientConv.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            prospClientConv.UpdatedBy = dr.Field<int>("UpdatedBy");
            prospClientConv.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");

            return prospClientConv;
        }


    }
}
