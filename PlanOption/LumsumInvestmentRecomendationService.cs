using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class LumsumInvestmentRecomendationService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT LumsumInvestmentRecomendation.Id, LumsumInvestmentRecomendation.PId, " +
            "LumsumInvestmentRecomendation.SchemeId, LumsumInvestmentRecomendation.Amount, " +
            "LumsumInvestmentRecomendation.FirstHolder, LumsumInvestmentRecomendation.SecondHolder, " +
            "LumsumInvestmentRecomendation.ChequeInFavourOff, LumsumInvestmentRecomendation.CreatedOn, " +
            "LumsumInvestmentRecomendation.CreatedBy, LumsumInvestmentRecomendation.UpdatedOn, " +
            "LumsumInvestmentRecomendation.UpdatedBy, LumsumInvestmentRecomendation.ThirdHolder, " +
            "Scheme.Name AS SchemeName,Scheme.Type, SchemeCategory.Name AS Category, Users.UserName, " +
            "AMC.Name AS AMC " +
            "FROM SchemeCategory INNER JOIN " +
            "Scheme ON SchemeCategory.Id = Scheme.CategoryId INNER JOIN " +
            "LumsumInvestmentRecomendation INNER JOIN " +
            "Users ON LumsumInvestmentRecomendation.UpdatedBy = Users.ID ON Scheme.Id = LumsumInvestmentRecomendation.SchemeId INNER JOIN " +
            "AMC ON Scheme.AMCId = AMC.Id " +
            "WHERE (LumsumInvestmentRecomendation.PId = {0})";

        const string INSERT_LUMSUM = "INSERT INTO[dbo].[LumsumInvestmentRecomendation] " +
            "([PId],[SchemeId],[Amount],[ChequeInFavourOff],[FirstHolder],[SecondHolder],[CreatedOn],[CreatedBy],[UpdatedOn],[UpdatedBy],[ThirdHolder]) " +
            "VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}',{7},'{8}',{9},'{10}')";
        const string DELETE_QUERY = "DELETE FROM LUMSUMINVESTMENTRECOMENDATION WHERE PID = {0} AND SCHEMEID = {1} AND AMOUNT ={2}";

        public IList<LumsumInvestmentRecomendation> GetAll(int plannerId)
        {
            IList<LumsumInvestmentRecomendation> lumsumInvestmentRecomendations = new List<LumsumInvestmentRecomendation>();
            try
            {
                Logger.LogInfo("Get: Lumsum investment process start");
                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    LumsumInvestmentRecomendation lumsumInvestmentRecomendation = convertToLumsumInvestmentRecomendationObject(dr);
                    lumsumInvestmentRecomendations.Add(lumsumInvestmentRecomendation);
                }
                Logger.LogInfo("Get: Lumsum investment process completed.");                
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);               
            }       
            return lumsumInvestmentRecomendations;
        }

        public void Add(LumsumInvestmentRecomendation lumsumInvestmentRecomendation)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_LUMSUM,
                   lumsumInvestmentRecomendation.Pid, 
                   lumsumInvestmentRecomendation.SchemeId,
                   lumsumInvestmentRecomendation.Amount,
                   lumsumInvestmentRecomendation.ChequeInFavourOff, 
                   lumsumInvestmentRecomendation.FirstHolder,
                   lumsumInvestmentRecomendation.SecondHolder, 
                   lumsumInvestmentRecomendation.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                   lumsumInvestmentRecomendation.CreatedBy,
                   lumsumInvestmentRecomendation.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                   lumsumInvestmentRecomendation.UpdatedBy,
                   lumsumInvestmentRecomendation.ThirdHolder));

                Activity.ActivitiesService.Add(ActivityType.CreateInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, lumsumInvestmentRecomendation.UpdatedByUserName, lumsumInvestmentRecomendation.SchemeName, lumsumInvestmentRecomendation.MachineName);
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

        public void Delete(LumsumInvestmentRecomendation lumsumInvestmentRecomendation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, lumsumInvestmentRecomendation.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY,
                  lumsumInvestmentRecomendation.Pid,lumsumInvestmentRecomendation.SchemeId,lumsumInvestmentRecomendation.Amount));
                
                Activity.ActivitiesService.Add(ActivityType.DeleteInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, lumsumInvestmentRecomendation.UpdatedByUserName, lumsumInvestmentRecomendation.SchemeName,
                         lumsumInvestmentRecomendation.MachineName);
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

        private LumsumInvestmentRecomendation convertToLumsumInvestmentRecomendationObject(DataRow dr)
        {
            LumsumInvestmentRecomendation lumsumInvestmentRecomendation = new LumsumInvestmentRecomendation();
            lumsumInvestmentRecomendation.Pid = dr.Field<int>("PId");
            lumsumInvestmentRecomendation.SchemeId  = dr.Field<int>("SchemeId");
            lumsumInvestmentRecomendation.SchemeName = dr.Field<string>("SchemeName");
            lumsumInvestmentRecomendation.Amount = double.Parse(dr["Amount"].ToString());
            lumsumInvestmentRecomendation.Category = dr.Field<string>("Category");
            lumsumInvestmentRecomendation.ChequeInFavourOff = dr.Field<string>("ChequeInfavourOff");
            lumsumInvestmentRecomendation.FirstHolder = dr.Field<string>("FirstHolder");
            lumsumInvestmentRecomendation.SecondHolder = dr.Field<string>("SecondHolder");
            lumsumInvestmentRecomendation.ThirdHolder = dr.Field<string>("ThirdHolder");
            lumsumInvestmentRecomendation.Type = dr.Field<string>("Type");
            lumsumInvestmentRecomendation.AMC = dr.Field<string>("AMC");
            return lumsumInvestmentRecomendation;
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
