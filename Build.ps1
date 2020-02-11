[CmdletBinding()]
Param (
    [switch]$ContinuousIntegration
)
Begin {
    $ProjectName = "Lib.Transform"
    $SolutionDir = $PSScriptRoot
    $OutputDir = Join-Path $SolutionDir -ChildPath ".releaseArtifacts"

    if (Test-Path($OutputDir)) {
        Remove-Item $OutputDir\* -force -recurse -ErrorAction SilentlyContinue | Out-Null
    }
    else {
        New-Item $OutputDir -ItemType Directory | Out-Null
    }

    $releaseDir = "$SolutionDir\$ProjectName\bin\release"
    if (Test-Path($releaseDir)) {
        Remove-Item $releaseDir\* -recurse -force | Out-Null
    }

    Set-Location $SolutionDir | Out-Null

    $GitShortHash = git log --pretty=format:'%h' -n 1
    $Version = git describe --tags --abbrev=0
    $SemVer10 = $Version.TrimStart('v').TrimStart('V')
    $SemVer20 = "$($SemVer10)+$($GitShortHash)"

    Write-Verbose "SemVer10: $SemVer10"
    Write-Verbose "SemVer20: $SemVer20"
}
Process {    
    dotnet publish $SolutionDir\$ProjectName\$ProjectName.csproj --configuration release --force --output "$OutputDir\$ProjectName" /p:Version=$SemVer20
    Update-ModuleManifest -Path "$OutputDir\$ProjectName\$ProjectName.psd1" -ModuleVersion $SemVer10

    if(-NOT($ContinuousIntegration.IsPresent)){
        .\install.bat
        pwsh.exe
    }    
}
End {    
}