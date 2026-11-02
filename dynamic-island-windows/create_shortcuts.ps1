$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $projectDir) { $projectDir = Get-Location }
$binDir = Join-Path $projectDir "bin"
$islandExe = Join-Path $binDir "DynamicIsland.exe"
$configExe = Join-Path $binDir "DynamicIslandConfig.exe"

$wscript = New-Object -ComObject WScript.Shell

function CreateLnk($targetFolder, $name, $targetExe, $desc) {
    try {
        if (-not (Test-Path $targetFolder)) { return $false }
        $lnkPath = Join-Path $targetFolder $name
        $shortcut = $wscript.CreateShortcut($lnkPath)
        $shortcut.TargetPath = $targetExe
        $shortcut.WorkingDirectory = (Split-Path -Parent $targetExe)
        $shortcut.Description = $desc
        $shortcut.Save()
        Write-Host "Created shortcut: $lnkPath" -ForegroundColor Green
        return $true
    } catch {
        return $false
    }
}

# Try User Desktop
$desktop = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
if (-not $desktop) { $desktop = "$env:USERPROFILE\Desktop" }

$successDesktop = CreateLnk $desktop "iOS Dynamic Island.lnk" $islandExe "Launch iOS Dynamic Island for Windows"
CreateLnk $desktop "Dynamic Island Control Center.lnk" $configExe "Configure and maintain iOS Dynamic Island"

# Also create in project directory for direct access
CreateLnk $projectDir "Launch iOS Dynamic Island.lnk" $islandExe "Launch iOS Dynamic Island for Windows"
CreateLnk $projectDir "Open Control Center.lnk" $configExe "Configure and maintain iOS Dynamic Island"

Write-Host "Done! You can launch Dynamic Island directly from the shortcuts or bin\DynamicIsland.exe." -ForegroundColor Cyan
