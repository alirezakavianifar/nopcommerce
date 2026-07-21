using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.MultiFactorAuth.SMS.Migrations;

[NopMigration("2026/07/21 00:00:00", "SMS MultiFactorAuth localization update for EN and FA", MigrationProcessType.Update)]
public class LocalizationMigration : MigrationBase
{
    public override void Down()
    {
    }

    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        var languages = languageService.GetAllLanguages(true);
        var enLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("en", System.StringComparison.OrdinalIgnoreCase));
        var faLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("fa", System.StringComparison.OrdinalIgnoreCase));

        var enResources = new Dictionary<string, string>
        {
            ["Plugins.MultiFactorAuth.SMS.Provider"] = "SMS Provider",
            ["Plugins.MultiFactorAuth.SMS.Provider.Hint"] = "Choose your SMS gateway provider (Twilio, Melipayamak, Kavenegar, Generic).",
            ["Plugins.MultiFactorAuth.SMS.ApiKey"] = "API Key / Username",
            ["Plugins.MultiFactorAuth.SMS.ApiKey.Hint"] = "Provide API key or username for the SMS service.",
            ["Plugins.MultiFactorAuth.SMS.ApiSecret"] = "API Secret / Password",
            ["Plugins.MultiFactorAuth.SMS.ApiSecret.Hint"] = "Provide API secret or password if required by the gateway.",
            ["Plugins.MultiFactorAuth.SMS.SenderNumber"] = "Sender Number / Webhook URL",
            ["Plugins.MultiFactorAuth.SMS.SenderNumber.Hint"] = "Provide the registered sender number or URL endpoint for generic webhook.",
            ["Plugins.MultiFactorAuth.SMS.CodeLength"] = "OTP Code Length",
            ["Plugins.MultiFactorAuth.SMS.CodeLifetimeMinutes"] = "OTP Lifetime (minutes)",
            ["Plugins.MultiFactorAuth.SMS.Force2FAForAdmins"] = "Force SMS 2FA for Administrators",
            ["Plugins.MultiFactorAuth.SMS.Force2FAForVendors"] = "Force SMS 2FA for Vendors",
            ["Plugins.MultiFactorAuth.SMS.EnableIpAllowlist"] = "Enable IP Allowlist Restrictions",
            ["Plugins.MultiFactorAuth.SMS.EnableDeviceBinding"] = "Enable Device / Browser Binding",
            ["Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForAdmins"] = "Force Device Binding for Admins",
            ["Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForVendors"] = "Force Device Binding for Vendors",

            ["Plugins.MultiFactorAuth.SMS.Customer.PhoneNumber"] = "Phone Number",
            ["Plugins.MultiFactorAuth.SMS.Customer.PhoneNumber.Required"] = "Please enter a valid phone number.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationCode"] = "SMS Verification Code",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationCode.Required"] = "Please enter the verification code sent to your phone.",
            ["Plugins.MultiFactorAuth.SMS.Customer.SendCode"] = "Send OTP Code",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerifyAndEnable"] = "Verify and Enable SMS 2FA",
            ["Plugins.MultiFactorAuth.SMS.Customer.SMSDisabled"] = "SMS 2FA is currently disabled.",
            ["Plugins.MultiFactorAuth.SMS.Customer.SMSEnabled"] = "SMS 2FA is active for your account.",
            ["Plugins.MultiFactorAuth.SMS.Customer.DisableButton"] = "Disable SMS 2FA",
            ["Plugins.MultiFactorAuth.SMS.Customer.Instruction"] = "Configure SMS-based 2FA to secure your account. Enter your phone number below to receive a verification OTP code.",
            ["Plugins.MultiFactorAuth.SMS.Customer.InstructionVerification"] = "An OTP code has been sent to your registered phone number. Enter the code below to complete verification.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationFailed"] = "Invalid verification code or code has expired.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationSuccess"] = "SMS Two-Factor Authentication configured successfully.",
            ["Plugins.MultiFactorAuth.SMS.Customer.CodeSent"] = "Verification code has been sent to your phone.",
            ["Plugins.MultiFactorAuth.SMS.Customer.DeviceVerificationChallenge"] = "Device Verification Challenge",
            ["Plugins.MultiFactorAuth.SMS.Customer.DeviceVerificationInstruction"] = "This device or browser is not recognized. For security purposes, a verification code must be sent to your registered phone number.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerifyAndRegisterDevice"] = "Verify and Register Device",
            ["Plugins.MultiFactorAuth.SMS.Customer.Confirm"] = "Confirm",
            ["Plugins.MultiFactorAuth.SMS.Description"] = "Enables SMS-based Two-Factor authentication by sending an OTP code via a configured gateway."
        };

        var faResources = new Dictionary<string, string>
        {
            ["Plugins.MultiFactorAuth.SMS.Provider"] = "ارائه‌دهنده پیامک",
            ["Plugins.MultiFactorAuth.SMS.Provider.Hint"] = "درگاه پیامکی خود را انتخاب کنید (Twilio، ملي پيامک، کاوه‌نگار، عمومی).",
            ["Plugins.MultiFactorAuth.SMS.ApiKey"] = "کلید API / نام کاربری",
            ["Plugins.MultiFactorAuth.SMS.ApiKey.Hint"] = "کلید API یا نام کاربری سرویس پیامک را وارد کنید.",
            ["Plugins.MultiFactorAuth.SMS.ApiSecret"] = "رمز API / کلمه عبور",
            ["Plugins.MultiFactorAuth.SMS.ApiSecret.Hint"] = "رمز API یا کلمه عبور درگاه را وارد کنید (در صورت نیاز).",
            ["Plugins.MultiFactorAuth.SMS.SenderNumber"] = "شماره فرستنده / آدرس وب‌هوک",
            ["Plugins.MultiFactorAuth.SMS.SenderNumber.Hint"] = "شماره فرستنده ثبت‌شده یا آدرس وب‌هوک را وارد کنید.",
            ["Plugins.MultiFactorAuth.SMS.CodeLength"] = "طول کد OTP",
            ["Plugins.MultiFactorAuth.SMS.CodeLifetimeMinutes"] = "مدت اعتبار کد OTP (دقیقه)",
            ["Plugins.MultiFactorAuth.SMS.Force2FAForAdmins"] = "اجباری کردن ورود دو مرحله‌ای پیامکی برای مدیران",
            ["Plugins.MultiFactorAuth.SMS.Force2FAForVendors"] = "اجباری کردن ورود دو مرحله‌ای پیامکی برای فروشندگان",
            ["Plugins.MultiFactorAuth.SMS.EnableIpAllowlist"] = "فعال‌سازی محدودیت فهرست مجاز IP",
            ["Plugins.MultiFactorAuth.SMS.EnableDeviceBinding"] = "فعال‌سازی اتصال دستگاه / مرورگر",
            ["Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForAdmins"] = "اجباری کردن اتصال دستگاه برای مدیران",
            ["Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForVendors"] = "اجباری کردن اتصال دستگاه برای فروشندگان",

            ["Plugins.MultiFactorAuth.SMS.Customer.PhoneNumber"] = "شماره تلفن همراه",
            ["Plugins.MultiFactorAuth.SMS.Customer.PhoneNumber.Required"] = "لطفاً یک شماره تلفن همراه معتبر وارد کنید.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationCode"] = "کد تأیید پیامکی",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationCode.Required"] = "لطفاً کد تأیید ارسال‌شده به تلفن همراه خود را وارد کنید.",
            ["Plugins.MultiFactorAuth.SMS.Customer.SendCode"] = "ارسال کد OTP",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerifyAndEnable"] = "تأیید و فعال‌سازی ورود دو مرحله‌ای پیامکی",
            ["Plugins.MultiFactorAuth.SMS.Customer.SMSDisabled"] = "ورود دو مرحله‌ای پیامکی در حال حاضر غیرفعال است.",
            ["Plugins.MultiFactorAuth.SMS.Customer.SMSEnabled"] = "ورود دو مرحله‌ای پیامکی برای حساب شما فعال است.",
            ["Plugins.MultiFactorAuth.SMS.Customer.DisableButton"] = "غیرفعال‌سازی ورود دو مرحله‌ای پیامکی",
            ["Plugins.MultiFactorAuth.SMS.Customer.Instruction"] = "جهت ایمن‌سازی حساب خود، ورود دو مرحله‌ای پیامکی را تنظیم کنید. شماره تلفن همراه خود را در زیر وارد کنید تا کد OTP دریافت شود.",
            ["Plugins.MultiFactorAuth.SMS.Customer.InstructionVerification"] = "یک کد OTP به شماره تلفن همراه ثبت‌شده شما ارسال شده است. کد را در زیر وارد کنید تا تأیید تکمیل شود.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationFailed"] = "کد تأیید نامعتبر است یا انقضای آن به پایان رسیده است.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationSuccess"] = "ورود دو مرحله‌ای پیامکی با موفقیت تنظیم شد.",
            ["Plugins.MultiFactorAuth.SMS.Customer.CodeSent"] = "کد تأیید به شماره تلفن همراه شما ارسال شد.",
            ["Plugins.MultiFactorAuth.SMS.Customer.DeviceVerificationChallenge"] = "احراز هویت دستگاه",
            ["Plugins.MultiFactorAuth.SMS.Customer.DeviceVerificationInstruction"] = "این دستگاه یا مرورگر شناسایی نشده است. جهت حفظ امنیت، کد تأیید باید به شماره تلفن همراه ثبت‌شده شما ارسال شود.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerifyAndRegisterDevice"] = "تأیید و ثبت دستگاه",
            ["Plugins.MultiFactorAuth.SMS.Customer.Confirm"] = "تأیید",
            ["Plugins.MultiFactorAuth.SMS.Description"] = "امکان ورود دو مرحله‌ای بر پایه پیامک با ارسال کد OTP از طریق درگاه پیکربندی‌شده."
        };

        if (enLang != null)
            localizationService.AddOrUpdateLocaleResource(enResources, enLang.Id);

        if (faLang != null)
            localizationService.AddOrUpdateLocaleResource(faResources, faLang.Id);
    }
}
