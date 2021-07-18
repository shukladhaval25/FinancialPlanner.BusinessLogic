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

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class DocumentService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Document N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Document N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0} AND N1.PID ={1}";

        const string INSERT_QUERY = "INSERT INTO Document VALUES (" +
            "{0},{1},'{2}','{3}','{4}','{5}',{6},'{7}',{8})";
        const string UPDATE_QUERY = "UPDATE Document SET NAME = '{0}',UPDATEDON = '{1}'," +
            "UPDATEDBY={2} WHERE ID ={3}";
        const string DELET_QUERY = "DELETE FROM Document WHERE ID ={0}";
        public IList<Document> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Document process start");
                IList<Document> lstDocument = new List<Document>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Document Document = convertToDocumentObject(dr);
                    lstDocument.Add(Document);
                }
                Logger.LogInfo("Get: Document process completed.");
                return lstDocument;
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

        public Document GetById(int id, int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Document process start");
                Document Document = new Document();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BYID,id,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Document = convertToDocumentObject(dr);
                }
                Logger.LogInfo("Get: Document process completed.");
                return Document;
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

        public void Add(Document document)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,document.Pid));
                string fullFilePath = getFullFilePath(document);
                DataBase.DBService.BeginTransaction();
   
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      document.Cid,document.Pid,document.Name, getPathWithoutRepository(document),
                      document.Category,
                      document.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), document.CreatedBy,
                      document.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), document.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateDocument, EntryStatus.Success,
                         Source.Server, document.UpdatedByUserName, document.Name, document.MachineName);

                byte[] arrBytes = Convert.FromBase64String(document.Data);
                File.WriteAllBytes(fullFilePath, arrBytes);
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

        private string getFullFilePath(Document document)
        {
            string applicationPath = getApplicationPath();

            if (applicationPath == null)
                return null;

            System.IO.Directory.CreateDirectory(
                Path.Combine(applicationPath, document.Cid.ToString(), 
                    document.Pid.ToString(),document.Category));
            return Path.Combine(Path.Combine(applicationPath, document.Cid.ToString(), document.Pid.ToString(), document.Category), document.Name);

        }

        private string getPathWithoutRepository(Document document)
        {
            string path = Path.Combine(document.Cid.ToString(), document.Pid.ToString(), document.Category, document.Name);
            return @path;
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

        public void Update(Document Document)
        {
            try
            {
                string clientName =
                    DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Document.Pid));

                //DataBase.DBService.BeginTransaction();
                //DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                //   Document.ItemCategory,
                //   Document.Item, (Document.OccuranceType.Equals("Monthly") ? 0 : 1),
                //   Document.Amount,
                //   Document.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Document.UpdatedBy, Document.Id), true);

                //Activity.ActivitiesService.Add(ActivityType.UpdateDocument, EntryStatus.Success,
                //         Source.Server, Document.UpdatedByUserName, clientName, Document.MachineName);
                //DataBase.DBService.CommitTransaction();
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

        public void Delete(Document Document)
        {
            try
            {
                string fullFilePath = getFullFilePath(Document);
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Document.Pid));
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELET_QUERY, Document.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteDocument, EntryStatus.Success,
                         Source.Server, Document.UpdatedByUserName, Document.Name, Document.MachineName);
                
                File.Delete(fullFilePath);
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
        private Document convertToDocumentObject(DataRow dr)
        {
            Document Document = new Document();
            Document.Id = dr.Field<int>("ID");
            Document.Cid = dr.Field<int>("CID");
            Document.Pid = dr.Field<int>("PID");
            Document.Category = dr.Field<string>("Category");
            Document.Name = dr.Field<string>("Name");
            Document.Path = dr.Field<string>("Path");
            Document.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            Document.UpdatedBy = dr.Field<int>("UpdatedBy");
            if (!string.IsNullOrEmpty(Document.Path))
            {
                string fullDocumentPath = getFullFilePath(Document);
                Document.Data = getStringfromFile(fullDocumentPath);
            }
            return Document;
        }

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
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
                        return Convert.ToBase64String(filebytes,            Base64FormattingOptions.InsertLineBreaks);
                    }
                }
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
            }
            return null;
        }
    }
}
