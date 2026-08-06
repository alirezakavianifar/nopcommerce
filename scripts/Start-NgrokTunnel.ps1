<#
.SYNOPSIS
    Exposes the local nopCommerce app to the internet via ngrok tunnel.

.DESCRIPTION
    This script:
      - Checks if ngrok is installed (and offers to install it via winget/Chocolatey/direct download)
      - Optionally configures your ngrok authtoken
      - Starts an ngrok HTTPS tunnel on the specified local port
      - Retrieves and displays the public URL from the ngrok API
      - Copies the URL to clipboard for easy sharing
      - Shows a live status loop until you press Ctrl+C

.PARAMETER Port
    The local port your nopCommerce app is running on. Default: 59580

.PARAMETER AuthToken
    (Optional) Your ngrok authtoken. Only needed on first run or if not already saved.
    Get yours free at: https://dashboard.ngrok.com/get-started/your-authtoken

.PARAMETER SubDomain
    (Optional) Custom subdomain (requires ngrok paid plan).

.EXAMPLE
    .\Start-NgrokTunnel.ps1
    .\Start-NgrokTunnel.ps1 -Port 59580
    .\Start-NgrokTunnel.ps1 -Port 59580 -AuthToken "your_token_here"
#>

[CmdletBinding()]
param(
    [int]$Port = 59580,
    [string]$AuthToken = "",
    [string]$Domain = "ground-buggy-karaoke.ngrok-free.dev",
    [string]$SubDomain = "",
    [switch]$StartApp = $false,
    [switch]$ForceClearPorts = $false
)

