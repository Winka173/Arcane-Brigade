param([string]$FFmpegPath)
$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not $FFmpegPath) { $FFmpegPath = (Get-ChildItem (Join-Path $project 'Logs/K7Tools') -Filter ffmpeg.exe -Recurse | Select-Object -First 1).FullName }
if (-not $FFmpegPath -or -not (Test-Path -LiteralPath $FFmpegPath)) { throw 'Provide a trusted ffmpeg executable.' }
$player = Join-Path $project 'Builds/FullRun/ArcaneBrigade.exe'
$fixture = Join-Path $project 'PreviewExports/K11/VideoFixture'
$video = Join-Path $project 'PreviewExports/K11/K11-Transition-Realtime.mp4'
$log = Join-Path $project 'Logs/K11-Video.log'
New-Item -ItemType Directory -Force -Path $fixture | Out-Null
$arguments = @('-arcane-k7-capture', '-arcane-k11', '-arcane-k11-video', '-capturePath', ('"' + $fixture + '"'), '-ffmpegPath', ('"' + $FFmpegPath + '"'), '-videoPath', ('"' + $video + '"'), '-force-d3d11', '-screen-fullscreen', '0', '-screen-width', '1920', '-screen-height', '1080', '-popupwindow', '-logFile', ('"' + $log + '"'))
$process = Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw ('Transition recording failed: ' + $process.ExitCode) }
& (Join-Path (Split-Path $FFmpegPath) 'ffprobe.exe') -v error -select_streams v:0 -show_entries stream=width,height,nb_frames,r_frame_rate -show_entries format=duration -of json $video
Get-Content -LiteralPath ($video + '.txt')
Get-Content -LiteralPath (Join-Path $fixture 'CaptureNotes.txt')
