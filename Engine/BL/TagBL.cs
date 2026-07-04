using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class TagBL
    {
        public List<Tag> GetByCompany(int companyId)
        {
            using (var dal = TagDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public int Create(Tag tag)
        {
            using (var dal = TagDAL.Instance)
            {
                return dal.Create(tag);
            }
        }

        public void Update(Tag tag)
        {
            using (var dal = TagDAL.Instance)
            {
                dal.Update(tag);
            }
        }

        public void Delete(int tagId, int modifiedBy)
        {
            using (var dal = TagDAL.Instance)
            {
                dal.Delete(tagId, modifiedBy);
            }
        }
    }
}
