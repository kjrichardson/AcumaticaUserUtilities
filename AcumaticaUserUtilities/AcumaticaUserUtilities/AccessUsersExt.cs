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
    public class AccessUsersExt : PXGraphExtension<AccessUsers>
    {
        #region Views

        public PXFilter<CopyUserFavoritesFilter> CopyUserFavoritesView;
        public SelectFrom<MUIFavoriteScreen>.Where<MUIFavoriteScreen.username.IsEqual<Users.username.FromCurrent>>.View FavoriteScreens;
        public SelectFrom<MUIFavoriteTile>.Where<MUIFavoriteTile.username.IsEqual<Users.username.FromCurrent>>.View FavoriteTiles;
        public SelectFrom<MUIPinnedScreen>.Where<MUIPinnedScreen.username.IsEqual<Users.username.FromCurrent>>.View PinnedScreens;

        #endregion

        #region Events
        public virtual void _(Events.RowSelected<PX.SM.Users> e, PXRowSelected del)
        {
            del?.Invoke(e.Cache, e.Args);

            var User = Base.UserList.Current;
            ConvertADRolesToLocal.SetEnabled(User.IsADUser == true && (User.OverrideADRoles == null || User.OverrideADRoles == false));
            ConvertADUserToLocal.SetEnabled(User.IsADUser == true);
        }


        #endregion

        #region Actions
        public PXAction<PX.SM.Users> ConvertADRolesToLocal;
        [PXUIField(DisplayName = "Convert AD Roles to Local Roles", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = CommonActionCategories.OtherCategoryID)]
        protected virtual IEnumerable convertADRolesToLocal(PXAdapter adapter)
        {
            var User = Base.UserList.Current;

            if (User.IsADUser == false || User.OverrideADRoles == true)
                return adapter.Get();
            ConvertRolesFromAD();

            return adapter.Get();
        }
        /// <summary>
        /// Converts roles from Active Directory (AD) and updates the current user's role selection.
        /// </summary>
        /// <remarks>This method retrieves the roles allowed by the current login type, marks them as selected,  and
        /// overrides the default role selection behavior for the current user.</remarks>
        public virtual void ConvertRolesFromAD()
        {
            List<string> Roles = new List<string>();
            foreach (EPLoginTypeAllowsRole role in Base.AllowedRoles.Select())
            {
                Roles.Add(role.Rolename);
            }

            Base.UserList.Cache.SetValueExt<PX.SM.Users.overrideADRoles>(Base.UserList.Current, true);
            foreach (EPLoginTypeAllowsRole role in Base.AllowedRoles.Select())
            {
                if (Roles.Contains(role.Rolename))
                    Base.AllowedRoles.Cache.SetValueExt<EPLoginTypeAllowsRole.selected>(role, true);
            }
        }

        /// <summary>
        /// Transfers grid preferences from one user to another.
        /// </summary>
        /// <remarks>This method updates the grid preferences in the database by reassigning them from the
        /// specified source user  to the target user. Both usernames must be valid and correspond to existing users in
        /// the system.</remarks>
        /// <param name="FromUser">The username of the user whose grid preferences will be transferred.</param>
        /// <param name="ToUser">The username of the user to whom the grid preferences will be assigned.</param>
        public virtual void MoveGridPreferencesToUser(string FromUser, string ToUser)
        {
            PXDatabase.Update<GridPreferences>(
                    new PXDataFieldAssign("UserName", ToUser),
                    new PXDataFieldRestrict("UserName", FromUser)
                );
        }


        /// <summary>
        /// Transfers all favorite screens, tiles, and pinned screens from one user to another.
        /// </summary>
        /// <remarks>This method updates the database to reassign all favorite screens, tiles, and pinned
        /// screens associated with  the <paramref name="FromUser"/> to the <paramref name="ToUser"/>. Both users must
        /// exist in the system.</remarks>
        /// <param name="FromUser">The username of the user whose favorites will be transferred. Cannot be null or empty.</param>
        /// <param name="ToUser">The username of the user to whom the favorites will be transferred. Cannot be null or empty.</param>
        public virtual void MoveFavoriesToUser(string FromUser, string ToUser)
        {
            PXDatabase.Update<MUIFavoriteScreen>(
                    new PXDataFieldAssign<MUIFavoriteScreen.username>(ToUser),
                    new PXDataFieldRestrict<MUIFavoriteScreen.username>(FromUser)
                );
            PXDatabase.Update<MUIFavoriteTile>(
                   new PXDataFieldAssign<MUIFavoriteTile.username>(ToUser),
                   new PXDataFieldRestrict<MUIFavoriteTile.username>(FromUser)
               );
            PXDatabase.Update<MUIPinnedScreen>(
                   new PXDataFieldAssign<MUIPinnedScreen.username>(ToUser),
                   new PXDataFieldRestrict<MUIPinnedScreen.username>(FromUser)
               );
        }

        /// <summary>
        /// Transfers all filters associated with one user to another user.
        /// </summary>
        /// <remarks>This method updates the ownership of filters in the database, reassigning them from
        /// the specified source user  (<paramref name="FromUser"/>) to the target user (<paramref name="ToUser"/>).
        /// Both users must exist in the system.</remarks>
        /// <param name="FromUser">The username of the user whose filters will be transferred. Cannot be null or empty.</param>
        /// <param name="ToUser">The username of the user to whom the filters will be assigned. Cannot be null or empty.</param>
        public virtual void MoveFiltersToUser(string FromUser, string ToUser)
        {
            PXDatabase.Update<FilterHeader>(
               new PXDataFieldAssign<FilterHeader.userName>(ToUser),
               new PXDataFieldRestrict<FilterHeader.userName>(FromUser)
           );

        }

        /// <summary>
        /// Converts an Active Directory (AD) user to a local user within the system.
        /// </summary>
        /// <remarks>This action is used to migrate user accounts from an external
        /// Active Directory into the local user management system. Ensure that the AD user exists and has the necessary
        /// attributes before invoking this action.</remarks>
        public PXAction<PX.SM.Users> ConvertADUserToLocal;
        [PXUIField(DisplayName = "Convert AD User to Local User", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = CommonActionCategories.OtherCategoryID)]
        protected virtual IEnumerable convertADUserToLocal(PXAdapter adapter)
        {
            var User = Base.UserList.Current;

            if (User.IsADUser == false)
                return adapter.Get();

            if (String.IsNullOrWhiteSpace(User.Email))
                throw new PXException(Messages.RequireEmailAddressToConvertToLocal);


            string OldUserName = User.Username;
            string NewUserName = User.Email;

            if (OldUserName.Contains(NewUserName) == false)
                throw new PXException(Messages.EmailRequiredToPreventDuplicates);

            using (var ts = new PXTransactionScope())
            {

                //check for existing user with the email address and fail if found.

                ConvertRolesFromAD();
                Base.Save.Press();
                PXDatabase.Update<Users>(
                     new PXDataFieldAssign<Users.username>(NewUserName),
                     new PXDataFieldAssign<Users.source>(PXUsersSourceListAttribute.Application),
                     new PXDataFieldAssign<Users.allowPasswordRecovery>(true),
                     new PXDataFieldAssign<Users.passwordChangeable>(true),
                     new PXDataFieldAssign<Users.password>(Guid.NewGuid().ToString()),
                     new PXDataFieldRestrict<Users.username>(OldUserName)
                 );
                MoveFavoriesToUser(OldUserName, NewUserName);
                MoveFiltersToUser(OldUserName, NewUserName);
                MoveGridPreferencesToUser(OldUserName, NewUserName);

                //menu on the left/top
                PXDatabase.Update<MUIUserPreferences>(
                       new PXDataFieldAssign<MUIUserPreferences.username>(NewUserName),
                       new PXDataFieldRestrict<MUIUserPreferences.username>(OldUserName)
                   );
                ts.Complete();
            }

            AccessUsers newGraph = PXGraph.CreateInstance<AccessUsers>();
            newGraph.UserList.Current = newGraph.UserList.Search<Users.username>(NewUserName);
            throw new PXRedirectRequiredException(newGraph, "Redirect");

        }



        /// <summary>
        /// Copies the favorite items from the specified user to the current user.
        /// </summary>
        /// <remarks>If the specified user exists, their favorite items are copied to the current user. 
        /// If the user does not exist or <paramref name="FromUserPKID"/> is <see langword="null"/>, the method does
        /// nothing.</remarks>
        /// <param name="FromUserPKID">The unique identifier of the user whose favorites are to be copied.  This parameter can be <see
        /// langword="null"/>, in which case no action is performed.</param>
        public virtual void CopyFavoritesFromUser(Guid? FromUserPKID)
        { 
            var User = Users.PK.Find(Base, FromUserPKID);
            if(User != null)
                CopyFavoritesFromUser(User.Username);
        }

        /// <summary>
        /// Copies the favorite screens, tiles, and pinned screens from the specified user to the current user.
        /// </summary>
        /// <remarks>This method retrieves the favorite screens, tiles, and pinned screens of the
        /// specified user and adds them to the current user's favorites, ensuring no duplicates are created. If the
        /// specified user does not exist, an exception is thrown.</remarks>
        /// <param name="FromUserName">The username of the user whose favorites will be copied. This parameter cannot be null or empty.</param>
        /// <exception cref="PXException">Thrown if the specified user does not exist.</exception>
        public virtual void CopyFavoritesFromUser(string FromUserName)
        {
            using (var ts = new PXTransactionScope())
            {
                var User = Base.UserList.Current;
                if (User == null) return;

                var FromUser = SelectFrom<Users>.Where<Users.username.IsEqual<@P.AsString>>.View.Select(Base, FromUserName).TopFirst;
                if (FromUser == null)
                    throw new PXException(Messages.UserNotFound);

                var CurrentFavoriteScreens = SelectFrom<MUIFavoriteScreen>.Where<MUIFavoriteScreen.username.IsEqual<@P.AsString>>.View.Select(Base, User.Username).RowCast<MUIFavoriteScreen>().ToList();
                var CurrentFavoriteTiles = SelectFrom<MUIFavoriteTile>.Where<MUIFavoriteTile.username.IsEqual<@P.AsString>>.View.Select(Base, User.Username).RowCast<MUIFavoriteTile>().ToList();
                var CurrentPinnedScreens = SelectFrom<MUIPinnedScreen>.Where<MUIPinnedScreen.username.IsEqual<@P.AsString>>.View.Select(Base, User.Username).RowCast<MUIPinnedScreen>().ToList();

                var FromUserFavoriteScreens = SelectFrom<MUIFavoriteScreen>.Where<MUIFavoriteScreen.username.IsEqual<@P.AsString>>.View.Select(Base, FromUserName).RowCast<MUIFavoriteScreen>().ToList();
                var FromUserFavoriteTiles = SelectFrom<MUIFavoriteTile>.Where<MUIFavoriteTile.username.IsEqual<@P.AsString>>.View.Select(Base, FromUserName).RowCast<MUIFavoriteTile>().ToList();
                var FromUserPinnedScreens = SelectFrom<MUIPinnedScreen>.Where<MUIPinnedScreen.username.IsEqual<@P.AsString>>.View.Select(Base, FromUserName).RowCast<MUIPinnedScreen>().ToList();

                foreach (MUIFavoriteScreen FromUserFavoriteScreen in FromUserFavoriteScreens)
                {
                    if (CurrentFavoriteScreens.Any(o => o.NodeID == FromUserFavoriteScreen.NodeID && o.IsPortal == FromUserFavoriteScreen.IsPortal) == false)
                    {
                        FavoriteScreens.Insert(new MUIFavoriteScreen()
                        {
                            Username = User.Username,
                            IsPortal = FromUserFavoriteScreen.IsPortal,
                            NodeID = FromUserFavoriteScreen.NodeID
                        });

                    }
                }

                foreach (MUIFavoriteTile FromUserFavoriteTile in FromUserFavoriteTiles)
                {
                    if (CurrentFavoriteTiles.Any(o => o.TileID == FromUserFavoriteTile.TileID && o.IsPortal == FromUserFavoriteTile.IsPortal) == false )
                    {
                        FavoriteTiles.Insert(new MUIFavoriteTile()
                        {
                            Username = User.Username,
                            IsPortal = FromUserFavoriteTile.IsPortal,
                            TileID = FromUserFavoriteTile.TileID
                        });

                    }
                }

                foreach (MUIPinnedScreen FromUserPinnedScreen in FromUserPinnedScreens)
                {
                    if (CurrentPinnedScreens.Any(o => o.NodeID == FromUserPinnedScreen.NodeID && o.IsPortal == FromUserPinnedScreen.IsPortal && o.WorkspaceID == FromUserPinnedScreen.WorkspaceID) == false)
                    {
                        PinnedScreens.Insert(new MUIPinnedScreen()
                        {
                            Username = User.Username,
                            IsPortal = FromUserPinnedScreen.IsPortal,
                            NodeID = FromUserPinnedScreen.NodeID,
                            WorkspaceID = FromUserPinnedScreen.WorkspaceID
                        });

                    }
                }
                Base.Persist();
                ts.Complete();
            }
        }

        /// <summary>
        /// Copies the favorites of the selected user to another user.
        /// </summary>
        /// <remarks>This action allows the duplication of a user's favorite items, enabling the selected
        /// user's favorites  to be shared or transferred to the current user. Ensure that the source user has favorites
        /// defined before  invoking this action.</remarks>
        public PXAction<Users> CopyUserFavorites;
        [PXUIField(DisplayName = "Copy User Favorites", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = CommonActionCategories.OtherCategoryID)]
        protected virtual IEnumerable copyUserFavorites(PXAdapter adapter)
        {

            if (CopyUserFavoritesView.AskExt() == WebDialogResult.OK)
            {
                var Filter = CopyUserFavoritesView.Current;
                if(Filter != null && Filter.FromUserID != null)
                {
                    CopyFavoritesFromUser(Filter.FromUserID);
                    CopyUserFavoritesView.Current.FromUserID = null;                 
                }
            }
            return adapter.Get();
        }

        #endregion

    }
}
