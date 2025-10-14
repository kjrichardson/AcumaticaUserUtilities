using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.EP;
using PX.Objects.GL;
using PX.SM;
using PX.Web.UI.Frameset.Model.DAC;
using System;
using System.Collections;
using System.Collections.Generic;


namespace AcumaticaUserUtilities
{

    [PXLocalizable]
    public class Messages
    {
        public const string EmailRequiredToPreventDuplicates = "The email address must be in the current username to continue to prevent duplicates.";
        public const string RequireEmailAddressToConvertToLocal = "An email address is required to convert to local.";
        public const string UserNotFound = "User not found.";
    }
}