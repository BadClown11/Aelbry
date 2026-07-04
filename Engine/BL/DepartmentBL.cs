using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class DepartmentBL
    {
        public List<Department> GetByCompany(int companyId)
        {
            using (var dal = DepartmentDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public Department GetById(int departmentId)
        {
            using (var dal = DepartmentDAL.Instance)
            {
                return dal.GetById(departmentId);
            }
        }

        public int Create(Department department)
        {
            using (var dal = DepartmentDAL.Instance)
            {
                return dal.Create(department);
            }
        }

        public void Update(Department department)
        {
            using (var dal = DepartmentDAL.Instance)
            {
                dal.Update(department);
            }
        }

        public void Delete(int departmentId, int modifiedBy)
        {
            using (var dal = DepartmentDAL.Instance)
            {
                dal.Delete(departmentId, modifiedBy);
            }
        }
    }
}
