param(
    [Parameter(Mandatory=$True)]
    $sourceRoot
)

Write-Output "Removing obj folders from $sourceRoot"
Dir -Path "$sourceRoot" obj -Directory -Recurse | Remove-Item -Force -Recurse

Write-Output "Removing bin folders from $sourceRoot"
Dir -Path "$sourceRoot" bin -Directory -Recurse | Remove-Item -Force -Recurse
