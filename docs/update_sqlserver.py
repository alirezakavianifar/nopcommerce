import subprocess

ps_script = """
$connectionString = "Server=.\\SQLEXPRESS;Database=nopCommerce490;Integrated Security=True;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
$command = $connection.CreateCommand()
$command.CommandText = "UPDATE Customer SET VendorId = 1 WHERE Email = 'admin@yourStore.com'"
$affected = $command.ExecuteNonQuery()
Write-Output "Updated rows: $affected"
$connection.Close()
"""

res = subprocess.run(["powershell", "-Command", ps_script], capture_output=True, text=True)
print("STDOUT:", res.stdout)
print("STDERR:", res.stderr)
