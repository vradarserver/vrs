param (
    [string] $projectName,
    [string] $configurationName,
    [string] $targetName,
    [string] $platformName
)
function Usage
{
    Write-Host 'usage: _PostBuild.ps1 -projectName <project name> -configurationName <configuration name> [-targetName <targetName>] [-platformName <platformName>]'
    Write-Host 'The targetName parameter is not optional for plugins or projects with translation files'
    Write-Host 'The platformName parameter is not optional for BaseStationImport'
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
$virtualRadarDirX86 = [io.Path]::Combine($solutionDir, 'VirtualRadar', 'bin', 'x86', $configurationName)
$virtualRadarDirX64 = [io.Path]::Combine($solutionDir, 'VirtualRadar', 'bin', 'x64', $configurationName)
$virtualRadarDir = switch($platformName) {
    'x64'   { $virtualRadarDirX64 }
    'x86'   { $virtualRadarDirX86 }
    default { '::UNDEFINED-' + $platformName + '::' }
}

Write-Host ('**********')
Write-Host ('Running post-build steps for project ' + $projectName + ' (' + $configurationName + ' configuration, ' + $platformName + ' platform)')
Write-Host ('Solution folder:                 ' + $solutionDir)
Write-Host ('Project folder:                  ' + $projectDir)
Write-Host ('VirtualRadar build folder (x86): ' + $virtualRadarDirX86)
Write-Host ('VirtualRadar build folder (x64): ' + $virtualRadarDirX64)
Write-Host ('VirtualRadar build folder:       ' + $virtualRadarDir)
Write-Host ('**********')

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

    $checksumScript = [IO.Path]::Combine($solutionDir, '_ChecksumFiles.ps1')

    & $checksumScript -root "$folder" -out "$checksumFile" -addContentChecksum
    if($LASTEXITCODE -ne 0) {
        Write-Host ([String]::Format('The checksum script failed with an exit code of {0}', $LASTEXITCODE))
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

        # Note that the file might not exist if no translations have been done for the plugin but
        # the plugin refers to a library that *does* have translations. The reference will create
        # the language folders but there won't be any resource DLLs for the plugin in them.

        $targetDir = [io.Path]::Combine($virtualRadarDirX86, $_.Name)
        Copy-File $sourceDll $targetDir -ignoreIfMissing

        $targetDir = [io.Path]::Combine($virtualRadarDirX64, $_.Name)
        Copy-File $sourceDll $targetDir -ignoreIfMissing
    }
}

function PostBuild-WebSite-Project
{
    $siteDir = [io.Path]::Combine($projectDir, 'Site')
    $webDir =  [io.Path]::Combine($siteDir, 'Web')

    # Checksum the web folder
    # The unit tests on the web site need to have the checksums file in a fixed location, i.e.
    # its folder should not include the configuration name

    $checksumOut = [io.Path]::Combine($virtualRadarDirX86, 'Checksums.txt')
    Checksum-Folder -folder $webDir -checksumFile $checksumOut
    Copy-File $checksumOut $siteDir

    $checksumOut = [io.Path]::Combine($virtualRadarDirX64, 'Checksums.txt')
    Checksum-Folder -folder $webDir -checksumFile $checksumOut
    Copy-File $checksumOut $siteDir

    # Copy the web folder to the VRS build folder

    $vrsWebDir = [io.Path]::Combine($virtualRadarDirX86, "Web")
    Copy-Folder $webDir $vrsWebDir -deleteBeforeCopy -recursive

    $vrsWebDir = [io.Path]::Combine($virtualRadarDirX64, "Web")
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

    foreach ($vrsBuildDir in $virtualRadarDirX86, $virtualRadarDirX64) {
        $pluginsRoot =    [io.Path]::Combine($vrsBuildDir, "Plugins")
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
}

function PostBuild-VirtualRadar-Service
{
    if([string]::IsNullOrWhiteSpace($targetName)) {
        Usage
    }

    $buildFolder = [io.Path]::Combine($projectDir, "bin", $platformName, $configurationName)

    Copy-File ([io.Path]::Combine($buildFolder, ($targetName + '.exe'))) $virtualRadarDir
    Copy-File ([io.Path]::Combine($buildFolder, ($targetName + '.exe.config'))) $virtualRadarDir
}

function PostBuild-BaseStationImport
{
    if([string]::IsNullOrWhiteSpace($targetName)) {
        Usage
    }

    $buildFolder = [io.Path]::Combine($projectDir, "bin", $platformName, $configurationName)

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
