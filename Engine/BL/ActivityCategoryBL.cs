using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class ActivityCategoryBL
    {
        public List<ActivityCategory> GetByCompany(int companyId)
        {
            using (var dal = ActivityCategoryDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public int Create(ActivityCategory category)
        {
            using (var dal = ActivityCategoryDAL.Instance)
            {
                return dal.Create(category);
            }
        }
    }
}
