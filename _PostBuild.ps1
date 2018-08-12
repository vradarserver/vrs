param (
    [string] $projectName,
    [string] $configurationName
)
if([string]::IsNullOrWhiteSpace($projectName) -or [string]::IsNullOrWhiteSpace($configurationName)) {
    Write-Host 'usage: _PostBuild.ps1 -projectName <project name> -configurationName <configuration name>'
    Exit 1
}

$solutionDir = Split-Path -Parent $PSCommandPath
$projectDir = [io.Path]::Combine($solutionDir, $projectName)
$virtualRadarDir = [io.Path]::Combine($solutionDir, 'VirtualRadar', 'bin', 'x86', $configurationName)

Write-Host ('Running post-build steps for project ' + $projectName + ' (' + $configurationName + ' configuration)')
Write-Host ('Solution folder:           ' + $solutionDir)
Write-Host ('Project folder:            ' + $projectDir)
Write-Host ('VirtualRadar build folder: ' + $virtualRadarDir)

function Copy-File
{
    param (
        [string] $source,
        [string] $destFolder
    )

    Write-Host ("Copying " + $source + " to " + $destFolder)
    Copy-Item $source $destFolder
}

function Copy-Folder
{
    param (
        [string] $sourceFolder,
        [string] $destFolder,
        [switch] $deleteBeforeCopy,
        [switch] $recursive
    )

    if([io.Directory]::Exists($destFolder) -and $deleteBeforeCopy) {
        Write-Host ('Deleting folder ' + $destFolder)
        [io.Directory]::Delete($destFolder, $true)

        if([io.Directory]::Exists($destFolder)) {
            Write-Host 'Could not remove folder'
            Exit 1
        }
    }

    Write-Host ("Copying " + $sourceFolder + " to " + $destFolder)
    if(!$recursive) {
        Copy-Item $sourceFolder $destFolder
    } else {
        Copy-Item $sourceFolder $destFolder -Recurse
    }
}

function Checksum-Files
{
    param (
        [string] $checksumRoot,
        [string] $checksumOut
    )
    Write-Host ('Generating checksums for ' + $checksumRoot)
    Write-Host ('Saving checksums to ' + $checksumOut)

    $checksumExe = [io.Path]::Combine($solutionDir, 'ThirdParty', 'ChecksumFiles', 'bin', $configurationName, 'ChecksumFiles.exe')
    if(![io.File]::Exists($checksumExe)) {
        Write-Host ('The checksum utility ' + $checksumExe + ' has not been built. Add it as a dependency to the project and try again.')
        Exit 1
    }

    & $checksumExe -root:"$checksumRoot" -out:"$checksumOut" -addContentChecksum
    if($LASTEXITCODE -ne 0) {
        Write-Host ('The checksum utility failed with an exit code of ' + $LASTEXITCODE)
        Exit 1
    }
}

function PostBuild-WebSite-Project
{
    $siteDir = [io.Path]::Combine($projectDir, 'Site')
    $webDir =  [io.Path]::Combine($siteDir, 'Web')

    # Checksum the web folder
    $checksumOut = [io.Path]::Combine($virtualRadarDir, 'Checksums.txt')
    Checksum-Files -checksumRoot $webDir -checksumOut $checksumOut

    # The unit tests on the web site need to have the checksums file in a fixed location, i.e.
    # its folder should not include the configuration name
    Copy-File $checksumOut $siteDir

    # Copy the web folder to the VRS build folder
    $vrsWebDir = [io.Path]::Combine($virtualRadarDir, "Web")
    Copy-Folder $webDir $vrsWebDir -deleteBeforeCopy -recursive
}

if($projectName.ToLower() -eq 'virtualradar.website') {
    PostBuild-WebSite-Project
}
