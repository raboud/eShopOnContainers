using HMS.Identity.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Identity.API.Data
{
    public class UserSeed
    {
		public List<string> roles { get; set; }
		public List<ApplicationUser> users { get; set; }
		public List<UserRole> userRoles { get; set; }

		public class UserRole
		{
			public string Email { get; set; }
			public string Role { get; set; }
		}
	}
}
