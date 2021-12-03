using FinancialPlanner.Common;
using FinancialPlanner.Common.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;


namespace FinancialPlanner.BusinessLogic.Process
{
    public class ProcessService
    {

        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_QUERY = "SELECT PrimaryStep.Id, PrimaryStep.StepNo, PrimaryStep.Title, PrimaryStep.Description, PrimaryStep.Remarks, PrimaryStep.DurationInMinutes, PrimaryStep.TimelineInDays, PrimaryStep.PrimaryResponsibility, PrimaryStep.Owner, PrimaryStep.Checker, LinkSubStep.Id AS LinkSubStep.Id, LinkSubStep.PrimaryStepId, LinkSubStep.StepNo AS LinkSubStep.StepNo, LinkSubStep.Title AS LinkSubStep.Title, LinkSubStep.Description AS LinkSubStep.Description, LinkSubStep.Remarks AS LinkSubStep.Remarks, LinkSubStep.DurationInMinutes AS LinkSubStep.DurationInMinutes, LinkSubStep.TimelineInDays AS LinkSubStep.TimelineInDays, LinkSubStep.PrimaryResponsibility AS LinkSubStep.PrimaryResponsibility, LinkSubStep.Owner AS LinkSubStep.Owner, LinkSubStep.Checker AS LinkSubStep.Checker FROM PrimaryStep INNER JOIN LinkSubStep ON PrimaryStep.Id = LinkSubStep.PrimaryStepId";

        const string SELECT_PRIMARY_STEP = "SELECT[Id],[StepNo],[Title],[Description],[Remarks]" +
            ",[DurationInMinutes],[TimelineInDays],[PrimaryResponsibility],[Owner],[Checker] FROM [dbo].[PrimaryStep]";
        const string SELECT_LINKSUBSTEP_QUERY_BY_PRIMARY_STEP = "SELECT [Id],[PrimaryStepId],[StepNo],[Title],[Description],[Remarks]" +
            ",[DurationInMinutes],[TimelineInDays],[PrimaryResponsibility],[Owner],[Checker] FROM [dbo].[LinkSubStep] WHERE PrimaryStepId = {0}";

        const string DELETE_PRIMARY_STEP_QUERY = "DELETE FROM PRIMARYSTEP WHERE ID = {0}";
        const string DELETE_LINKSUBSTEP_QUERY_BY_PRIMARYSTEPID = "DELETE FROM LINKSUBSTEP WHERE PRIMARYSTEPID = {0}";
        const string DELETE_LINKSUBSTEP_QUERY_BY_ID = "DELETE FROM LINKSUBSTEP WHERE ID = {0}";

        const string INSERT_PRIMARY_STEP_QUERY = "INSERT INTO [dbo].[PrimaryStep] " +
           "([StepNo],[Title],[Description],[Remarks],[DurationInMinutes],[TimelineInDays],[PrimaryResponsibility],[Owner],[Checker]) VALUES ({0},'{1}','{2}','{3}',{4},{5},{6},{7},{8})";
        const string INSERT_LINKSUB_STEP_QUERY = "INSERT INTO [dbo].[LinkSubStep] " +
            "([PrimaryStepId],[StepNo],[Title],[Description],[Remarks],[DurationInMinutes],[TimelineInDays],[PrimaryResponsibility],[Owner],[Checker]) VALUES ({0},{1},'{2}','{3}','{4}',{5},{6},{7},{8},{9})";

        const string UPDATE_PRIMARY_STEP_QUERY = "UPDATE [dbo].[PrimaryStep] " +
            "SET [StepNo] = {0}" +
            ",[Title] = '{1}'" +
            ",[Description] = '{2}'" +
            ",[Remarks] = '{3}'" +
            ",[DurationInMinutes] = {4}" +
            ",[TimelineInDays] = {5}" +
            ",[PrimaryResponsibility] = {6}" +
            ",[Owner] = {7}" +
            ",[Checker] = {8}" +
            "WHERE Id = {9}";

        const string UPDATE_LINKSUB_STEP_QUERY = "UPDATE [dbo].[LinkSubStep] " +
           "SET [StepNo] = {0}" +
           ",[Title] = '{1}'" +
           ",[Description] = '{2}'" +
           ",[Remarks] = '{3}'" +
           ",[DurationInMinutes] = {4}" +
           ",[TimelineInDays] = {5}" +
           ",[PrimaryResponsibility] = {6}" +
           ",[Owner] = {7}" +
           ",[Checker] = {8}" +
           "WHERE Id = {9}";

