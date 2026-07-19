using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.MultiFactorAuth.SMS.Domains;

namespace Nop.Plugin.MultiFactorAuth.SMS.Services;

public interface ICustomerSecurityRestrictionService
{
    /// <summary>
    /// Checks if a given IP address is allowed for the customer based on their IP binding settings
    /// </summary>
    Task<bool> IsIpAddressAllowedAsync(int customerId, string ipAddress);

    /// <summary>
    /// Checks if a device token is registered and approved for the customer
    /// </summary>
    Task<bool> IsDeviceTokenValidAsync(int customerId, string deviceToken);

    /// <summary>
    /// Registers/binds a device for the customer
    /// </summary>
    Task RegisterDeviceAsync(int customerId, string deviceToken, string deviceName, bool isApproved);

    /// <summary>
    /// Gets all security restrictions and registered devices for a customer
    /// </summary>
    Task<IList<CustomerSecurityRestriction>> GetSecurityRestrictionsByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets a security restriction by ID
    /// </summary>
    Task<CustomerSecurityRestriction> GetSecurityRestrictionByIdAsync(int id);

    /// <summary>
    /// Deletes a security restriction record
    /// </summary>
    Task DeleteSecurityRestrictionAsync(CustomerSecurityRestriction restriction);

    /// <summary>
    /// Inserts or updates a security restriction record
    /// </summary>
    Task SaveSecurityRestrictionAsync(CustomerSecurityRestriction restriction);
}
