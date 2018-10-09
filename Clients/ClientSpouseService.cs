using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class ClientSpouseService
    {
        private const string INSERT_QUERY = "INSERT INTO CLIENTSPOUSE VALUES (" +
            "{0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}'," +
            "'{11}','{12}','{13}',{14},'{15}',{16})";

        private const string SELECT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENTSPOUSE C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.CID = {0}";

        private const string UPDATE_QUERY = "UPDATE CLIENTSPOUSE SET NAME = '{0}'," +
                "FATHERNAME = '{1}', MOTHERNAME = '{2}',GENDER ='{3}',DOB ='{4}',PAN ='{5}', AADHAR = '{6}'," +
                "PLACEOFBIRTH ='{7}',Married ='{8}',MARRIAGEANNIVERSARY ='{9}', Occupation = '{10}'," +
                "INCOMESLAB = '{11}', UPDATEDON = '{12}', UPDATEDBY = {13} WHERE CID= {14}";
        private const string IS_RECORD_EXIST = "SELECT COUNT(*) FROM CLIENTSPOUSE WHERE CID = {0}";
        //private const string DELETE_QUERY = "DELETE FROM USERS WHERE ID = {0}";

        public ClientSpouse Get(int id)
        {
            ClientSpouse clientSpouse = new ClientSpouse();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                clientSpouse = convertToClientSpouseObject(dr);
            }
            return clientSpouse;
        }

        private ClientSpouse convertToClientSpouseObject(DataRow dr)
        {
            ClientSpouse client = new ClientSpouse();
            client.ID = dr.Field<int>("ID");
            client.ClientId = dr.Field<int>("CID");
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

        public void Update(ClientSpouse clientSpouse)
        {
            try
            {
                string value = DataBase.DBService.ExecuteCommandScalar(string.Format(IS_RECORD_EXIST,clientSpouse.ClientId));                
                bool isRecordExist = (value.Equals("0")) ? false : true;
                if (isRecordExist)
                {
                    DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                        clientSpouse.Name, clientSpouse.FatherName, clientSpouse.MotherName,
                        clientSpouse.Gender, clientSpouse.DOB.ToString("yyyy-MM-dd"), clientSpouse.PAN,
                        clientSpouse.Aadhar, clientSpouse.PlaceOfBirth, clientSpouse.IsMarried,
                        ((clientSpouse.MarriageAnniversary == null) ? null : clientSpouse.MarriageAnniversary.Value.ToString("yyyy-MM-dd")),
                        clientSpouse.Occupation, clientSpouse.IncomeSlab,
                        clientSpouse.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientSpouse.UpdatedBy, clientSpouse.ClientId));

                    Activity.ActivitiesService.Add(ActivityType.UpdateClientSpouse, EntryStatus.Success,
                             Source.Server, clientSpouse.UpdatedByUserName, clientSpouse.Name, clientSpouse.MachineName);
                }
                else
                {
                    DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                        clientSpouse.ClientId,
                    clientSpouse.Name, clientSpouse.FatherName, clientSpouse.MotherName,
                    clientSpouse.Gender, clientSpouse.DOB.ToString("yyyy-MM-dd"), clientSpouse.PAN,
                    clientSpouse.Aadhar, clientSpouse.PlaceOfBirth, clientSpouse.IsMarried,
                    ((clientSpouse.MarriageAnniversary == null) ? null : clientSpouse.MarriageAnniversary.Value.ToString("yyyy-MM-dd")), clientSpouse.Occupation, clientSpouse.IncomeSlab,
                    clientSpouse.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientSpouse.CreatedBy, clientSpouse.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), clientSpouse.UpdatedBy));


                    Activity.ActivitiesService.Add(ActivityType.UpdateClientSpouse, EntryStatus.Success,
                             Source.Server, clientSpouse.UpdatedByUserName, clientSpouse.Name, clientSpouse.MachineName);
                }
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);

            }
        }
    }
}
