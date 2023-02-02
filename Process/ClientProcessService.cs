using FinancialPlanner.BusinessLogic.ProspectClients;
using FinancialPlanner.BusinessLogic.TaskManagements;
using FinancialPlanner.BusinessLogic.Users;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Process
{
   public class ClientProcessService
   {
        int primaryStepNo;
        IList<PrimaryStep> primarySteps;
        IList<LinkSubStep> linkSubSteps;
        IList<User> users;
        PrimaryStep primaryStep;
        LinkSubStep linkSubStep;
        ProcessService processService = new ProcessService();

        const string DESCRIPTION = "You have to complete step no {0} for financial planner step which having title like {1}";
        private const string SELECT_ID_BY_TASKDETAILS = "SELECT ID FROM TASKCARD WHERE PROJECTID = {0} AND TITLE ='{1}' AND CID ={2} AND ASSIGNTO ={3} AND OWNER = {4} AND TransactionType ='{5}' AND CREATEDBY ={6} AND OtherName = '{7}';";

        private const string UPDATE_TASKID = "UPDATE TASKCARD SET TASKID ='{0}' WHERE ID ={1}";

        private const string SELECT_USERID_FROM_PRIMARYSTEP = "select Users.Id AS UserId from PrimaryStep, Users where StepNo = {0} and Users.DesignationId = PrimaryStep.Owner and Users.ID = {1}";

        #region "Client Process SQL Query"
        private const string INSERT_CLIENT_PROCESS = "INSERT INTO CLIENTPROCESS (ClientId," +
            "PrimaryStepId,LinkSubStepId,Status,IsProspectClient) VALUES ({0},{1},{2},'{3}',{4})";
        private const string SELECT_MAX_CLIENTPROCESS_ID = "SELECT MAX(ID) FROM CLIENTPROCESS";

        private const string SELECT_PRIMARY_STEP = "select PrimaryStep.*, Users.Id AS UserId from PrimaryStep, Users where StepNo = {0} and Users.DesignationId = PrimaryStep.PrimaryResponsibility";

        private const string INSERT_CLIENT_PROCESS_DETAILS = "INSERT INTO ClientProcessDetail" +
            "(CPID,ASSIGNTO,ASSIGNDATE,EXPECTEDCOMPLEDATE,REFTASKID) VALUES ({0},{1},'{2}','{3}','{4}')";

        private const string INSERT_CLIENT_PROCESS_DETAILS_WITH_COMPLITIONDATE = "INSERT INTO ClientProcessDetail" +
          "(CPID,ASSIGNTO,ASSIGNDATE,EXPECTEDCOMPLEDATE,ACTUALCOMPLETEDATE,REFTASKID) VALUES ({0},{1},'{2}','{3}','{4}','{5}')";

        private const string SELECT_ALL_CURRENT_CLIENT_PROCESS= "SELECT Client.ID,Client.Name,LinkSubStep.Title, ClientProcess.Status, ClientProcess.PrimaryStepId,ClientProcess.LinkSubStepId,PrimaryStep.StepNo as PrimaryStepNo,LinkSubStep.StepNo as LinkSubStepNo,ClientProcessDetail.RefTaskId,LinkSubStep.AllowByPassProcess,Users.UserName,ClientProcessDetail.AssignTo,ClientProcessDetail.ExpectedCompleDate,ClientProcessDetail.ActualCompleteDate," +
          " CASE " +
          " WHEN TaskStatus = 0 THEN 'Backlog' "+
		  " WHEN TaskStatus = 1 THEN 'In Progress' " +
		  " WHEN TaskStatus = 2 THEN 'Backlog'" +
		  " when TaskStatus = 3 THEN 'Complete'" + 
		  " when TaskStatus = 4 THEN 'Discard'" + 
		  " when TaskStatus = 5 then 'Close'" +
		  " ELSE 'Unknown'" + 
	      " END AS TaskStatus" +
            " FROM  Client " +
            "Inner JOIN ClientProcess ON Client.ID = ClientProcess.ClientId " +
            "inner JOIN PrimaryStep ON ClientProcess.PrimaryStepId = PrimaryStep.Id " +
            "inner join LinkSubStep on ClientProcess.LinkSubStepId = LinkSubStep.Id " +
            "inner join ClientProcessDetail on ClientProcess.Id = ClientProcessDetail.CPID " +
            "inner join Users on ClientProcessDetail.AssignTo = Users.ID " +
            "inner join TaskCard on ClientProcessDetail.RefTaskId = TaskCard.TaskId " +
            "where ClientProcess.Status = 'P'";


        private const string SELECT_CLIENT_PROCESS_BY_CLIENTID_PLANNERID = "SELECT Client.ID,Client.Name,LinkSubStep.Title, ClientProcess.Status, ClientProcess.PrimaryStepId,ClientProcess.LinkSubStepId, PrimaryStep.StepNo as PrimaryStepNo,LinkSubStep.StepNo as LinkSubStepNo,ClientProcessDetail.RefTaskId,LinkSubStep.AllowByPassProcess,Users.UserName, ClientProcessDetail.AssignTo,ClientProcessDetail.ExpectedCompleDate,ClientProcessDetail.ActualCompleteDate,ClientProcess.PlannerId," +
            " CASE " +
          " WHEN TaskStatus = 0 THEN 'Backlog' " +
          " WHEN TaskStatus = 1 THEN 'In Progress' " +
          " WHEN TaskStatus = 2 THEN 'Backlog'" +
          " when TaskStatus = 3 THEN 'Complete'" +
          " when TaskStatus = 4 THEN 'Discard'" +
          " when TaskStatus = 5 then 'Close'" +
          " ELSE 'Unknown'" +
          " END AS TaskStatus" +
            " FROM  Client " +
            "Inner JOIN ClientProcess ON Client.ID = ClientProcess.ClientId " +
            "inner JOIN PrimaryStep ON ClientProcess.PrimaryStepId = PrimaryStep.Id " +
            "inner join LinkSubStep on ClientProcess.LinkSubStepId = LinkSubStep.Id " +
            "inner join ClientProcessDetail on ClientProcess.Id = ClientProcessDetail.CPID " +
            "inner join Users on ClientProcessDetail.AssignTo = Users.ID " +
            "inner join TaskCard on ClientProcessDetail.RefTaskId = TaskCard.TaskId " +
            "where ClientProcess.ClientId ={0}"; 
        #endregion

        public ClientProcessService()
        {
            primarySteps = processService.GetPrimarySteps();
            users = new UserService().Get();
        }

        public int GettTaskAssignTo(int primaryStepId, int linkStepId, int clientId)
        {
            string query = "";
            if (primaryStepId.Equals(2) && linkStepId.Equals(4))
            {
                query = string.Format("select id  from Users where DesignationId in (select PrimaryResponsibility from LinkSubStep where PrimaryStepId = {0} and id = {1})", primaryStepId, linkStepId);
                string primaryResponsibilityDesignationId = DataBase.DBService.ExecuteCommandScalar(query);
                return (string.IsNullOrEmpty(primaryResponsibilityDesignationId) ? 0 : int.Parse(primaryResponsibilityDesignationId));
            }
            else
            {
                query = string.Format("select ProspectClient.ResponsibilityAssignTo from ProspectClient where ClientId  = {0}", clientId);
                string primaryResponsibilityDesignationId = DataBase.DBService.ExecuteCommandScalar(query);
                return (string.IsNullOrEmpty(primaryResponsibilityDesignationId) ? 0 : int.Parse(primaryResponsibilityDesignationId));
            }
        }

        public IList<CurrentClientProcess> GetClientProcess(int clientId,int? plannerId)
        {
            IList<CurrentClientProcess> currentClientProcesses = new List<CurrentClientProcess>();

            
            
            string queryString = string.Format(SELECT_CLIENT_PROCESS_BY_CLIENTID_PLANNERID, clientId);
            if (plannerId != null)
                queryString = queryString +" and (ClientProcess.PlannerId is null or ClientProcess.PlannerId = " + plannerId +")";

            DataTable dtCurrentClientProcess = DataBase.DBService.ExecuteCommand(queryString);
            foreach (DataRow dr in dtCurrentClientProcess.Rows)
            {
                CurrentClientProcess currentClientProcess = convertToCurrentClientProcess(dr);
                currentClientProcesses.Add(currentClientProcess);
            }
            return currentClientProcesses;
        }


        public IList<CurrentClientProcess> GetAll()
        {
            IList<CurrentClientProcess> currentClientProcesses = new List<CurrentClientProcess>();

            DataTable dtCurrentClientProcess = DataBase.DBService.ExecuteCommand(SELECT_ALL_CURRENT_CLIENT_PROCESS);
            foreach (DataRow dr in dtCurrentClientProcess.Rows)
            {
                CurrentClientProcess currentClientProcess = convertToCurrentClientProcess(dr);
                currentClientProcesses.Add(currentClientProcess);
            }
            return currentClientProcesses;
        }

        private CurrentClientProcess convertToCurrentClientProcess(DataRow dr)
        {
            CurrentClientProcess currentClientProcess = new CurrentClientProcess();
            currentClientProcess.ClientId = dr.Field<int>("ID");
            currentClientProcess.ClientName = dr.Field<string>("Name");
            currentClientProcess.ProcessTitle = dr.Field<string>("Title");
            currentClientProcess.ProcessStatus = dr.Field<string>("Status");
            currentClientProcess.PrimaryStepId = dr.Field<int>("PrimaryStepId");
            currentClientProcess.LinkSubStepId = dr.Field<int>("LinkSubStepId");
            currentClientProcess.PrimaryStepNo = dr.Field<int>("PrimaryStepNo");
            currentClientProcess.LinkSubStepNo = dr.Field<int>("LinkSubStepNo");
            currentClientProcess.AllowByPassProcess = (dr["AllowByPassProcess"] == DBNull.Value) ? false : bool.Parse(dr["AllowByPassProcess"].ToString());
            currentClientProcess.RefTaskId = dr.Field<string>("RefTaskId");
            currentClientProcess.AssignTo = dr.Field<int>("AssignTo");
            currentClientProcess.UserName = dr.Field<string>("UserName");
            currentClientProcess.ExpectedCompletionDate = dr.Field<DateTime>("ExpectedCompleDate");
            if (dr["ActualCompleteDate"] != DBNull.Value)
            {
                currentClientProcess.ActualCompletionDate = DateTime.Parse(dr["ActualCompleteDate"].ToString());
            }
            else
            {
                currentClientProcess.ActualCompletionDate = null;
            }
            currentClientProcess.TaskStatus = dr.Field<string>("TaskStatus");
            return currentClientProcess;
        }

        public void Add(ClientProcess clientProcess, int assignTo,bool addTaskForProcess = true )
        {
            try
            {
                if (primarySteps.Count() > 0)
                {                   
                    PrimaryStep primaryStep = primarySteps.First(i => i.Id == clientProcess.PrimaryStepId);

                    if (primaryStep != null)
                    {
                        linkSubSteps = processService.GetLinkSubSteps(clientProcess.PrimaryStepId);
                        primaryStepNo = primaryStep.StepNo;
                    }
                    else
                    {
                        throw new Exception("Invalid process step as parameter.");
                    }

                    string taskId = string.Empty;
                   
                    if (addTaskForProcess)
                    {
                        taskId = addTask(clientProcess, assignTo);
                    }
                    addClientProcess(clientProcess, assignTo,taskId);
                }
            }
            catch(Exception ex)
            {
                //log error
            }
        }

        private void addClientProcess(ClientProcess clientProcess, int assignTo,string taskId)
        {
           
            DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_PROCESS,
                       clientProcess.ClientId,
                       clientProcess.PrimaryStepId,
                       clientProcess.LinkStepId,
                       clientProcess.Status,
                       (clientProcess.IsProcespectClient) ? 1 : 0));

            System.Threading.Thread.Sleep(1000);
            int maxProcessId = int.Parse(DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_CLIENTPROCESS_ID));

           
            if (taskId.Equals("") && clientProcess.Status.Equals("C"))
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_PROCESS_DETAILS_WITH_COMPLITIONDATE,
                   maxProcessId,
                   assignTo,
                   DateTime.Now.ToString("yyyy-MM-dd"),
                   getDueDate(clientProcess).ToString("yyyy-MM-dd"),
                   DateTime.Now.ToString("yyyy-MM-dd"),
                   taskId));

            }
            else
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_CLIENT_PROCESS_DETAILS,
                   maxProcessId,
                   assignTo,
                   DateTime.Now.ToString("yyyy-MM-dd"),
                   getDueDate(clientProcess).ToString("yyyy-MM-dd"),
                   taskId));

            }
        }

        private string addTask(ClientProcess clientProcess, int AssignTo)
        {
            TaskCard taskCard = getTaskCard(AssignTo, clientProcess);
            TaskService taskService = new TaskService();
            taskService.Add(taskCard);
            System.Threading.Thread.Sleep(1000);
            int taskId = getTaskID(taskCard);
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_TASKID, taskCard.TaskId + "-" + taskId, taskId), true);
            return taskCard.TaskId + "-" + taskId;           
        }

        private static int getTaskID(TaskCard taskcard)
        {
            try
            {
                return int.Parse(DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID_BY_TASKDETAILS,
                     taskcard.ProjectId,
                     taskcard.Title,
                     taskcard.CustomerId,
                     taskcard.AssignTo,
                     taskcard.Owner,
                     taskcard.TransactionType,
                     taskcard.CreatedBy,
                     taskcard.OtherName
                     )));
            }
            catch(Exception ex)
            {
                return 0;
            }
        }


        private TaskCard getTaskCard(int AssignTo,ClientProcess clientProcess)
        {


            TaskCard taskCard = new TaskCard();
            taskCard.TaskId = "OT";
            taskCard.ProjectId = 3;
            taskCard.TransactionType = "";
            taskCard.Type = CardType.Task;
            taskCard.CustomerId = 0;
            taskCard.Title = getTaskTitleByProcessId(clientProcess.PrimaryStepId, clientProcess.LinkStepId);
            taskCard.Owner = 1;
            taskCard.CreatedBy = 1;
            taskCard.CreatedOn = System.DateTime.Now.Date;
            taskCard.UpdatedBy = taskCard.CreatedBy;
            taskCard.UpdatedByUserName = "Admin";
            taskCard.UpdatedOn = System.DateTime.Now.Date;
            taskCard.AssignTo = AssignTo;
            taskCard.Priority = Priority.High;
            taskCard.TaskStatus = Common.Model.TaskManagement.TaskStatus.Backlog;
            taskCard.DueDate = getDueDate(clientProcess);
            taskCard.CompletedPercentage = 0;
            taskCard.Description = clientProcess.Description + System.Environment.NewLine + string.Format(DESCRIPTION, primaryStepNo, taskCard.Title);
            taskCard.MachineName = System.Environment.MachineName;
            if (clientProcess.IsProcespectClient)
            {
                ProspectClient prospectClient = new ProspectClientService().GetById(clientProcess.ClientId);
                taskCard.OtherName = prospectClient.Name;
            }
            else
            {
                taskCard.CustomerId = clientProcess.ClientId;
            }
            return taskCard;
        }

        private DateTime getDueDate(ClientProcess clientProcess)
        {
            DateTime dueDate = DateTime.Now.Date;
            if (!clientProcess.LinkStepId.Equals(0))
            {
                LinkSubStep linkSub = linkSubSteps.First(i => i.Id.Equals(clientProcess.LinkStepId));
                if (linkSub != null)
                {
                    dueDate = dueDate.AddDays(linkSub.TimelineInDays);
                }
                else
                {
                    
                }
            }
            return dueDate;
        }

        private string getTaskTitleByProcessId(int primaryStepId, int linkStepId)
        {
            string title = string.Empty;
            if (!linkStepId.Equals(0))
            {
                LinkSubStep linkSub =  linkSubSteps.First(i => i.PrimaryStepId == primaryStepId && i.Id.Equals(linkStepId));
                if (linkSub != null)
                {
                    title = linkSub.Title;
                }
            }
            else
            {
                PrimaryStep primaryStep = primarySteps.First(i => i.Id.Equals(primaryStepId));
                if (primaryStep != null)
                {
                    title = primaryStep.Title;
                }
            }
            return title;
        }

        public void UpdateClientDetailProcess(ClientProcessDetails processDetails)
        {

        }
   }
}
