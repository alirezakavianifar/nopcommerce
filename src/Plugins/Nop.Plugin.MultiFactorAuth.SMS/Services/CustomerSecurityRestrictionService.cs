using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.MultiFactorAuth.SMS.Domains;

namespace Nop.Plugin.MultiFactorAuth.SMS.Services;

public class CustomerSecurityRestrictionService : ICustomerSecurityRestrictionService
{
    protected readonly IRepository<CustomerSecurityRestriction> _repository;

    public CustomerSecurityRestrictionService(IRepository<CustomerSecurityRestriction> repository)
    {
        _repository = repository;
    }

    public virtual async Task<bool> IsIpAddressAllowedAsync(int customerId, string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return true;

        var restrictions = await GetSecurityRestrictionsByCustomerIdAsync(customerId);
        
        var ipRecord = restrictions.FirstOrDefault(r => string.IsNullOrEmpty(r.DeviceTokenHash) && !string.IsNullOrEmpty(r.AllowedIpAddresses));
        if (ipRecord == null)
            return true; 

        var allowedIps = ipRecord.AllowedIpAddresses
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(ip => ip.Trim())
            .ToList();

        if (!allowedIps.Any())
            return true;

        foreach (var allowed in allowedIps)
        {
            if (allowed.Equals(ipAddress, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (allowed.Contains('/'))
            {
                if (IsInSubnet(ipAddress, allowed))
                    return true;
            }
        }

        return false;
    }

    public virtual async Task<bool> IsDeviceTokenValidAsync(int customerId, string deviceToken)
    {
        if (string.IsNullOrEmpty(deviceToken))
            return false;

        var hash = HashToken(deviceToken);
        var restrictions = await GetSecurityRestrictionsByCustomerIdAsync(customerId);

        var deviceRecord = restrictions.FirstOrDefault(r => r.DeviceTokenHash == hash);
        if (deviceRecord == null || !deviceRecord.IsApproved)
            return false;

        deviceRecord.LastUsedUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(deviceRecord);

        return true;
    }

    public virtual async Task RegisterDeviceAsync(int customerId, string deviceToken, string deviceName, bool isApproved)
    {
        if (string.IsNullOrEmpty(deviceToken))
            return;

        var hash = HashToken(deviceToken);

        var restrictions = await GetSecurityRestrictionsByCustomerIdAsync(customerId);
        var existing = restrictions.FirstOrDefault(r => r.DeviceTokenHash == hash);

        if (existing != null)
        {
            existing.DeviceName = deviceName;
            existing.IsApproved = isApproved;
            existing.LastUsedUtc = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
        }
        else
        {
            var restriction = new CustomerSecurityRestriction
            {
                CustomerId = customerId,
                DeviceTokenHash = hash,
                DeviceName = deviceName,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
                LastUsedUtc = DateTime.UtcNow
            };
            await _repository.InsertAsync(restriction);
        }
    }

    public virtual async Task<IList<CustomerSecurityRestriction>> GetSecurityRestrictionsByCustomerIdAsync(int customerId)
    {
        return await _repository.Table
            .Where(r => r.CustomerId == customerId)
            .ToListAsync();
    }

    public virtual async Task<CustomerSecurityRestriction> GetSecurityRestrictionByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task DeleteSecurityRestrictionAsync(CustomerSecurityRestriction restriction)
    {
        ArgumentNullException.ThrowIfNull(restriction);
        await _repository.DeleteAsync(restriction);
    }

    public virtual async Task SaveSecurityRestrictionAsync(CustomerSecurityRestriction restriction)
    {
        ArgumentNullException.ThrowIfNull(restriction);
        if (restriction.Id > 0)
        {
            await _repository.UpdateAsync(restriction);
        }
        else
        {
            await _repository.InsertAsync(restriction);
        }
    }

    #region Utilities

    protected virtual string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private bool IsInSubnet(string ipAddress, string subnet)
    {
        try
        {
            var parts = subnet.Split('/');
            if (parts.Length != 2) return false;

            var subnetAddress = System.Net.IPAddress.Parse(parts[0]);
            int maskLength = int.Parse(parts[1]);

            var ipBytes = System.Net.IPAddress.Parse(ipAddress).GetAddressBytes();
            var subnetBytes = subnetAddress.GetAddressBytes();

            if (ipBytes.Length != subnetBytes.Length) return false;

            int bytesToCheck = maskLength / 8;
            for (int i = 0; i < bytesToCheck; i++)
            {
                if (ipBytes[i] != subnetBytes[i]) return false;
            }

            int bitsToCheck = maskLength % 8;
            if (bitsToCheck > 0)
            {
                int bitIndex = bytesToCheck;
                byte mask = (byte)(0xFF << (8 - bitsToCheck));
                if ((ipBytes[bitIndex] & mask) != (subnetBytes[bitIndex] & mask))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
