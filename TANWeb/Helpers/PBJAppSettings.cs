using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Helpers.AppSettings;
using TANWeb.Resources;

namespace TANWeb.Helpers
{
    [ExcludeFromCodeCoverage]
    public class TANAppSettings
    {
        public static void SetAppSettings(IConfiguration Configuration)
        {
            //DefaultUser appsettings.json details
            DefaultUserAppSettings.UserName = Configuration[TANResource.DefaultUserUserName];
            DefaultUserAppSettings.FirstName = Configuration[TANResource.DefaultUserFirstName];
            DefaultUserAppSettings.LastName = Configuration[TANResource.DefaultUserLastName];
            DefaultUserAppSettings.Email = Configuration[TANResource.DefaultUserEmail];
            DefaultUserAppSettings.EmailConfirmed = Convert.ToBoolean(Configuration[TANResource.DefaultUserConfirmedEmail]);
            DefaultUserAppSettings.PhoneNumberConfirmed = Convert.ToBoolean(Configuration[TANResource.DefaultUserPhoneNumberConfirmed]);
            DefaultUserAppSettings.Password = Configuration[TANResource.DefaultUserPassword];
            DefaultUserAppSettings.TwoFactorEnabled = Convert.ToBoolean(Configuration[TANResource.DefaultUserTwoFactorEnabled]);

            //SuperAdmin appsettings.json details
            SuperAdminAppSettings.UserName = Configuration[TANResource.SuperAdminUserName];
            SuperAdminAppSettings.FirstName = Configuration[TANResource.SuperAdminFirstName];
            SuperAdminAppSettings.LastName = Configuration[TANResource.SuperAdminLastName];
            SuperAdminAppSettings.Email = Configuration[TANResource.SuperAdminEmail];
            SuperAdminAppSettings.EmailConfirmed = Convert.ToBoolean(Configuration[TANResource.SuperAdminConfirmedEmail]);
            SuperAdminAppSettings.PhoneNumberConfirmed = Convert.ToBoolean(Configuration[TANResource.SuperAdminPhoneNumberConfirmed]);
            SuperAdminAppSettings.Password = Configuration[TANResource.SuperAdminPassword];
            SuperAdminAppSettings.TwoFactorEnabled = Convert.ToBoolean(Configuration[TANResource.SuperAdminTwoFactorEnabled]);

			//Admin appsettings.json details
			AdminUserAppSettings.UserName = Configuration[TANResource.AdminUserName];
			AdminUserAppSettings.FirstName = Configuration[TANResource.AdminFirstName];
			AdminUserAppSettings.LastName = Configuration[TANResource.AdminLastName];
			AdminUserAppSettings.Email = Configuration[TANResource.AdminEmail];
			AdminUserAppSettings.EmailConfirmed = Convert.ToBoolean(Configuration[TANResource.AdminConfirmedEmail]);
			AdminUserAppSettings.PhoneNumberConfirmed = Convert.ToBoolean(Configuration[TANResource.AdminConfirmedEmail]);
			AdminUserAppSettings.Password = Configuration[TANResource.AdminPassword];
			AdminUserAppSettings.TwoFactorEnabled = Convert.ToBoolean(Configuration[TANResource.AdminTwoFactorEnabled]);

			SupportedCulture.DefaultCulture = Configuration[TANResource.DefaultRequestCulture];
           
        }
    }

    [ExcludeFromCodeCoverage]
    public static class TANData
    {
        public static Dictionary<string, string> GetStateCodeList()
        {
            Dictionary<string, string> states = new Dictionary<string, string>();
            states.Add("AL", "Alabama");
            states.Add("AK", "Alaska");
            states.Add("AZ", "Arizona");
            states.Add("AR", "Arkansas");
            states.Add("CA", "California");
            states.Add("CO", "Colorado");
            states.Add("CT", "Connecticut");
            states.Add("DE", "Delaware");
            states.Add("DC", "District Of Columbia");
            states.Add("FL", "Florida");
            states.Add("GA", "Georgia");
            states.Add("HI", "Hawaii");
            states.Add("ID", "Idaho");
            states.Add("IL", "Illinois");
            states.Add("IN", "Indiana");
            states.Add("IA", "Iowa");
            states.Add("KS", "Kansas");
            states.Add("KY", "Kentucky");
            states.Add("LA", "Louisiana");
            states.Add("ME", "Maine");
            states.Add("MD", "Maryland");
            states.Add("MA", "Massachusetts");
            states.Add("MI", "Michigan");
            states.Add("MN", "Minnesota");
            states.Add("MS", "Mississippi");
            states.Add("MO", "Missouri");
            states.Add("MT", "Montana");
            states.Add("NE", "Nebraska");
            states.Add("NV", "Nevada");
            states.Add("NH", "New Hampshire");
            states.Add("NJ", "New Jersey");
            states.Add("NM", "New Mexico");
            states.Add("NY", "New York");
            states.Add("NC", "North Carolina");
            states.Add("ND", "North Dakota");
            states.Add("OH", "Ohio");
            states.Add("OK", "Oklahoma");
            states.Add("OR", "Oregon");
            states.Add("PA", "Pennsylvania");
            states.Add("RI", "Rhode Island");
            states.Add("SC", "South Carolina");
            states.Add("SD", "South Dakota");
            states.Add("TN", "Tennessee");
            states.Add("TX", "Texas");
            states.Add("UT", "Utah");
            states.Add("VT", "Vermont");
            states.Add("VA", "Virginia");
            states.Add("WA", "Washington");
            states.Add("WV", "West Virginia");
            states.Add("WI", "Wisconsin");
            states.Add("WY", "Wyoming");

            return states;
        }
    }
}
