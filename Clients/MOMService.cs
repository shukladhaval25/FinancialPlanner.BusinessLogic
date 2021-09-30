using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class MOMService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_MOM_BY_CLIENT_ID = "select * from MOM where ClientId = {0}";
        private const string SELECT_MOMPOINT_BY_CLIENT_ID = "select MOMPoints.*, Users.UserName, " +
            " Client.Name from  MOMPoints " +
            " INNER JOIN MOM ON MOMPoints.MId = MOM.MId " +
            " INNER JOIN Client ON MOM.ClientId = Client.ID " +
            " LEFT OUTER JOIN Users ON MOMPoints.EmpId = Users.ID " +
            " WHERE (MOM.ClientId = {0})";

        private const string SELECT_MOM_BY_MOMINFORAMTION = "SELECT MID FROM MOM WHERE MEETINGDATE = '{0}' AND MeetingType = '{1}' AND  ClientId = {2} AND MarkAsImportant = {3} AND " +
            " Duration ='{4}' AND NOTES = '{5}'";

        private const string INSERT_MOM_POINTS = "INSERT INTO MOMPOINTS VALUES ({0},'{1}','{2}',{3},'{4}','{5}')";

        private const string UPDATE_MOM_POINTS = "UPDATE MOMPOINTS SET DiscussedPoint='{0}',Responsibility ='{1}',EmpId ={2},TaskId='{3}',TaskStatus ='{4}' WHERE ID = {5}";

        private const string DELETE_MOMPOINTS_BY_ID = "DELETE FROM MOMPOINTS WHERE ID = {0}";
        private const string DELETE_MOMPOINTS_BY_MID = "DELETE FROM MOMPOINTS WHERE MID = {0}";

        private const string INSERT_MOM = "INSERT INTO MOM VALUES  ('{0}','{1}',{2},{3},'{4}','{5}')";

        private const string UPDATE_MOM = "UPDATE MOM SET MeetingDate='{0}', MeetingType ='{1}', ClientId = {2}, MarkAsImportant = {3}, Duration ='{4}', Notes ='{5}' WHERE MID = {6}";

        private const string DELETE_MOM_BY_MID = "DELETE FROM MOM WHERE MID = {0}";



        public List<MOMTransaction> GetMOM(int clientId)
        {
            List<MOMTransaction> mOMTransactions = new List<MOMTransaction>();
            try
            {
                Logger.LogInfo("Get: MOM details information process start");

                DataTable dtMOM = DataBase.DBService.ExecuteCommand(string.Format(SELECT_MOM_BY_CLIENT_ID, clientId));
                DataTable dtMOMPoints = DataBase.DBService.ExecuteCommand(string.Format(SELECT_MOMPOINT_BY_CLIENT_ID, clientId));
                foreach (DataRow dr in dtMOM.Rows)
                {
                    MOMTransaction mOMTransaction = convertToMOMObject(dr, dtMOMPoints);
                    mOMTransactions.Add(mOMTransaction);
                }
                Logger.LogInfo("Get: MOM details information process completed.");
                return mOMTransactions;
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

        public void DeleteMOM(int mId)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_MOM_BY_MID,
                    mId), true);
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_MOMPOINTS_BY_MID,
                    mId), true);
                DataBase.DBService.CommitTransaction();
            }catch(Exception ex){
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Update(MOMTransaction mOMTransaction)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, mOMTransaction.CId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_MOM,
                    mOMTransaction.MeetingDate,
                    mOMTransaction.MeetingType,
                    mOMTransaction.CId,
                    (mOMTransaction.MarkAsImportant == true) ? 1 : 0,
                    mOMTransaction.Duration,
                    mOMTransaction.Notes,
                    mOMTransaction.MId), true);



                foreach (MOMPoint point in mOMTransaction.MOMPoints)
                {
                    if (point.Id == 0)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_MOM_POINTS,
                        mOMTransaction.MId,
                        point.DiscussedPoint,
                        point.Responsibility,
                        point.EmpId,
                        point.TaskId,
                        point.TaskSatus), true);
                    }
                    else
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_MOM_POINTS,
                        point.DiscussedPoint,
                        point.Responsibility,
                        point.EmpId,
                        point.TaskId,
                        point.TaskSatus,
                        point.Id), true);
                    }
                }

                DataBase.DBService.CommitTransaction();
                //return mid;

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

        public void DeleteMOMPoint(int id)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_MOMPOINTS_BY_ID, id));
                //Activity.ActivitiesService.Add(ActivityType.DeleteMOMPoint, EntryStatus.Success,
                //         Source.Server, company.UpdatedByUserName, company.Name, company.MachineName);
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

        public int Add(MOMTransaction mOMTransaction)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, mOMTransaction.CId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_MOM,
                    mOMTransaction.MeetingDate,
                    mOMTransaction.MeetingType,
                    mOMTransaction.CId,
                    (mOMTransaction.MarkAsImportant == true) ? 1 : 0,
                    mOMTransaction.Duration,
                    mOMTransaction.Notes), true);

                int mid = getMOMId(mOMTransaction);

                if (mid > 0)
                {
                    foreach (MOMPoint point in mOMTransaction.MOMPoints)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_MOM_POINTS,
                        mid,
                        point.DiscussedPoint,
                        point.Responsibility,
                        point.EmpId,
                        point.TaskId,
                        point.TaskSatus), true);
                    }
                }
                else
                {
                    DataBase.DBService.RollbackTransaction();
                    return 0;
                }
                DataBase.DBService.CommitTransaction();
                return mid;

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

        private int getMOMId(MOMTransaction mOMTransaction)
        {
            return int.Parse(DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_MOM_BY_MOMINFORAMTION,
                 mOMTransaction.MeetingDate.ToString("yyyy-MM-dd hh:mm:ss"),
                 mOMTransaction.MeetingType,
                 mOMTransaction.CId,
                 (mOMTransaction.MarkAsImportant == true) ? 1 : 0,
                 mOMTransaction.Duration,
                 mOMTransaction.Notes
                 )));
        }

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }
        private MOMTransaction convertToMOMObject(DataRow dr, DataTable dtMOMPoints)
        {
            MOMTransaction mOMTransaction = new MOMTransaction();
            mOMTransaction.MId = int.Parse(dr["MId"].ToString());
            mOMTransaction.MeetingDate = DateTime.Parse(dr["MeetingDate"].ToString());
            mOMTransaction.MeetingType = dr.Field<string>("MeetingType");
            mOMTransaction.CId = int.Parse(dr["ClientId"].ToString());
            mOMTransaction.MarkAsImportant = bool.Parse(dr["MarkAsImportant"].ToString());
            mOMTransaction.Duration = dr.Field<string>("Duration");
            mOMTransaction.Notes = dr.Field<string>("Notes");

            mOMTransaction.MOMPoints = new List<MOMPoint>();
            foreach (DataRow drMOMPoint in dtMOMPoints.Select("MId = " + mOMTransaction.MId))
            {
                MOMPoint mOMPoint = new MOMPoint();
                mOMPoint.DiscussedPoint = drMOMPoint.Field<string>("DiscussedPoint");
                mOMPoint.EmpId = drMOMPoint["EmpId"] == DBNull.Value ? 0 : int.Parse(drMOMPoint["EmpId"].ToString());
                mOMPoint.Id = int.Parse(drMOMPoint["Id"].ToString());
                mOMPoint.MId = int.Parse(drMOMPoint["MId"].ToString());
                mOMPoint.Responsibility = drMOMPoint.Field<string>("Responsibility");
                mOMPoint.TaskId = drMOMPoint["TaskId"] == DBNull.Value ? "" : drMOMPoint.Field<string>("TaskId");
                mOMPoint.TaskSatus = drMOMPoint.Field<string>("TaskStatus");
                mOMPoint.UserName = drMOMPoint.Field<string>("UserName");
                mOMTransaction.MOMPoints.Add(mOMPoint);
            }

            return mOMTransaction;
        }
    }
}
