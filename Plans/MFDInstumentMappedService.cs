using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.MFD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class MFDInstumentMappedService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";

        private const string GET_ALL_MAPPED_INSTRUMENT = "SELECT  * FROM MFDINSTRUMENTMAPPED WHERE PID = {0} AND GOALID = {1}";

        private const string GET_MAPPED_INSTRUMENT_BY_ID = "SELECT  * FROM MFDINSTRUMENTMAPPED WHERE ID = {0}";
        private const string INSERT_QUERY = "INSERT INTO [dbo].[MFDInstrumentMapped]" +
            "([GoalId],[PID],[InstrumentName],[CurrentValue],[AdditionalAnnualInv],[RealisationYear],[ROI]) VALUES " +
            "({0},{1},'{2}',{3},{4},'{5}',{6})";

        private const string UPDATE_QUERY = "UPDATE [dbo].[MFDInstrumentMapped] " +
            "SET[GoalId] = {0}," +
            "[PID] = {1} ," +
            "[InstrumentName] = '{2}'," +
            "[CurrentValue] = {3}, " +
            "[AdditionalAnnualInv] = {4}, " +
            "[RealisationYear] = '{5}'," +
            "[ROI] = {6} " +
            " WHERE ID = {7}";
        private readonly string DELET_QUERY = "DELETE FROM [dbo].[MFDInstrumentMapped] WHERE ID = {0}";

        public IList<MFDInstrumentMapped> GetAll(int plannerId, int goalId)
        {
            try
            {
                Logger.LogInfo("Get: MFD instrument  mapped process start");
                IList<MFDInstrumentMapped> instrumentMappeds = new List<MFDInstrumentMapped>();

                DataTable dtGoals = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_MAPPED_INSTRUMENT, plannerId,goalId));
                foreach (DataRow dr in dtGoals.Rows)
                {
                    MFDInstrumentMapped instrumentMapped = convertToMappedInstrument(dr);
                    instrumentMappeds.Add(instrumentMapped);
                }
                Logger.LogInfo("Get: MFD instrument completed.");
                return instrumentMappeds;
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

        public void Add(MFDInstrumentMapped instrumentMapped)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, instrumentMapped.PId));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   instrumentMapped.GoalId,
                   instrumentMapped.PId,
                   instrumentMapped.InstrumentName,
                   instrumentMapped.CurrentValue,
                   instrumentMapped.AdditionalAnnualInv,
                   instrumentMapped.RealisationYear,
                   instrumentMapped.ROI));

                //Activity.ActivitiesService.Add(ActivityType.CreateLoan, EntryStatus.Success,
                //         Source.Server, loan.UpdatedByUserName, clientName, loan.MachineName);
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

        public void Update(MFDInstrumentMapped instrumentMapped)
        {

            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, instrumentMapped.PId));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   instrumentMapped.GoalId,
                   instrumentMapped.PId,
                   instrumentMapped.InstrumentName,
                   instrumentMapped.CurrentValue,
                   instrumentMapped.AdditionalAnnualInv,
                   instrumentMapped.RealisationYear,
                   instrumentMapped.ROI,
                   instrumentMapped.Id));

                //Activity.ActivitiesService.Add(ActivityType.UpdateLoan, EntryStatus.Success,
                //         Source.Server, loan.UpdatedByUserName, clientName, loan.MachineName);
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

        public void Delete(MFDInstrumentMapped instrumentMapped)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, instrumentMapped.PId));

                DataBase.DBService.ExecuteCommand(string.Format(DELET_QUERY,
                  instrumentMapped.Id));

                //Activity.ActivitiesService.Add(ActivityType.DeleteLoan, EntryStatus.Success,
                //         Source.Server, instrumentMapped.UpdatedByUserName, clientName, instrumentMapped.MachineName);
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

        public MFDInstrumentMapped GetById(int id)
        {
            try
            {
                Logger.LogInfo("Get: MFD instrument mapped process start");
                MFDInstrumentMapped instrumentMapped = new MFDInstrumentMapped();
                DataTable dtGoals = DataBase.DBService.ExecuteCommand(string.Format(GET_MAPPED_INSTRUMENT_BY_ID, id));
                foreach (DataRow dr in dtGoals.Rows)
                {
                    instrumentMapped = convertToMappedInstrument(dr);
                }
                Logger.LogInfo("Get: MFD instrument mapped process completed.");
                return instrumentMapped;
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

        private MFDInstrumentMapped convertToMappedInstrument(DataRow dr)
        {
            MFDInstrumentMapped instrumentMapped = new MFDInstrumentMapped();
            instrumentMapped.Id = dr.Field<int>("ID");
            instrumentMapped.GoalId = dr.Field<int>("GoalId");
            instrumentMapped.PId = dr.Field<int>("PID");
            instrumentMapped.InstrumentName = dr.Field<string>("InstrumentName");
            instrumentMapped.CurrentValue = double.Parse(dr["CurrentValue"].ToString());
            instrumentMapped.AdditionalAnnualInv = double.Parse(dr["AdditionalAnnualInv"].ToString());
            instrumentMapped.RealisationYear = dr.Field<string>("RealisationYear");
            instrumentMapped.ROI = double.Parse(dr["ROI"].ToString());
            return instrumentMapped;
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
