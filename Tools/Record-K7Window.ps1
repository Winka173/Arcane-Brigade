param(
    [ValidateSet('combat', 'shop')][string]$Mode = 'combat',
    [ValidateSet('K7', 'K7B')][string]$Phase = 'K7',
    [string]$FFmpegPath
)
$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$player = Join-Path $project 'Builds/FullRun/ArcaneBrigade.exe'
if (-not (Test-Path -LiteralPath $player)) { throw 'Build the Windows candidate first.' }
if (-not $FFmpegPath) { $FFmpegPath = (Get-ChildItem (Join-Path $project 'Logs/K7Tools') -Filter ffmpeg.exe -Recurse | Select-Object -First 1).FullName }
if (-not $FFmpegPath -or -not (Test-Path -LiteralPath $FFmpegPath)) { throw 'Provide -FFmpegPath using a build linked from ffmpeg.org.' }
$fixture = Join-Path $project ('PreviewExports/' + $Phase + '/VideoFixture/' + $Mode)
New-Item -ItemType Directory -Force -Path $fixture | Out-Null
$video = Join-Path $project ('PreviewExports/' + $Phase + '/' + $Phase + '-' + $(if ($Mode -eq 'shop') { 'Shop' } else { 'Gameplay' }) + '-Realtime.mp4')
$flag = if ($Mode -eq 'shop') { '-arcane-k7-shop-video' } else { '-arcane-k7-video' }
$log = Join-Path $project ('Logs/K7-Video-' + $Mode + '.log')
$arguments = @('-arcane-k7-capture', $flag, '-capturePath', ('"' + $fixture + '"'), '-ffmpegPath', ('"' + $FFmpegPath + '"'), '-videoPath', ('"' + $video + '"'), '-force-d3d11', '-screen-fullscreen', '0', '-logFile', ('"' + $log + '"'))
if ($Phase -eq 'K7B') { $arguments += '-arcane-k7b' }
$process = Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw ('Framebuffer recorder or fixture failed: player=' + $process.ExitCode) }
$probe = Join-Path (Split-Path $FFmpegPath) 'ffprobe.exe'
& $probe -v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate,avg_frame_rate,nb_frames -show_entries format=duration -of json $video
Get-Content -LiteralPath ($video + '.txt')
Get-Content -LiteralPath (Join-Path $fixture 'CaptureNotes.txt')
