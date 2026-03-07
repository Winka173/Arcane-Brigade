"""Summarise production bot runs without treating timeouts as wins."""
import csv
import json
import math
import statistics
import sys
from collections import defaultdict
from pathlib import Path


def read(path):
    return list(csv.DictReader(path.open(encoding="utf-8-sig"))) if path.exists() else []


def median(values):
    return round(statistics.median(values), 3) if values else None


def summarize(rows):
    return {
        "n": len(rows),
        "wins": sum(r["outcome"] == "VICTORY" for r in rows),
        "deaths": sum(r["outcome"] == "DEFEAT" for r in rows),
        "unresolved": sum(r["outcome"] not in ("VICTORY", "DEFEAT") for r in rows),
        "median_last_wave": median([int(r["last_wave"]) for r in rows]),
        "median_kills": median([int(r["kills"]) for r in rows]),
        "median_allies": median([int(r["allies"]) for r in rows]),
        "median_items": median([int(r["items"]) for r in rows]),
        "median_recruit_spent": median([int(r["recruit_spent"]) for r in rows]),
        "median_item_spent": median([int(r["item_spent"]) for r in rows]),
        "median_reroll_spent": median([int(r["reroll_spent"]) for r in rows]),
    }


def main():
    root = Path(sys.argv[1])
    supplements = [Path(arg) for arg in sys.argv[3:] if not arg.startswith("--")] or [root]
    supplement = supplements[0]
    datasets = {}
    tables = {}
    report = ["# Build balance observations", "", "Bot samples; timeouts are unresolved. Natural spending and injected laboratory fixtures are separate. Each build has its own shop RNG path after purchases. These are not human win rates.", ""]
    for name in ("Pilot", "Natural", "Archetypes", "Focused", "FocusedSix"):
        source = supplements[-1] if name == "FocusedSix" else supplement if name == "Focused" else root
        rows = read(source / f"{name}.csv")
        overrides = []
        for extra in (supplements[1:] if name == "Focused" else supplements):
            if extra != root and name in ("Natural", "Archetypes", "Focused"):
                filename = "Collision.csv" if name == "Archetypes" else f"{name}.csv"
                overrides.extend(dict(r, source_pass=extra.name) for r in read(extra / filename))
        if overrides:
            merged = {r["id"]: dict(r, source_pass=(supplement if name == "Focused" else root).name) for r in rows}
            for r in overrides:
                if r["id"] in merged:
                    merged[r["id"]] = r
            rows = list(merged.values())
            with (root / f"Reviewed_{name}.csv").open("w", newline="", encoding="utf-8") as stream:
                writer = csv.DictWriter(stream, fieldnames=list(rows[0]))
                writer.writeheader()
                writer.writerows(rows)
        datasets[name] = rows
        if not rows:
            continue
        groups = defaultdict(list)
        hero_groups = defaultdict(list)
        for row in rows:
            groups[row["build"], row["difficulty"]].append(row)
            hero_groups[row["hero"], row["build"]].append(row)
        summary = {f"{b}/difficulty{d}": summarize(g) for (b, d), g in sorted(groups.items())}
        tables[name] = {"overall": summarize(rows), "groups": summary, "heroes": {f"{h}/{b}": summarize(g) for (h, b), g in sorted(hero_groups.items())}}
        report += [f"## {name}", "", "| Build / difficulty | Runs | Wins | Deaths | Unresolved | Median final wave | Allies / items | Gold recruits / items / rerolls |", "|---|---:|---:|---:|---:|---:|---|---|"]
        for key, s in summary.items():
            report.append(f"| {key} | {s['n']} | {s['wins']} | {s['deaths']} | {s['unresolved']} | {s['median_last_wave']} | {s['median_allies']} / {s['median_items']} | {s['median_recruit_spent']} / {s['median_item_spent']} / {s['median_reroll_spent']} |")
        report.append("")
        pairs = defaultdict(dict)
        for row in rows:
            pairs[row["hero"], row["approach"], row["difficulty"], row["seed"]][row["build"]] = row
        paired = []
        for key, pair in pairs.items():
            if "Buff" not in pair or "Recruit" not in pair:
                continue
            buff, recruit = pair["Buff"], pair["Recruit"]
            paired.append({"hero": key[0], "approach": key[1], "difficulty": key[2], "seed": key[3], "buff_wave": int(buff["last_wave"]), "recruit_wave": int(recruit["last_wave"]), "buff_outcome": buff["outcome"], "recruit_outcome": recruit["outcome"]})
        tables[name]["paired"] = paired
        tables[name]["buff_minus_recruit_wave_median"] = median([p["buff_wave"] - p["recruit_wave"] for p in paired])
    for name in ("Companions", "Weapons"):
        rows = read(root / f"{name}.csv")
        if not rows:
            continue
        key = "companion" if name == "Companions" else "weapon"
        zero = [r for r in rows if float(r["damage"]) == 0]
        tables[name] = {"samples": len(rows), "identities": len({r[key] for r in rows}), "zero_damage_samples": zero, "median_dps": median([float(r["damage"]) / max(.05, float(r["seconds"])) for r in rows])}
        report += [f"## {name}", "", f"{len(rows)} samples, {tables[name]['identities']} identities. Zero direct-damage samples: {len(zero)}; inspect support modifiers/healing/coins before treating them as failures.", ""]
    bosses = read(root / "Bosses.csv")
    if bosses:
        tables["Bosses"] = {"samples": len(bosses), "wins": sum(r["result"] == "Victory" and float(r["boss_left"]) <= 0 for r in bosses), "withdrawn": sum(r["result"] == "Victory" and float(r["boss_left"]) > 0 for r in bosses), "unresolved": sum(r["result"] == "Combat" for r in bosses)}
    optional = read(root / "Optional_Bosses.csv")
    if supplement != root:
        merged_optional = {(r["id"], r["boss"]): dict(r, source_pass=root.name) for r in optional}
        for extra in supplements:
            for r in read(extra / "Optional_Bosses.csv"):
                merged_optional[r["id"], r["boss"]] = dict(r, source_pass=extra.name)
        optional = list(merged_optional.values())
        if optional:
            with (root / "Reviewed_Optional_Bosses.csv").open("w", newline="", encoding="utf-8") as stream:
                writer = csv.DictWriter(stream, fieldnames=list(optional[0]))
                writer.writeheader()
                writer.writerows(optional)
    if optional:
        def boss_counts(rows):
            return {"samples": len(rows), "kills": sum(r["boss_defeated"] == "True" for r in rows), "withdrawn": sum(r["withdrawn"] == "True" for r in rows), "unresolved": sum(r["result"] == "Combat" for r in rows)}
        tables["OptionalBosses"] = dict(boss_counts(optional), bosses={boss: boss_counts([r for r in optional if r["boss"] == boss]) for boss in sorted({r["boss"] for r in optional})})
    if len(sys.argv) > 2:
        baseline = read(Path(sys.argv[2]) / "Pilot.csv")
        current = {r["id"]: r for r in datasets.get("Natural", [])}
        pairs = [(b, current[b["id"]]) for b in baseline if b["id"] in current]
        comparison = {"matched_cases": len(pairs), "builds": {}}
        for build in ("Buff", "Recruit", "Hybrid"):
            group = [(b, a) for b, a in pairs if b["build"] == build]
            comparison["builds"][build] = {"before": summarize([b for b, _ in group]), "after": summarize([a for _, a in group])}
        (root / "Comparison.json").write_text(json.dumps(comparison, indent=2), encoding="utf-8")
        tables["MatchedComparison"] = comparison
    focused = datasets.get("Focused", [])
    if focused:
        def paired_key(r):
            return r["hero"], r["approach"], r["difficulty"], r["seed"]
        recruit = {paired_key(r): r for r in datasets.get("Natural", []) if r["build"] == "Recruit"}
        pairs = [(r, recruit[paired_key(r)]) for r in focused if paired_key(r) in recruit]
        by_hero = defaultdict(list)
        for buff, army in pairs:
            by_hero[buff["hero"]].append(int(buff["outcome"] == "VICTORY") - int(army["outcome"] == "VICTORY"))
        difficulties = sorted({b["difficulty"] for b, _ in pairs}, key=int)
        tables["FocusedVsRecruit"] = {"matched_cases": len(pairs), "focused": summarize([b for b, _ in pairs]), "recruit": summarize([a for _, a in pairs]), "median_paired_final_wave_delta": median([int(b["last_wave"]) - int(a["last_wave"]) for b, a in pairs]), "hero_mean_win_delta": {h: round(statistics.mean(v), 3) for h, v in by_hero.items()}, "difficulties": {d: {"focused": summarize([b for b, _ in pairs if b["difficulty"] == d]), "recruit": summarize([a for _, a in pairs if a["difficulty"] == d])} for d in difficulties}}
        (root / "Focused_Comparison.json").write_text(json.dumps(tables["FocusedVsRecruit"], indent=2), encoding="utf-8")
        report += ["## Matched buff focus versus recruitment", "", "Same hero, approach, difficulty and seed. FocusedBuff favours items with a small crew; Buff above never purchases a companion. Representative heroes have more samples, so inspect difficulty and hero rows as well as totals.", "", "| Difficulty | Matched runs | Focused wins | Recruit wins | Median final wave: focused / recruit |", "|---|---:|---:|---:|---|"]
        for d, values in tables["FocusedVsRecruit"]["difficulties"].items():
            b, a = values["focused"], values["recruit"]
            report.append(f"| {d} | {b['n']} | {b['wins']} | {a['wins']} | {b['median_last_wave']} / {a['median_last_wave']} |")
        report += ["", "## Hero build coverage", "", "Counts aggregate the sampled difficulties; they are not human success probabilities. See CSV for each seed and difficulty.", "", "| Hero | Items only wins / samples | Buff focus, four bodies | Buff focus, six bodies | Recruit wins / samples | Hybrid wins / samples |", "|---|---|---|---|---|---|"]
        natural = datasets.get("Natural", [])
        for hero in sorted({r["hero"] for r in natural}):
            cells = []
            for build in ("Buff", "FocusedBuff", "FocusedBuff6", "Recruit", "Hybrid"):
                build_rows = datasets.get("FocusedSix", []) if build == "FocusedBuff6" else focused if build == "FocusedBuff" else natural
                selected = [r for r in build_rows if r["hero"] == hero and r["build"] == build]
                cells.append(f"{sum(r['outcome'] == 'VICTORY' for r in selected)} / {len(selected)}")
            report.append("| " + " | ".join([hero] + cells) + " |")
    six = datasets.get("FocusedSix", [])
    if six:
        recruit_six = {(r["hero"], r["approach"], r["difficulty"], r["seed"]): r for r in datasets["Natural"] if r["build"] == "Recruit"}
        pairs_six = [(b, recruit_six[(b["hero"], b["approach"], b["difficulty"], b["seed"])]) for b in six]
        difficulties_six = sorted({r["difficulty"] for r in six}, key=int)
        tables["FocusedSixVsRecruit"] = {"matched_cases": len(pairs_six), "focused": summarize([b for b, _ in pairs_six]), "recruit": summarize([a for _, a in pairs_six]), "median_paired_final_wave_delta": median([int(b["last_wave"]) - int(a["last_wave"]) for b, a in pairs_six]), "difficulties": {d: {"focused": summarize([b for b, _ in pairs_six if b["difficulty"] == d]), "recruit": summarize([a for _, a in pairs_six if a["difficulty"] == d])} for d in difficulties_six}}
        (root / "FocusedSix_Comparison.json").write_text(json.dumps(tables["FocusedSixVsRecruit"], indent=2), encoding="utf-8")
        report += ["", "## Matched buff focus with six bodies", "", "Separate policy variant, same item preference, seeds and movement, with six voluntary bodies instead of four. Does not change gameplay rules. Four-body observations above are retained; no per-case choice of the better policy.", "", "| Difficulty | Matched runs | Focused-six wins | Recruit wins | Median final wave: focused / recruit |", "|---|---:|---:|---:|---|"]
        for d, values in tables["FocusedSixVsRecruit"]["difficulties"].items():
            b, a = values["focused"], values["recruit"]
            report.append(f"| {d} | {b['n']} | {b['wins']} | {a['wins']} | {b['median_last_wave']} / {a['median_last_wave']} |")
    (root / "Summary.json").write_text(json.dumps(tables, ensure_ascii=False, indent=2), encoding="utf-8")
    (root / "Summary.md").write_text("\n".join(report), encoding="utf-8")
    if "--verify" in sys.argv:
        checks = []
        for completed_pass in {root, *supplements}:
            assert (completed_pass / "Timing.txt").exists(), f"Runner has not finished successfully: {completed_pass}"
        checks.append("All supplied runner passes reached their successful completion marker")
        expected = {"Natural": 294, "Archetypes": 108, "Focused": 98, "FocusedSix": 98}
        for name, count in expected.items():
            rows = datasets.get(name, [])
            assert len(rows) == count and len({r["id"] for r in rows}) == count, f"Incomplete or duplicate {name}"
            assert all(r["outcome"] in ("VICTORY", "DEFEAT", "TIMEOUT") for r in rows), f"Blocked shop in {name}"
            for r in rows:
                assert all(math.isfinite(float(r[k])) for k in ("last_wave", "seconds", "hero_damage", "ally_damage")), f"Nonfinite {name}/{r['id']}"
            checks.append(f"{name}: {count} unique completed samples; timeouts retained separately")
        for name, count, identity, identities in (("Companions", 396, "companion", 66), ("Weapons", 588, "weapon", 98)):
            rows = read(root / f"{name}.csv")
            assert len(rows) == count and len({(r[identity], r["tier"], r["wave"]) for r in rows}) == count, f"Incomplete or duplicate {name}"
            assert len({r[identity] for r in rows}) == identities, f"Missing {name} identities"
            assert all(math.isfinite(float(r["damage"])) and float(r["damage"]) >= 0 for r in rows), f"Invalid {name} damage"
            checks.append(f"{name}: {identities} identities across three tiers and two waves")
        assert len(bosses) == 504, "Incomplete boss fixtures"
        boss_groups = defaultdict(list)
        for r in bosses:
            boss_groups[r["hero"], r["build"], r["difficulty"]].append(r)
        assert len(boss_groups) == 54, "Missing boss hero/build/difficulty group"
        for (hero, build, difficulty), rows in boss_groups.items():
            expected_count = 16 if hero in ("LanternKnight", "Postmaster", "LoneJelly") and difficulty != "2" else 6
            assert len(rows) == expected_count, f"Incomplete boss group {hero}/{build}/{difficulty}"
        # A story boss can also be present in the all-bodies sweep at the same wave.
        # Such repeated boss IDs are intentional, not duplicate campaign records.
        checks.append("Bosses: 504 fixtures; withdrawal is not counted as a kill")
        assert len(datasets["Natural"]) // 3 == len(pairs) == 98, "Missing matched focused/recruit pair"
        checks.append("Focused comparison: 98 matched hero/approach/difficulty/seed pairs")
        assert len(pairs_six) == 98, "Missing focused-six matched pair"
        checks.append("Focused-six comparison: 98 independently sampled matched pairs; four-body observations retained")
        hmb_path = root.parent.parent / "HeroMechanicBalance" / "BuildBalanceFinal" / "Runs.tsv"
        with hmb_path.open(encoding="utf-8-sig") as stream:
            hmb = list(csv.DictReader(stream, delimiter="\t"))
        assert len(hmb) == 558 and len({(r["hero"], r["approach"], r["wave"], r["seed"]) for r in hmb}) == 558, "Incomplete hero approach fixtures"
        assert len({(r["hero"], r["approach"]) for r in hmb}) == 93, "Missing hero approach"
        checks.append("Hero identities: 93 approaches, 558 on rows with 186 cached off counterfactual fights")
        (root / "Data_Verification.json").write_text(json.dumps({"status": "PASS", "checks": checks}, indent=2), encoding="utf-8")
    print(json.dumps({k: v.get("overall", {x: y for x, y in v.items() if not isinstance(y, list)}) for k, v in tables.items()}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
