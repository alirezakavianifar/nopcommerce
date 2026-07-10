import re
from pathlib import Path

pub = Path(r"e:\projects\nopCommerce_4.90.3_Source\harajbozorg\_apk_extracted\assets\public")
apis = set()
for js in pub.glob("*.js"):
    t = js.read_text(encoding="utf-8", errors="ignore")
    for m in re.finditer(r"""['"`]([a-zA-Z][a-zA-Z0-9_/?.=&-]{3,80})['"`]""", t):
        s = m.group(1)
        if "/" in s and not s.startswith("http") and "assets/" not in s and ".js" not in s:
            if any(
                x in s.lower()
                for x in ("cart", "product", "customer", "catalog", "order", "search", "vendor", "auth", "login", "mobile")
            ):
                apis.add(s)

out = Path(r"e:\projects\nopCommerce_4.90.3_Source\harajbozorg\apk-analysis\api-endpoints.txt")
out.write_text("\n".join(sorted(apis)), encoding="utf-8")
print("TOTAL", len(apis))
for a in sorted(apis)[:60]:
    print(a)
