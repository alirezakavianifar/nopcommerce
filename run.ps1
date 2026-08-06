# run.ps1 - nopCommerce Dev Runner with Auto-Restart
# Handles the case where nopCommerce shuts itself down (e.g. after plugin install/uninstall)
# and automatically restarts it, just like IIS would in production.
# Usage: .\run.ps1
# Stop: Ctrl+C

param(
    [string]$Project = "src\Presentation\Nop.Web\Nop.Web.csproj",
    [int]$HttpPort = 59580,
    [int]$HttpsPort = 59579
)

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  nopCommerce Dev Runner (with auto-restart)" -ForegroundColor Cyan
Write-Host "  Press Ctrl+C to fully stop the application" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

function Stop-ProcessOnPort([int]$TargetPort) {
    $pids = @()
    try {
        $connections = Get-NetTCPConnection -LocalPort $TargetPort -ErrorAction SilentlyContinue
        if ($connections) {
            $pids += $connections.OwningProcess | Select-Object -Unique
        }
    } catch {}

    try {
        $netstat = netstat -ano
        foreach ($line in $netstat) {
            if ($line -match ":$TargetPort\s+.*\s+(\d+)\s*$") {
                $pids += [int]$Matches[1]
            }
        }
    } catch {}

    $pids = $pids | Select-Object -Unique | Where-Object { $_ -gt 0 -and $_ -ne $PID }
    foreach ($procId in $pids) {
        try {
            Stop-Process -Id $procId -Force -ErrorAction Stop
        } catch {
            try { taskkill /F /PID $procId /T | Out-Null } catch {}
        }
    }
}

function Stop-NopWebProcesses {
    # 1. Kill any process named 'Nop.Web' or 'Nop.Web.exe'
    Get-Process -Name "Nop.Web" -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID } | ForEach-Object {
        try { Stop-Process -Id $_.Id -Force -ErrorAction Stop } catch { try { taskkill /F /PID $_.Id /T | Out-Null } catch {} }
    }

    # 2. Kill dotnet process running Nop.Web.dll or nopCommerce
    try {
        $dotnetProcs = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe' OR Name = 'Nop.Web.exe'" -ErrorAction SilentlyContinue
        foreach ($p in $dotnetProcs) {
            if ($p.ProcessId -ne $PID -and $p.CommandLine -and ($p.CommandLine -like "*Nop.Web*" -or $p.CommandLine -like "*nopCommerce*")) {
                try { Stop-Process -Id $p.ProcessId -Force -ErrorAction Stop } catch { try { taskkill /F /PID $p.ProcessId /T | Out-Null } catch {} }
            }
        }
    } catch {}

    # 3. Forcibly clear both HTTP and HTTPS ports
    Stop-ProcessOnPort $HttpPort
    Stop-ProcessOnPort $HttpsPort
}

$restartCount = 0
$maxRestarts = 20

while ($restartCount -lt $maxRestarts) {
    Write-Host ""
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Starting nopCommerce... (attempt $($restartCount + 1))" -ForegroundColor Green
    
    # Forcibly stop any locked Nop.Web / dotnet processes and free ports before running app
    Stop-NopWebProcesses
    
    # Wait a moment for OS to release ports
    if ($restartCount -gt 0) {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Waiting 2 seconds for ports to clear..." -ForegroundColor DarkYellow
        Start-Sleep -Seconds 2
    }

    dotnet run --project $Project
    $exitCode = $LASTEXITCODE
    $restartCount++
    
    Write-Host ""
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] App exited (code: $exitCode)" -ForegroundColor Yellow
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Auto-restarting..." -ForegroundColor Yellow
}

Write-Host "Max restarts ($maxRestarts) reached. Stopping." -ForegroundColor Red
