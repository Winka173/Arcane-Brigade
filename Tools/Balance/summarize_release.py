"""Summarize release evidence without treating laboratory DPS as campaign win rates."""
import argparse
import csv
import hashlib
import json
import math
import statistics
from collections import defaultdict
from pathlib import Path


def read(path):
    with path.open(encoding="utf-8-sig", newline="") as stream:
        return list(csv.DictReader(stream, delimiter="\t"))


def mean(values):
    return round(statistics.mean(values), 4) if values else None


def dps(paths):
    result = []
    for path in paths:
        rows = read(path / "Heroes.tsv")
        keys = [tuple(r[k] for k in ("hero", "approach", "wave", "targets", "team", "mode")) for r in rows]
        if len(keys) != len(set(keys)):
            raise ValueError(f"Duplicate DPS fixture: {path}")
        heroes = sorted({r["hero"] for r in rows})
        counts = {hero: sum(r["hero"] == hero for r in rows) for hero in heroes}
        if len(heroes) != 36 or any(count != 72 for count in counts.values()):
            raise ValueError(f"Incomplete 36 x 72 DPS matrix: {path}: {counts}")
        for row in rows:
            for key in ("heroDps", "teamDps", "seconds"):
                if not math.isfinite(float(row[key])) or float(row[key]) < 0:
                    raise ValueError(f"Invalid DPS: {row}")
        bands = []
        for targets in (1, 8):
            for wave in (10, 20):
                values = {}
                for hero in heroes:
                    selected = [r for r in rows if r["hero"] == hero and r["team"] == "True"
                                and int(r["targets"]) == targets and int(r["wave"]) == wave]
                    # Equal weight for 3 approaches x 3 movement/pressure conditions.
                    values[hero] = mean([float(r["teamDps"]) for r in selected])
                low = min(values, key=values.get)
                high = max(values, key=values.get)
                ratio = values[high] / values[low] if values[low] else None
                bands.append(dict(targets=targets, wave=wave, values=values, lowest=low, highest=high,
                                  maxMinRatio=round(ratio, 4) if ratio else None,
                                  withinThirtyPercent=ratio is not None and ratio <= 1.3))
        result.append(dict(path=path.as_posix(), samples=len(rows), bands=bands,
                           interpretation="Controlled offence only. Total team includes hero; SoloMaw has no brigade. "
                                          "No survival/win-rate claim. Support bonus columns must not be added again."))
    return result


