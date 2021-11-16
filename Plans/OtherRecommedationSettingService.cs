using FinancialPlanner.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model;
using System.Data;

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class OtherRecommedationSettingService
    {

        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";

        private const string UPDATE_QUERY = "UPDATE [dbo].[OtherRecommendationOptions] " +
            "SET [IsSelected] = {0}, [Description] = '{1}' WHERE [Title] = '{2}' and PID = {3}";

        private const string INSERT_QUERY = "INSERT INTO OTHERRECOMMENDATIONOPTIONS VALUES ({0},'{1}',{2},'{3}')";

        private const string SELECT_BY_PROJECT_ID = "SELECT * FROM OtherRecommendationOptions WHERE PID ={0}";

        
        public IList<OtherRecommendationSetting> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Other recommenation process start");
                IList<OtherRecommendationSetting> personalAccidentInsurances = new List<OtherRecommendationSetting>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_PROJECT_ID, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    OtherRecommendationSetting personalAccidentInsurance = convertToOtherRecommendationObject(dr);
                    personalAccidentInsurances.Add(personalAccidentInsurance);
                }
                Logger.LogInfo("Get: Other recommendation process completed.");
                return personalAccidentInsurances;
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
              
        public void Update(IList<OtherRecommendationSetting> otherRecommendationSettings)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                IList<OtherRecommendationSetting> otherRecommendations = GetAll(otherRecommendationSettings[0].PID);

                foreach (OtherRecommendationSetting otherRecommendationSetting in otherRecommendationSettings)
                {
                    int count = 0;
                    foreach(OtherRecommendationSetting otherRecommendation in otherRecommendations)
                    {
                        if (otherRecommendation.PID == otherRecommendationSetting.PID && 
                            otherRecommendation.Title == otherRecommendationSetting.Title)
                        {
                            count = count + 1;
                        }
                    }
                    //var selectedRecords = otherRecommendations.Select(i => i.PID == otherRecommendationSetting.PID && i.Title == otherRecommendationSetting.Title);
                    if (count > 0)
                    {
                        DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                           (otherRecommendationSetting.IsSelected) ? 1 : 0,
                           otherRecommendationSetting.Description,
                           otherRecommendationSetting.Title,
                           otherRecommendationSetting.PID));
                    }
                    else
                    {
                        DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                             otherRecommendationSetting.PID,
                              otherRecommendationSetting.Title,
                          (otherRecommendationSetting.IsSelected) ? 1 : 0,
                          otherRecommendationSetting.Description
                         ));
                    }
                }
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

        private OtherRecommendationSetting convertToOtherRecommendationObject(DataRow dr)
        {
            OtherRecommendationSetting otherRecommendationSetting = new OtherRecommendationSetting();
            otherRecommendationSetting.PID = dr.Field<int>("PID");
            otherRecommendationSetting.Title = dr.Field<string>("Title");
            otherRecommendationSetting.Description = dr.Field<string>("Description");
            otherRecommendationSetting.IsSelected = bool.Parse(dr["IsSelected"].ToString());
          
            return otherRecommendationSetting;
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
