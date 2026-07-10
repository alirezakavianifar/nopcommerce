# Fixed-Price Quote: Pyk PWA Header Link

**Document:** Phase 2 — Module A  
**Quote validity:** 30 days from issue date  
**Prerequisite:** Client supplies assets listed below before work starts

---

## Scope of work

Add a **button and/or icon** to the top of the main storefront website. When a visitor clicks it, they are redirected to the **Pyk progressive web app** (mobile-oriented experience), similar in purpose to your harajbozorg mobile reference.

### Included

- Configurable **Pyk URL** in admin (no code deploy needed to change link)
- **Icon + optional label** in the site header (desktop and mobile responsive)
- Opens Pyk in the same tab or new tab (configurable)
- Widget plugin approach — no core nopCommerce modifications
- Basic RTL/Persian layout compatibility with existing theme
- Installation on staging, then production

### Explicitly excluded (available as separate Phase 2 items)

- Building or hosting the Pyk PWA itself
- Single sign-on (shared login between main site and Pyk)
- Shared shopping cart or session between sites
- Deep linking to specific Pyk pages from product pages
- Custom analytics beyond standard link `target` / UTM parameters you provide

---

## Client must provide before start

| Item | Required |
|------|----------|
| Final Pyk PWA URL (e.g. `https://pyk.example.com`) | Yes |
| Icon file (SVG or PNG, min 64×64) | Yes |
| Button label text (e.g. "Pyk" / "اپ پیک") or icon-only | Yes |
| Open in new tab vs same tab | Yes |
| Staging URL for UAT | Yes |

Work begins after all items are received. Delay in assets shifts delivery date accordingly.

---

## Effort and pricing

| Item | Estimate |
|------|----------|
| Development | 4–12 hours |
| Testing (desktop + mobile browsers) | Included |
| Deployment support | Included |

### Fixed price options (fill in your rate)

Choose one line to send to the client:

| Package | Hours | Rate | Total |
|---------|-------|------|-------|
| Standard | 8 h | ___ / h | ___ |
| Fixed cap | 12 h max | Flat | ___ |

**Recommended presentation:** Offer a **flat fixed fee** (e.g. 8 hours equivalent) with 12-hour cap if requirements expand slightly (e.g. multiple header positions).

### Payment terms (suggested)

- 50% on approval  
- 50% on staging sign-off  

---

## Delivery timeline

| Milestone | Target |
|-----------|--------|
| Assets received | Day 0 |
| Staging delivery | Day 2–3 business days |
| Client UAT | 2 business days |
| Production deploy | Within 1 business day after UAT approval |

---

## Technical approach (for your reference)

Injection via existing header widget zones in `src/Presentation/Nop.Web/Views/Shared/_Header.cshtml`:

- `PublicWidgetZones.HeaderBefore`
- `PublicWidgetZones.HeaderMiddle`
- `PublicWidgetZones.HeaderAfter`

New plugin: `Nop.Plugin.Widgets.PykLauncher` (or theme partial if client prefers minimal footprint).

---

## Acceptance criteria

1. Icon/button visible on homepage header on desktop and mobile widths.
2. Click navigates to the configured Pyk URL.
3. Admin can enable/disable and change URL without developer.
4. No regression to existing header, search, or cart.

---

## Sign-off

| | Client | Developer |
|---|--------|-----------|
| Name | | |
| Date | | |
| Agreed fixed price | | |
