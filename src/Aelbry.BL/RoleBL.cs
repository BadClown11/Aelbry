using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class RoleBL
    {
        public List<Role> GetAll()
        {
            using (var dal = RoleDAL.Instance)
            {
                return dal.GetAll();
            }
        }

        public Role GetById(int roleId)
        {
            using (var dal = RoleDAL.Instance)
            {
                return dal.GetById(roleId);
            }
        }

        public int Create(Role role)
        {
            using (var dal = RoleDAL.Instance)
            {
                return dal.Create(role);
            }
        }

        public void Update(Role role)
        {
            using (var dal = RoleDAL.Instance)
            {
                dal.Update(role);
            }
        }

        public void Delete(int roleId, int modifiedBy)
        {
            using (var dal = RoleDAL.Instance)
            {
                dal.Delete(roleId, modifiedBy);
            }
        }

        public List<Permission> GetPermissions(int roleId)
        {
            using (var dal = RoleDAL.Instance)
            {
                return dal.GetPermissions(roleId);
            }
        }

        public void AssignPermission(int roleId, int permissionId)
        {
            using (var dal = RoleDAL.Instance)
            {
                dal.AssignPermission(roleId, permissionId);
            }
        }

        public void RemovePermission(int roleId, int permissionId)
        {
            using (var dal = RoleDAL.Instance)
            {
                dal.RemovePermission(roleId, permissionId);
            }
        }

        public List<Permission> GetAllPermissions()
        {
            using (var dal = PermissionDAL.Instance)
            {
                return dal.GetAll();
            }
        }
    }
}
