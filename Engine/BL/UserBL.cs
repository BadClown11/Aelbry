using Aelbry.BL.Security;
using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class UserBL
    {
        public List<User> GetByCompany(int companyId)
        {
            using (var dal = UserDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public User GetById(int userId)
        {
            using (var dal = UserDAL.Instance)
            {
                return dal.GetById(userId);
            }
        }

        public int Create(User user, string plainPassword)
        {
            user.PasswordHash = PasswordHasher.Hash(plainPassword);

            using (var dal = UserDAL.Instance)
            {
                return dal.Create(user);
            }
        }

        public void Update(User user)
        {
            using (var dal = UserDAL.Instance)
            {
                dal.Update(user);
            }
        }

        public void ChangePassword(int userId, string currentPassword, string newPassword, int modifiedBy)
        {
            using (var dal = UserDAL.Instance)
            {
                var user = dal.GetById(userId);

                if (user == null || !PasswordHasher.Verify(currentPassword, user.PasswordHash))
                {
                    throw new InvalidOperationException("La contrasena actual no es valida.");
                }

                dal.UpdatePassword(userId, PasswordHasher.Hash(newPassword), modifiedBy);
            }
        }

        public void Delete(int userId, int modifiedBy)
        {
            using (var dal = UserDAL.Instance)
            {
                dal.Delete(userId, modifiedBy);
            }
        }

        public List<Role> GetRoles(int userId)
        {
            using (var dal = UserDAL.Instance)
            {
                return dal.GetRoles(userId);
            }
        }

        public void AssignRole(int userId, int roleId, int companyId)
        {
            using (var dal = UserDAL.Instance)
            {
                dal.AssignRole(userId, roleId, companyId);
            }
        }

        public void RemoveRole(int userId, int roleId, int companyId)
        {
            using (var dal = UserDAL.Instance)
            {
                dal.RemoveRole(userId, roleId, companyId);
            }
        }
    }
}
