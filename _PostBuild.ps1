param (
    [string] $projectName,
    [string] $configurationName,
    [string] $targetName
)
function Usage
{
    Write-Host 'usage: _PostBuild.ps1 -projectName <project name> -configurationName <configuration name> [-targetName <targetName>]'
    Write-Host 'The targetName parameter is not optional for plugins'
    Exit 1
}

if([string]::IsNullOrWhiteSpace($projectName) -or [string]::IsNullOrWhiteSpace($configurationName)) {
    Usage
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

    Write-Host ('Copying ' + $source + ' to ' + $destFolder)

    if(![io.Directory]::Exists($destFolder)) {
        Write-Host ('Creating folder ' + $destFolder)
        $dirInfo = [io.Directory]::CreateDirectory($destFolder)
    }

    if(![io.File]::Exists($source)) {
        Write-Host ($source + ' does not exist')
        Write-Host 'Cannot copy missing file'
        Exit 1
    }

    Copy-Item $source $destFolder
}

function Delete-Folder
{
    param (
        [string] $folder
    )

    if([io.Directory]::Exists($folder)) {
        Write-Host ('Deleting folder ' + $folder)
        Remove-Item $folder -Recurse -Force

        $retryCounter = 0
        while([io.Directory]::Exists($folder)) {
            if($retryCounter -gt 3) {
                Write-Host 'Could not remove folder'
                Exit 1
            }
            ++$retryCounter
            Start-Sleep -Milliseconds 500
        }
    }
}

function Copy-Folder
{
    param (
        [string] $sourceFolder,
        [string] $destFolder,
        [switch] $deleteBeforeCopy,
        [switch] $recursive
    )

    if($deleteBeforeCopy) {
        Delete-Folder $destFolder
    }

    Write-Host ("Copying " + $sourceFolder + " to " + $destFolder)
    if(!$recursive) {
        Copy-Item $sourceFolder $destFolder
    } else {
        Copy-Item $sourceFolder $destFolder -Recurse
    }
}

function Checksum-Folder
{
    param (
        [string] $folder,
        [string] $checksumFile
    )
    Write-Host ('Generating checksums for ' + $folder)
    Write-Host ('Saving checksums to ' + $checksumFile)

    $checksumExe = [io.Path]::Combine($solutionDir, 'ThirdParty', 'ChecksumFiles', 'bin', $configurationName, 'ChecksumFiles.exe')
    if(![io.File]::Exists($checksumExe)) {
        Write-Host ('The checksum utility ' + $checksumExe + ' has not been built. Add it as a dependency to the project and try again.')
        Exit 1
    }

    & $checksumExe -root:"$folder" -out:"$checksumFile" -addContentChecksum
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
    Checksum-Folder -folder $webDir -checksumFile $checksumOut

    # The unit tests on the web site need to have the checksums file in a fixed location, i.e.
    # its folder should not include the configuration name
    Copy-File $checksumOut $siteDir

    # Copy the web folder to the VRS build folder
    $vrsWebDir = [io.Path]::Combine($virtualRadarDir, "Web")
    Copy-Folder $webDir $vrsWebDir -deleteBeforeCopy -recursive
}

function PostBuild-Plugin
{
    param (
        [string] $pluginName = '',
        [string] $projectWebDir = 'Web',
        [string] $destWebDir = 'Web'
    )
    if([string]::IsNullOrWhiteSpace($targetName)) {
        Usage
    }
    if([string]::IsNullOrWhiteSpace($pluginName)) {
        $lastDotIdx = $projectName.LastIndexOf('.')
        if($lastDotIdx -eq -1) {
            Write-Host ("Cannot extract name of plugin from " + $projectName + ", use -pluginName parameter")
            Exit 1
        }
        $pluginName = $projectName.Substring($lastDotIdx + 1)
    }

    $pluginsRoot =    [io.Path]::Combine($virtualRadarDir, "Plugins")
    $pluginDir =      [io.Path]::Combine($pluginsRoot, $pluginName)
    $pluginDll =      [io.Path]::Combine($projectDir, "bin", $configurationName, ($targetName + '.dll'))
    $pluginManifest = [io.Path]::Combine($projectDir, "bin", $configurationName, ($targetName + '.xml'))

    Delete-Folder $pluginDir

    Copy-File $pluginDll $pluginDir
    Copy-File $pluginManifest $pluginDir

    if(![string]::IsNullOrWhiteSpace($projectWebDir)) {
        $webDir = [io.Path]::Combine($projectDir, $projectWebDir)
        if([io.Directory]::Exists($webDir)) {
            $pluginWebDir = [io.Path]::Combine($pluginDir, $destWebDir);
            Copy-Folder $webDir $pluginWebDir -recursive
        }
    }
}

switch($projectName.ToLower()) {
    'virtualradar.website'              { PostBuild-WebSite-Project }
    'plugin.basestationdatabasewriter'  { PostBuild-Plugin }
}
