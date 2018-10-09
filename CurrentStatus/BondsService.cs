using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.CurrentStatus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.CurrentStatus
{
    public class BondsService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Bonds N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Bonds N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_Bonds= "INSERT INTO Bonds VALUES (" +
            "{0},'{1}','{2}','{3}',{4},{5},{6},{7},'{8}',{9},'{10}',{11},'{12}',{13})";

        const string UPDATE_Bonds = "UPDATE Bonds SET " +
            "[INVESTERNAME] = '{0}'," +
            "[COMPANYNAME] = '{1}'," +
            "[FOLIONO] ='{2}', RATE ={3}, NOOFBOND = {4},FACEVALUE ={5},CURRENTVALUE = {6}," +
            "MATURITYDATE = '{7}',GoalId ={8}, " +
            "[UpdatedOn] = '{9}', [UpdatedBy] ={10} " +
            "WHERE ID = {11} ";

        const string DELETE_Bonds = "DELETE FROM Bonds WHERE ID = {0}";


        public IList<Bonds> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Bonds process start");
                IList<Bonds> lstBondsOption = new List<Bonds>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Bonds mf = convertToBonds(dr);
                    lstBondsOption.Add(mf);
                }
                Logger.LogInfo("Get: Bonds fund process completed.");
                return lstBondsOption;
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
   

        public Bonds Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: Bonds by id process start");
                Bonds Bonds = new Bonds();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Bonds = convertToBonds(dr);
                }
                Logger.LogInfo("Get: Bonds by id process completed");
                return Bonds;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public void Add(Bonds Bonds)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Bonds.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_Bonds,
                      Bonds.Pid, Bonds.InvesterName, Bonds.CompanyName,
                      Bonds.FolioNo,
                      Bonds.Rate, Bonds.NoOfBond, Bonds.FaceValue,
                      Bonds.CurrentValue, Bonds.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"), Bonds.GoalId,
                      Bonds.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Bonds.CreatedBy,
                      Bonds.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Bonds.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateBonds, EntryStatus.Success,
                         Source.Server, Bonds.UpdatedByUserName, Bonds.CompanyName, Bonds.MachineName);
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

        public void Update(Bonds Bonds)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Bonds.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_Bonds,
                      Bonds.InvesterName,
                      Bonds.CompanyName,
                      Bonds.FolioNo,
                      Bonds.Rate,
                      Bonds.NoOfBond,
                      Bonds.FaceValue,
                      Bonds.CurrentValue,
                      Bonds.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      (Bonds.GoalId == null) ? null : Bonds.GoalId.Value.ToString(),
                      Bonds.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      Bonds.UpdatedBy,
                      Bonds.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateBonds, EntryStatus.Success,
                         Source.Server, Bonds.UpdatedByUserName, Bonds.CompanyName, Bonds.MachineName);
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

        public void Delete(Bonds Bonds)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Bonds.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_Bonds,
                      Bonds.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteBonds, EntryStatus.Success,
                         Source.Server, Bonds.UpdatedByUserName, Bonds.CompanyName, Bonds.MachineName);
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

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }
        private Bonds convertToBonds(DataRow dr)
        {
            Bonds bonds = new Bonds();
            bonds.Id = dr.Field<int>("ID");
            bonds.Pid = dr.Field<int>("PID");
            bonds.InvesterName = dr.Field<string>("InvesterName");
            bonds.CompanyName = dr.Field<string>("CompanyName");
            bonds.FolioNo = dr.Field<string>("FolioNo");
            bonds.FaceValue = float.Parse(dr["FaceValue"].ToString());
            bonds.Rate = float.Parse(dr["Rate"].ToString());
            bonds.NoOfBond = dr.Field<int>("NoOfBond");
            bonds.CurrentValue = Double.Parse(dr["CurrentValue"].ToString());           
            bonds.GoalId = dr.Field<int>("GoalId");
            bonds.MaturityDate = dr.Field<DateTime>("MaturityDate");
            bonds.UpdatedBy = dr.Field<int>("UpdatedBy");
            bonds.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            bonds.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            bonds.UpdatedBy = dr.Field<int>("UpdatedBy");
            bonds.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            bonds.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return bonds;
        }
    }
}
