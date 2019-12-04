using FinancialPlanner.Common;
using FinancialPlanner.Common.Permission;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.Permissions
{
    public class FormsService
    {
        private const string  SELECT_ALL = "SELECT * FROM FORMS";
        public IList<Forms> GetAll()
        {
            try
            {
                Logger.LogInfo("Get: Forms process start");
                IList<Forms> forms = new List<Forms>();

                DataTable dtGoals = DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtGoals.Rows)
                {
                    Forms form = convertToFormObject(dr);
                    forms.Add(form);
                }
                Logger.LogInfo("Get: Forms process completed.");
                return forms;
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

        private Forms convertToFormObject(DataRow dr)
        {
            Forms form = new Forms();
            form.Id = dr.Field<int>("Id");
            form.FormName = dr.Field<string>("FormName");
            form.GroupName = dr.Field<string>("GroupName");
            return form;
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
