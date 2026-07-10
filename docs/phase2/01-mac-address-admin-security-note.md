# Admin Security: MAC Address Restriction — Technical Note for Client

**Document:** Phase 2 — Security clarification  
**Audience:** Client / project stakeholders  
**Status:** For review before quoting device-binding work

---

## Summary

Your request includes restricting admin panel access by **IP address** and **MAC address**. The marketplace **already supports global IP allowlisting** for the admin area (Configuration → General → Security). **MAC address filtering is not technically possible** for a standard web-based admin panel accessed through a browser over the internet.

This note explains why, and proposes practical alternatives you can choose from for Phase 2.

---

## What already works today (no extra development)

| Feature | Status |
|---------|--------|
| Admin IP allowlist (comma-separated list) | **Built in** — applies to all `/Admin` requests after login |
| Google Authenticator (TOTP) MFA | **Built in** — optional plugin, per user |
| MFA framework (supports SMS type in architecture) | **Built in** — SMS plugin must be developed separately |

**How to enable IP restriction now:**  
Admin → Configuration → Settings → General → Security → *Admin area allowed IP addresses*

Leave empty to allow all IPs. Add office/static IPs separated by commas.

---

## Why MAC address restriction cannot work on a website

A **MAC address** identifies a network interface on a local network (e.g. Wi‑Fi or Ethernet). It is used by routers on the same LAN segment.

When an administrator opens the admin panel in Chrome, Firefox, or Safari:

1. The browser connects to your server over **HTTPS** (the public internet or VPN).
2. Only **IP address**, cookies, and HTTP headers are visible to the server.
3. The device **MAC address is never sent** in HTTP requests — by design, for privacy and security.
4. Web applications **cannot read** the user's MAC address (no browser API exists).

Therefore, nopCommerce — or any normal web admin — **cannot** enforce “only this laptop’s MAC may log in” without additional non-web components.

This is not a limitation of our implementation; it is how the web platform works worldwide.

---

## Recommended alternatives (quote separately)

### Option 1 — Global + per-user IP allowlist (recommended baseline)

- Keep the existing **global** admin IP list for your office/VPN egress IP.
- Add **per-administrator IP rules** so each staff account can only log in from approved locations.
- Combine with **SMS two-factor authentication** (Phase 2 module F).

**Effort:** ~3–5 days development  
**Security level:** Strong for fixed offices and VPN users

---

### Option 2 — VPN-only admin access

- Admin panel is reachable only through a company VPN with a fixed egress IP.
- Configure the built-in IP allowlist to that VPN IP only.
- Add SMS 2FA for account-level protection.

**Effort:** Mostly infrastructure (VPN setup); minimal or no custom code  
**Security level:** Strong; common enterprise pattern

---

### Option 3 — Registered device / browser token (web-realistic “device binding”)

- On first successful login (with SMS 2FA), the admin **registers** the browser/device.
- Server stores a secure device token (cookie + optional fingerprint hash).
- Unknown devices are blocked until a super-admin approves them.

**Effort:** ~1–2 weeks development  
**Security level:** Good; not as strong as hardware MAC but achievable on the web  
**Note:** This is what we can offer **instead of MAC filtering** if you need device-level control.

---

### Option 4 — Desktop admin agent (only if MAC is mandatory)

- A small Windows/macOS agent or custom desktop app reports hardware identity to the server.
- Admin web login only succeeds when the agent is running and verified.

**Effort:** ~4+ weeks; separate desktop application  
**Security level:** Highest for MAC-like binding; highest maintenance cost

---

## Suggested decision for Phase 2

| Your original ask | Our recommendation |
|-------------------|-------------------|
| IP restriction | **Use built-in setting now**; optional per-user IP in Phase 2 |
| MAC restriction | **Replace with Option 3 (device registration)** or **Option 2 (VPN)** |
| SMS 2FA per user | **Build custom SMS MFA plugin** (Phase 2 module F) |

Please confirm which alternative you prefer before we finalize the security module quote.

---

## References in delivered codebase

- IP validation filter: `src/Presentation/Nop.Web.Framework/Mvc/Filters/ValidateIpAddressAttribute.cs`
- Security settings: `src/Libraries/Nop.Core/Domain/Security/SecuritySettings.cs`
- MFA login flow: `src/Presentation/Nop.Web/Controllers/CustomerController.cs` (`MultiFactorVerification`)
