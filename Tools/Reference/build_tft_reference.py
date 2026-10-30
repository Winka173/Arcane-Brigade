import json, re, html, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = r"C:\Users\Winka\OneDrive\Documents\Arcane-Brigade\Docs\Reference\TFT_All_Sets_Traits_And_Abilities.md"

# set number -> (title, source file, note)
SETS = [
    (1, "Set 1: Beta (original)", "p_9.20.json", "patch 9.20 data"),
    (2, "Set 2: Rise of the Elements (data name: War of the Elements)", "p_10.5.json", "patch 10.5 data"),
    (3, "Set 3: Galaxies", "tft_en.json", "current client data (Galaxies revival)"),
    (4, "Set 4: Fates", "p_15.5.json", "patch 15.5 data (Fates revival)"),
    (5, "Set 5: Reckoning", "p_14.24.json", "patch 14.24 data (Reckoning revival)"),
    (6, "Set 6: Gizmos & Gadgets", "p_12.10.json", "patch 12.10 data"),
    (7, "Set 7: Dragonlands", "tft_en.json", "current client data"),
    (8, "Set 8: Monsters Attack!", "p_13.10.json", "patch 13.10 data"),
    (9, "Set 9: Runeterra Reforged", "p_14.10.json", "patch 14.10 data"),
    (10, "Set 10: Remix Rumble", "p_15.10.json", "patch 15.10 data"),
    (11, "Set 11: Inkborn Fables", "p_14.24.json", "patch 14.24 data"),
    (12, "Set 12: Magic n' Mayhem", "p_14.24.json", "patch 14.24 data"),
    (13, "Set 13: Into the Arcane", "tft_en.json", "current client data"),
    (14, "Set 14: Cyber City", "tft_en.json", "current client data"),
    (15, "Set 15: K.O. Coliseum", "tft_en.json", "current client data"),
    (16, "Set 16: Lore & Legends", "tft_en.json", "current client data"),
    (17, "Set 17: Space Gods", "tft_en.json", "current client data"),
    (18, "Set 18: Enchanted Wilds", "tft_en.json", "current client data"),
]
cache = {}


def load(f):
    if f not in cache:
        cache[f] = json.load(open(os.path.join(HERE, f), encoding="utf-8"))
    return cache[f]


def clean(text, variables=None):
    if not text:
        return ""
    vm = {}
    for v in variables or []:
        val = v.get("value")
        if isinstance(val, list) and val:
            nums = val[1:4] if len(val) >= 4 else val
            vm[(v.get("name") or "").lower()] = nums
    t = text.replace("<br>", " ").replace("<br/>", " ")
    t = re.sub(r"<(row|expandRow)>.*?</\1>", "", t, flags=re.S)

    def sub(m):
        expr = m.group(1)
        mult = 1.0
        if "*" in expr:
            name, _, k = expr.partition("*")
            try:
                mult = float(k)
            except ValueError:
                mult = 1.0
        else:
            name = expr
        nums = vm.get(name.strip().lower())
        if nums:
            vals = [round(x * mult, 2) for x in nums]
            vals = [int(x) if float(x).is_integer() else x for x in vals]
            return "/".join(str(x) for x in vals) if len(set(vals)) > 1 else str(vals[0])
        return "[" + name.strip() + "]"

    t = re.sub(r"@([^@]+)@", sub, t)
    t = re.sub(r"%i:[A-Za-z]+%", "", t)
    t = re.sub(r"<[^>]+>", "", t)
    t = html.unescape(t).replace("\xa0", " ")
    t = re.sub(r"\s+", " ", t).strip()
    return t


def fmt_val(v):
    if isinstance(v, float) and v.is_integer():
        return str(int(v))
    if isinstance(v, float):
        return str(round(v, 3))
    return str(v)


def trait_block(t):
    desc = clean(t.get("desc"))
    effs = t.get("effects") or []
    lines = []
    for e in effs:
        lo, hi = e.get("minUnits"), e.get("maxUnits")
        label = str(lo) if hi in (None, lo, 25000) or hi == lo else f"{lo}-{hi}"
        vs = e.get("variables") or {}
        parts = []
        for k, v in vs.items():
            if k.startswith("{"):
                parts.append(fmt_val(v))
            else:
                parts.append(f"{k}={fmt_val(v)}")
        lines.append(f"  - ({label}) " + (", ".join(parts) if parts else "(no numbers)"))
    return desc, lines


def main():
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    out = []
    out.append("# Teamfight Tactics: every set's traits and every champion's ability (reference)\n")
    out.append("Generated 27/10/2026 from CommunityDragon's raw game data (raw.communitydragon.org, the TFT `en_us.json` of several patches). "
               "Numbers are the values in that patch's data, not necessarily the set's launch values. Placeholder names in square brackets "
               "are computed values the data does not spell out; `{...}` hashed variables are shown as bare numbers (in the order the data lists them). "
               "Star levels in ability numbers read 1-star/2-star/3-star. This file is for design reference only.\n")
    out.append("## Contents\n")
    for n, title, f, note in SETS:
        out.append(f"- [{title}](#set-{n})")
    out.append("")
    total_c = total_t = 0
    for n, title, f, note in SETS:
        d = load(f)
        s = d["sets"][str(n)]
        champs = s.get("champions", [])
        traits = s.get("traits", [])
        out.append(f'\n<a id="set-{n}"></a>\n\n# {title}\n')
        out.append(f"Source: {note}. {len(champs)} champions, {len(traits)} traits.\n")
        out.append("## Traits\n")
        for t in sorted(traits, key=lambda x: (x.get("name") or "")):
            desc, lines = trait_block(t)
            out.append(f"### {t.get('name')}\n")
            out.append(desc + "\n" if desc else "")
            out.extend(lines)
            out.append("")
            total_t += 1
        def entry(c):
            ab = c.get("ability") or {}
            st = c.get("stats") or {}
            tr = ", ".join(c.get("traits") or []) or "(none)"
            mana = ""
            if st.get("mana"):
                mana = f" | mana {fmt_val(st.get('initialMana', 0))}/{fmt_val(st.get('mana'))}"
            role = c.get("role") or ""
            out.append(f"### {c.get('name')} ({c.get('cost')}-cost, {role})" + chr(10))
            out.append(f"- Traits: {tr}{mana}")
            out.append(f"- **{ab.get('name') or '(no ability)'}**: {clean(ab.get('desc'), ab.get('variables'))}" + chr(10))
        ordered = sorted(champs, key=lambda x: (x.get("cost") or 0, x.get("name") or ""))
        playable = [c for c in ordered if c.get("traits")]
        others = [c for c in ordered if not c.get("traits")]
        out.append(f"## Champions ({len(playable)})" + chr(10))
        for c in playable:
            entry(c)
            total_c += 1
        out.append(f"## Monsters, summons, pets and other trait-less units ({len(others)})" + chr(10))
        out.append("Units with no trait: neutral PvE monsters, summoned units, pets and similar. Their abilities show what non-player units do." + chr(10))
        for c in others:
            entry(c)
            total_c += 1

            total_c += 1
    out.append(f"\n---\nTotal: {total_c} champion entries and {total_t} traits across {len(SETS)} sets.\n")
    open(OUT, "w", encoding="utf-8", newline="\n").write("\n".join(out))
    print("written", OUT, os.path.getsize(OUT) // 1000, "KB", total_c, "champions", total_t, "traits")


if __name__ == "__main__":
    main()
