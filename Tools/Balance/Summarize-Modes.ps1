param([string]$Root = 'PreviewExports/ModeBalance', [string[]]$Labels = @('BeforeCampaign', 'Campaign'))
$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$destination = Join-Path $project $Root
$summary = @()
foreach ($label in $Labels) {
    if (-not (Test-Path -LiteralPath (Join-Path $destination ($label + '/Method.txt')))) { throw "Incomplete diagnostic pass: $label" }
    $rows = @(Import-Csv -LiteralPath (Join-Path $destination ($label + '/Runs.csv')))
    $runs = @($rows | Group-Object mode, hero, difficulty, seed | ForEach-Object {
        $terminal = $_.Group[-1]
        $goal = if ($terminal.mode -eq 'first-light') {
            @($_.Group | Where-Object { $_.wave -eq '10' -and $_.result -eq 'Victory' }).Count -gt 0
        } else { $terminal.wave -eq '20' -and $terminal.result -eq 'Victory' -and ($terminal.mode -ne 'small-brigade' -or [int]$terminal.allies -le 6) }
        [pscustomobject]@{ Mode = $terminal.mode; Difficulty = [int]$terminal.difficulty; Hero = $terminal.hero; Seed = [int]$terminal.seed; Wave = [int]$terminal.wave; Result = $terminal.result; ObjectiveCleared = $goal }
    })
    $groups = @($runs | Group-Object Mode, Difficulty | ForEach-Object {
        [pscustomobject]@{
            Mode = $_.Group[0].Mode
            Difficulty = $_.Group[0].Difficulty
            Runs = $_.Count
            CampaignWins = @($_.Group | Where-Object { $_.Wave -eq 20 -and $_.Result -eq 'Victory' }).Count
            ObjectiveClears = @($_.Group | Where-Object ObjectiveCleared).Count
            Defeats = @($_.Group | Where-Object Result -eq 'Defeat').Count
            Unresolved = @($_.Group | Where-Object Result -eq 'Combat').Count
            MeanTerminalWave = ($_.Group.Wave | Measure-Object -Average).Average
        }
    })
    $summary += [pscustomobject]@{ Label = $label; Runs = $runs.Count; WaveSamples = $rows.Count; Groups = $groups; TerminalRuns = $runs }
}
$summary | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $destination 'Comparison.json') -Encoding UTF8
$summary | ForEach-Object { $_.Label; $_.Groups | Format-Table -AutoSize }