def campaigns(root):
    compiled = read(root / "Compiled.tsv")
    compiled_keys = [(r["hero"], r["difficulty"], r["wave"]) for r in compiled]
    difficulty_file = root / "Difficulties.tsv"
    difficulty_names = read(difficulty_file) if difficulty_file.exists() else [dict(index=str(d), name=f"Historical index {d}") for d in range(8)]
    difficulty_count = len(difficulty_names)
    expected_compiled = 36 * difficulty_count * 15
    if len(compiled_keys) != expected_compiled or len(set(compiled_keys)) != expected_compiled:
        raise ValueError(f"Incomplete compiled matrix: expected 36 heroes x {difficulty_count} difficulties x 15 waves")
    for row in compiled:
        for key in row.keys() - {"hero", "boss"}:
            if not math.isfinite(float(row[key])) or float(row[key]) < 0:
                raise ValueError(f"Nonfinite or negative compiled stat: {row}")
        for key in ("heroHp", "heroCooldown", "enemyHp", "enemySpeed", "spawnInterval", "priceScale"):
            if float(row[key]) <= 0:
                raise ValueError(f"Invalid positive compiled stat {key}: {row}")
        wave = int(row["wave"])
        if float(row["duration"]) == 0 and not (wave == 20 and row["boss"] == "TheLastCustomer"):
            raise ValueError(f"Unexpected untimed encounter: {row}")
        if wave <= 20 and bool(row["boss"]) != (wave in (4, 8, 12, 16, 20)):
            raise ValueError(f"Unexpected story boss schedule: {row}")
    ladders = defaultdict(list)
    for row in compiled:
        ladders[(row["hero"], row["wave"])].append(row)
    for key, ladder in ladders.items():
        ladder.sort(key=lambda r: int(r["difficulty"]))
        if [int(r["difficulty"]) for r in ladder] != list(range(difficulty_count)):
            raise ValueError(f"Incomplete difficulty ladder: {key}")
        for low, high in zip(ladder, ladder[1:]):
            if any(float(high[stat]) <= float(low[stat]) for stat in ("enemyHp", "enemyHit")):
                raise ValueError(f"Non-increasing compiled difficulty HP/damage: {key}")
    runs = read(root / "Campaigns.tsv")
    waves = read(root / "Waves.tsv")
    by_key = {row["key"]: row for row in runs}
    if len(by_key) != len(runs):
        raise ValueError("Duplicate completed campaign keys")
    plan_file = root / "CampaignPlan.tsv"
    planned = {r["key"] for r in read(plan_file)} if plan_file.exists() else set(by_key)
    if not set(by_key).issubset(planned):
        raise ValueError("Campaign outside declared matrix")
    selected = [row for row in waves if row["key"] in by_key]
    wave_keys = [(row["key"], row["wave"]) for row in selected]
    if len(wave_keys) != len(set(wave_keys)):
        raise ValueError("Duplicate wave rows; interrupted campaigns need a fresh label")
    groups = defaultdict(list)
    for row in runs:
        groups[(row["difficulty"], row["policy"])].append(row)
    outcomes = []
    for (difficulty, policy), group in sorted(groups.items()):
        outcomes.append(dict(difficulty=int(difficulty), policy=policy, runs=len(group),
                             difficultyName=difficulty_names[int(difficulty)]["name"],
                             metaLevels=sorted({int(r.get("metaLevel", 0)) for r in group}),
                             wins=sum(r["result"] == "Victory" for r in group),
                             defeats=sum(r["result"] == "Defeat" for r in group),
                             unresolved=sum(r["result"] not in ("Victory", "Defeat") for r in group),
                             meanTerminalWave=mean([int(r["wave"]) for r in group])))
    economy = []
    for meta in sorted({int(r.get("metaLevel", 0)) for r in selected}):
        for first, last in ((1, 3), (4, 7), (8, 11), (12, 15), (16, 19)):
            group = [r for r in selected if first <= int(r["wave"]) <= last and r["result"] == "Victory"
                     and int(r.get("metaLevel", 0)) == meta]
            ratios = [float(r["wallet"]) / float(r["meanOffer"]) for r in group if float(r["meanOffer"]) > 0]
            economy.append(dict(waves=f"{first}-{last}", metaLevel=meta, samples=len(group), walletPerMeanOffer=mean(ratios),
                                medianWalletPerMeanOffer=round(statistics.median(ratios), 4) if ratios else None,
                                meanBuys=mean([int(r["buys"]) for r in group]),
                                meanRerollSpent=mean([int(r["rerollSpent"]) for r in group])))
    bosses = []
    for wave in (4, 8, 12, 16, 20):
        group = [r for r in selected if int(r["wave"]) == wave and r["boss"]]
        kills = [r for r in group if float(r["bossLeft"]) <= 0]
        bosses.append(dict(wave=wave, samples=len(group), kills=len(kills),
                           meanEncounterSecondsOnKill=mean([float(r["seconds"]) for r in kills]),
                           meanBossFellAt=mean([float(r["bossFellAt"]) for r in kills if "bossFellAt" in r and float(r["bossFellAt"]) >= 0]),
                           timeouts=sum(r["result"] == "Combat" for r in group)))
    checks = (root / "Checks.tsv").read_text(encoding="utf-8").splitlines()
    matched = defaultdict(dict)
    for row in runs:
        matched[(row["hero"], row["seed"], row["policy"], row.get("metaLevel", "0"))][int(row["difficulty"])] = row
    inversions = []
    for (hero, seed, policy, meta), levels in matched.items():
        for lower in range(difficulty_count - 1):
            if lower not in levels or lower + 1 not in levels:
                continue
            low, high = levels[lower], levels[lower + 1]
            if high["result"] == "Victory" and low["result"] != "Victory":
                inversions.append(dict(hero=hero, seed=int(seed), policy=policy, metaLevel=int(meta),
                    lowerDifficulty=lower, lowerResult=low["result"], lowerWave=int(low["wave"]),
                    higherDifficulty=lower + 1, higherResult=high["result"]))
    bosses_by_difficulty = []
    for d in range(difficulty_count):
        for wave in (4, 8, 12, 16, 20):
            group = [r for r in selected if int(r["wave"]) == wave and r["boss"] and int(by_key[r["key"]]["difficulty"]) == d]
            kills = [r for r in group if float(r["bossLeft"]) <= 0]
            bosses_by_difficulty.append(dict(difficulty=d, wave=wave, samples=len(group), kills=len(kills),
                meanBossFellAt=mean([float(r["bossFellAt"]) for r in kills if "bossFellAt" in r and float(r["bossFellAt"]) >= 0]),
                timeouts=sum(r["result"] == "Combat" for r in group)))
    return dict(compiledSamples=len(compiled), compiledDifficultyLaddersPass=True, difficulties=difficulty_names,
                plannedRuns=len(planned), campaignMatrixComplete=set(by_key) == planned,
                completedRuns=len(runs), measuredWaves=len(selected), ignoredPartialWaves=len(waves)-len(selected),
                outcomes=outcomes, economy=economy, bosses=bosses, bossesByDifficulty=bosses_by_difficulty,
                matchedDifficultyInversions=inversions, checks=checks,
                releaseChecksPass=bool(checks) and all("\tPASS" in line for line in checks),
                caveats=["Bot survival changes the sampled population at late waves.",
                         "Read Method.txt and metaLevel: tree-gated campaigns may use a full owned-profile fixture; they are not fresh-profile outcomes.",
                         "Optional event deals are declined; do not infer event balance.",
                         "TIMEOUT and BLOCKED_SHOP remain unresolved, never counted as defeats or wins."])


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--dps", nargs="+", type=Path, required=True)
    parser.add_argument("--campaign", type=Path)
    parser.add_argument("--additional-campaign", nargs="*", type=Path, default=[])
    parser.add_argument("--compare-compiled", type=Path)
    parser.add_argument("--out", type=Path, required=True)
    parser.add_argument("--require-complete", action="store_true")
    args = parser.parse_args()
    summary = dict(damageGate="max/min <= 1.30 within matched target count, wave and investment; "
                              "average approaches and movement modes equally", dps=dps(args.dps))
    if args.campaign:
        summary["campaigns"] = campaigns(args.campaign)
        if args.require_complete and not summary["campaigns"]["campaignMatrixComplete"]:
            raise ValueError("Campaign matrix still incomplete")
    if args.additional_campaign:
        summary["additionalCampaigns"] = [campaigns(folder) for folder in args.additional_campaign]
        if args.require_complete and any(not item["campaignMatrixComplete"] for item in summary["additionalCampaigns"]):
            raise ValueError("Additional campaign matrix still incomplete")
    if args.compare_compiled:
        if not args.campaign:
            raise ValueError("--compare-compiled requires --campaign")
        left = args.campaign / "Compiled.tsv"
        right = args.compare_compiled / "Compiled.tsv"
        identical = left.read_bytes() == right.read_bytes()
        summary["compiledComparison"] = dict(before=left.as_posix(), after=right.as_posix(),
                                              identical=identical, samples=len(read(left)))
        if not identical:
            raise ValueError("Compiled gameplay stats changed across the migration fix")
    summary["damageGatePass"] = all(b["withinThirtyPercent"] for matrix in summary["dps"] for b in matrix["bands"])
    summary["inputSha256"] = {p.as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
                              for folder in args.dps + ([args.campaign] if args.campaign else []) + args.additional_campaign
                              for p in sorted(folder.rglob("*")) if p.is_file() and p.suffix in (".tsv", ".txt", ".json")}
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"damageGatePass": summary["damageGatePass"],
                      "DpsRatios": [[b["maxMinRatio"] for b in m["bands"]] for m in summary["dps"]],
                      "campaigns": summary.get("campaigns", {}).get("completedRuns")}, indent=2))


if __name__ == "__main__":
    main()
