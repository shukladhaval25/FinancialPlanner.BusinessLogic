using FinancialPlanner.BusinessLogic.Clients;
using FinancialPlanner.BusinessLogic.Plans;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic
{
    public class PlannerAssumptionService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PLANNERASSUMPTION N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PLANNERASSUMPTION N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0} AND N1.PID ={1}";
        const string SELECT_COUNT = "SELECT COUNT(*) FROM PLANNERASSUMPTION N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        //const string INSERT_QUERY = "INSERT INTO PLANNERASSUMPTION VALUES (" +
        //    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},'{10}','{11}',{12},'{13}',{14},'{15}',{16},{17},{18},{19},{20})";

        const string INSERT_QUERY = "INSERT INTO [dbo].[PlannerAssumption] ([PID],[ClientRetirementAge],[SpouseRetirementAge],[ClientLifeExpectancy],[SpouseLifeExpectancy],[PreRetirementInflactionRate],[PostRetirementInflactionRate],[EquityReturnRate],[DebtReturnRate],[OtherReturnRate],[Description],[CreatedOn],[CreatedBy],[UpdatedOn],[UpdatedBy],[ConsiderClientAgeForRetirment],[ClientIncomeRise],[SpouseIncomeRise],[OngoingExpRise],[PostRetirementInvestmentReturnRate]) VALUES (" +
            "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},'{10}','{11}',{12},'{13}',{14},'{15}',{16},{17},{18},{19})";

        const string UPDATE_QUERY = "UPDATE PLANNERASSUMPTION SET " +
            "ClientRetirementAge = {0}," +
            "SpouseRetirementAge = {1}," +
            "ClientLifeExpectancy = {2}," +
            "SpouseLifeExpectancy = {3}," +
            "PreRetirementInflactionRate ={4}," +
            "PostRetirementInflactionRate ={5}," +
            "EquityReturnRate = {6}," +
            "DebtReturnRate = {7}," +
            "OtherReturnRate = {8}," +
            "Description = '{9}'," +
            "UPDATEDON = '{10}'," +
            "UPDATEDBY={11}, ConsiderClientAgeForRetirment = '{12}'," +
            "ClientIncomeRise ={13}, SpouseIncomeRise = {14}," +
            "OngoingExpRise ={15}," +
            "PostRetirementInvestmentReturnRate ={17} WHERE ID ={16}";

        public PlannerAssumption GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: PlannerAssumption process start");
                PlannerAssumption plannerAssumption = new PlannerAssumption();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    plannerAssumption = convertToPlannerAssumptionObject(dr);
                }
                Logger.LogInfo("Get: PlannerAssumption process completed.");
                return plannerAssumption;
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

        public void Update(PlannerAssumption PlannerAssumption)
        {
            try
            {
                string clientName =
                    DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, PlannerAssumption.Pid));
                string recordCount = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_COUNT, PlannerAssumption.Pid));

                var currentplannerAssumption = GetAll(PlannerAssumption.Pid);

               
                if (recordCount != "0")
                {

                    if (currentplannerAssumption.ClientRetirementAge != PlannerAssumption.ClientRetirementAge)
                    {
                        updateClientIncomeExpenseGoalsWithAssumption(PlannerAssumption, currentplannerAssumption);
                    }
                    else if ((currentplannerAssumption.SpouseRetirementAge != PlannerAssumption.SpouseRetirementAge))
                    {
                        updateClientSpouseIncomeExpenseGoalsWithRetirementAge(PlannerAssumption, currentplannerAssumption);


                        //Check Client Age and Spouce age 

                        //DataBase.DBService.ExecuteCommand(string.Format())
                        //Update Income from salary for client and spouse based on retirement age change. 
                        //Update Retirement goal if Retirement age assumption get updated

                    }
                }
                else
                {
                    DataBase.DBService.BeginTransaction();
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      PlannerAssumption.Pid,
                      PlannerAssumption.ClientRetirementAge,
                      PlannerAssumption.SpouseRetirementAge,
                      PlannerAssumption.ClientLifeExpectancy,
                      PlannerAssumption.SpouseLifeExpectancy,
                      PlannerAssumption.PreRetirementInflactionRate,
                      PlannerAssumption.PostRetirementInflactionRate,
                      PlannerAssumption.EquityReturnRate,
                      PlannerAssumption.DebtReturnRate,
                      PlannerAssumption.OtherReturnRate,
                      PlannerAssumption.Decription,
                      PlannerAssumption.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      PlannerAssumption.CreatedBy,
                      PlannerAssumption.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      PlannerAssumption.UpdatedBy,
                      PlannerAssumption.IsClientRetirmentAgeIsPrimary,
                      PlannerAssumption.ClientIncomeRise, PlannerAssumption.SpouseIncomeRise,
                      PlannerAssumption.OngoingExpRise,
                      PlannerAssumption.PostRetirementInvestmentReturnRate), true);
                }
                Activity.ActivitiesService.Add(ActivityType.UpdatePlannerAssumption, EntryStatus.Success,
                            Source.Server, PlannerAssumption.UpdatedByUserName, clientName, PlannerAssumption.MachineName);
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        private static void updateClientSpouseIncomeExpenseGoalsWithRetirementAge(PlannerAssumption PlannerAssumption, PlannerAssumption currentplannerAssumption)
        {
            PlannerService ps = new PlannerService();
            var planner = ps.GetByPlannerId(PlannerAssumption.Pid);
            if (planner != null)
            {
                ClientSpouseService cs = new ClientSpouseService();
                var client = cs.Get(planner.ClientId);
                int currentRetirementYear = (client.DOB.Year + currentplannerAssumption.SpouseRetirementAge);
                int updatedRetirementYear = (client.DOB.Year + PlannerAssumption.SpouseRetirementAge);

                DataBase.DBService.BeginTransaction();

                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                       PlannerAssumption.ClientRetirementAge,
                       PlannerAssumption.SpouseRetirementAge,
                       PlannerAssumption.ClientLifeExpectancy,
                       PlannerAssumption.SpouseLifeExpectancy,
                       PlannerAssumption.PreRetirementInflactionRate,
                       PlannerAssumption.PostRetirementInflactionRate,
                       PlannerAssumption.EquityReturnRate,
                       PlannerAssumption.DebtReturnRate,
                       PlannerAssumption.OtherReturnRate,
                       PlannerAssumption.Decription,
                       PlannerAssumption.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                       PlannerAssumption.UpdatedBy,
                       PlannerAssumption.IsClientRetirmentAgeIsPrimary,
                       PlannerAssumption.ClientIncomeRise, PlannerAssumption.SpouseIncomeRise,
                       PlannerAssumption.OngoingExpRise,
                       PlannerAssumption.Id,
                       PlannerAssumption.PostRetirementInvestmentReturnRate), true);

                //update income where end year = currentRetirementYear
                const string UPDATEINCOMEWITHRETIREMENTAGE = "UPDATE INCOME SET ENDYEAR = {0} WHERE PID ={1} AND " +
                    "ENDYEAR = {2} AND INCOMEBY ='{3}' AND (UPPER(SOURCE) = 'BUSINESS' OR UPPER(SOURCE) ='SALARY')";

                DataBase.DBService.ExecuteCommandString(string.Format(UPDATEINCOMEWITHRETIREMENTAGE,
                        updatedRetirementYear, PlannerAssumption.Pid, currentRetirementYear, client.Name), true);
                ////update Expence where end year = currentRetirementYear
                //const string UPDATEEXPENSEWITHRETIRETIREMENTAGE = "UPDATE EXPENSES SET ENDYEAR = {0} WHERE PID = {1} AND " +
                //     "ENDYEAR = {2}";
                //DataBase.DBService.ExecuteCommandString(string.Format(UPDATEEXPENSEWITHRETIRETIREMENTAGE,
                //       updatedRetirementYear, PlannerAssumption.Pid, currentRetirementYear), true);

                ////update Goal with retirement where category = retirement and start year = currentRetirementyear
                //const string UPDATE_RETIRMENT_GOAL = "UPDATE GOALS SET STARTYEAR = {0} WHERE PID = {1} AND " +
                //    "STARTYEAR = {2} AND CATEGORY ='Retirement'";
                //DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_RETIRMENT_GOAL,
                //    updatedRetirementYear + 1, PlannerAssumption.Pid, currentRetirementYear + 1), true);
            }
        }

        private static void updateClientIncomeExpenseGoalsWithAssumption(PlannerAssumption PlannerAssumption, PlannerAssumption currentplannerAssumption)
        {
            PlannerService ps = new PlannerService();
            var planner = ps.GetByPlannerId(PlannerAssumption.Pid);
            if (planner != null)
            {
                ClientService cs = new ClientService();
                var client = cs.Get(planner.ClientId);
                int currentRetirementYear = (client.DOB.Year + currentplannerAssumption.ClientRetirementAge);
                int updatedRetirementYear = (client.DOB.Year + PlannerAssumption.ClientRetirementAge);

                DataBase.DBService.BeginTransaction();

                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                       PlannerAssumption.ClientRetirementAge,
                       PlannerAssumption.SpouseRetirementAge,
                       PlannerAssumption.ClientLifeExpectancy,
                       PlannerAssumption.SpouseLifeExpectancy,
                       PlannerAssumption.PreRetirementInflactionRate,
                       PlannerAssumption.PostRetirementInflactionRate,
                       PlannerAssumption.EquityReturnRate,
                       PlannerAssumption.DebtReturnRate,
                       PlannerAssumption.OtherReturnRate,
                       PlannerAssumption.Decription,
                       PlannerAssumption.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                       PlannerAssumption.UpdatedBy,
                       PlannerAssumption.IsClientRetirmentAgeIsPrimary,
                       PlannerAssumption.ClientIncomeRise, PlannerAssumption.SpouseIncomeRise,
                       PlannerAssumption.OngoingExpRise,
                       PlannerAssumption.Id,
                       PlannerAssumption.PostRetirementInvestmentReturnRate), true);

                //update income where end year = currentRetirementYear
                const string UPDATEINCOMEWITHRETIREMENTAGE = "UPDATE INCOME SET ENDYEAR = {0} WHERE PID ={1} AND " +
                    "ENDYEAR = {2} AND INCOMEBY ='{3}' AND (UPPER(SOURCE) = 'BUSINESS' OR UPPER(SOURCE) ='SALARY')";

                DataBase.DBService.ExecuteCommandString(string.Format(UPDATEINCOMEWITHRETIREMENTAGE,
                        updatedRetirementYear, PlannerAssumption.Pid, currentRetirementYear, client.Name), true);
                //update Expence where end year = currentRetirementYear
                const string UPDATEEXPENSEWITHRETIRETIREMENTAGE = "UPDATE EXPENSES SET ENDYEAR = {0} WHERE PID = {1} AND " +
                     "ENDYEAR = {2}";
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATEEXPENSEWITHRETIRETIREMENTAGE,
                       updatedRetirementYear, PlannerAssumption.Pid, currentRetirementYear), true);

                //update Goal with retirement where category = retirement and start year = currentRetirementyear
                const string UPDATE_RETIRMENT_GOAL = "UPDATE GOALS SET STARTYEAR = {0} WHERE PID = {1} AND " +
                    "STARTYEAR = {2} AND CATEGORY ='Retirement'";
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_RETIRMENT_GOAL,
                    updatedRetirementYear + 1, PlannerAssumption.Pid, currentRetirementYear + 1), true);

            }
        }

        private PlannerAssumption convertToPlannerAssumptionObject(DataRow dr)
        {
            PlannerAssumption plannerAssumption = new PlannerAssumption();
            plannerAssumption.Id = dr.Field<int>("ID");
            plannerAssumption.Pid = dr.Field<int>("PID");
            plannerAssumption.ClientRetirementAge = dr.Field<int>("ClientRetirementAge");
            plannerAssumption.SpouseRetirementAge = dr.Field<int>("SpouseRetirementAge");
            plannerAssumption.ClientLifeExpectancy = dr.Field<int>("ClientLifeExpectancy");
            plannerAssumption.SpouseLifeExpectancy = dr.Field<int>("SpouseLifeExpectancy");
            plannerAssumption.PreRetirementInflactionRate = dr.Field<decimal>("PreRetirementInflactionRate");
            plannerAssumption.PostRetirementInflactionRate = dr.Field<decimal>("PostRetirementInflactionRate");
            plannerAssumption.EquityReturnRate = dr.Field<decimal>("EquityReturnRate");
            plannerAssumption.DebtReturnRate = dr.Field<decimal>("DebtReturnRate");
            plannerAssumption.OtherReturnRate = dr.Field<decimal>("OtherReturnRate");
            plannerAssumption.Decription = dr.Field<string>("Description");
            plannerAssumption.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            plannerAssumption.UpdatedBy = dr.Field<int>("UpdatedBy");
            plannerAssumption.IsClientRetirmentAgeIsPrimary = dr.Field<bool>("ConsiderClientAgeForRetirment");
            plannerAssumption.ClientIncomeRise = dr.Field<decimal>("ClientIncomeRise");
            plannerAssumption.SpouseIncomeRise = dr.Field<decimal>("SpouseIncomeRise");
            plannerAssumption.OngoingExpRise = dr.Field<decimal>("OngoingExpRise");
            plannerAssumption.PostRetirementInvestmentReturnRate = dr.Field<decimal>("PostRetirementInvestmentReturnRate");
            return plannerAssumption;
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