        public IList<PrimaryStep> GetPrimarySteps()
        {
           
            try
            {
                Logger.LogInfo("Get: Primary steps process start");
                IList<PrimaryStep> processSteps = new List<PrimaryStep>();
                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_PRIMARY_STEP);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    PrimaryStep processStep = convertToPrimaryStep(dr);
                    processSteps.Add(processStep);
                }
                Logger.LogInfo("Get: Primary steps process completed.");
                return processSteps;
            }
            catch (Exception ex)
            {
                return null;
            }           
        }

        public IList<LinkSubStep> GetLinkSubSteps(int primaryStepId)
        {

            try
            {
                Logger.LogInfo("Get: LinkSubStep process start");
                IList<LinkSubStep> processSteps = new List<LinkSubStep>();
                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_LINKSUBSTEP_QUERY_BY_PRIMARY_STEP,primaryStepId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    LinkSubStep processStep = convertToLinkSubStep(dr);
                    processSteps.Add(processStep);
                }
                Logger.LogInfo("Get: LinkSubStep process completed.");
                return processSteps;
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

        public PrimaryStep UpdatePrimaryStep(PrimaryStep primaryStep)
        {
            try
            {
                string countResult = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                if (primaryStep.Id == 0)
                {
                    DataBase.DBService.ExecuteCommand(string.Format(INSERT_PRIMARY_STEP_QUERY,
                       primaryStep.StepNo, primaryStep.Title, primaryStep.Description,
                       primaryStep.Remarks, primaryStep.DurationInMinutes,
                       primaryStep.TimelineInDays, primaryStep.PrimaryResponsibility,
                       primaryStep.Owner, primaryStep.Checker));
                }
                else
                {
                    DataBase.DBService.ExecuteCommand(string.Format(UPDATE_PRIMARY_STEP_QUERY,
                        primaryStep.StepNo,
                        primaryStep.Title,
                        primaryStep.Description,
                        primaryStep.Remarks,
                        primaryStep.DurationInMinutes,
                        primaryStep.TimelineInDays,
                        primaryStep.PrimaryResponsibility,
                        primaryStep.Owner,
                        primaryStep.Checker,
                        primaryStep.Id));
                }

                string primaryStepId = DataBase.DBService.ExecuteCommandScalar(string.Format("SELECT PrimaryStep.Id FROM PrimaryStep where StepNo = {0} and Title = '{1}' and DurationInMinutes = {2} and TimelineInDays = {3}", primaryStep.StepNo, primaryStep.Title, primaryStep.DurationInMinutes, primaryStep.TimelineInDays));
                if (!string.IsNullOrEmpty(primaryStepId))
                {
                    primaryStep.Id = int.Parse(primaryStepId);
                }
                return primaryStep;
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

        public LinkSubStep UpdateLinkSubSteps(LinkSubStep linkSubStep)
        {
            try
            {
                string countResult = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                if (linkSubStep.Id == 0)
                {
                    DataBase.DBService.ExecuteCommand(string.Format(INSERT_LINKSUB_STEP_QUERY,
                       linkSubStep.PrimaryStepId,
                       linkSubStep.StepNo, linkSubStep.Title, linkSubStep.Description,
                       linkSubStep.Remarks, linkSubStep.DurationInMinutes,
                       linkSubStep.TimelineInDays, linkSubStep.PrimaryResponsibility,
                       linkSubStep.Owner, linkSubStep.Checker));
                }
                else
                {
                    DataBase.DBService.ExecuteCommand(string.Format(UPDATE_LINKSUB_STEP_QUERY,
                        linkSubStep.StepNo,
                        linkSubStep.Title,
                        linkSubStep.Description,
                        linkSubStep.Remarks,
                        linkSubStep.DurationInMinutes,
                        linkSubStep.TimelineInDays,
                        linkSubStep.PrimaryResponsibility,
                        linkSubStep.Owner,
                        linkSubStep.Checker,
                        linkSubStep.Id));
                }

                string linkSubStepId = DataBase.DBService.ExecuteCommandScalar(string.Format("SELECT Id FROM LinkSubStep where StepNo = {0} and Title = '{1}' and DurationInMinutes = {2} and TimelineInDays = {3} and PrimaryStepId ={4}", linkSubStep.StepNo, linkSubStep.Title, linkSubStep.DurationInMinutes, linkSubStep.TimelineInDays,linkSubStep.PrimaryStepId));
                if (!string.IsNullOrEmpty(linkSubStepId))
                {
                    linkSubStep.Id = int.Parse(linkSubStepId);
                }
                return linkSubStep;
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

        public void DeletePrimaryStep(int id)
        {
            DataBase.DBService.BeginTransaction();
            try
            {
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_LINKSUBSTEP_QUERY_BY_PRIMARYSTEPID, id), true);
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_PRIMARY_STEP_QUERY, id),true);
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

        public void DeleteLinkSubStep(int id)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_LINKSUBSTEP_QUERY_BY_ID, id));
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

        private PrimaryStep convertToPrimaryStep(DataRow dr)
        {
            PrimaryStep primaryStep = new PrimaryStep();
            primaryStep.Id = dr.Field<int>("ID");
            primaryStep.StepNo = dr.Field<int>("StepNo");
            primaryStep.Title = dr.Field<string>("Title");
            primaryStep.Description = dr.Field<string>("Description");
            primaryStep.Remarks = dr.Field<string>("Remarks");
            primaryStep.DurationInMinutes = dr.Field<int>("DurationInMinutes");
            primaryStep.TimelineInDays = dr.Field<int>("TimelineInDays");
            primaryStep.PrimaryResponsibility = dr.Field<int>("PrimaryResponsibility");
            primaryStep.Owner = dr.Field<int>("Owner");
            primaryStep.Checker = dr.Field<int>("Checker");
            return primaryStep;
        }

        private LinkSubStep convertToLinkSubStep(DataRow dr)
        {
            LinkSubStep linkSubStep = new LinkSubStep();
            linkSubStep.Id = dr.Field<int>("ID");
            linkSubStep.PrimaryStepId = dr.Field<int>("PrimaryStepId");
            linkSubStep.StepNo = dr.Field<int>("StepNo");
            linkSubStep.Title = dr.Field<string>("Title");
            linkSubStep.Description = dr.Field<string>("Description");
            linkSubStep.Remarks = dr.Field<string>("Remarks");
            linkSubStep.DurationInMinutes = dr.Field<int>("DurationInMinutes");
            linkSubStep.TimelineInDays = dr.Field<int>("TimelineInDays");
            linkSubStep.PrimaryResponsibility = dr.Field<int>("PrimaryResponsibility");
            linkSubStep.Owner = dr.Field<int>("Owner");
            linkSubStep.Checker = dr.Field<int>("Checker");
            return linkSubStep;
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
