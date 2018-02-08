using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopOnContainers.Services.Identity.API.Extensions;
using Microsoft.eShopOnContainers.Services.Identity.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Identity.API.Data;

namespace Microsoft.eShopOnContainers.Services.Identity.API.Data
{


    public class ApplicationDbContextSeed
    {
		private RoleManager<IdentityRole> _roleManager;
		private UserManager<ApplicationUser> _userManager;
		private ILogger<ApplicationDbContextSeed> _logger;
		private IHostingEnvironment _env;
		private IOptions<AppSettings> _settings;
		private ApplicationDbContext _context;

		public ApplicationDbContextSeed(
			ApplicationDbContext context,
			RoleManager<IdentityRole> roleManager, 
			UserManager<ApplicationUser> userManager, 
			ILogger<ApplicationDbContextSeed> logger,
			IHostingEnvironment env,
			IOptions<AppSettings> settings
			)
		{
			this._context = context;
			this._roleManager = roleManager;
			this._userManager = userManager;
			this._logger = logger;
			this._env = env;
			this._settings = settings;
		}


		public async Task SeedAsync(int? retry = 0)
        {
            int retryForAvaiability = retry.Value;

            try
            {
				UserSeed seed = await this.GetSeedData();

				await this.CreateRoles(seed.roles);
				await this.CreateUsers(seed.users);
				await this.AddRolesToUsers(seed.userRoles);

				GetPreconfiguredImages();
            }
            catch (Exception ex)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;
                    
                    _logger.LogError(ex.Message,$"There is an error migrating data for ApplicationDbContext");

                    await SeedAsync(retryForAvaiability);
                }
            }
        }

		private async Task<UserSeed> GetSeedData()
		{
			string seedData = "{\"roles\":[\"admin\",\"user\"],\"users\":[{\"CardHolderName\":\"DemoUser\",\"CardNumber\":\"4012888888881881\",\"CardType\":1,\"City\":\"Redmond\",\"Country\":\"U.S.\",\"Email\":\"demouser@microsoft.com\",\"Expiration\":\"12/20\",\"LastName\":\"DemoLastName\",\"Name\":\"DemoUser\",\"PhoneNumber\":\"1234567890\",\"UserName\":\"demouser@microsoft.com\",\"ZipCode\":\"98052\",\"State\":\"WA\",\"Street\":\"15703 NE 61st Ct\",\"SecurityNumber\":\"535\",\"PasswordHash\":\"Pass@word1\"},{\"CardHolderName\":\"DemoAdmin\",\"CardNumber\":\"4012888888881881\",\"CardType\":1,\"City\":\"Redmond\",\"Country\":\"U.S.\",\"Email\":\"demoadmin@microsoft.com\",\"Expiration\":\"12/20\",\"LastName\":\"DemoLastName\",\"Name\":\"DemoAdmin\",\"PhoneNumber\":\"1234567890\",\"UserName\":\"demoadmin@microsoft.com\",\"ZipCode\":\"98052\",\"State\":\"WA\",\"Street\":\"15703 NE 61st Ct\",\"SecurityNumber\":\"535\",\"PasswordHash\":\"Pass@word1\"}],\"userRoles\":[{\"Email\":\"demoadmin@microsoft.com\",\"Role\":\"admin\"},{\"Email\":\"demouser@microsoft.com\",\"Role\":\"user\"}]}";

			if (_settings.Value.UseCustomizationData)
			{
				string jsonFile = Path.Combine(_env.ContentRootPath, "Setup", "UserSeed.json");

				if (File.Exists(jsonFile))
				{
					seedData = await File.ReadAllTextAsync(jsonFile);
				}
			}

			UserSeed seed = JsonConvert.DeserializeObject<UserSeed>(seedData);
			return seed;
		}


         void GetPreconfiguredImages()
        {
			if (_settings.Value.UseCustomizationData)
			{
				try
				{
					string imagesZipFile = Path.Combine(_env.ContentRootPath, "Setup", "images.zip");
					if (!File.Exists(imagesZipFile))
					{
						_logger.LogError($" zip file '{imagesZipFile}' does not exists.");
						return;
					}

					string imagePath = Path.Combine(_env.WebRootPath, "images");
					string[] imageFiles = Directory.GetFiles(imagePath).Select(file => Path.GetFileName(file)).ToArray();

					using (ZipArchive zip = ZipFile.Open(imagesZipFile, ZipArchiveMode.Read))
					{
						foreach (ZipArchiveEntry entry in zip.Entries)
						{
							if (imageFiles.Contains(entry.Name))
							{
								string destinationFilename = Path.Combine(imagePath, entry.Name);
								if (File.Exists(destinationFilename))
								{
									File.Delete(destinationFilename);
								}
								entry.ExtractToFile(destinationFilename);
							}
							else
							{
								_logger.LogWarning($"Skip file '{entry.Name}' in zipfile '{imagesZipFile}'");
							}
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Exception in method GetPreconfiguredImages WebMVC. Exception Message={ex.Message}");
				}
			}
        }

		private async Task CreateUsers(List<ApplicationUser> users)
		{
			//initializing custom roles 
			IdentityResult result;

			foreach (var user in users)
			{
				ApplicationUser appUser = await this._userManager.FindByEmailAsync(user.Email);
				if (appUser == null)
				{
					string password = user.PasswordHash;
					user.PasswordHash = null;
					//create the roles and seed them to the database: Question 1
					result = await this._userManager.CreateAsync(user, password);
				}
			}
		}

		private async Task CreateRoles(List<string> roles)
		{
			//initializing custom roles 
			IdentityResult roleResult;

			foreach (var roleName in roles)
			{
				var roleExist = await this._roleManager.RoleExistsAsync(roleName);
				if (!roleExist)
				{
					//create the roles and seed them to the database: Question 1
					roleResult = await this._roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}
		}

		private async Task AddRolesToUsers(List<UserSeed.UserRole> userRoles)
		{
			IdentityResult result;

			foreach (UserSeed.UserRole userRole in userRoles)
			{
				if (await _roleManager.RoleExistsAsync(userRole.Role))
				{
					ApplicationUser user = await _userManager.FindByEmailAsync(userRole.Email);
					if (user != null)
					{
						result = await _userManager.AddToRoleAsync(user, userRole.Role);
					}
				}
			}
		}
	}
}
