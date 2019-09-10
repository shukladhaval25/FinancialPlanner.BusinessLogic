using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.ApplicationMaster
{
    public class SchemeService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT  AMC.Name AS AMCName, Scheme.*,Users.USERNAME AS UPDATEDBYUSERNAME " +
            "FROM AMC INNER JOIN Scheme ON AMC.Id = Scheme.AMCId INNER JOIN " +
            "Users ON AMC.UpdatedBy = Users.ID";

        private const string SELECT_ALL_BY_AMC = "SELECT  AMC.Name AS AMCName, Scheme.*,Users.USERNAME AS UPDATEDBYUSERNAME " +
            "FROM AMC INNER JOIN Scheme ON AMC.Id = Scheme.AMCId INNER JOIN " +
            "Users ON AMC.UpdatedBy = Users.ID WHERE Scheme.AMCID = {0}";

        private const string SELECT_COUNT_BASEDON_AMC = "SELECT COUNT(*) FROM SCHEME WHERE AMCID = {0}";
            
           // "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Scheme C1, USERS U WHERE C1.UPDATEDBY = U.ID";

        private const string INSERT_QUERY = "INSERT INTO Scheme VALUES ({0},'{1}','{2}',{3},'{4}',{5})";
        private const string UPDATE_QUERY = "UPDATE Scheme SET NAME ='{0}',[UpdatedOn] = '{1}', [UpdatedBy] = {2}, " +
            "AMCId = {3} WHERE ID = {4}";

        private const string DELETE_BY_ID = "DELETE FROM Scheme WHERE ID ='{0}'";

        public IList<Scheme> Get()
        {
            try
            {
                Logger.LogInfo("Get: Scheme process start");
                IList<Scheme> lstScheme = new List<Scheme>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Scheme Scheme = convertToSchemeObject(dr);
                    lstScheme.Add(Scheme);
                }
                Logger.LogInfo("Get: Scheme process completed.");
                return lstScheme;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }
        public IList<Scheme> GetAll(int amcId)
        {
            try
            {
                Logger.LogInfo("Get: Scheme process start");
                IList<Scheme> lstScheme = new List<Scheme>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_AMC,amcId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Scheme Scheme = convertToSchemeObject(dr);
                    lstScheme.Add(Scheme);
                }
                Logger.LogInfo("Get: Scheme process completed.");
                return lstScheme;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public int Get(int amcId)
        {
            try
            {
                Logger.LogInfo("Get: Scheme process start");
                string recordCount = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_COUNT_BASEDON_AMC,amcId));
                if (string.IsNullOrEmpty(recordCount))
                    return 0;
                else
                    return int.Parse(recordCount);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return 0;
            }
        }

        public void Add(Scheme Scheme)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   Scheme.AmcId,
                   Scheme.Name,
                   Scheme.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Scheme.CreatedBy,
                   Scheme.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Scheme.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateScheme, EntryStatus.Success,
                         Source.Server, Scheme.UpdatedByUserName, Scheme.Name, Scheme.MachineName);
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

        public void Update(Scheme Scheme)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   Scheme.Name,
                   Scheme.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Scheme.UpdatedBy,
                   Scheme.AmcId,
                   Scheme.Id));

                Activity.ActivitiesService.Add(ActivityType.UpdateScheme, EntryStatus.Success,
                         Source.Server, Scheme.UpdatedByUserName, Scheme.Name, Scheme.MachineName);
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

        public void Delete(Scheme Scheme)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, Scheme.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteScheme, EntryStatus.Success,
                         Source.Server, Scheme.UpdatedByUserName, Scheme.Name, Scheme.MachineName);
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

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        private Scheme convertToSchemeObject(DataRow dr)
        {
            Scheme Scheme = new Scheme();
            Scheme.Id = dr.Field<int>("ID");
            Scheme.AmcId = dr.Field<int>("AMCId");
            Scheme.AmcName = dr.Field<string>("AMCName");
            Scheme.Name = dr.Field<string>("Name");
            Scheme.UpdatedBy = dr.Field<int>("UpdatedBy");
            Scheme.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            Scheme.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            Scheme.CreatedBy = dr.Field<int>("CreatedBy");
            Scheme.CreatedOn = dr.Field<DateTime>("CreatedOn");
            Scheme.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return Scheme;
        }
    }
}
