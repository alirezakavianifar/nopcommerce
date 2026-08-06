import subprocess

ps_script = """
$connectionString = "Server=.\\SQLEXPRESS;Database=nopCommerce490;Integrated Security=True;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

$resources = @{
    "plugins.multifactorauth.sms.provider" = "ارائه‌دهنده پیامک"
    "plugins.multifactorauth.sms.provider.hint" = "درگاه پیامکی خود را انتخاب کنید (Twilio، ملي پيامک، کاوه‌نگار، عمومی)."
    "plugins.multifactorauth.sms.apikey" = "کلید API / نام کاربری"
    "plugins.multifactorauth.sms.apikey.hint" = "کلید API یا نام کاربری سرویس پیامک را وارد کنید."
    "plugins.multifactorauth.sms.apisecret" = "رمز API / کلمه عبور"
    "plugins.multifactorauth.sms.apisecret.hint" = "رمز API یا کلمه عبور درگاه را وارد کنید (در صورت نیاز)."
    "plugins.multifactorauth.sms.sendernumber" = "شماره فرستنده / آدرس وب‌هوک"
    "plugins.multifactorauth.sms.sendernumber.hint" = "شماره فرستنده ثبت‌شده یا آدرس وب‌هوک را وارد کنید."
    "plugins.multifactorauth.sms.codelength" = "طول کد OTP"
    "plugins.multifactorauth.sms.codelifetimeminutes" = "مدت اعتبار کد OTP (دقیقه)"
    "plugins.multifactorauth.sms.force2faforadmins" = "اجباری کردن ورود دو مرحله‌ای پیامکی برای مدیران"
    "plugins.multifactorauth.sms.force2faforvendors" = "اجباری کردن ورود دو مرحله‌ای پیامکی برای فروشندگان"
    "plugins.multifactorauth.sms.enableipallowlist" = "فعال‌سازی محدودیت فهرست مجاز IP"
    "plugins.multifactorauth.sms.enabledevicebinding" = "فعال‌سازی اتصال دستگاه / مرورگر"
    "plugins.multifactorauth.sms.forcedevicebindingforadmins" = "اجباری کردن اتصال دستگاه برای مدیران"
    "plugins.multifactorauth.sms.forcedevicebindingforvendors" = "اجباری کردن اتصال دستگاه برای فروشندگان"
    
    "Plugins.MultiFactorAuth.SMS.Provider" = "ارائه‌دهنده پیامک"
    "Plugins.MultiFactorAuth.SMS.ApiKey" = "کلید API / نام کاربری"
    "Plugins.MultiFactorAuth.SMS.ApiSecret" = "رمز API / کلمه عبور"
    "Plugins.MultiFactorAuth.SMS.SenderNumber" = "شماره فرستنده / آدرس وب‌هوک"
    "Plugins.MultiFactorAuth.SMS.CodeLength" = "طول کد OTP"
    "Plugins.MultiFactorAuth.SMS.CodeLifetimeMinutes" = "مدت اعتبار کد OTP (دقیقه)"
    "Plugins.MultiFactorAuth.SMS.Force2FAForAdmins" = "اجباری کردن ورود دو مرحله‌ای پیامکی برای مدیران"
    "Plugins.MultiFactorAuth.SMS.Force2FAForVendors" = "اجباری کردن ورود دو مرحله‌ای پیامکی برای فروشندگان"
    "Plugins.MultiFactorAuth.SMS.EnableIpAllowlist" = "فعال‌سازی محدودیت فهرست مجاز IP"
    "Plugins.MultiFactorAuth.SMS.EnableDeviceBinding" = "فعال‌سازی اتصال دستگاه / مرورگر"
    "Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForAdmins" = "اجباری کردن اتصال دستگاه برای مدیران"
    "Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForVendors" = "اجباری کردن اتصال دستگاه برای فروشندگان"
}

$LanguageId = 2

foreach ($key in $resources.Keys) {
    $val = $resources[$key]
    
    # Check if exists
    $checkCmd = $connection.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(1) FROM LocaleStringResource WHERE LanguageId = @lang AND ResourceName = @key"
    $checkCmd.Parameters.AddWithValue("@lang", $LanguageId) | Out-Null
    $checkCmd.Parameters.AddWithValue("@key", $key) | Out-Null
    $count = $checkCmd.ExecuteScalar()
    
    if ($count -eq 0) {
        $insertCmd = $connection.CreateCommand()
        $insertCmd.CommandText = "INSERT INTO LocaleStringResource (LanguageId, ResourceName, ResourceValue) VALUES (@lang, @key, @val)"
        $insertCmd.Parameters.AddWithValue("@lang", $LanguageId) | Out-Null
        $insertCmd.Parameters.AddWithValue("@key", $key) | Out-Null
        $insertCmd.Parameters.AddWithValue("@val", $val) | Out-Null
        $insertCmd.ExecuteNonQuery() | Out-Null
        Write-Output "Inserted locale key: $key"
    } else {
        $updateCmd = $connection.CreateCommand()
        $updateCmd.CommandText = "UPDATE LocaleStringResource SET ResourceValue = @val WHERE LanguageId = @lang AND ResourceName = @key"
        $updateCmd.Parameters.AddWithValue("@lang", $LanguageId) | Out-Null
        $updateCmd.Parameters.AddWithValue("@key", $key) | Out-Null
        $updateCmd.Parameters.AddWithValue("@val", $val) | Out-Null
        $updateCmd.ExecuteNonQuery() | Out-Null
        Write-Output "Updated locale key: $key"
    }
}

$connection.Close()
Write-Output "Localization database update completed successfully!"
"""

res = subprocess.run(["powershell", "-Command", ps_script], capture_output=True, text=True)
print("STDOUT:", res.stdout)
print("STDERR:", res.stderr)
