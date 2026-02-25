# ============================================
# BLE Tracking Backend - Build & Install Script
# PowerShell script for managing services
# ============================================

[CmdletBinding()]
param(
    [switch]$Publish,       # Publish service(s)
    [switch]$Install,       # Install service(s) as Windows Service
    [switch]$Stop,          # Stop and delete service(s)
    [switch]$Status,        # Check service status
    [switch]$List,          # List all available services
    [switch]$Restart,       # Restart service(s)
    [string]$ServiceName = "",  # Service name (optional - empty = all, "?" = interactive)
    [switch]$Help,          # Show help
    [int]$ParallelCount = 4 # Number of parallel publish jobs (default: 4)
)

# ============================================
# Configuration
# ============================================
$Script:ServicesPath = Join-Path $PSScriptRoot "Services.API"
$Script:PublishPath = Join-Path $PSScriptRoot "publish"
$Script:ServicePrefix = "BleTracking_"
$Script:ParallelCount = $ParallelCount

# ============================================
# Helper Functions
# ============================================

function Write-ColorOutput {
    <#
    .SYNOPSIS
    Write colored output to console
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message,
        [ValidateSet("Black", "DarkBlue", "DarkGreen", "DarkCyan", "DarkRed", "DarkMagenta", "DarkYellow", "Gray", "DarkGray", "Blue", "Green", "Cyan", "Red", "Magenta", "Yellow", "White")]
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Get-AllServices {
    <#
    .SYNOPSIS
    Get all available services from Services.API folder
    #>
    $projects = Get-ChildItem -Path $Script:ServicesPath -Filter "*.csproj" -Recurse
    $services = @()

    foreach ($project in $projects) {
        $serviceName = $project.Directory.Name
        $services += [PSCustomObject]@{
            Name = $serviceName
            ProjectPath = $project.FullName
            DisplayName = $serviceName
        }
    }

    return $services | Sort-Object Name
}

function Show-ServiceMenu {
    <#
    .SYNOPSIS
    Show interactive menu for service selection
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$Title,
        [Parameter(Mandatory=$false)]
        [string]$Prompt = "Pilih service:"
    )

    $services = Get-AllServices

    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "$Title" -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host ""

    $index = 1
    foreach ($service in $services) {
        Write-Host "[$index] " -NoNewline -ForegroundColor Cyan
        Write-Host "$($service.Name)"
        $index++
    }

    Write-Host "[0] " -NoNewline -ForegroundColor Red
    Write-Host "Batal"
    Write-Host ""
    $selection = Read-Host "$Prompt"

    if ($selection -eq "0" -or [string]::IsNullOrWhiteSpace($selection)) {
        return $null
    }

    $selectedIndex = [int]$selection - 1
    if ($selectedIndex -ge 0 -and $selectedIndex -lt $services.Count) {
        return $services[$selectedIndex]
    }

    Write-Host "Pilihan tidak valid!" -ForegroundColor Red
    return $null
}

function Publish-ServiceInternal {
    <#
    .SYNOPSIS
    Publish a single service
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$SvcName,
        [Parameter(Mandatory=$true)]
        [string]$ProjectPath
    )

    # Output directly to publish/ folder (all services in one folder, like script.targets)
    $outputDir = $Script:PublishPath

    Write-Host ""
    Write-Host "----------------------------------------" -ForegroundColor DarkGray
    Write-Host "Publishing: $SvcName" -ForegroundColor Green
    Write-Host "----------------------------------------" -ForegroundColor DarkGray

    try {
        dotnet publish $ProjectPath --configuration Release --runtime win-x64 --self-contained true --output $outputDir /p:DeleteExistingFiles=true /p:PublishReadyToRun=true

        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK $SvcName published successfully" -ForegroundColor Green
            return $true
        } else {
            Write-Host "X Failed to publish $SvcName (exit code: $LASTEXITCODE)" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "X Error publishing $SvcName`: $_" -ForegroundColor Red
        return $false
    }
}

