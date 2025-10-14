using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcumaticaUserUtilities
{
    public class CopyUserFavoritesFilter : PXBqlTable, IBqlTable
    {
        #region FromUserID
        [PXGuid]
        [PXSelector(typeof(Users.pKID), SubstituteKey = typeof(Users.username), DescriptionField = typeof(Users.displayName))]
        [PXUIField(DisplayName = "User to Copy From")]
        public virtual Guid? FromUserID { get; set; }
        public abstract class fromUserID : PX.Data.BQL.BqlGuid.Field<fromUserID> { }
        #endregion

    }
}
