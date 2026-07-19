namespace Nop.Plugin.MultiFactorAuth.SMS;

public static class SMSDefaults
{
    public const string SystemName = "MultiFactorAuth.SMS";
    public const string ConfigurationRouteName = "Plugin.MultiFactorAuth.SMS.Configure";
    public const string CacheKeyPrefix = "Nop.Plugin.MultiFactorAuth.SMS.";
    
    // Attribute names for customer generic attributes
    public const string SMS2FAEnabledAttribute = "SMS2FA.IsEnabled";
    public const string SMS2FAPhoneNumberAttribute = "SMS2FA.PhoneNumber";
    
    // Cache prefix
    public static string PrefixCacheKey => "Nop.plugin.multifactorauth.sms.";
}
