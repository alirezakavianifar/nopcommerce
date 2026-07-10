# AI Duplicate Product Detection — Scope Confirmation Questionnaire

**Document:** Phase 2 — Module C  
**Purpose:** Lock matching rules, admin workflow, and price before development  
**Audience:** Client (please answer each section)

---

## Background

You requested an **AI system** that learns product names, images, and specifications to **prevent duplicate product entries**. When a duplicate is registered, the **admin** should see it in the management panel, retrieve the existing product and attributes, and apply updates.

This is **not** included in standard nopCommerce 4.90 (which only supports manual “copy product” and SKU-based import matching). Module C is **custom development** (estimated **4–8 weeks** depending on answers below).

---

## Section 1 — When does detection run?

| # | Trigger | Include? |
|---|---------|----------|
| 1.1 | Vendor creates new product | ☐ |
| 1.2 | Vendor edits existing product | ☐ |
| 1.3 | Admin creates/edits any product | ☐ |
| 1.4 | Bulk product import (Excel/XML) | ☐ |
| 1.5 | API / mobile app product create | ☐ |

| Block save until admin reviews? | ☐ Yes (strict) ☐ No — warn only ☐ Configurable |
|---------------------------------|--------------------------------------------------|

---

## Section 2 — Matching scope (critical for marketplace trust)

| # | Question | Options | Your answer |
|---|----------|---------|-------------|
| 2.1 | Compare new product against | ☐ Same vendor only ☐ All vendors globally ☐ Configurable per store | |
| 2.2 | If duplicate is another **vendor’s** product, action | ☐ Flag admin only ☐ Block listing ☐ Notify both vendors ☐ N/A (same vendor only) | |
| 2.3 | Minimum similarity to flag (conceptual) | ☐ High (fewer false positives) ☐ Medium ☐ Low (catch more, more review work) | |

**Recommendation for multi-vendor:** Detect within **same vendor** automatically; cross-vendor matches go to **admin review only** (no auto-merge across sellers).

| Accept recommendation? | ☐ Yes ☐ No — explain: _____________ |

---

## Section 3 — What signals are used?

| Signal | Weight / include? |
|--------|-------------------|
| Product name (text similarity, Persian + English) | ☐ High ☐ Medium ☐ Low ☐ Off |
| SKU / GTIN / MPN exact match | ☐ Block duplicate ☐ Flag only ☐ Off |
| Product images (visual similarity / AI embedding) | ☐ High ☐ Medium ☐ Low ☐ Off |
| Specifications / attributes | ☐ High ☐ Medium ☐ Low ☐ Off |
| Short description / full description | ☐ Medium ☐ Low ☐ Off |
| Price similarity | ☐ Low ☐ Off |

---

## Section 4 — Admin panel workflow

Describe desired flow by checking options:

| Step | Option |
|------|--------|
| 4.1 Queue location | ☐ New admin menu “Duplicate review” ☐ Notification on dashboard ☐ Email alert |
| 4.2 Queue shows | ☐ New product ☐ Suspected match(es) ☐ Similarity score ☐ Side-by-side diff |
| 4.3 Admin actions | ☐ Dismiss (not duplicate) ☐ Merge into existing ☐ Edit new product ☐ Delete new product ☐ Assign to existing vendor |
| 4.4 Vendor visibility | ☐ Vendor sees warning ☐ Admin only |
| 4.5 After merge, orders on old product | ☐ Keep history on surviving SKU ☐ Manual policy |

---

## Section 5 — “Apply necessary updates” — define merge rules

| # | Rule | Your policy |
|---|------|-------------|
| 5.1 | Which record survives merge? | ☐ Older ☐ Newer ☐ Higher stock ☐ Admin chooses |
| 5.2 | Images | ☐ Keep all ☐ Keep best ☐ Replace with new |
| 5.3 | Descriptions | ☐ Keep longer ☐ Keep newer ☐ Admin edits in UI |
| 5.4 | Price | ☐ Keep lower ☐ Keep newer ☐ Admin chooses |
| 5.5 | Categories | ☐ Union of both ☐ Keep existing |

---

## Section 6 — AI provider and indexing

| # | Question | Your answer |
|---|----------|-------------|
| 6.1 | Preferred AI backend | ☐ OpenAI ☐ Google Gemini ☐ DeepSeek ☐ No preference |
| 6.2 | Who pays API costs? | ☐ Client account ☐ Bundled in hosting |
| 6.3 | Re-index all products on go-live? | ☐ Yes (~one-time cost) ☐ Only new products |
| 6.4 | Catalog size (approx.) | ______ products, ______ vendors |

**Ongoing cost note:** Image embedding and LLM calls are billed per product check and per catalog re-index. Provide a monthly API budget estimate after catalog size is known.

---

## Section 7 — Effort impact summary

| Scope | Effort |
|-------|--------|
| Same-vendor, name + SKU + basic image hash | **~4 weeks** |
| + AI image embeddings + Persian NLP | **+1–2 weeks** |
| + Cross-vendor detection + merge UI | **+1–2 weeks** |
| + Import/API hooks + block-on-save | **+1 week** |

---

## Section 8 — Out of scope unless requested

- Fully automatic merge without admin click  
- Legal judgment on counterfeit / IP infringement  
- Integration with external product databases (UPC global DB)

---

## Client sign-off

| Field | Response |
|-------|----------|
| Company | |
| Contact | |
| Date | |
| Matching scope (2.1) | |
| Merge policy summary | |
| Approved budget range | |

**Return completed form before Module C quote is finalized.**
