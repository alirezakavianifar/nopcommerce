import subprocess

ps_script = """
$connectionString = "Server=.\\SQLEXPRESS;Database=nopCommerce490;Integrated Security=True;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
$command = $connection.CreateCommand()
$command.CommandText = "UPDATE Customer SET TwoFactorAuthenticationProviderSystemName = NULL WHERE Email = 'admin@yourStore.com'"
$affected = $command.ExecuteNonQuery()
Write-Output "Disabled 2FA for admin. Updated rows: $affected"
$connection.Close()
"""

res = subprocess.run(["powershell", "-Command", ps_script], capture_output=True, text=True)
print("STDOUT:", res.stdout)
print("STDERR:", res.stderr)