function Install-ServiceInternal {
    <#
    .SYNOPSIS
    Install a single service as Windows Service
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$SvcName
    )

    # Exe is in publish/ folder directly (publish/SvcName.exe)
    $exePath = Join-Path $Script:PublishPath "$SvcName.exe"
    $winServiceName = "$Script:ServicePrefix$SvcName"
    $displayName = "BLE Tracking - $SvcName"

    Write-Host ""
    Write-Host "----------------------------------------" -ForegroundColor DarkGray
    Write-Host "Installing: $SvcName" -ForegroundColor Cyan
    Write-Host "----------------------------------------" -ForegroundColor DarkGray

    # Check if exe exists
    if (-not (Test-Path $exePath)) {
        Write-Host "X Executable not found: $exePath" -ForegroundColor Red
        Write-Host "  Run '.\build.ps1 -Publish -ServiceName $SvcName' first" -ForegroundColor Yellow
        return $false
    }

    # Check if service exists and remove it
    $existingService = Get-Service -Name $winServiceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Host "  Service '$winServiceName' already exists. Removing..." -ForegroundColor Yellow
        try {
            if ($existingService.Status -eq "Running") {
                Stop-Service -Name $winServiceName -Force
                Write-Host "  Stopped" -ForegroundColor Yellow
            }
            # Remove service using sc.exe
            $result = & sc.exe delete $winServiceName 2>&1
            Start-Sleep -Seconds 2
            Write-Host "  Removed existing service" -ForegroundColor Yellow
        } catch {
            Write-Host "  Warning: Could not remove existing service: $_" -ForegroundColor Yellow
        }
    }

    # Create new service
    Write-Host "  Creating service: $winServiceName" -ForegroundColor Cyan

    try {
        $binPath = "`"$exePath`""
        $result = & sc.exe create $winServiceName binPath= $binPath DisplayName= "$displayName" start= auto 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK Service created: $winServiceName" -ForegroundColor Green

            # Start the service
            Write-Host "  Starting service..." -ForegroundColor Cyan
            Start-Service -Name $winServiceName -ErrorAction Stop

            $service = Get-Service -Name $winServiceName
            if ($service.Status -eq "Running") {
                Write-Host "OK Service started: $winServiceName" -ForegroundColor Green
                return $true
            } else {
                Write-Host "! Service created but not running. Status: $($service.Status)" -ForegroundColor Yellow
                return $true
            }
        } else {
            Write-Host "X Failed to create service: $result" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "X Error installing service: $_" -ForegroundColor Red
        return $false
    }
}

function Stop-ServiceInternal {
    <#
    .SYNOPSIS
    Stop and delete a Windows Service
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$SvcName
    )

    $winServiceName = "$Script:ServicePrefix$SvcName"

    Write-Host ""
    Write-Host "----------------------------------------" -ForegroundColor DarkGray
    Write-Host "Stopping: $SvcName" -ForegroundColor Yellow
    Write-Host "----------------------------------------" -ForegroundColor DarkGray

    try {
        $service = Get-Service -Name $winServiceName -ErrorAction SilentlyContinue

        if (-not $service) {
            Write-Host "  Service '$winServiceName' not found" -ForegroundColor Yellow
            return $false
        }

        # Stop service if running
        if ($service.Status -eq "Running") {
            Write-Host "  Stopping service..." -ForegroundColor Yellow
            Stop-Service -Name $winServiceName -Force
            Write-Host "  Stopped" -ForegroundColor Yellow
        }

        # Delete service
        Write-Host "  Deleting service..." -ForegroundColor Yellow
        $result = & sc.exe delete $winServiceName 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK Service deleted: $winServiceName" -ForegroundColor Green

            # Kill any lingering process
            $exeName = "$SvcName.exe"
            $process = Get-Process -Name $SvcName -ErrorAction SilentlyContinue
            if ($process) {
                Stop-Process -Name $SvcName -Force -ErrorAction SilentlyContinue
                Write-Host "  Killed lingering process" -ForegroundColor Yellow
            }

            return $true
        } else {
            Write-Host "X Failed to delete service: $result" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "X Error stopping service: $_" -ForegroundColor Red
        return $false
    }
}

function Get-ServiceStatusInternal {
    <#
    .SYNOPSIS
    Get status of all BLE Tracking services
    #>
    param(
        [Parameter(Mandatory=$false)]
        [string]$SvcName
    )

    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "BLE Tracking Services Status" -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host ""

    if ([string]::IsNullOrWhiteSpace($SvcName)) {
        # Get all BLE Tracking services
        $services = Get-Service | Where-Object { $_.Name -like "$Script:ServicePrefix*" }

        if ($services.Count -eq 0) {
            Write-Host "No BLE Tracking services found." -ForegroundColor Yellow
            return
        }

        foreach ($service in $services) {
            $statusColor = switch ($service.Status) {
                "Running" { "Green" }
                "Stopped" { "Red" }
                default { "Yellow" }
            }

            Write-Host "[$("$($service.Status)".PadRight(10))] " -NoNewline -ForegroundColor $statusColor
            Write-Host "$($service.Name)" -NoNewline
            Write-Host " - $($service.DisplayName)"
        }
    } else {
        # Get specific service
        $winServiceName = "$Script:ServicePrefix$SvcName"
        $service = Get-Service -Name $winServiceName -ErrorAction SilentlyContinue

        if ($service) {
            $statusColor = switch ($service.Status) {
                "Running" { "Green" }
                "Stopped" { "Red" }
                default { "Yellow" }
            }

            Write-Host "[$("$($service.Status)".PadRight(10))] " -NoNewline -ForegroundColor $statusColor
            Write-Host "$($service.Name)" -NoNewline
            Write-Host " - $($service.DisplayName)"

            Write-Host ""
            Write-Host "Start Type: $($service.StartType)" -ForegroundColor Gray
        } else {
            Write-Host "Service '$winServiceName' not found." -ForegroundColor Red
        }
    }

    Write-Host ""
}

function Restart-ServiceInternal {
    <#
    .SYNOPSIS
    Restart a Windows Service
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$SvcName
    )

    $winServiceName = "$Script:ServicePrefix$SvcName"

    Write-Host ""
    Write-Host "----------------------------------------" -ForegroundColor DarkGray
    Write-Host "Restarting: $SvcName" -ForegroundColor Magenta
    Write-Host "----------------------------------------" -ForegroundColor DarkGray

    try {
        $service = Get-Service -Name $winServiceName -ErrorAction SilentlyContinue

        if (-not $service) {
            Write-Host "X Service '$winServiceName' not found" -ForegroundColor Red
            return $false
        }

        Write-Host "  Stopping service..." -ForegroundColor Yellow
        Stop-Service -Name $winServiceName -Force
        Start-Sleep -Seconds 1

        Write-Host "  Starting service..." -ForegroundColor Green
        Start-Service -Name $winServiceName

        $service = Get-Service -Name $winServiceName
        if ($service.Status -eq "Running") {
            Write-Host "OK Service restarted: $winServiceName" -ForegroundColor Green
            return $true
        } else {
            Write-Host "! Service restarted but not running. Status: $($service.Status)" -ForegroundColor Yellow
            return $true
        }
    } catch {
        Write-Host "X Error restarting service: $_" -ForegroundColor Red
        return $false
    }
}

# ============================================
# Main Operations
# ============================================

function Publish-AllServices {
    <#
    .SYNOPSIS
    Publish all services (with parallel processing)
    #>
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "Publishing All Services (Parallel: $Script:ParallelCount jobs)" -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan

    $services = Get-AllServices
    $successCount = 0
    $failCount = 0
    $results = [System.Collections.Concurrent.ConcurrentBag[object]]::new()

    # Create publish directory if not exists
    if (-not (Test-Path $Script:PublishPath)) {
        New-Item -ItemType Directory -Path $Script:PublishPath -Force | Out-Null
    }

    # Use parallel processing if PowerShell 7+
    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $services | ForEach-Object -Parallel {
            $svcName = $_.Name
            $projPath = $_.ProjectPath
            $publishPath = $using:Script:PublishPath

            $outputDir = $publishPath

            Write-Host "[$svcName] Publishing..." -ForegroundColor DarkGray

            $result = dotnet publish $projPath --configuration Release --runtime win-x64 --self-contained true --output $outputDir /p:DeleteExistingFiles=true /p:PublishReadyToRun=true 2>&1

            if ($LASTEXITCODE -eq 0) {
                Write-Host "[$svcName] OK" -ForegroundColor Green
                ([System.Collections.Concurrent.ConcurrentBag[object]]$using:results).Add(@{ Name = $svcName; Success = $true })
            } else {
                Write-Host "[$svcName] FAILED (exit: $LASTEXITCODE)" -ForegroundColor Red
                ([System.Collections.Concurrent.ConcurrentBag[object]]$using:results).Add(@{ Name = $svcName; Success = $false })
            }
        } -ThrottleLimit $Script:ParallelCount

        $successCount = ($results | Where-Object { $_.Success }).Count
        $failCount = ($results | Where-Object { -not $_.Success }).Count
    } else {
        # Fallback to sequential for older PowerShell
        Write-Host "Note: Using sequential mode. Upgrade to PowerShell 7+ for parallel." -ForegroundColor Yellow

        foreach ($service in $services) {
            if (Publish-ServiceInternal -SvcName $service.Name -ProjectPath $service.ProjectPath) {
                $successCount++
            } else {
                $failCount++
            }
        }
    }

    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "Publish Summary:" -ForegroundColor Cyan
    Write-Host "  Success: $successCount" -ForegroundColor Green
    if ($failCount -gt 0) {
        Write-Host "  Failed:  $failCount" -ForegroundColor Red
    }
    Write-Host "==============================================" -ForegroundColor Cyan
}

function Publish-SingleService {
    <#
    .SYNOPSIS
    Publish a single service
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$SvcName
    )

    $services = Get-AllServices
    $service = $services | Where-Object { $_.Name -eq $SvcName }

    if (-not $service) {
        Write-Host "X Service '$SvcName' not found!" -ForegroundColor Red
        Write-Host "  Run '.\build.ps1 -List' to see available services." -ForegroundColor Yellow
        return
    }

    # Create publish directory if not exists
    if (-not (Test-Path $Script:PublishPath)) {
        New-Item -ItemType Directory -Path $Script:PublishPath -Force | Out-Null
    }

    Publish-ServiceInternal -SvcName $service.Name -ProjectPath $service.ProjectPath
}

function Install-AllServices {
    <#
    .SYNOPSIS
    Install all services as Windows Services
    #>
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "Installing All Services" -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan

    if (-not (Test-Path $Script:PublishPath)) {
        Write-Host "X Publish folder not found: $Script:PublishPath" -ForegroundColor Red
        Write-Host "  Run '.\build.ps1 -Publish' first." -ForegroundColor Yellow
        return
    }

    # Get all exe files directly in publish/ folder
    $exeFiles = Get-ChildItem -Path $Script:PublishPath -Filter "*.exe" -File -ErrorAction SilentlyContinue

    if ($exeFiles.Count -eq 0) {
        Write-Host "X No published services found in $Script:PublishPath" -ForegroundColor Red
        Write-Host "  Run '.\build.ps1 -Publish' first." -ForegroundColor Yellow
        return
    }

    $successCount = 0
    $failCount = 0

    foreach ($exe in $exeFiles) {
        $serviceName = $exe.BaseName  # Get filename without extension
        if (Install-ServiceInternal -SvcName $serviceName) {
            $successCount++
        } else {
            $failCount++
        }
    }

    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "Install Summary:" -ForegroundColor Cyan
    Write-Host "  Success: $successCount" -ForegroundColor Green
    if ($failCount -gt 0) {
        Write-Host "  Failed:  $failCount" -ForegroundColor Red
    }
    Write-Host "==============================================" -ForegroundColor Cyan
}

function Install-SingleService {
    <#
    .SYNOPSIS
    Install a single service as Windows Service
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$SvcName
    )

    if (-not (Test-Path $Script:PublishPath)) {
        Write-Host "X Publish folder not found: $Script:PublishPath" -ForegroundColor Red
        Write-Host "  Run '.\build.ps1 -Publish -ServiceName $SvcName' first." -ForegroundColor Yellow
        return
    }

    $exePath = Join-Path $Script:PublishPath "$SvcName.exe"
    if (-not (Test-Path $exePath)) {
        Write-Host "X Service '$SvcName' not found in publish folder." -ForegroundColor Red
        Write-Host "  Run '.\build.ps1 -Publish -ServiceName $SvcName' first." -ForegroundColor Yellow
        return
    }

    Install-ServiceInternal -SvcName $SvcName
}

function Stop-AllServices {
    <#
    .SYNOPSIS
    Stop and delete all BLE Tracking services
    #>
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Yellow
    Write-Host "Stopping All BLE Tracking Services" -ForegroundColor Yellow
    Write-Host "==============================================" -ForegroundColor Yellow

    $services = Get-Service | Where-Object { $_.Name -like "$Script:ServicePrefix*" }

    if ($services.Count -eq 0) {
        Write-Host "No BLE Tracking services found." -ForegroundColor Yellow
        return
    }

    $successCount = 0
    $failCount = 0

    foreach ($service in $services) {
        $serviceName = $service.Name -replace "^$Script:ServicePrefix", ""
        if (Stop-ServiceInternal -SvcName $serviceName) {
            $successCount++
        } else {
            $failCount++
        }
    }

    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Yellow
    Write-Host "Stop Summary:" -ForegroundColor Yellow
    Write-Host "  Success: $successCount" -ForegroundColor Green
    if ($failCount -gt 0) {
        Write-Host "  Failed:  $failCount" -ForegroundColor Red
    }
    Write-Host "==============================================" -ForegroundColor Yellow
}

function Stop-SingleService {
    <#
    .SYNOPSIS
    Stop and delete a single Windows Service
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$SvcName
    )

    Stop-ServiceInternal -SvcName $SvcName
}

function Restart-AllServices {
    <#
    .SYNOPSIS
    Restart all BLE Tracking services
    #>
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Magenta
    Write-Host "Restarting All BLE Tracking Services" -ForegroundColor Magenta
    Write-Host "==============================================" -ForegroundColor Magenta

    $services = Get-Service | Where-Object { $_.Name -like "$Script:ServicePrefix*" }

    if ($services.Count -eq 0) {
        Write-Host "No BLE Tracking services found." -ForegroundColor Yellow
        return
    }

    $successCount = 0
    $failCount = 0

    foreach ($service in $services) {
        $serviceName = $service.Name -replace "^$Script:ServicePrefix", ""
        if (Restart-ServiceInternal -SvcName $serviceName) {
            $successCount++
        } else {
            $failCount++
        }
    }

    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Magenta
    Write-Host "Restart Summary:" -ForegroundColor Magenta
    Write-Host "  Success: $successCount" -ForegroundColor Green
    if ($failCount -gt 0) {
        Write-Host "  Failed:  $failCount" -ForegroundColor Red
    }
    Write-Host "==============================================" -ForegroundColor Magenta
}

function Show-Help {
    <#
    .SYNOPSIS
    Show help information
    #>
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "BLE Tracking Backend - Build and Install Script" -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor White
    Write-Host "  .\build.ps1 [OPTIONS]" -ForegroundColor Gray
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor White
    Write-Host "  -Publish           Publish service(s) to publish/ folder" -ForegroundColor Green
    Write-Host "  -Install           Install service(s) as Windows Service" -ForegroundColor Green
    Write-Host "  -Stop              Stop and delete service(s)" -ForegroundColor Yellow
    Write-Host "  -Restart           Restart service(s)" -ForegroundColor Magenta
    Write-Host "  -Status            Show service(s) status" -ForegroundColor Cyan
    Write-Host "  -List              List all available services" -ForegroundColor Cyan
    Write-Host "  -ServiceName [name] Specify single service (? = interactive)" -ForegroundColor Gray
    Write-Host "  -ParallelCount [n]  Number of parallel publish jobs (default: 4)" -ForegroundColor Gray
    Write-Host "  -Help              Show this help message" -ForegroundColor Gray
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor White
    Write-Host "  # List all available services" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -List" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Publish all services" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Publish" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Publish single service (interactive)" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Publish -ServiceName ?" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Publish single service (with name)" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Publish -ServiceName Auth" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Install all services" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Install" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Publish and Install all services" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Publish -Install" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Publish and Install single service" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Publish -Install -ServiceName Auth" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Stop all services" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Stop" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Stop single service (interactive)" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Stop -ServiceName ?" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Restart single service" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Restart -ServiceName Auth" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  # Check status of all services" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Status" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "AVAILABLE SERVICES:" -ForegroundColor White

    $services = Get-AllServices
    $index = 1
    foreach ($service in $services) {
        Write-Host "  $($index.ToString().PadLeft(2)). " -NoNewline -ForegroundColor Gray
        Write-Host $service.Name
        $index++
    }

    Write-Host ""
    Write-Host "NOTES:" -ForegroundColor White
    Write-Host "  - Published services are stored in: publish/ folder (all in one folder)" -ForegroundColor Gray
    Write-Host "  - Windows Service names follow pattern: BleTracking_[ServiceName]" -ForegroundColor Gray
    Write-Host "  - Use -ServiceName ? or -ServiceName menu for interactive menu" -ForegroundColor Gray
    Write-Host ""
}

function Show-List {
    <#
    .SYNOPSIS
    List all available services
    #>
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "Available Services" -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host ""

    $services = Get-AllServices
    $index = 1
    foreach ($service in $services) {
        Write-Host "[$index] " -NoNewline -ForegroundColor Cyan
        Write-Host "$($service.Name)"
        $index++
    }

    Write-Host ""
    Write-Host "Total: $($services.Count) services" -ForegroundColor Gray
    Write-Host ""
}

# ============================================
# Script Entry Point
# ============================================

function Main {
    # Check for PowerShell version
    if ($PSVersionTable.PSVersion.Major -lt 7) {
        Write-Host "Note: PowerShell 7+ is recommended for best experience." -ForegroundColor Yellow
    }

    # If no parameters, show help
    if (-not ($Publish -or $Install -or $Stop -or $Status -or $List -or $Restart -or $Help)) {
        Show-Help
        return
    }

    # Show help if requested
    if ($Help) {
        Show-Help
        return
    }

    # List services
    if ($List) {
        Show-List
        return
    }

    # Status check
    if ($Status) {
        if ([string]::IsNullOrWhiteSpace($ServiceName)) {
            Get-ServiceStatusInternal
        } else {
            Get-ServiceStatusInternal -SvcName $ServiceName
        }
        return
    }

    # Determine if single service or all services
    $targetService = $null

    if ([string]::IsNullOrWhiteSpace($ServiceName)) {
        # No service specified - operate on all services
        $targetService = $null
    } else {
        # Service name provided
        if ($ServiceName -eq "?" -or $ServiceName -eq "menu") {
            # Interactive mode
            $targetService = Show-ServiceMenu -Title "Select Service"
            if (-not $targetService) {
                Write-Host "Cancelled." -ForegroundColor Yellow
                return
            }
        } else {
            # Validate service name
            $services = Get-AllServices
            $targetService = $services | Where-Object { $_.Name -eq $ServiceName }

            if (-not $targetService) {
                Write-Host "X Service '$ServiceName' not found!" -ForegroundColor Red
                Write-Host "  Run '.\build.ps1 -List' to see available services." -ForegroundColor Yellow
                return
            }
        }
    }

    # Execute operations
    if ($Stop) {
        if ($targetService) {
            Stop-SingleService -SvcName $targetService.Name
        } else {
            Stop-AllServices
        }
    }

    if ($Restart) {
        if ($targetService) {
            Restart-ServiceInternal -SvcName $targetService.Name
        } else {
            Restart-AllServices
        }
    }

    if ($Publish) {
        if ($targetService) {
            Publish-SingleService -SvcName $targetService.Name
        } else {
            Publish-AllServices
        }
    }

    if ($Install) {
        if ($targetService) {
            Install-SingleService -SvcName $targetService.Name
        } else {
            Install-AllServices
        }
    }
}

# Run main function
Main
