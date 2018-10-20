using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class NonFinancialAssetService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NONFINANCIALASSET N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BY_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NONFINANCIALASSET N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0} AND N1.ID = {1}";
        const string INSERT_QUERY = "INSERT INTO NONFINANCIALASSET VALUES ({0},'{1}'," +
            "{2},{3},{4},'{5}',{6},{7},{8},'{9}','{10}','{11}',{12},'{13}',{14},{15})";
        const string UPDATE_QUERY ="UPDATE NONFINANCIALASSET SET NAME ='{0}',CURRENTVALUE ={1},PRIMARYSHARE ={2},SECONDARYSHARE ={3}," +
            "OTHERHOLDERNAME ='{4}',OTHERHOLDERSHARE ={5},MAPPEDGOALID = {6},ASSETMAPPINGSHARE ={7},ASSETREALISATIONYEAR ='{8}'," +
            "DESCRIPTION ='{9}', UPDATEDON = '{10}', UPDATEDBY={11},GROWTHPERCENTAGE ={12} WHERE ID ={13}";
        const string DELETE_QUERY = "DELETE NONFINANCIALASSET WHERE ID ={0}";

        public IList<NonFinancialAsset> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Non financial asset process start");
                IList<NonFinancialAsset> lstNonFinancialAsset = new List<NonFinancialAsset>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    NonFinancialAsset nonfinancialAsset = convertToNonFinancialAssetObject(dr);
                    lstNonFinancialAsset.Add(nonfinancialAsset);
                }
                Logger.LogInfo("Get: Family member information process completed.");
                return lstNonFinancialAsset;
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

        public NonFinancialAsset GetByID(int id, int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Non financial asset by id process start");
                NonFinancialAsset nonFinancialAsset = new NonFinancialAsset();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID,plannerId,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    nonFinancialAsset = convertToNonFinancialAssetObject(dr);                    
                }
                Logger.LogInfo("Get: Family member information process completed.");
                return nonFinancialAsset;
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

        public void Add(NonFinancialAsset nonFinancialAsset)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,nonFinancialAsset.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   nonFinancialAsset.Pid, nonFinancialAsset.Name, nonFinancialAsset.CurrentValue,
                   nonFinancialAsset.PrimaryholderShare,nonFinancialAsset.SecondaryHolderShare,
                   nonFinancialAsset.OtherHolderName,nonFinancialAsset.OtherHolderShare,
                   nonFinancialAsset.MappedGoalId,nonFinancialAsset.AssetMappingShare,nonFinancialAsset.AssetRealisationYear,
                   nonFinancialAsset.Description,
                   nonFinancialAsset.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), nonFinancialAsset.CreatedBy,
                   nonFinancialAsset.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), nonFinancialAsset.UpdatedBy,
                   nonFinancialAsset.GrowthPercentage));

                Activity.ActivitiesService.Add(ActivityType.CreateNonFinancialAsset, EntryStatus.Success,
                         Source.Server, nonFinancialAsset.UpdatedByUserName, clientName, nonFinancialAsset.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Update(NonFinancialAsset nonFinancialAsset)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,nonFinancialAsset.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   nonFinancialAsset.Name, nonFinancialAsset.CurrentValue,
                   nonFinancialAsset.PrimaryholderShare, nonFinancialAsset.SecondaryHolderShare,
                   nonFinancialAsset.OtherHolderName, nonFinancialAsset.OtherHolderShare,
                   nonFinancialAsset.MappedGoalId, nonFinancialAsset.AssetMappingShare, nonFinancialAsset.AssetRealisationYear,
                   nonFinancialAsset.Description,
                   nonFinancialAsset.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), nonFinancialAsset.UpdatedBy,
                   nonFinancialAsset.GrowthPercentage,
                   nonFinancialAsset.Id));

                Activity.ActivitiesService.Add(ActivityType.UpdateNonFinancialAsset, EntryStatus.Success,
                         Source.Server, nonFinancialAsset.UpdatedByUserName, clientName, nonFinancialAsset.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        public void Delete(NonFinancialAsset nonFinancialAsset)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY, nonFinancialAsset.Id));                   
                Activity.ActivitiesService.Add(ActivityType.DeleteNonFinancialAsset, EntryStatus.Success,
                         Source.Server, nonFinancialAsset.UpdatedByUserName, nonFinancialAsset.Name, nonFinancialAsset.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        private NonFinancialAsset convertToNonFinancialAssetObject(DataRow dr)
        {
            NonFinancialAsset nonFinancialAsset = new NonFinancialAsset();
            nonFinancialAsset.Id = dr.Field<int>("ID");
            nonFinancialAsset.Pid = dr.Field<int>("PID");
            nonFinancialAsset.Name = dr.Field<string>("Name");
            nonFinancialAsset.CurrentValue = double.Parse(dr["CurrentValue"].ToString());
            nonFinancialAsset.PrimaryholderShare = dr.Field<int>("Primaryshare");
            nonFinancialAsset.SecondaryHolderShare = dr.Field<int>("Secondaryshare");
            nonFinancialAsset.OtherHolderName = dr.Field<string>("OtherHolderName");
            nonFinancialAsset.OtherHolderShare = dr.Field<int>("OtherHolderShare");
            nonFinancialAsset.MappedGoalId = dr.Field<int>("MappedGoalId");
            nonFinancialAsset.AssetMappingShare = dr.Field<int>("AssetMappingShare");
            nonFinancialAsset.AssetRealisationYear = dr.Field<string>("AssetRealisationYear");
            nonFinancialAsset.Description = dr.Field<string>("Description");
            nonFinancialAsset.GrowthPercentage = dr.Field<decimal>("GrowthPercentage");
            return nonFinancialAsset;
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
