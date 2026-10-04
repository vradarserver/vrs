param (
    [switch] $Undo
)

function Confirm-Step
{
    param (
        [string] $prompt
    )

    $result = $null
    while($null -eq $result) {
        $reply = "$(Read-Host "$prompt [Y/n]")".Trim()
        if($reply -eq '' -or $reply -eq 'y') {
            $result = $true
        } elseif($reply -eq 'n') {
            $result = $false
            Write-Host "(skipped)"
        }
    }

    return $result
}

function Get-ReleaseVersion
{
    param (
        [string] $path
    )

    $result = $null
    $content = Get-Content -Raw -Path $path
    $versionMatch = [regex]::Match($content, '(?m)^\s*\[assembly:\s*AssemblyVersion\("(\d+)\.(\d+)\.(\d+)')
    if($versionMatch.Success) {
        $informationalMatch = [regex]::Match($content, '(?m)^\s*\[assembly:\s*AssemblyInformationalVersion\("([^"]*)"')
        $result = [pscustomobject] @{
            Version   = $versionMatch.Groups[1].Value + '.' + $versionMatch.Groups[2].Value + '.' + $versionMatch.Groups[3].Value
            IsPreview = $informationalMatch.Success -and $informationalMatch.Groups[1].Value -match 'alpha|beta'
        }
    }

    return $result
}

function Get-HighestPreviewNumber
{
    param (
        [string] $version
    )

    $prefix = 'v' + $version + '-preview-'
    $pattern = '^' + [regex]::Escape($prefix) + '(\d+)$'
    $result = 0
    foreach($existingTag in @(git tag --list ($prefix + '*'))) {
        $tagMatch = [regex]::Match($existingTag, $pattern)
        if($tagMatch.Success) {
            $number = [int] $tagMatch.Groups[1].Value
            if($number -gt $result) {
                $result = $number
            }
        }
    }

    return $result
}

function Get-ReleaseTag
{
    param (
        $release
    )

    $result = 'v' + $release.Version
    if($release.IsPreview) {
        $result = $result + '-preview-' + ((Get-HighestPreviewNumber $release.Version) + 1)
    }

    return $result
}

function Get-UndoTag
{
    param (
        $release
    )

    $result = 'v' + $release.Version
    if($release.IsPreview) {
        $highest = Get-HighestPreviewNumber $release.Version
        $result = if($highest -eq 0) { $null } else { $result + '-preview-' + $highest }
    }

    return $result
}

function Test-TagExists
{
    param (
        [string] $tag
    )

    git rev-parse -q --verify "refs/tags/$tag" | Out-Null
    return $LASTEXITCODE -eq 0
}

function Test-RemoteTagExists
{
    param (
        [string] $tag
    )

    $result = $null
    $lines = @(git ls-remote --tags origin "refs/tags/$tag")
    if($LASTEXITCODE -eq 0) {
        $result = $lines.Count -gt 0
    }

    return $result
}

function Invoke-GhJson
{
    param (
        [string[]] $arguments
    )

    $output = & gh @arguments
    $succeeded = $LASTEXITCODE -eq 0
    $data = @()
    if($succeeded) {
        $data = @(($output -join "`n") | ConvertFrom-Json | ForEach-Object { $_ })
    }

    return [pscustomobject] @{
        Succeeded = $succeeded
        Data      = $data
    }
}

