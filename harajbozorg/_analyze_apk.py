import re
import os
from pathlib import Path

ROOT = Path(r"e:\projects\nopCommerce_4.90.3_Source\harajbozorg\_apk_extracted")

def extract_strings(data: bytes, min_len=4):
    return re.findall(rb"[\x20-\x7e\u0600-\u06ff]{%d,200}" % min_len, data)

def analyze_file(path: Path):
    data = path.read_bytes()
    text = data.decode("utf-8", errors="ignore")
    results = {
        "urls": sorted(set(re.findall(r"https?://[a-zA-Z0-9._-]+\.(?:com|ir|net|org)[^\s\"'<>\\]*", text))),
        "paths": sorted(set(re.findall(r"/api/[a-zA-Z0-9_./-]+", text))),
        "domains": sorted(set(re.findall(r"[a-zA-Z0-9.-]*harajbozorg[a-zA-Z0-9./_-]*", text, re.I))),
        "forosh": sorted(set(re.findall(r"[a-zA-Z0-9.-]*foroshgostar[a-zA-Z0-9./_-]*", text, re.I))),
    }
    return results

# Main bundle + dex
targets = list((ROOT / "assets" / "public").glob("*.js")) + [
    ROOT / "classes.dex",
    ROOT / "classes3.dex",
]

all_urls, all_paths, all_domains = set(), set(), set()
for t in targets:
    if not t.exists():
        continue
    r = analyze_file(t)
    if any(r.values()):
        name = t.name
        if r["urls"] or r["paths"] or r["domains"]:
            print(f"\n### {name}")
            if r["domains"]:
                print("domains:", r["domains"][:20])
            if r["forosh"]:
                print("forosh:", r["forosh"][:20])
            if r["urls"]:
                print("urls:", [u for u in r["urls"] if "google" not in u and "github" not in u and "mozilla" not in u][:30])
            if r["paths"]:
                print("api paths:", r["paths"][:40])
        all_urls.update(r["urls"])
        all_paths.update(r["paths"])
        all_domains.update(r["domains"])

print("\n\n=== AGGREGATE (filtered) ===")
for u in sorted(all_urls):
    if any(x in u for x in ("haraj", "forosh", "bozorg")) or u.endswith(".ir/") or ".ir/" in u:
        print("URL", u)
for p in sorted(all_paths):
    print("PATH", p)
for d in sorted(all_domains):
    print("DOMAIN", d)

# Feature keywords in all JS
keywords = ["chatbot", "voice", "speech", "microphone", "camera", "duplicate", "backup", "restore", "otp", "sms", "2fa", "search-by-image", "visual"]
print("\n=== FEATURE KEYWORD FILES ===")
for js in (ROOT / "assets" / "public").glob("*.js"):
    text = js.read_text(encoding="utf-8", errors="ignore").lower()
    hits = [k for k in keywords if k in text]
    if hits:
        print(js.name, hits)

# embedded JSON configs
import json
main = (ROOT / "assets" / "public" / "main.2adb5120bf211a83.js").read_text(encoding="utf-8", errors="ignore")
print("\n=== EMBEDDED JSON CONFIG ===")
for m in re.finditer(r"JSON\.parse\('(\{.*?\})'\)", main):
    try:
        raw = m.group(1).encode("utf-8").decode("unicode_escape")
        obj = json.loads(raw)
        print(json.dumps(obj, ensure_ascii=False, indent=2))
    except Exception as e:
        print("parse err", e, m.group(1)[:300])
    print("---")

# Persian UI terms
terms = ["جستجو", "صوتی", "تصویری", "دوربین", "چت", "هوش", "پیامک", "احراز", "تکراری", "بازیابی", "پشتیبان"]
print("\n=== PERSIAN TERMS BY FILE ===")
for js in sorted((ROOT / "assets" / "public").glob("*.js")):
    t = js.read_text(encoding="utf-8", errors="ignore")
    found = [x for x in terms if x in t]
    if found:
        print(js.name, found)
