using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Helpers.AppSettings
{
    [ExcludeFromCodeCoverage]
    public static class DefaultUserAppSettings
    {
        [AllowNull]
        public static string UserName { get; set; }
        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        [AllowNull]
        public static string Email { get; set; }
        public static bool EmailConfirmed { get; set; }
        public static bool PhoneNumberConfirmed { get; set; }
        [AllowNull]
        public static string Password { get; set; }
        public static bool TwoFactorEnabled { get; set; } = true;
    }

    [ExcludeFromCodeCoverage]
    public static class AdminUserAppSettings
	{
		[AllowNull]
		public static string UserName { get; set; }
		public static string FirstName { get; set; }
		public static string LastName { get; set; }
		[AllowNull]
		public static string Email { get; set; }
		public static bool EmailConfirmed { get; set; }
		public static bool PhoneNumberConfirmed { get; set; }
		[AllowNull]
		public static string Password { get; set; }
        public static bool TwoFactorEnabled { get; set; } = true;
    }
  
}
