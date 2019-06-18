param (
    [string] $solutionDir,    # e.g. c:\source\vrs\
    [string] $targetPath      # e.g. c:\source\vrs\project\bin\build\alt.project.dll
)
$dryRun = $false
$dryRunDesc = ''
if($dryRun) {
    $dryRunDesc = ' [DRY RUN]'
}

function Usage
{
    Write-Host 'usage: _PostBuild.ps1 -solutionDir <solution directory> -targetPath <target path>'
    exit 1
}

if([string]::IsNullOrWhiteSpace($solutionDir) -or [string]::IsNullOrWhiteSpace($targetPath)) {
    Usage
}

$projBinIndex = $targetPath.IndexOf('/bin/', [System.StringComparison]::OrdinalIgnoreCase)
if($projBinIndex -eq -1) {
    $projBinIndex = $targetPath.IndexOf('\bin\', [System.StringComparison]::OrdinalIgnoreCase)
}
if($projBinIndex -eq -1) {
    Write-Host ('Cannot find the bin folder in ' + $targetPath)
    exit 1
}

$targetDir = [IO.Path]::GetDirectoryName($targetPath) + [IO.Path]::DirectorySeparatorChar   # e.g. c:\source\vrs\project\bin\build\
$targetName = [IO.Path]::GetFileNameWithoutExtension($targetPath)                           # e.g. alt.project
$projectDir = $targetPath.Substring(0, $projBinIndex)
$projectName = [IO.Path]::GetFileName($projectDir)                                          # e.g. project
$projectDir = $projectDir + [IO.Path]::DirectorySeparatorChar                               # e.g. c:\source\vrs\project\
$outDir = [IO.Path]::GetDirectoryName($targetPath.Substring($projBinIndex + 1)) + [IO.Path]::DirectorySeparatorChar     # e.g. bin\build
$configurationName = ''
foreach($pathPart in $outDir.Split([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)) {
    if($pathPart -eq 'debug' -or $pathPart -eq 'release') {
        $configurationName = $pathPart
        break
    }
}

$virtualRadarDir = [IO.Path]::Combine($solutionDir, 'VirtualRadar', 'bin', 'x86', $configurationName)

Write-Host ('_PostBuild.ps1 variables:')
Write-Host ('$configurationName: ' + $configurationName)
Write-Host ('$dryRun:            ' + $dryRun)
Write-Host ('$outDir:            ' + $outDir)
Write-Host ('$projectDir:        ' + $projectDir)
Write-Host ('$projectName:       ' + $projectName)
Write-Host ('$solutionDir:       ' + $solutionDir)
Write-Host ('$targetDir:         ' + $targetDir)
Write-Host ('$targetName:        ' + $targetName)
Write-Host ('$targetPath:        ' + $targetPath)
Write-Host ('$virtualRadarDir:   ' + $virtualRadarDir)

function Copy-File
{
    param (
        [string] $source,
        [string] $destFolder,
        [switch] $ignoreIfMissing,
        [parameter(mandatory=$false)][string] $destFileName
    )

    Write-Host ('Copying ' + $source + ' to ' + $destFolder + $dryRunDesc)

    if(![IO.File]::Exists($source)) {
        if(!$ignoreIfMissing) {
            Write-Host ($source + ' does not exist')
            Write-Host 'Cannot copy missing file'
            exit 1
        }
    } else {
        if(![IO.Directory]::Exists($destFolder)) {
            Write-Host ('Creating folder ' + $destFolder + $dryRunDesc)
            if(!$dryRun) {
                $assigningToVariableStopsExtraneousOutput = [IO.Directory]::CreateDirectory($destFolder)
            }
        }

        $destination = $destFolder
        if(!String.IsNullOrWhiteSpace($destFileName)) {
            $destination = [IO.Path]::Combine($destFolder, $destFilename)
        }

        if(!$dryRun) {
            Copy-Item $source $destination
        }
    }
}

function Delete-Folder
{
    param (
        [string] $folder
    )

    if([IO.Directory]::Exists($folder)) {
        Write-Host ('Deleting folder ' + $folder + $dryRunDesc)
        if(!$dryRun) {
            Remove-Item $folder -Recurse -Force
        }

        if(!$dryRun) {
            $retryCounter = 0
            while([IO.Directory]::Exists($folder)) {
                if($retryCounter -gt 3) {
                    Write-Host 'Could not remove folder'
                    exit 1
                }
                ++$retryCounter
                Start-Sleep -Milliseconds 500

                if([IO.Directory]::Exists($folder)) {
                    Remove-Item $folder -Recurse -Force
                }
            }
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

    Write-Host ("Copying " + $sourceFolder + " to " + $destFolder + $dryRunDesc)
    if(!$dryRun) {
        if(!$recursive) {
            Copy-Item $sourceFolder $destFolder
        } else {
            Copy-Item $sourceFolder $destFolder -Recurse
        }
    }
}

function Checksum-Folder
{
    param (
        [string] $folder,
        [string] $checksumFile
    )
    Write-Host ('Generating checksums for ' + $folder)
    Write-Host ('Saving checksums to ' + $checksumFile + $dryRunDesc)

    $checksumExe = [IO.Path]::Combine($solutionDir, 'ThirdParty', 'ChecksumFiles', 'bin', $configurationName, 'ChecksumFiles.exe')
    if(![IO.File]::Exists($checksumExe)) {
        Write-Host ('The checksum utility ' + $checksumExe + ' has not been built. Add it as a dependency to the project and try again.')
        exit 1
    }

    if(!$dryRun) {
        & $checksumExe -root:"$folder" -out:"$checksumFile" -addContentChecksum
        if($LASTEXITCODE -ne 0) {
            Write-Host ('The checksum utility failed with an exit code of ' + $LASTEXITCODE)
            exit 1
        }
    }
}

function Copy-PluginWebFolder
{
    param (
        [string] $projectWebDir,
        [string] $destWebDir
    )

    if(![string]::IsNullOrWhiteSpace($projectWebDir)) {
        $webDir = [IO.Path]::Combine($projectDir, $projectWebDir)
        if([IO.Directory]::Exists($webDir)) {
            $pluginWebDir = [IO.Path]::Combine($pluginDir, $destWebDir)
            Copy-Folder $webDir $pluginWebDir -recursive
        }
    }
}

function Copy-Translations
{
    Get-ChildItem $targetDir -Directory |
    Where-Object {
        $_.Name -match '[a-z]{2}\-[A-Z]{2}'
    } |
    ForEach-Object {
        $sourceDll = [IO.Path]::Combine($_.FullName, ($targetName + ".resources.dll"))
        $destDir = [IO.Path]::Combine($virtualRadarDir, $_.Name)

        # Note that the file might not exist if no translations have been done for the plugin but
        # the plugin refers to a library that *does* have translations. The reference will create
        # the language folders but there won't be any resource DLLs for the plugin in them.
        Copy-File $sourceDll $destDir -ignoreIfMissing
    }
}

function PostBuild-WebSite-Project
{
    $siteDir = [IO.Path]::Combine($projectDir, 'Site')
    $webDir =  [IO.Path]::Combine($siteDir, 'Web')

    # Checksum the web folder
    $checksumOut = [IO.Path]::Combine($virtualRadarDir, 'Checksums.txt')
    Checksum-Folder -folder $webDir -checksumFile $checksumOut

    # The unit tests on the web site need to have the checksums file in a fixed location, i.e.
    # its folder should not include the configuration name
    Copy-File $checksumOut $siteDir

    # Copy the web folder to the VRS build folder
    $vrsWebDir = [IO.Path]::Combine($virtualRadarDir, "Web")
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
    if([string]::IsNullOrWhiteSpace($pluginName)) {
        $lastDotIdx = $projectName.LastIndexOf('.')
        if($lastDotIdx -eq -1) {
            Write-Host("Cannot extract name of plugin from " + $projectName + ", use -pluginName parameter")
            exit 1
        }
        $pluginName = $projectName.Substring($lastDotIdx + 1)
    }

    $pluginsRoot =    [IO.Path]::Combine($virtualRadarDir, "Plugins")
    $pluginDir =      [IO.Path]::Combine($pluginsRoot, $pluginName)
    $pluginManifest = [IO.Path]::Combine($targetDir, ($targetName + '.xml'))

    Delete-Folder $pluginDir

    Copy-File $targetPath       $pluginDir
    Copy-File $pluginManifest   $pluginDir

    Copy-PluginWebFolder $projectWebDir      $destWebDir
    Copy-PluginWebFolder $projectWebAdminDir $destWebAdminDir

    Copy-Translations
}

function PostBuild-VirtualRadar-Service
{
    Copy-File $targetPath                                                     $virtualRadarDir
    Copy-File ([IO.Path]::Combine($targetDir, ($targetName + '.exe.config'))) $virtualRadarDir
}

function PostBuild-BaseStationImport
{
    Copy-File ([IO.Path]::Combine($targetDir, ($targetName + '.exe')))        $virtualRadarDir
    Copy-File ([IO.Path]::Combine($targetDir, ($targetName + '.dll')))        $virtualRadarDir
    Copy-File ([IO.Path]::Combine($targetDir, ($targetName + '.dll.config'))) $virtualRadarDir -destFileName ($targetName + '.exe.config')
    Copy-File ([IO.Path]::Combine($targetDir, ($targetName + '.pdb')))        $virtualRadarDir
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
