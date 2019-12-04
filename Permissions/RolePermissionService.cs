using FinancialPlanner.Common;
using FinancialPlanner.Common.Permission;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Permissions
{
    public class RolePermissionService
    {
        //private const string SELECT_ROLE_PERMISSION = "SELECT Role.Id as RoleId, Role.Name, Role.IsCustomRole, " +
        //    "RolePermission.FormId, RolePermission.[View], RolePermission.[Add], " +
        //    "RolePermission.[Update], RolePermission.[Delete], Forms.FormName " +
        //    "FROM RolePermission CROSS JOIN Role CROSS JOIN  Forms" +
        //    "WHERE (Role.Id = {0})";

        private const string SELECT_ROLE_PERMISSION = "SELECT Role.Id as RoleId, Role.Name, Role.IsCustomRole, " +  
            "RolePermission.FormId, Forms.FormName, RolePermission.[View], " +
            "RolePermission.[Add], RolePermission.[Update], RolePermission.[Delete] " +
            "FROM RolePermission INNER JOIN Role ON RolePermission.RoleId = Role.Id INNER JOIN " +
            "Forms ON RolePermission.FormId = Forms.ID " +
            "WHERE (Role.Id = {0})";

        private const string SELECT_ROLE = "SELECT * FROM ROLE";
        public async Task<IList<Role>> GetAll()
        {
            try
            {
                Logger.LogInfo("Get: Role permission process start");
                IList<Role> roles = new List<Role>();

                DataTable dtGoals = await Task.Run(()=> DataBase.DBService.ExecuteCommand(SELECT_ROLE));
                foreach (DataRow dr in dtGoals.Rows)
                {
                    Role role = await convertToRole(dr);
                    roles.Add(role);
                }
                Logger.LogInfo("Get: Role permission process completed.");
                return roles;
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

        private async Task<Role> convertToRole(DataRow dr)
        {
            Role role = new Role();
            role.Id = dr.Field<int>("Id");
            role.Name = dr.Field<string>("Name");
            role.IsCutomRole = dr.Field<bool>("IsCustomRole");
            role.Permissions = await (Task.Run(()=> getRolePermission(role.Id)));
            return role;
        }

        private async Task<List<RolePermission>> getRolePermission(int id)
        {
            List<RolePermission> rolePermissions = new List<RolePermission>();

            DataTable dtGoals = await Task.Run(() => DataBase.DBService.ExecuteCommand(String.Format(SELECT_ROLE_PERMISSION,id)));
            foreach (DataRow dr in dtGoals.Rows)
            {
                RolePermission rolePermission = convertToRolePermissionObject(dr);
                rolePermissions.Add(rolePermission);
            }
            return rolePermissions;
        }

        private RolePermission convertToRolePermissionObject(DataRow dr)
        {
            RolePermission rolePermission = new RolePermission();
            rolePermission.RoleId = dr.Field<int>("RoleId");
            rolePermission.FormId = dr.Field<int>("FormId");
            rolePermission.FormName = dr.Field<string>("FormName");
            rolePermission.IsView = dr.Field<bool>("View");
            rolePermission.IsAdd = dr.Field<bool>("Add");
            rolePermission.IsUpdate = dr.Field<bool>("Update");
            rolePermission.IsDelete = dr.Field<bool>("Delete");
            return rolePermission;
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