# ---------------------------------------------
#  Helpers
# ---------------------------------------------
function Write-Banner {
    Clear-Host
    Write-Host ""
    Write-Host "  +--------------------------------------------------+" -ForegroundColor Cyan
    Write-Host "  |       nopCommerce  -  ngrok Tunnel Launcher      |" -ForegroundColor Cyan
    Write-Host "  +--------------------------------------------------+" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step([string]$Icon, [string]$Message, [string]$Color = "White") {
    Write-Host "  [$Icon] $Message" -ForegroundColor $Color
}

function Stop-NopWebProcesses {
    Write-Step "Lock Check" "Checking for locked Nop.Web / dotnet process instances..." "Yellow"
    
    # 1. Kill any process named 'Nop.Web' or 'Nop.Web.exe'
    $nopProcs = Get-Process -Name "Nop.Web" -ErrorAction SilentlyContinue
    foreach ($proc in $nopProcs) {
        if ($proc.Id -ne $PID) {
            try {
                Write-Step "Killing" "Forcibly stopping locked Nop.Web process (PID: $($proc.Id))..." "Cyan"
                Stop-Process -Id $proc.Id -Force -ErrorAction Stop
                Write-Step "OK" "Stopped Nop.Web process (PID: $($proc.Id))." "Green"
            } catch {
                try { taskkill /F /PID $proc.Id /T | Out-Null } catch {}
                Write-Step "WARN" "Failed to stop Nop.Web process with PID $($proc.Id): $_" "Yellow"
            }
        }
    }

    # 2. Kill dotnet process running Nop.Web.dll or nopCommerce
    try {
        $dotnetProcs = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe' OR Name = 'Nop.Web.exe'" -ErrorAction SilentlyContinue
        foreach ($p in $dotnetProcs) {
            if ($p.ProcessId -ne $PID -and $p.CommandLine -and ($p.CommandLine -like "*Nop.Web*" -or $p.CommandLine -like "*nopCommerce*")) {
                try {
                    Write-Step "Killing" "Forcibly stopping locked host process '$($p.Name)' (PID: $($p.ProcessId))..." "Cyan"
                    Stop-Process -Id $p.ProcessId -Force -ErrorAction Stop
                    Write-Step "OK" "Stopped process PID $($p.ProcessId)." "Green"
                } catch {
                    try { taskkill /F /PID $p.ProcessId /T | Out-Null } catch {}
                }
            }
        }
    } catch {
        # Fallback if WMI/CIM is restricted
    }

    # 3. Forcibly clear both HTTP (59580) and HTTPS (59579) ports
    Stop-ProcessOnPort -TargetPort 59580
    Stop-ProcessOnPort -TargetPort 59579

    Start-Sleep -Seconds 1
}

function Stop-ExistingNgrokProcesses {
    $ngrokProcs = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue
    if ($ngrokProcs) {
        foreach ($proc in $ngrokProcs) {
            if ($proc.Id -ne $PID) {
                try {
                    Write-Step "Killing" "Forcibly stopping existing ngrok process (PID: $($proc.Id))..." "Cyan"
                    Stop-Process -Id $proc.Id -Force -ErrorAction Stop
                    Write-Step "OK" "Stopped ngrok process (PID: $($proc.Id))." "Green"
                } catch {
                    try { taskkill /F /PID $proc.Id /T | Out-Null } catch {}
                }
            }
        }
        Start-Sleep -Seconds 1
    }
}

function Stop-ProcessOnPort([int]$TargetPort) {
    Write-Step "Port Check" "Checking if port $TargetPort is in use..." "Yellow"
    $pids = @()
    try {
        $connections = Get-NetTCPConnection -LocalPort $TargetPort -ErrorAction SilentlyContinue
        if ($connections) {
            $pids += $connections.OwningProcess | Select-Object -Unique
        }
    } catch {
        # Fallback to netstat if Get-NetTCPConnection is not available or fails
    }

    try {
        $netstat = netstat -ano
        foreach ($line in $netstat) {
            if ($line -match ":$TargetPort\s+.*\s+(\d+)\s*$") {
                $pids += [int]$Matches[1]
            }
        }
    } catch { }
    
    # Filter out PID 0 (System Idle Process) and the current PowerShell process PID
    $pids = $pids | Select-Object -Unique | Where-Object { $_ -gt 0 -and $_ -ne $PID }

    if ($pids) {
        foreach ($procId in $pids) {
            try {
                $proc = Get-Process -Id $procId -ErrorAction Stop
                Write-Step "Killing" "Forcibly stopping process '$($proc.Name)' (PID: $procId) occupying port $TargetPort..." "Cyan"
                Stop-Process -Id $procId -Force -ErrorAction Stop
                Write-Step "OK" "Stopped process '$($proc.Name)' (PID: $procId)." "Green"
            } catch {
                try {
                    taskkill /F /PID $procId /T | Out-Null
                    Write-Step "OK" "Forcibly killed PID $procId via taskkill." "Green"
                } catch {
                    Write-Step "WARN" "Failed to stop process with PID ${procId}: $_" "Yellow"
                }
            }
        }
        # Give the operating system a moment to release the port
        Start-Sleep -Seconds 1
    } else {
        Write-Step "OK" "Port $TargetPort is free." "Green"
    }
}

function Clear-RequiredPorts([int]$AppPort, [bool]$ClearAppPort = $false) {
    Write-Step "Force Clear" "Clearing open ports needed by ngrok and nopCommerce by force..." "Yellow"
    
    # Stop existing ngrok processes
    Stop-ExistingNgrokProcesses
    
    # Forcibly clear port 4040 (ngrok Web Inspector API)
    Stop-ProcessOnPort -TargetPort 4040
    
    # If requested or starting app, clear the application ports
    if ($ClearAppPort) {
        Stop-NopWebProcesses
        Stop-ProcessOnPort -TargetPort $AppPort
        if ($AppPort -gt 1) {
            Stop-ProcessOnPort -TargetPort ($AppPort - 1)
        }
    }
}

function Write-Divider {
    Write-Host "  -----------------------------------------------------" -ForegroundColor DarkGray
}

# ---------------------------------------------
#  Step 1 - Display banner & optionally force clear ports
# ---------------------------------------------
Write-Banner

if ($ForceClearPorts) {
    Clear-RequiredPorts -AppPort $Port -ClearAppPort $true
}

# ---------------------------------------------
#  Step 2 - Check if the local app is reachable
# ---------------------------------------------
Write-Step "Checking" "Checking local app on port $Port..." "Yellow"

$appRunning = $false
try {
    $null = Invoke-WebRequest -Uri "http://127.0.0.1:$Port" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
    Write-Step "OK" "nopCommerce is running on port $Port." "Green"
    $appRunning = $true
} catch {
    Write-Step "WARN" "Could not reach http://127.0.0.1:$Port  - is the app running?" "Yellow"
}

$appProc = $null
if (-not $appRunning) {
    $shouldStart = $StartApp
    if (-not $shouldStart) {
        Write-Host ""
        $response = Read-Host "  Would you like to start the app now? (Y/n)"
        if ($response -eq "" -or $response -match "^[Yy]") {
            $shouldStart = $true
        }
    }

    if ($shouldStart) {
        Stop-NopWebProcesses
        Stop-ProcessOnPort -TargetPort $Port
        Write-Step "Launch" "Starting the app process..." "Cyan"
        $repoRoot = (Resolve-Path "$PSScriptRoot\..").Path
        $runScript = Join-Path $repoRoot "run.ps1"
        if (Test-Path $runScript) {
            $appProc = Start-Process -FilePath "powershell.exe" -ArgumentList "-NoExit", "-File", "`"$runScript`"" -WorkingDirectory $repoRoot -PassThru
            Write-Step "OK" "App launched in a new window." "Green"
            
            # Wait for the app to start up and become reachable
            Write-Host "  Waiting for the app to initialize on port $Port..." -ForegroundColor Yellow
            $started = $false
            $maxAppWait = 90
            $appElapsed = 0
            while ($appElapsed -lt $maxAppWait) {
                Start-Sleep -Seconds 3
                $appElapsed += 3
                try {
                    $null = Invoke-WebRequest -Uri "http://127.0.0.1:$Port" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
                    $started = $true
                    break
                } catch {
                    # Not ready yet
                }
            }
            if ($started) {
                Write-Step "OK" "nopCommerce is now running on port $Port." "Green"
            } else {
                Write-Step "WARN" "App was launched but could not verify it running on port $Port after $maxAppWait seconds." "Yellow"
            }
        } else {
            Write-Step "ERROR" "Could not find run.ps1 at $runScript" "Red"
            $continue = Read-Host "  Continue anyway? (y/N)"
            if ($continue -notmatch "^[Yy]") {
                Write-Host "  Exiting." -ForegroundColor Red
                exit 1
            }
        }
    } else {
        $continue = Read-Host "  Continue anyway? (y/N)"
        if ($continue -notmatch "^[Yy]") {
            Write-Host "  Exiting." -ForegroundColor Red
            exit 1
        }
    }
}

Write-Divider

# ---------------------------------------------
#  Step 3 - Locate / install ngrok
# ---------------------------------------------
Write-Step "Searching" "Looking for ngrok..." "Yellow"

$ngrokCmd = Get-Command ngrok -ErrorAction SilentlyContinue

if (-not $ngrokCmd) {
    Write-Step "ERROR" "ngrok not found in PATH." "Red"
    Write-Host ""
    Write-Host "  Choose an installation method:" -ForegroundColor Cyan
    Write-Host "    [1] winget  (Windows Package Manager)"
    Write-Host "    [2] Chocolatey  (choco)"
    Write-Host "    [3] Download ZIP automatically (no package manager needed)"
    Write-Host "    [4] I'll install it myself - exit now"
    Write-Host ""
    $choice = Read-Host "  Enter choice (1-4)"

    switch ($choice) {
        "1" {
            Write-Step "Installing" "Installing ngrok via winget..." "Cyan"
            winget install --id Ngrok.Ngrok -e --silent
        }
        "2" {
            Write-Step "Installing" "Installing ngrok via Chocolatey..." "Cyan"
            choco install ngrok -y
        }
        "3" {
            Write-Step "Downloading" "Downloading ngrok ZIP..." "Cyan"
            $zipUrl  = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip"
            $zipPath = "$env:TEMP\ngrok.zip"
            $destDir = "$env:LOCALAPPDATA\ngrok"

            Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath -UseBasicParsing
            Expand-Archive -Path $zipPath -DestinationPath $destDir -Force
            Remove-Item $zipPath -Force

            # Add to PATH for this session
            $env:PATH += ";$destDir"
            Write-Step "OK" "ngrok extracted to $destDir and added to session PATH." "Green"
            Write-Step "TIP" "To make permanent, add $destDir to your system PATH." "Yellow"
        }
        default {
            Write-Host ""
            Write-Host "  Download ngrok from: https://ngrok.com/download" -ForegroundColor Cyan
            exit 0
        }
    }

    # Re-check after install
    $ngrokCmd = Get-Command ngrok -ErrorAction SilentlyContinue
    if (-not $ngrokCmd) {
        Write-Step "ERROR" "ngrok still not found. Please restart the shell after installation." "Red"
        exit 1
    }
}

$ngrokVersion = (ngrok version 2>&1)
Write-Step "OK" "ngrok found: $ngrokVersion" "Green"
Write-Divider

# ---------------------------------------------
#  Step 4 - Configure authtoken (if provided)
# ---------------------------------------------
if ($AuthToken -ne "") {
    Write-Step "Key" "Saving ngrok authtoken..." "Yellow"
    ngrok config add-authtoken $AuthToken | Out-Null
    Write-Step "OK" "Authtoken saved." "Green"
    Write-Divider
}

# ---------------------------------------------
#  Step 5 - Build ngrok command and start tunnel
# ---------------------------------------------
Stop-ExistingNgrokProcesses
Stop-ProcessOnPort -TargetPort 4040
Write-Step "Launch" "Starting ngrok tunnel -> http://127.0.0.1:$Port" "Cyan"
Write-Host ""

$ngrokArgs = @("http", "http://127.0.0.1:$Port", "--log=stdout")

$targetDomain = $Domain
if ($SubDomain -ne "") {
    $targetDomain = $SubDomain
}

if ($targetDomain -ne "") {
    if (-not ($targetDomain -like "http*")) {
        $targetDomain = "https://$targetDomain"
    }
    $ngrokArgs += "--url=$targetDomain"
}

# Start ngrok as a background process so we can query its API
$ngrokProc = Start-Process -FilePath "ngrok" -ArgumentList $ngrokArgs -PassThru -NoNewWindow

# Wait for ngrok API to become available
$publicUrl  = $null
$maxWaitSec = 20
$elapsed    = 0

Write-Host "  Waiting for tunnel to be established..." -ForegroundColor Yellow

while ($elapsed -lt $maxWaitSec) {
    Start-Sleep -Seconds 1
    $elapsed++

    try {
        $tunnels     = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -ErrorAction Stop
        $httpsTunnel = $tunnels.tunnels | Where-Object { $_.proto -eq "https" } | Select-Object -First 1

        if ($httpsTunnel) {
            $publicUrl = $httpsTunnel.public_url
            break
        }
    } catch {
        # API not ready yet - keep waiting
    }
}

Write-Host ""

if (-not $publicUrl) {
    Write-Step "ERROR" "Could not retrieve public URL from ngrok API." "Red"
    Write-Host ""
    Write-Host "  Possible causes:" -ForegroundColor Yellow
    Write-Host "   - No authtoken configured (free plan requires it)" -ForegroundColor Yellow
    Write-Host "   - ngrok rate-limited or network issue" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Get your free authtoken at: https://dashboard.ngrok.com/get-started/your-authtoken" -ForegroundColor Cyan
    Write-Host "  Then re-run:  .\Start-NgrokTunnel.ps1 -AuthToken ""your_token_here""" -ForegroundColor Cyan
    Write-Host ""
    if ($ngrokProc) { Stop-Process -Id $ngrokProc.Id -Force -ErrorAction SilentlyContinue }
    exit 1
}

# ---------------------------------------------
#  Step 6 - Display results
# ---------------------------------------------
Write-Banner

Write-Host "  Tunnel is LIVE!" -ForegroundColor Green
Write-Host ""
Write-Host "  +---------------------------------------------------------+" -ForegroundColor Green
Write-Host "  |  Public URL:   $publicUrl" -ForegroundColor White
Write-Host "  |  Local URL:    http://127.0.0.1:$Port" -ForegroundColor White
Write-Host "  |  Inspector:    http://localhost:4040" -ForegroundColor White
Write-Host "  |  Admin Email:  admin@yourStore.com" -ForegroundColor White
Write-Host "  |  Admin Pass:   admin" -ForegroundColor White
Write-Host "  +---------------------------------------------------------+" -ForegroundColor Green
Write-Host ""

# Copy URL to clipboard
try {
    Set-Clipboard -Value $publicUrl
    Write-Step "Copied" "Public URL copied to clipboard!" "Cyan"
} catch {
    Write-Step "TIP" "Copy this URL manually and share with your client." "Yellow"
}

Write-Host ""
Write-Host "  Share this URL with your client: " -ForegroundColor Yellow -NoNewline
Write-Host $publicUrl -ForegroundColor White
Write-Host ""
Write-Divider
Write-Host ""
Write-Host "  INFO: The ngrok Web Inspector is available at: http://localhost:4040" -ForegroundColor DarkGray
Write-Host "  INFO: Tunnel will stay active until you press Ctrl+C or close this window." -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Press Ctrl+C to stop the tunnel." -ForegroundColor Red
Write-Host ""

# ---------------------------------------------
#  Step 7 - Keep alive + refresh URL display
# ---------------------------------------------
try {
    while ($true) {
        Start-Sleep -Seconds 30

        # Confirm tunnel is still up
        try {
            $tunnels     = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -ErrorAction Stop
            $httpsTunnel = $tunnels.tunnels | Where-Object { $_.proto -eq "https" } | Select-Object -First 1

            if (-not $httpsTunnel) {
                Write-Host ""
                Write-Step "WARN" "Tunnel appears to have dropped. Exiting." "Red"
                break
            }
        } catch {
            Write-Step "WARN" "Lost connection to ngrok API. The tunnel may have closed." "Yellow"
            break
        }

        # Heartbeat
        $timestamp = Get-Date -Format "HH:mm:ss"
        Write-Host "  [$timestamp] Tunnel alive -> $publicUrl" -ForegroundColor DarkGreen
    }
} finally {
    Write-Host ""
    Write-Step "Stopping" "Stopping ngrok tunnel..." "Red"
    if ($ngrokProc) { Stop-Process -Id $ngrokProc.Id -Force -ErrorAction SilentlyContinue }

    if ($appProc) {
        Write-Step "Stopping" "Stopping app process..." "Red"
        Stop-Process -Id $appProc.Id -Force -ErrorAction SilentlyContinue
    }

    # Kill any remaining ngrok process
    Get-Process -Name "ngrok" -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Step "OK" "Tunnel stopped. Goodbye!" "Green"
    Write-Host ""
}
