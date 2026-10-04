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

function Get-ReleaseTag
{
    param (
        $release
    )

    $result = 'v' + $release.Version
    if($release.IsPreview) {
        $prefix = $result + '-preview-'
        $pattern = '^' + [regex]::Escape($prefix) + '(\d+)$'
        $highest = 0
        foreach($existingTag in @(git tag --list ($prefix + '*'))) {
            $tagMatch = [regex]::Match($existingTag, $pattern)
            if($tagMatch.Success) {
                $number = [int] $tagMatch.Groups[1].Value
                if($number -gt $highest) {
                    $highest = $number
                }
            }
        }
        $result = $prefix + ($highest + 1)
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

$exitCode = 0
Push-Location $PSScriptRoot
try {
    $assemblyInfoPath = [io.Path]::Combine($PSScriptRoot, 'VirtualRadar', 'Properties', 'AssemblyInfo.cs')

    git status
    if($LASTEXITCODE -ne 0) {
        $exitCode = 1
    }

    if($exitCode -eq 0 -and (Confirm-Step '* Run git pull?')) {
        git pull
        if($LASTEXITCODE -ne 0) {
            Write-Host 'git pull failed' -ForegroundColor Red
            $exitCode = 1
        }
    }

    if($exitCode -eq 0 -and (Confirm-Step '* Run git push?')) {
        git push
        if($LASTEXITCODE -ne 0) {
            Write-Host 'git push failed' -ForegroundColor Red
            $exitCode = 1
        }
    }

    if($exitCode -eq 0) {
        $release = Get-ReleaseVersion $assemblyInfoPath
        if($null -eq $release) {
            Write-Host "Could not find the AssemblyVersion in $assemblyInfoPath" -ForegroundColor Red
            $exitCode = 1
        } else {
            $tag = Get-ReleaseTag $release
            $releaseKind = if($release.IsPreview) { 'preview' } else { 'release' }
            Write-Host ''
            Write-Host "Version: $($release.Version) ($releaseKind)"
            Write-Host "Tag:     $tag"
            Write-Host ''

            if(Test-TagExists $tag) {
                Write-Host "The tag $tag already exists" -ForegroundColor Red
                $exitCode = 1
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
                        $exitCode = 1
                    }
                }

                if($exitCode -eq 0 -and (Confirm-Step '* Run git push --tags?')) {
                    git push --tags
                    if($LASTEXITCODE -ne 0) {
                        Write-Host 'git push failed' -ForegroundColor Red
                        $exitCode = 1
                    }
                }
            }
        }
    }
} finally {
    Pop-Location
}

exit $exitCode
