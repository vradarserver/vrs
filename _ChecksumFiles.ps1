param (
    [string] $root,
    [string] $out,
    [Switch] $addContentChecksum
)
function Usage
{
    Write-Host 'usage: _ChecksumFiles.ps1 -root <folder> [-out <filename>] [-addContentChecksum]'
    Exit 1
}

if([String]::IsNullOrWhiteSpace($root)) {
    Usage
}
if(-Not [IO.Directory]::Exists($root)) {
    Write-Host ([String]::Format('{0} does not exist', $root))
    Usage
}

[string] $outFileName;
if(-Not [String]::IsNullOrWhiteSpace($out)) {
    $outFileName = $out
    $folder = [IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($outFileName))
    if(-Not [IO.Directory]::Exists($folder)) {
        [IO.Directory]::CreateDirectory($folder)
    }
}
[IO.TextWriter] $outWriter = if($outFileName -eq $null) { [Console]::Out } else { [IO.StreamWriter]::new($outFileName, $false)}

$checksummer = [Crc64]::new()
$folder = $root
foreach($fileName in [IO.Directory]::GetFiles($folder, '*.*', [IO.SearchOption]::AllDirectories)) {
    $relativePath = $fileName.Substring($folder.Length)
    $content = [IO.File]::ReadAllBytes($fileName)
    $checksum = $checksummer.ComputeChecksumString($content, 0, $content.Length)
    $outWriter.WriteLine([String]::Format(
        '{0} {1,9} {2}',
        $checksum,
        [IO.FileInfo]::new($fileName).Length,
        $relativePath
    ))
}

if($outFileName -ne $null) {
    $outWriter.Dispose()
}

if($addContentChecksum.IsPresent -and $outFileName -ne $null) {
    $checksums = [IO.File]::ReadAllLines($outFileName)
    $checksumBytes = [Text.Encoding]::UTF8.GetBytes([String]::Concat($checksums))
    $checksum = $checksummer.ComputeChecksumString($checksumBytes, 0, $checksumBytes.Length)

    $contentLines = [string[]]::new($checksums.Length + 1)
    $contentLines[0] = [String]::Format(
        '{0} {1,9} {2}',
        $checksum,
        [System.Linq.Enumerable]::Sum($checksums, [Func[string, int]]{ param($r) $r.Length }),
        '\**CONTENT CHECKSUM**'
    )
    for($i = 0;$i -lt $checksums.Length;++$i) {
        $contentLines[$i + 1] = $checksums[$i]
    }
    [IO.File]::WriteAllLines($outFileName, $contentLines)
}

Exit 0

class Crc64
{
    [uint64] hidden $_Polynomial

    [uint64[]] hidden $_LookupTable = [uint64[]]::new(256)

    Crc64()
    {
        $this._Polynomial = [uint64]"0xC96C5795D7870F42"

        for($i = 0;$i -lt 256;++$i) {
            [uint64]$crc = $i
            for($j = 8;$j -gt 0;--$j) {
                if(($crc -band 1) -eq 1) {
                    $crc = [uint64](($crc -shr 1) -bxor $this._Polynomial)
                } else {
                    $crc = $crc -shr 1
                }
            }

            $this._LookupTable[$i] = $crc
        }
    }

    [string] ComputeChecksumString([byte[]] $bytes, [int] $offset, [int] $length)
    {
        $crc = $this.ComputeChecksum($bytes, $offset, $length)
        return [String]::Format('{0:X16}', $crc)
    }

    [uint64] ComputeChecksum([byte[]] $bytes, [int] $offset, [int] $length)
    {
        [uint64] $crc = [uint64]"0xFFFFFFFFFFFFFFFF"

        for($i = $offset;$i -lt $offset + $length;++$i) {
            $index = [byte](($crc -band 0xff) -bxor $bytes[$i])
            $crc = [uint64](($crc -shr 8) -bxor $this._LookupTable[$index])
        }

        return $crc
    }
}