function Invoke-Release
{
    $assemblyInfoPath = [io.Path]::Combine($PSScriptRoot, 'VirtualRadar', 'Properties', 'AssemblyInfo.cs')

    git status
    if($LASTEXITCODE -ne 0) {
        $script:exitCode = 1
    }

    if($script:exitCode -eq 0 -and (Confirm-Step '* Run git pull?')) {
        git pull
        if($LASTEXITCODE -ne 0) {
            Write-Host 'git pull failed' -ForegroundColor Red
            $script:exitCode = 1
        }
    }

    if($script:exitCode -eq 0 -and (Confirm-Step '* Run git push?')) {
        git push
        if($LASTEXITCODE -ne 0) {
            Write-Host 'git push failed' -ForegroundColor Red
            $script:exitCode = 1
        }
    }

    if($script:exitCode -eq 0) {
        $release = Get-ReleaseVersion $assemblyInfoPath
        if($null -eq $release) {
            Write-Host "Could not find the AssemblyVersion in $assemblyInfoPath" -ForegroundColor Red
            $script:exitCode = 1
        } else {
            $tag = Get-ReleaseTag $release
            $releaseKind = if($release.IsPreview) { 'preview' } else { 'release' }
            Write-Host ''
            Write-Host "Version: $($release.Version) ($releaseKind)"
            Write-Host "Tag:     $tag"
            Write-Host ''

            if(Test-TagExists $tag) {
                Write-Host "The tag $tag already exists" -ForegroundColor Red
                $script:exitCode = 1
            } else {
                $defaultMessage = 'Released ' + (Get-Date).ToString('dd-MMM-yyyy', [Globalization.CultureInfo]::InvariantCulture)
                $message = "$(Read-Host "* Commit message for git tag [$defaultMessage]")".Trim()
                if($message -eq '') {
                    $message = $defaultMessage
                }

                if(Confirm-Step "* Run git tag -a $tag -m `"$message`"?") {
                    git tag -a $tag -m $message
                    if($LASTEXITCODE -ne 0) {
                        Write-Host 'git tag failed' -ForegroundColor Red
                        $script:exitCode = 1
                    }
                }

                if($script:exitCode -eq 0 -and (Confirm-Step '* Run git push --tags?')) {
                    git push --tags
                    if($LASTEXITCODE -ne 0) {
                        Write-Host 'git push failed' -ForegroundColor Red
                        $script:exitCode = 1
                    }
                }
            }
        }
    }
}

function Invoke-Undo
{
    $assemblyInfoPath = [io.Path]::Combine($PSScriptRoot, 'VirtualRadar', 'Properties', 'AssemblyInfo.cs')

    gh auth status
    if($LASTEXITCODE -ne 0) {
        Write-Host 'gh is not installed or is not logged in' -ForegroundColor Red
        $script:exitCode = 1
    }

    if($script:exitCode -eq 0) {
        $release = Get-ReleaseVersion $assemblyInfoPath
        if($null -eq $release) {
            Write-Host "Could not find the AssemblyVersion in $assemblyInfoPath" -ForegroundColor Red
            $script:exitCode = 1
        } else {
            $tag = Get-UndoTag $release
            if($null -eq $tag) {
                Write-Host "There are no preview tags for version $($release.Version)" -ForegroundColor Red
                $script:exitCode = 1
            }
        }
    }

    if($script:exitCode -eq 0) {
        $runs = Invoke-GhJson @('run', 'list', '--workflow', 'build-release.yml', '--branch', $tag, '--event', 'push', '--json', 'databaseId,status')
        $releases = Invoke-GhJson @('api', 'repos/{owner}/{repo}/releases?per_page=100')
        $remoteTagExists = Test-RemoteTagExists $tag
        if(!$runs.Succeeded -or !$releases.Succeeded -or $null -eq $remoteTagExists) {
            Write-Host 'Could not read the build runs, releases or tags from GitHub' -ForegroundColor Red
            $script:exitCode = 1
        }
    }

    if($script:exitCode -eq 0) {
        $runningIds = @($runs.Data | Where-Object { $_.status -ne 'completed' } | ForEach-Object { $_.databaseId })
        $githubReleases = @($releases.Data | Where-Object { "$($_.name)" -ceq $tag -or "$($_.name)".StartsWith("$tag [", [StringComparison]::Ordinal) })
        $localTagExists = Test-TagExists $tag

        $releaseKind = if($release.IsPreview) { 'preview' } else { 'release' }
        Write-Host ''
        Write-Host "Version:        $($release.Version) ($releaseKind)"
        Write-Host "Tag:            $tag"
        Write-Host "Running builds: $(if($runningIds.Count -gt 0) { $runningIds -join ', ' } else { '(not found)' })"
        if($githubReleases.Count -eq 0) {
            Write-Host 'Releases:       (not found)'
        }
        foreach($githubRelease in $githubReleases) {
            $state = if($githubRelease.draft) { 'draft' } else { 'published' }
            Write-Host "Release:        $($githubRelease.name) ($state, id $($githubRelease.id))"
        }
        Write-Host "Remote tag:     $(if($remoteTagExists) { 'present' } else { '(not found)' })"
        Write-Host "Local tag:      $(if($localTagExists) { 'present' } else { '(not found)' })"
        Write-Host ''

        foreach($githubRelease in $githubReleases) {
            if(!$githubRelease.draft) {
                Write-Host "$($githubRelease.name) has been published, it cannot be undone" -ForegroundColor Red
                $script:exitCode = 1
            }
        }
    }

    if($script:exitCode -eq 0) {
        foreach($runId in $runningIds) {
            if($script:exitCode -eq 0 -and (Confirm-Step "* Cancel build run $($runId)?")) {
                gh run cancel $runId
                if($LASTEXITCODE -ne 0) {
                    Write-Host 'gh run cancel failed' -ForegroundColor Red
                    $script:exitCode = 1
                } else {
                    gh run watch $runId --interval 5
                }
            }
        }
    }

    if($script:exitCode -eq 0) {
        foreach($githubRelease in $githubReleases) {
            if($script:exitCode -eq 0) {
                $releaseId = $githubRelease.id
                if(Confirm-Step "* Delete draft release $($githubRelease.name) (id $releaseId)?") {
                    gh api -X DELETE "repos/{owner}/{repo}/releases/$releaseId"
                    if($LASTEXITCODE -ne 0) {
                        Write-Host 'Deleting the release failed' -ForegroundColor Red
                        $script:exitCode = 1
                    }
                }
            }
        }
    }

    if($script:exitCode -eq 0 -and $remoteTagExists -and (Confirm-Step "* Run git push origin --delete $($tag)?")) {
        git push origin --delete $tag
        if($LASTEXITCODE -ne 0) {
            Write-Host 'git push failed' -ForegroundColor Red
            $script:exitCode = 1
        }
    }

    if($script:exitCode -eq 0 -and $localTagExists -and (Confirm-Step "* Run git tag -d $($tag)?")) {
        git tag -d $tag
        if($LASTEXITCODE -ne 0) {
            Write-Host 'git tag failed' -ForegroundColor Red
            $script:exitCode = 1
        }
    }

    if($script:exitCode -eq 0 -and (Confirm-Step '* Run git fetch --prune --prune-tags?')) {
        git fetch --prune --prune-tags
        if($LASTEXITCODE -ne 0) {
            Write-Host 'git fetch failed' -ForegroundColor Red
            $script:exitCode = 1
        }
    }
}

$exitCode = 0
Push-Location $PSScriptRoot
try {
    if($Undo) {
        Invoke-Undo
    } else {
        Invoke-Release
    }
} finally {
    Pop-Location
}

exit $exitCode
