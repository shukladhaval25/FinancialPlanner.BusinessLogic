using FinancialPlanner.BusinessLogic.Users;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.Approval;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace FinancialPlanner.BusinessLogic.Approval
{
    public class ApprovalService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";

        private const string INSERT_QUERY = "INSERT INTO APPROVALS " +
                "(LINKEDITEMID,REQUESTRAISEDBY,REQUESTEDON, AUTHORISEDUSERSTOAPPROVE,APPROVALSTATUS," +
                    " DESCRIPTION, APPROVALTYPE ) " +
                " VALUES (" +
                "{0},{1},'{2}','{3}',{4},'{5}',{6})";


        //DECLARE @tags NVARCHAR(400) = 'Admin,Dhaval,Parag,road,touring,bike'

        //SELECT value FROM STRING_SPLIT(@tags, ',')  where value = 'Dhaval'
        private readonly string SELECT_ALL_BY_APPROVALTYPE =
        "SELECT Approvals.*,Users.UserName as RequestedBy,U1.UserName as ActionBy, " +
            "case " +
                "when Approvals.ApprovalType = 1 then TaskCard.TaskId " +
                "WHEN Approvals.ApprovalType = 2 THEN Planner.Name " +
            "END AS ItemId" +
            " FROM APPROVALS " +
            " LEFT JOIN TaskCard ON TaskCard.ID = Approvals.LinkedItemId AND Approvals.ApprovalType = 1 " +
            " LEFT JOIN Planner ON Planner.ID = Approvals.LinkedItemId AND Approvals.ApprovalType = 2 " +
            "LEFT Join Users on Users.ID = Approvals.RequestRaisedBy " +
            "LEFT JOIN Users U1 ON U1.ID = Approvals.ActionTakenBy " +
            " WHERE APPROVALTYPE = {0} AND " +
               " APPROVALSTATUS in (0,2)  AND (AuthorisedUsersToApprove LIKE '%,{1}' OR AuthorisedUsersToApprove LIKE '%,{1},%' OR AuthorisedUsersToApprove LIKE '{1},%')";

        private readonly string SELECT_ALL_BY_ALL_APPROVALTYPE =
        "SELECT Approvals.*,Users.UserName as RequestedBy,U1.UserName as ActionBy, " +
            "case " +
                "when Approvals.ApprovalType = 1 then TaskCard.TaskId " +
                "WHEN Approvals.ApprovalType = 2 THEN Planner.Name " +
            "END AS ItemId" +
            " FROM APPROVALS " +
            " LEFT JOIN TaskCard ON TaskCard.ID = Approvals.LinkedItemId AND Approvals.ApprovalType = 1 " +
            " LEFT JOIN Planner ON Planner.ID = Approvals.LinkedItemId AND Approvals.ApprovalType = 2 " +
            "LEFT Join Users on Users.ID = Approvals.RequestRaisedBy " +
            "LEFT JOIN Users U1 ON U1.ID = Approvals.ActionTakenBy " +
            " WHERE (AuthorisedUsersToApprove LIKE '%,{0}' OR " +
            "AuthorisedUsersToApprove LIKE '%,{0},%' OR AuthorisedUsersToApprove LIKE '{0},%') AND " +
               " APPROVALSTATUS in (0,2)";

        private readonly string SELECT_APPROVAL_ITEM_BY_LINKEDITEMID = "SELECT " +
            "  Approvals.*,Users.UserName as RequestedBy,U1.UserName as ActionBy " +
            "FROM APPROVALS " +
            "LEFT Join Users on Users.ID = Approvals.RequestRaisedBy " +
            "LEFT JOIN Users U1 ON U1.ID = Approvals.ActionTakenBy " +
            " WHERE LINKEDITEMID = {0}";
            

        private const string UPDATE_APPROVAL_QUERY = "UPDATE APPROVALS SET APPROVALSTATUS = {0},ACTIONTAKENBY={1},ACTIONTAKENON ='{2}',DESCRIPTION='{5}' WHERE ID ={3} AND APPROVALTYPE ={4}";

        private readonly string UPDATE_REASSIGN_QUERY = "UPDATE APPROVALS SET APPROVALSTATUS = " + (int)ApprovalStatus.WaitingForApproval + ", AUTHORISEDUSERSTOAPPROVE = '{0}',ACTIONTAKENBY={1},ACTIONTAKENON ='{2}',DESCRIPTION='{5}' WHERE ID ={3} AND APPROVALTYPE ={4}";

        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM ULIP N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";
        public void Add(ApprovalDTO approval)
        {
            try
            {
                UserService userService = new UserService();
                User user = userService.Get(approval.RequestRaisedBy);


                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                   approval.LinkedId,
                   approval.RequestRaisedBy,
                   approval.RequestedOn,
                   string.Format("1,{0}", approval.AuthorisedUsersToApprove),
                   Convert.ToInt32(approval.Status),
                   approval.Description,
                   Convert.ToInt32(approval.ApprovalType)),true);

                //Activity.ActivitiesService.Add(ActivityType.AddApproval, EntryStatus.Success,
                //         Source.Server, user.UserName, "Apply for approval", "");
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

        public List<ApprovalDTO> GetApprovals(ApprovalType approvalType, string userId)
        {
            try
            {
                Logger.LogInfo("Get: Approvals by approval type process start");
                List<ApprovalDTO> approvalDTOs = new List<ApprovalDTO>();

                DataTable dtApprovals;
                if (approvalType  == ApprovalType.All)
                    dtApprovals = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_ALL_APPROVALTYPE , userId));
                else
                    dtApprovals = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_APPROVALTYPE, approvalType,userId));
                foreach (DataRow dr in dtApprovals.Rows)
                {
                    ApprovalDTO approvalDTO = convertToApprovalDTO(dr);
                    approvalDTOs.Add(approvalDTO);
                }
                Logger.LogInfo("Get: Approvals by approval type process completed.");
                return approvalDTOs;
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

        public List<ApprovalDTO> GetApprovals(int itemId)
        {
            try
            {
                Logger.LogInfo("Get: Approvals by approval type process start");
                List<ApprovalDTO> approvalDTOs = new List<ApprovalDTO>();

                DataTable dtApprovals = DataBase.DBService.ExecuteCommand(string.Format(SELECT_APPROVAL_ITEM_BY_LINKEDITEMID, itemId));
                foreach (DataRow dr in dtApprovals.Rows)
                {
                    ApprovalDTO approvalDTO = convertToApprovalDTO(dr);
                    approvalDTOs.Add(approvalDTO);
                }
                Logger.LogInfo("Get: Approvals by approval type process completed.");
                return approvalDTOs;
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

        private ApprovalDTO convertToApprovalDTO(DataRow dr)
        {
            ApprovalDTO approvalDTO = new ApprovalDTO();
            approvalDTO.Id = dr.Field<int>("ID");
            approvalDTO.LinkedId = dr.Field<int>("LinkedItemId");
            approvalDTO.RequestRaisedBy = dr.Field<int>("RequestRaisedBy");
            approvalDTO.RequestedBy = dr.Field<string>("RequestedBy"); ;
            approvalDTO.RequestedOn = dr.Field<DateTime>("RequestedOn");
            approvalDTO.AuthorisedUsersToApprove = dr.Field<string>("AuthorisedUsersToApprove");

            approvalDTO.Status = (ApprovalStatus)Enum.Parse(typeof(ApprovalStatus), dr["ApprovalStatus"].ToString());
            
            if (dr["ActionTakenBy"] != DBNull.Value)
            {
                approvalDTO.ActionTakenBy = dr.Field<int>("ActionTakenBy") ;
                approvalDTO.ActionBy = dr.Field<string>("ActionBy");
            }
            if (dr["ActionTakenOn"] != DBNull.Value)
            {
                approvalDTO.ActionTakenOn = dr.Field<DateTime>("ActionTakenOn");
            }
            approvalDTO.Description = dr.Field<string>("Description");
            approvalDTO.ApprovalType = (ApprovalType)Enum.Parse(typeof(ApprovalType), dr["ApprovalType"].ToString());
            approvalDTO.ItemId = dr.Field<string>("ItemId");
            return approvalDTO;
        }

        public bool Approved(ApprovalDTO approval)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID, approval.ActionTakenBy));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_APPROVAL_QUERY,
                       (int) ApprovalStatus.Approve,
                       approval.ActionTakenBy,
                        approval.ActionTakenOn,
                       approval.Id,
                       (int) approval.ApprovalType,
                       approval.Description), true);

                // Activity.ActivitiesService.Add(ActivityType.UpdateULIP, EntryStatus.Success,
                //          Source.Server, ULIP.UpdatedByUserName, "ULIP", ULIP.MachineName);
                DataBase.DBService.CommitTransaction();
                return true;
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

        public bool Reject(ApprovalDTO approval)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID, approval.ActionTakenBy));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_APPROVAL_QUERY,
                      (int)  ApprovalStatus.Reject,
                       approval.ActionTakenBy,
                       approval.ActionTakenOn,
                       approval.Id,
                       (int) approval.ApprovalType,
                       approval.Description), true);

                //Activity.ActivitiesService.Add(ActivityType.UpdateULIP, EntryStatus.Success,
                //          Source.Server, ULIP.UpdatedByUserName, "ULIP", ULIP.MachineName);
                DataBase.DBService.CommitTransaction();
                return true;
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

        public bool Reassign(ApprovalDTO approval)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID, approval.ActionTakenBy));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_REASSIGN_QUERY,
                      string.Format("1,{0}",approval.AuthorisedUsersToApprove),
                       approval.ActionTakenBy,
                       approval.ActionTakenOn,
                       approval.Id,
                       (int) approval.ApprovalType,
                       approval.Description), true);

                // Activity.ActivitiesService.Add(ActivityType.UpdateULIP, EntryStatus.Success,
                //          Source.Server, ULIP.UpdatedByUserName, "ULIP", ULIP.MachineName);
                DataBase.DBService.CommitTransaction();
                return true;
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