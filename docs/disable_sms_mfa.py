import subprocess

ps_script = """
$connectionString = "Server=.\\SQLEXPRESS;Database=nopCommerce490;Integrated Security=True;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
$command = $connection.CreateCommand()
$command.CommandText = "UPDATE Setting SET Value = 'False' WHERE Name LIKE 'smssettings%'"
$affected = $command.ExecuteNonQuery()
Write-Output "Disabled SMS MFA settings in Setting table. Updated rows: $affected"
$connection.Close()
"""

res = subprocess.run(["powershell", "-Command", ps_script], capture_output=True, text=True)
print("STDOUT:", res.stdout)
print("STDERR:", res.stderr)
