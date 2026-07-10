import re
import json
from pathlib import Path

main = Path(r"e:\projects\nopCommerce_4.90.3_Source\harajbozorg\_apk_extracted\assets\public\main.2adb5120bf211a83.js").read_text(encoding="utf-8", errors="ignore")
out = Path(r"e:\projects\nopCommerce_4.90.3_Source\harajbozorg\apk-analysis")

configs = []
for m in re.finditer(r"JSON\.parse\('(\{.*?\})'\)", main):
    raw = m.group(1).encode("utf-8").decode("unicode_escape")
    configs.append(json.loads(raw))

out.mkdir(exist_ok=True)
(out / "embedded-configs.json").write_text(json.dumps(configs, ensure_ascii=False, indent=2), encoding="utf-8")

# route-like paths in all js
pub = Path(r"e:\projects\nopCommerce_4.90.3_Source\harajbozorg\_apk_extracted\assets\public")
routes = set()
for js in pub.glob("*.js"):
    t = js.read_text(encoding="utf-8", errors="ignore")
    for m in re.finditer(r"path:\s*['\"]([a-zA-Z0-9_./-]+)['\"]", t):
        routes.add(m.group(1))
    for m in re.finditer(r"['\"](/[a-zA-Z0-9_./-]{3,50})['\"]", t):
        p = m.group(1)
        if any(k in p.lower() for k in ("search", "chat", "backup", "restore", "otp", "2fa", "visual", "image", "voice", "product", "vendor", "admin")):
            routes.add(p)

(out / "interesting-routes.txt").write_text("\n".join(sorted(routes)), encoding="utf-8")
print("configs", len(configs))
print("routes", len(routes))
for c in configs:
    print("keys:", list(c.keys())[:20])
