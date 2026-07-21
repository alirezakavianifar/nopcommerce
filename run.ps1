# run.ps1 - nopCommerce Dev Runner with Auto-Restart
# Handles the case where nopCommerce shuts itself down (e.g. after plugin install/uninstall)
# and automatically restarts it, just like IIS would in production.
# Usage: .\run.ps1
# Stop: Ctrl+C

param(
    [string]$Project = "src\Presentation\Nop.Web\Nop.Web.csproj"
)

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  nopCommerce Dev Runner (with auto-restart)" -ForegroundColor Cyan
Write-Host "  Press Ctrl+C to fully stop the application" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

$restartCount = 0
$maxRestarts = 20

while ($restartCount -lt $maxRestarts) {
    Write-Host ""
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Starting nopCommerce... (attempt $($restartCount + 1))" -ForegroundColor Green
    
    # Wait a moment for ports to be released
    if ($restartCount -gt 0) {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Waiting 3 seconds for ports to clear..." -ForegroundColor DarkYellow
        Start-Sleep -Seconds 3
    }
    
    # Forcibly stop any locked Nop.Web processes before running build & app
    Get-Process -Name "Nop.Web" -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID } | Stop-Process -Force -ErrorAction SilentlyContinue

    dotnet run --project $Project
    $exitCode = $LASTEXITCODE
    $restartCount++
    
    Write-Host ""
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] App exited (code: $exitCode)" -ForegroundColor Yellow
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Auto-restarting..." -ForegroundColor Yellow
}

Write-Host "Max restarts ($maxRestarts) reached. Stopping." -ForegroundColor Red
