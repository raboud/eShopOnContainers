using System.Collections.Generic;
using System.Threading.Tasks;
using HMS.Core.Models.Permissions;

namespace HMS.Core.Services.Permissions
{
    public interface IPermissionsService
    {
        Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission);
        Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions);
    }
}
