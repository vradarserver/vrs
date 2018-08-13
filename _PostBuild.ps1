param (
    [string] $projectName,
    [string] $configurationName,
    [string] $targetName
)
function Usage
{
    Write-Host 'usage: _PostBuild.ps1 -projectName <project name> -configurationName <configuration name> [-targetName <targetName>]'
    Write-Host 'The targetName parameter is not optional for plugins or projects with translation files'
    Exit 1
}

if([string]::IsNullOrWhiteSpace($projectName) -or [string]::IsNullOrWhiteSpace($configurationName)) {
    Usage
}

$pathFromSolution = '';
if($projectName.ToLower() -eq 'basestationimport') {
    $pathFromSolution = 'Utilities'
}

$solutionDir = Split-Path -Parent $PSCommandPath
$projectDir = [io.Path]::Combine($solutionDir, $pathFromSolution, $projectName)
$virtualRadarDir = [io.Path]::Combine($solutionDir, 'VirtualRadar', 'bin', 'x86', $configurationName)

Write-Host ('Running post-build steps for project ' + $projectName + ' (' + $configurationName + ' configuration)')
Write-Host ('Solution folder:           ' + $solutionDir)
Write-Host ('Project folder:            ' + $projectDir)
Write-Host ('VirtualRadar build folder: ' + $virtualRadarDir)

function Copy-File
{
    param (
        [string] $source,
        [string] $destFolder,
        [switch] $ignoreIfMissing
    )

    Write-Host ('Copying ' + $source + ' to ' + $destFolder)

    if(![io.File]::Exists($source)) {
        if(!$ignoreIfMissing) {
            Write-Host ($source + ' does not exist')
            Write-Host 'Cannot copy missing file'
            Exit 1
        }
    } else {
        if(![io.Directory]::Exists($destFolder)) {
            Write-Host ('Creating folder ' + $destFolder)
            $dirInfo = [io.Directory]::CreateDirectory($destFolder)
        }

        Copy-Item $source $destFolder
    }
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

function Copy-PluginWebFolder
{
    param (
        [string] $projectWebDir,
        [string] $destWebDir
    )

    if(![string]::IsNullOrWhiteSpace($projectWebDir)) {
        $webDir = [io.Path]::Combine($projectDir, $projectWebDir)
        if([io.Directory]::Exists($webDir)) {
            $pluginWebDir = [io.Path]::Combine($pluginDir, $destWebDir);
            Copy-Folder $webDir $pluginWebDir -recursive
        }
    }
}

function Copy-Translations
{
    param (
        [string] $buildFolder
    )
    if([string]::IsNullOrWhiteSpace($targetName)) {
        Usage
    }

    Get-ChildItem $buildFolder -Directory |
    Where-Object {
        $_.Name -match '[a-z]{2}\-[A-Z]{2}'
    } |
    ForEach-Object {
        $sourceDll = [io.Path]::Combine($_.FullName, ($targetName + ".resources.dll"))
        $targetDir = [io.Path]::Combine($virtualRadarDir, $_.Name)

        # Note that the file might not exist if no translations have been done for the plugin but
        # the plugin refers to a library that *does* have translations. The reference will create
        # the language folders but there won't be any resource DLLs for the plugin in them.
        Copy-File $sourceDll $targetDir -ignoreIfMissing
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
        [string] $destWebDir = 'Web',
        [string] $projectWebAdminDir = 'Web-WebAdmin',
        [string] $destWebAdminDir = 'Web-WebAdmin'
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
    $pluginBuildDir = [io.Path]::Combine($projectDir, "bin", $configurationName)
    $pluginDll =      [io.Path]::Combine($pluginBuildDir, ($targetName + '.dll'))
    $pluginManifest = [io.Path]::Combine($pluginBuildDir, ($targetName + '.xml'))

    Delete-Folder $pluginDir

    Copy-File $pluginDll $pluginDir
    Copy-File $pluginManifest $pluginDir

    Copy-PluginWebFolder $projectWebDir $destWebDir
    Copy-PluginWebFolder $projectWebAdminDir $destWebAdminDir

    Copy-Translations -buildFolder $pluginBuildDir
}

function PostBuild-VirtualRadar-Service
{
    if([string]::IsNullOrWhiteSpace($targetName)) {
        Usage
    }

    $buildFolder = [io.Path]::Combine($projectDir, "bin", "x86", $configurationName)

    Copy-File ([io.Path]::Combine($buildFolder, ($targetName + '.exe'))) $virtualRadarDir
    Copy-File ([io.Path]::Combine($buildFolder, ($targetName + '.exe.config'))) $virtualRadarDir
}

function PostBuild-BaseStationImport
{
    if([string]::IsNullOrWhiteSpace($targetName)) {
        Usage
    }

    $buildFolder = [io.Path]::Combine($projectDir, "bin", "x86", $configurationName)

    Copy-File ([io.Path]::Combine($buildFolder, ($targetName + '.exe'))) $virtualRadarDir
    Copy-File ([io.Path]::Combine($buildFolder, ($targetName + '.exe.config'))) $virtualRadarDir
    Copy-File ([io.Path]::Combine($buildFolder, ($targetName + '.pdb'))) $virtualRadarDir
}

# Main switch
$caselessProject = $projectName.ToLower()
if($caselessProject -eq 'virtualradar.website') {
    PostBuild-WebSite-Project
} elseif($caselessProject.StartsWith('plugin.')) {
    PostBuild-Plugin
} elseif($caselessProject -eq 'virtualradar-service') {
    PostBuild-VirtualRadar-Service
} elseif($caselessProject -eq 'basestationimport') {
    PostBuild-BaseStationImport
} else {
    Write-Host ('Need to add code for ' + $projectName + ' to the _PostBuild.ps1 script')
}
