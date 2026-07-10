# Vendor Backup & Restore — Scope Confirmation Questionnaire

**Document:** Phase 2 — Module E  
**Purpose:** Lock scope and price before development  
**Audience:** Client (please answer each section)

---

## Background

You requested that **each marketplace vendor** can take a backup from their panel and restore it **with admin approval**. The delivered nopCommerce site supports:

- Vendor-scoped admin access (products, orders, etc.)
- Admin **full database** backup only (entire store, SQL Server)
- Product export — but **not** a vendor self-service backup/restore workflow

Module E is **custom development**. The answers below directly affect effort (estimated **3–5 weeks** at full scope).

---

## Section 1 — Who can backup?

| # | Question | Options | Your answer |
|---|----------|---------|-------------|
| 1.1 | Which users may create backups? | ☐ Vendors only ☐ Admins only ☐ Both | |
| 1.2 | Can a vendor download backups on demand, or on a schedule? | ☐ On demand ☐ Scheduled (daily/weekly) ☐ Both | |
| 1.3 | Maximum backup frequency per vendor? | e.g. 1 per day / unlimited | |

---

## Section 2 — What data is included in a vendor backup?

Check **all** that apply. Unchecked items are **out of scope** unless added later.

### Catalog (typical — lower risk)

| # | Data | Include? | Notes |
|---|------|----------|-------|
| 2.1 | Products (names, descriptions, prices) | ☐ Yes ☐ No | |
| 2.2 | Product images and downloads | ☐ Yes ☐ No | Large storage |
| 2.3 | Categories assigned to vendor products | ☐ Yes ☐ No | |
| 2.4 | Product attributes / specifications | ☐ Yes ☐ No | |
| 2.5 | Inventory / stock quantities | ☐ Yes ☐ No | |
| 2.6 | Product SEO fields | ☐ Yes ☐ No | |

### Commercial (medium risk)

| # | Data | Include? | Notes |
|---|------|----------|-------|
| 2.7 | Vendor profile (name, description, logo) | ☐ Yes ☐ No | |
| 2.8 | Vendor-specific discounts | ☐ Yes ☐ No | |
| 2.9 | Vendor shipping settings | ☐ Yes ☐ No | |

### Orders & customers (high risk — legal/PII)

| # | Data | Include? | Notes |
|---|------|----------|-------|
| 2.10 | Orders containing vendor products | ☐ Yes ☐ No | Contains customer PII |
| 2.11 | Customer addresses from those orders | ☐ Yes ☐ No | GDPR / local law |
| 2.12 | Shipment / tracking records | ☐ Yes ☐ No | |

### Recommended starter scope (for faster, safer delivery)

> **Products + images + attributes + inventory + vendor profile only**  
> Exclude orders unless you have a legal basis and retention policy.

| Do you accept the recommended starter scope? | ☐ Yes ☐ No — we need: _____________ |

---

## Section 3 — Restore workflow

| # | Question | Options | Your answer |
|---|----------|---------|-------------|
| 3.1 | Who may **request** a restore? | ☐ Vendor ☐ Admin only | |
| 3.2 | Who must **approve** restore? | ☐ Super admin ☐ Any administrator ☐ Named role | |
| 3.3 | Restore mode | ☐ Merge (update existing SKUs) ☐ Replace all vendor catalog ☐ Preview diff then choose | |
| 3.4 | If SKU exists on restore, behavior | ☐ Skip ☐ Overwrite ☐ Create new SKU suffix | |
| 3.5 | Should restore run on staging first? | ☐ Yes (recommended) ☐ Direct to production | |
| 3.6 | Retention of backup files on server | e.g. 30 / 90 / 365 days | |

---

## Section 4 — Format and size

| # | Question | Your answer |
|---|----------|-------------|
| 4.1 | Preferred format | ☐ ZIP (JSON + files) ☐ Excel/XML (nop export style) ☐ Both |
| 4.2 | Expected max products per vendor | e.g. 500 / 5,000 / 50,000 |
| 4.3 | Expected max backup file size | e.g. 500 MB / 2 GB |
| 4.4 | Storage location | ☐ Server disk ☐ Client download only ☐ Cloud (S3/Azure) |

---

## Section 5 — Compliance and audit

| # | Requirement | Yes/No |
|---|-------------|--------|
| 5.1 | Audit log: who backed up, when, file hash | ☐ |
| 5.2 | Audit log: who approved restore, when | ☐ |
| 5.3 | Email notification to admin on restore request | ☐ |
| 5.4 | Vendor notified when restore approved/rejected | ☐ |
| 5.5 | Encrypt backup files at rest | ☐ |

---

## Section 6 — Effort impact summary

| Scope choice | Effort |
|--------------|--------|
| Products + media + attributes + inventory + vendor profile | **~3 weeks** |
| + Discounts and shipping rules | **+3–5 days** |
| + Orders and customer data | **+1–2 weeks** + legal review |
| + Scheduled backups + cloud storage | **+1 week** |

---

## Client sign-off

| Field | Response |
|-------|----------|
| Company | |
| Contact | |
| Date | |
| Approved scope summary | |
| Approved budget range | |

**Return completed form to developer before Module E quote is finalized.**
