using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class CompanyBL
    {
        public List<Company> GetAll()
        {
            using (var dal = CompanyDAL.Instance)
            {
                return dal.GetAll();
            }
        }

        public Company GetById(int companyId)
        {
            using (var dal = CompanyDAL.Instance)
            {
                return dal.GetById(companyId);
            }
        }

        public int Create(Company company)
        {
            using (var dal = CompanyDAL.Instance)
            {
                return dal.Create(company);
            }
        }

        public void Update(Company company)
        {
            using (var dal = CompanyDAL.Instance)
            {
                dal.Update(company);
            }
        }

        public void Delete(int companyId, int modifiedBy)
        {
            using (var dal = CompanyDAL.Instance)
            {
                dal.Delete(companyId, modifiedBy);
            }
        }
    }
}
