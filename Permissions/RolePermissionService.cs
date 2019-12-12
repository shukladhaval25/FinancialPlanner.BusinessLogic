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
        private const string SELECT_ROLE_PERMISSION = "SELECT Role.Id as RoleId, Role.Name, Role.IsCustomRole, " +  
            "RolePermission.FormId, Forms.FormName, RolePermission.[View], " +
            "RolePermission.[Add], RolePermission.[Update], RolePermission.[Delete] " +
            "FROM RolePermission INNER JOIN Role ON RolePermission.RoleId = Role.Id INNER JOIN " +
            "Forms ON RolePermission.FormId = Forms.ID " +
            "WHERE (Role.Id = {0})";

        private const string SELECT_ROLE = "SELECT * FROM ROLE";

        private const string SELECT_ROLE_BY_NAME = "SELECT ID FROM ROLE WHERE NAME = '{0}'";

        private const string INSERT_ROLE = "INSERT INTO[dbo].[Role]" +
           "([Name],[IsCustomRole],[CreatedOn],[CreatedBy],[UpdatedOn],[UpdatedBy]) " +
           "VALUES ('{0}','{1}','{2}',{3},'{4}',{5})";

        private const string INSERT_ROLE_PERMISSION = "INSERT INTO [dbo].[RolePermission] " +
            "([RoleId],[FormId],[View],[Add],[Update],[Delete]) " +
            "VALUES ({0},{1},'{2}','{3}','{4}','{5}')";

        private const string UPDATE_ROLE = "UPDATE ROLE SET NAME = '{0}',UPDATEDON = '{1}', UPDATEDBY = {2} " +
            "WHERE ID = {3}";

        public async Task Add(Role role)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                await Task.Run(() => DataBase.DBService.ExecuteCommandString(string.Format(INSERT_ROLE,
                      role.Name,
                      role.IsCustomRole,
                      role.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      role.CreatedBy,
                      role.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      role.UpdatedBy), true));

                int id = int.Parse(DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ROLE_BY_NAME,role.Name)));
                if (id > 0)
                {
                    role.Id = id;
                    savePermission(role);
                }
                //Activity.ActivitiesService.Add(ActivityType.CreateGeneralInsurance, EntryStatus.Success,
                //         Source.Server, GeneralInsurance.UpdatedByUserName, clientName, GeneralInsurance.MachineName);
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

        public async Task Delete(Role role)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                await Task.Run(() => DataBase.DBService.ExecuteCommandString(
                    string.Format("DELETE FROM ROLE WHERE ID ={0}", role.Id), true));

                deleteExistingPermissions(role.Id);

                //Activity.ActivitiesService.Add(ActivityType.CreateGeneralInsurance, EntryStatus.Success,
                //         Source.Server, GeneralInsurance.UpdatedByUserName, clientName, GeneralInsurance.MachineName);
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

        public async Task Update(Role role)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                await Task.Run(() => DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_ROLE,
                      role.Name,                    
                      role.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      role.UpdatedBy,
                      role.Id), true));


                deleteExistingPermissions(role.Id);
                savePermission(role);
               
                //Activity.ActivitiesService.Add(ActivityType.CreateGeneralInsurance, EntryStatus.Success,
                //         Source.Server, GeneralInsurance.UpdatedByUserName, clientName, GeneralInsurance.MachineName);
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

        private void deleteExistingPermissions(int id)
        {
            DataBase.DBService.ExecuteCommandString(
                string.Format("DELETE FROM ROLEPERMISSION WHERE ROLEID = {0}",id), true);
        }

        private void savePermission(Role role)
        {
            foreach (RolePermission rolePermission in role.Permissions)
            {
                 DataBase.DBService.ExecuteCommandString(string.Format(INSERT_ROLE_PERMISSION,
                      role.Id,
                      rolePermission.FormId,
                      rolePermission.IsView,
                      rolePermission.IsAdd,
                      rolePermission.IsUpdate,
                      rolePermission.IsDelete), true);
            }            
        }

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
            role.IsCustomRole = dr.Field<bool>("IsCustomRole");
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
