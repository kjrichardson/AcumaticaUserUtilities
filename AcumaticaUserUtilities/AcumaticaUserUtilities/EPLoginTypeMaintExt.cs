using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.EP;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.SM;
using PX.Web.UI.Frameset.Model.DAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AcumaticaUserUtilities
{

    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class EPLoginTypeMaintExt : PXGraphExtension<EPLoginTypeMaint>
    {
        public PXAction<EPLoginType> AddGuestRoles;
        [PXUIField(DisplayName = "Add Guest Roles", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = CommonActionCategories.OtherCategoryID)]
        protected virtual IEnumerable addGuestRoles(PXAdapter adapter)
        {
            EPLoginType current = Base.LoginType.Current;
            if (current == null) return adapter.Get();
            AddRoles(true, false);
            return adapter.Get();
        }

        public PXAction<EPLoginType> AddEmployeeRoles;
        [PXUIField(DisplayName = "Add Employee Roles", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = CommonActionCategories.OtherCategoryID)]
        protected virtual IEnumerable addEmployeeRoles(PXAdapter adapter)
        {
            EPLoginType current = Base.LoginType.Current;
            if (current == null) return adapter.Get();
            AddRoles(false, true);
            return adapter.Get();
        }

        public PXAction<EPLoginType> AddAllRoles;
        [PXUIField(DisplayName = "Add All Roles", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = CommonActionCategories.OtherCategoryID)]
        protected virtual IEnumerable addAllRoles(PXAdapter adapter)
        {
            EPLoginType current = Base.LoginType.Current;
            if (current == null) return adapter.Get();
            AddRoles(true, true);
            return adapter.Get();
        }

        /// <summary>
        /// Adds roles to the allowed roles list based on the specified criteria for guest and employee roles.
        /// </summary>
        /// <remarks>This method filters roles based on the specified criteria and adds them to the
        /// allowed roles list  if they are not already present. If both <paramref name="GuestRoles"/> and <paramref
        /// name="EmployeeRoles"/>  are <see langword="false"/>, no roles will be added.</remarks>
        /// <param name="GuestRoles">A boolean value indicating whether guest roles should be included.  If <see langword="true"/>, only guest
        /// roles are considered; otherwise, guest roles are excluded.</param>
        /// <param name="EmployeeRoles">A boolean value indicating whether employee roles should be included.  If <see langword="true"/>, only
        /// employee roles are considered; otherwise, employee roles are excluded.</param>
        public virtual void AddRoles(bool GuestRoles, bool EmployeeRoles)
        {
            PXSelect<Roles> cmd = new PXSelect<Roles>(Base);


            if (GuestRoles == false && EmployeeRoles == true)
            {
                cmd.WhereNew<Where<Roles.guest, Equal<False>>>();
            }
            else if (GuestRoles == true && EmployeeRoles == false)
            {
                cmd.WhereNew<Where<Roles.guest, Equal<True>>>();
            }
            var roles = cmd.Select();
            var ExistingRoles = Base.AllowedRoles.Select().RowCast<EPLoginTypeAllowsRole>().ToList();
            foreach (Roles role in roles)
            {
                if (ExistingRoles.Any(o => o.Rolename == role.Rolename) == false)
                {
                    Base.AllowedRoles.Insert(new EPLoginTypeAllowsRole { Rolename = role.Rolename });

                }
            }

        }


    }
}