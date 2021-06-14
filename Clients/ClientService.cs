using FinancialPlanner.BusinessLogic.ApplictionConfiguration;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class ClientService
    {
        private const string INSERT_QUERY = "INSERT INTO CLIENT VALUES (" +           "'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}'," +
            "'{11}','{12}',{13},'{14}',{15},'{16}',{17},'{18}','{19}','{20}',{21},'{22}')";

        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENT C1, USERS U WHERE C1.UPDATEDBY = U.ID AND C1.ISDELETED = 0";
        private const string SELECT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENT C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.ID = {0} AND C1.ISDELETED = 0";
        private const string SELECT_WITH_OTHER_VALUES = "SELECT C1.*, U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENT C1, USERS U WHERE C1.UPDATEDBY = U.ID and " +
            "C1.NAME = '{0}' AND C1.PAN ='{1}'  AND C1.ISDELETED = 0";
        
        private const string UPDATE_QUERY = "UPDATE CLIENT SET  NAME = '{0}'," +
                "FATHERNAME = '{1}', MOTHERNAME = '{2}',GENDER ='{3}',DOB ='{4}',PAN ='{5}', AADHAR = '{6}'," +
                "PLACEOFBIRTH ='{7}',Married ='{8}',MARRIAGEANNIVERSARY ='{9}', Occupation = '{10}'," +
                "INCOMESLAB = '{11}', UPDATEDON = '{12}', UPDATEDBY = {13},IMAGEPATH = '{14}', " +
                "RATING ='{15}', CLIENTTYPE ='{16}',RESISTATUS ='{17}'," +
            "ISACTIVE = {19},NOTE = '{20}' WHERE ID= {18}";
        private const string DELETE_QUERY = "UPDATE CLIENT SET ISDELETED = 1, " +
            "UPDATEDON = '{0}', UPDATEDBY = {1} WHERE ID = {2}";

        #region "Client ARN"
        private const string SELECT_ARN = "SELECT ClientARN.*, ARN.ARNNumber, ARN.Name FROM ClientARN INNER JOIN" +
                         " ARN ON ClientARN.ARNId = ARN.Id where clientARN.CId = {0}";
        private const string UPDATE_ARN = "UPDATE [ClientARN] SET[CId] = {0}, [ARNId] = {1} WHERE CId = {0}";
        private const string INSERT_ARN = "INSERT INTO [dbo].[ClientARN] ([CId],[ARNId]) VALUES ({0},{1})";
        private const string DELETE_ARN = "DELETE FROM CLIENTARN WHERE CID = {0}";
        #endregion

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

        public Client GetByOtherValues(string name, string pancard)
        {
            Client client = new Client();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_WITH_OTHER_VALUES, name,pancard));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                client = convertToClientObject(dr);
            }
            return client;
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
            client.MarriageAnniversary = dr.Field<DateTime?>("MarriageAnniversary");
            client.FatherName = dr.Field<string>("FatherName");
            client.MotherName = dr.Field<string>("MotherName");
            client.Occupation = dr.Field<string>("Occupation");
            client.IncomeSlab = dr.Field<string>("IncomeSlab");
            client.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            client.UpdatedBy = dr.Field<int>("UpdatedBy");
            client.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            client.ImagePath = dr.Field<string>("IMAGEPATH");
            client.IsDeleted = dr.Field<bool>("IsDeleted");
            client.Rating = dr.Field<string>("Rating");
            client.ClientType = dr.Field<string>("ClientType");
            client.ResiStatus = dr.Field<string>("ResiStatus");
            client.IsActive = dr.Field<bool>("IsActive");
            client.Note  = dr.Field<string>("Note");

            if (!string.IsNullOrEmpty(client.ImagePath))
            {
                string actualImagePath = getImagePath(client);
                client.ImageData = getStringfromFile(actualImagePath);
             }
            return client;
        }

        public void Delete(Client client)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY,
                            client.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), client.UpdatedBy,
                            client.ID), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteClient, EntryStatus.Success,
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
                throw ex;
            }
        }

        public void Add(Client client)
        {
            try
            {
                string imagePath = getImagePath(client);

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                    client.Name, client.FatherName,client.MotherName,
                    client.Gender,client.DOB.ToString("yyyy-MM-dd"),client.PAN,
                    client.Aadhar,client.PlaceOfBirth,client.IsMarried,
                    ((client.MarriageAnniversary == null) ? null : client.MarriageAnniversary.Value.ToString("yyyy-MM-dd")),client.Occupation,client.IncomeSlab,
                    client.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), client.CreatedBy, 
                    client.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), client.UpdatedBy,
                    imagePath,0,client.Rating,client.ClientType,client.ResiStatus,
                    (client.IsActive) ? 1 : 0,client.Note),true);


                Activity.ActivitiesService.Add(ActivityType.CreateClient, EntryStatus.Success,
                         Source.Server, client.UpdatedByUserName, client.Name, client.MachineName);

                if (client.ImageData != null)
                {
                    byte[] arrBytes = Convert.FromBase64String(client.ImageData);
                    File.WriteAllBytes(imagePath, arrBytes);
                }
                DataBase.DBService.CommitTransaction(); 
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

        private string getImagePath(Client client)
        {
            if (string.IsNullOrEmpty(client.ImagePath))
                return string.Empty;

            string applicationPath = getApplicationPath();

            if (applicationPath == null)
                return null;

            System.IO.Directory.CreateDirectory(
                Path.Combine(applicationPath, client.ID.ToString()));
            return Path.Combine(applicationPath,client.ID.ToString(), client.ImagePath);
        }

        public void Update(Client client)
        {
            try
            {
                string imagePath = getImagePath(client);
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                            client.Name, client.FatherName, client.MotherName,
                            client.Gender, client.DOB.ToString("yyyy-MM-dd"), client.PAN,
                            client.Aadhar, client.PlaceOfBirth, client.IsMarried,
                            ((client.MarriageAnniversary == null) ? null : client.MarriageAnniversary.Value.ToString("yyyy-MM-dd")),
                            client.Occupation, client.IncomeSlab,
                            client.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), client.UpdatedBy,imagePath,
                            client.Rating,client.ClientType,
                            client.ResiStatus,
                            client.ID,
                            (client.IsActive) ? 1 : 0,client.Note),true);

                Activity.ActivitiesService.Add(ActivityType.UpdateClient, EntryStatus.Success,
                         Source.Server, client.UpdatedByUserName, client.Name, client.MachineName);

                if (client.ImageData != null)
                {
                    byte[] arrBytes = Convert.FromBase64String(client.ImageData);
                    File.WriteAllBytes(imagePath, arrBytes);
                }
                DataBase.DBService.CommitTransaction();
            }
            catch(Exception ex)
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
        private string getApplicationPath()
        {
            ApplicationConfiService appConfig = new ApplicationConfiService();
            IList<ApplicationConfiguration> appConfigs =   appConfig.Get();
            var resultConfig = appConfigs.First(i => i.SettingName == "Application Path");
            if (resultConfig != null)
                return resultConfig.SettingValue.ToString();
            return null;
        }

        private string getStringfromFile(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        byte[] filebytes = new byte[fs.Length];
                        fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
                        return Convert.ToBase64String(filebytes,
                                                      Base64FormattingOptions.InsertLineBreaks);
                    }                    
                }                
            }
            catch(Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);                
            }
            return null;
        }

        #region "Client ARN"

        public ClientARN GetARN(int cid)
        {
            ClientARN clientARN = new ClientARN();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ARN, cid));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                clientARN = convertToClientARNObject(dr);
            }
            return clientARN;
        }

        private ClientARN convertToClientARNObject(DataRow dr)
        {
            ClientARN clientARN = new ClientARN();
            clientARN.Id = dr.Field<int>("Id");
            clientARN.Cid = dr.Field<int>("CId");
            clientARN.ARNId = dr.Field<int>("ARNId");
            clientARN.ARNName = dr.Field<string>("Name");
            clientARN.ArnNumber = dr.Field<string>("ARNNumber");
            return clientARN;
        }

        public void UpdateARN(ClientARN clientARN)
        {
            try
            {
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_ARN,
                            clientARN.Cid, clientARN.ARNId));
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void InsertARN(ClientARN clientARN)
        {
            try
            {
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_ARN,
                            clientARN.Cid, clientARN.ARNId));
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void DeleteARN(ClientARN clientARN)
        {
            try
            {
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_ARN,
                            clientARN.Cid));
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        #endregion
    }
}
