using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.MultiFactorAuth.SMS.Domains;

namespace Nop.Plugin.MultiFactorAuth.SMS.Services;

public interface ISMSService
{
    /// <summary>
    /// Generates a random OTP code and sends it via SMS to the specified phone number
    /// </summary>
    Task<string> GenerateAndSendCodeAsync(string userIdentifier, string phoneNumber);

    /// <summary>
    /// Validates the submitted OTP code
    /// </summary>
    Task<bool> ValidateCodeAsync(string userIdentifier, string submittedCode);

    /// <summary>
    /// Checks if SMS 2FA is forced/enabled for the given customer based on their roles and settings
    /// </summary>
    Task<bool> IsSMS2FAForcedOrEnabledForCustomerAsync(Customer customer);
}
