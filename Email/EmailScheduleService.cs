using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Email
{
    public class EmailScheduleService
    {
        private const string GET_ALL_EMAILSCHEDULE_QUERY = "SELECT ES.*, U.USERNAME AS UPDATEDBYUSERNAME, EA.NAME AS ARTICLEGROUPNAME " +
            "FROM  EMAILSCHEDULER ES , USERS U, EMAILGROUP EA WHERE U.ID = ES.UPDATEDBY AND ES.ARTICLEGROUPID = EA.ID ORDER BY SCHEDULETITLE";

        private const string INSERT_QUERY = "INSERT INTO EMAILSCHEDULER (" +
            "ArticleGroupID,EmailSenderGroupId,ScheduleTitle,ScheduleType,MonthDayInterval,"+
            "WeekDays,StartDateTime,NextRunDateTime,AllowRepeat,"+
            "CreatedOn,CreatedBy,UpdatedOn,UpdatedBy) " +
            " VALUES ({0},{1},'{2}',{3},{4}," +
            "'{5}','{6}','{7}','{8}','{9}',{10},'{11}',{12})";

        private const string UPDATE_QUERY = "UPDATE EMAILSCHEDULER SET " +
            "ArticleGroupID = {0},EmailSenderGroupId = {1}, ScheduleTitle ='{2}'," +
            "ScheduleType = {3}, MonthDayInterval = {4}, WeekDays = {5}, StartDateTime = '{6}'," +
            "NextRunDateTime = '{7}', AllowRepeat = '{8}', UPDATEDON = '{9}', UPDATEDBY = {10} " +
            "Where ID = {11}";

        private const string DELETE_QUERY = "DELETE FROM EMAILSCHEDULER WHERE ID = {0}";
        public object Get()
        {
            IList<EmailScheduler> lstEmailScheduler = new List<EmailScheduler>();

            DataTable dtEmailArticle =  DataBase.DBService.ExecuteCommand(GET_ALL_EMAILSCHEDULE_QUERY);
            ///DataTable dtEmailArticle = getDummyArticleData();
            foreach (DataRow dr in dtEmailArticle.Rows)
            {
                EmailScheduler emailScheduler = new EmailScheduler();
                emailScheduler.ID = dr.Field<int>("ID");
                emailScheduler.ArticleGroupId = dr.Field<int>("ArticleGroupId");
                emailScheduler.ArticleGroupName = dr.Field<string>("ArticleGroupName");
                emailScheduler.EmailSenderGroupId = dr.Field<int>("EmailSenderGroupId");
                emailScheduler.ScheduleTitle = dr.Field<string>("ScheduleTitle");
                emailScheduler.ScheduleType = (ScheduleOccurranceType)dr.Field<int>("ScheduleType");
                emailScheduler.MonthDayInterval = dr.Field<int?>("MonthDayInterval");
                emailScheduler.WeekDays = dr.Field<string>("WeekDays");
                emailScheduler.StartDateTime = dr.Field<DateTime>("StartDateTime");
                emailScheduler.NextRunDateTime = dr.Field<DateTime>("NextRunDateTime");
                emailScheduler.AllowRepeat = dr.Field<bool>("AllowRepeat");
                emailScheduler.CreatedOn = dr.Field<DateTime>("CreatedOn");
                emailScheduler.CreatedBy = dr.Field<int>("CreatedBy");
                emailScheduler.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
                emailScheduler.UpdatedBy = dr.Field<int>("UpdatedBy");
                emailScheduler.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");


                lstEmailScheduler.Add(emailScheduler);
            }
            return lstEmailScheduler;
        }

        public void Delete(EmailScheduler emailScheduler)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Goals.Pid));

                DataBase.DBService.BeginTransaction();
                int scheduleType = getscheduleType(emailScheduler.ScheduleType);
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY,emailScheduler.ID), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteEmailSchedule, EntryStatus.Success,
                       Source.Server, emailScheduler.UpdatedByUserName, emailScheduler.ScheduleTitle, emailScheduler.MachineName);
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

        public void Add(EmailScheduler emailScheduler)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Goals.Pid));

                DataBase.DBService.BeginTransaction();
                int scheduleType = getscheduleType(emailScheduler.ScheduleType);
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                    emailScheduler.ArticleGroupId, emailScheduler.EmailSenderGroupId,
                    emailScheduler.ScheduleTitle, scheduleType,
                    emailScheduler.MonthDayInterval, emailScheduler.WeekDays,
                    emailScheduler.StartDateTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    emailScheduler.NextRunDateTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    emailScheduler.AllowRepeat,
                    emailScheduler.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    emailScheduler.CreatedBy,
                    emailScheduler.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    emailScheduler.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateEmailSchedule, EntryStatus.Success,
                       Source.Server, emailScheduler.UpdatedByUserName, emailScheduler.ScheduleTitle, emailScheduler.MachineName);
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

        public void Update(EmailScheduler emailScheduler)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Goals.Pid));

                DataBase.DBService.BeginTransaction();
                int scheduleType = getscheduleType(emailScheduler.ScheduleType);
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                    emailScheduler.ArticleGroupId, emailScheduler.EmailSenderGroupId,
                    emailScheduler.ScheduleTitle, scheduleType,
                    emailScheduler.MonthDayInterval, emailScheduler.WeekDays,
                    emailScheduler.StartDateTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    emailScheduler.NextRunDateTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    emailScheduler.AllowRepeat,                    
                    emailScheduler.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    emailScheduler.UpdatedBy,
                    emailScheduler.ID), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateEmailSchedule, EntryStatus.Success,
                       Source.Server, emailScheduler.UpdatedByUserName, emailScheduler.ScheduleTitle, emailScheduler.MachineName);
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

        private int getscheduleType(ScheduleOccurranceType scheduleType)
        {
            if (scheduleType == ScheduleOccurranceType.Daily)
                return 0;
            if (scheduleType == ScheduleOccurranceType.Monthly)
                return 1;
            if (scheduleType == ScheduleOccurranceType.Weekly)
                return 2;
            return 0;
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
