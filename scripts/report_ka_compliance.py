#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Compare each X12_005010 map with KA Software schema.
Output: brief report "corresponds" / "does not correspond" per transaction.
Uses same cache as update_mandatory_loops.py.
"""

import re
from pathlib import Path
from typing import Optional

import urllib.request
import urllib.error

MAPS_DIR = Path(__file__).resolve().parent.parent / "EdiStandards" / "X12_005010" / "Maps"
CACHE_DIR = Path(__file__).resolve().parent / "ka_cache"


def get_all_transaction_numbers() -> list[int]:
    nums = []
    for p in MAPS_DIR.glob("M_*.cs"):
        try:
            nums.append(int(p.stem[2:]))
        except ValueError:
            continue
    return sorted(nums)


def fetch_ka_page(num: int, cache_dir: Optional[Path] = None, fetch: bool = True) -> Optional[str]:
    if cache_dir:
        cache_file = cache_dir / f"{num}.html"
        if cache_file.exists():
            return cache_file.read_text(encoding="utf-8", errors="replace")
    if not fetch:
        return None
    try:
        import time
        time.sleep(0.85)
        url = f"https://www.kasoftware.com/schema/edi/x12/00501/messages/{num}/"
        req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(req, timeout=30) as r:
            return r.read().decode("utf-8", errors="replace")
    except urllib.error.HTTPError as e:
        if e.code == 404:
            return None
        raise
    except Exception:
        return None


def parse_mandatory_segments(html: str) -> list[str]:
    seg_pattern = re.compile(r"\[([A-Z0-9]+)\]\(https://")
    mandatory = []
    pos = 0
    while True:
        m = seg_pattern.search(html, pos)
        if not m:
            break
        seg_id = m.group(1)
        chunk = html[m.start() : m.start() + 400]
        if "M必须(Mandatory)" in chunk or "M必须 (Mandatory)" in chunk:
            mandatory.append(seg_id)
        pos = m.end()
    return mandatory


def get_loop_first_segment(map_path: Path) -> dict[str, str]:
    text = map_path.read_text(encoding="utf-8-sig")
    result = {}
    for m in re.finditer(r"public class (L_[A-Za-z0-9_]+)\s*:\s*MapLoop", text):
        class_name = m.group(1)
        start = m.end()
        content_match = re.search(r"Content\.AddRange\s*\([^)]+\)\s*\{\s*", text[start : start + 2000])
        if not content_match:
            continue
        content_start = start + content_match.end()
        first_new = re.search(r"new\s+([A-Za-z0-9_]+)\s*\(", text[content_start : content_start + 300])
        if first_new:
            seg = first_new.group(1)
            result[class_name] = seg
    for k, v in list(result.items()):
        while v.startswith("L_") and v in result:
            v = result[v]
        result[k] = v
    return result


def get_mandatory_segment_ids_in_map(map_path: Path) -> set[str]:
    text = map_path.read_text(encoding="utf-8-sig")
    loop_first = get_loop_first_segment(map_path)
    mandatory = set()
    for m in re.finditer(r"new\s+([A-Za-z0-9_]+)\s*\((?:this\s*)?\)\s*\{\s*ReqDes\s*=\s*RequirementDesignator\.Mandatory", text):
        name = m.group(1)
        if name.startswith("L_"):
            seg = loop_first.get(name)
            if seg and not seg.startswith("L_"):
                mandatory.add(seg)
        else:
            mandatory.add(name)
    return mandatory


def main():
    import sys
    fetch = "--cache-only" not in sys.argv
    nums = get_all_transaction_numbers()
    if "--limit" in sys.argv:
        try:
            i = sys.argv.index("--limit")
            limit = int(sys.argv[i + 1])
            nums = nums[:limit]
        except (ValueError, IndexError):
            pass
    corresponds = []
    not_corresponds = []
    no_ka = []

    for num in nums:
        map_path = MAPS_DIR / f"M_{num}.cs"
        if not map_path.exists():
            continue
        try:
            html = fetch_ka_page(num, cache_dir=CACHE_DIR, fetch=fetch)
        except Exception:
            no_ka.append(num)
            continue
        if html is None:
            no_ka.append(num)
            continue
        ka_mandatory = parse_mandatory_segments(html)
        if not ka_mandatory:
            corresponds.append(num)
            continue
        map_mandatory = get_mandatory_segment_ids_in_map(map_path)
        ka_unique = set(ka_mandatory)
        missing = ka_unique - map_mandatory
        if missing:
            not_corresponds.append((num, sorted(missing)))
        else:
            corresponds.append(num)

    print("=== Соответствует KA ===")
    for n in corresponds:
        print(f"  M_{n}: соответствует")
    print(f"\nВсего: {len(corresponds)}")

    print("\n=== Не соответствует KA (отсутствует Mandatory) ===")
    for n, missing in not_corresponds:
        print(f"  M_{n}: не соответствует (нет M: {', '.join(missing)})")
    print(f"\nВсего: {len(not_corresponds)}")

    print("\n=== Нет данных KA (404 или ошибка) ===")
    for n in no_ka:
        print(f"  M_{n}: нет данных KA")
    print(f"\nВсего: {len(no_ka)}")

    print(f"\nИтого мап: {len(corresponds) + len(not_corresponds) + len(no_ka)}")


if __name__ == "__main__":
    main()
