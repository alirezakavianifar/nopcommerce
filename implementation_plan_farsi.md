# Implementation Plan - Translate English Resources to Persian

This implementation plan details the strategy to translate the remaining ~7,700 resources (5,187 admin resources and 2,510 storefront resources) from English to Persian in the database.

---

## User Review Required

> [!WARNING]
> Translating ~7,700 resources programmatically via external translation services (like Google Translate or OpenAI API) may incur minor API costs or run into rate limits. The plan details batching and local caching to minimize costs and prevent failures.

---

## Open Questions

> [!IMPORTANT]
> Please review and provide feedback on the following questions:
> 1. **Translation Engine Choice**: Which service should we use to translate the strings?
>    - **Option 1 (Recommended)**: Use an automated PowerShell script leveraging a Translation API (e.g., Google Cloud Translation or OpenAI GPT model).
>    - **Option 2**: Use a community-contributed nopCommerce Persian language pack XML file (if you have one) to override standard resources, then auto-translate only the remaining custom plugin strings.
> 2. **Translation Scope**: Do you want to translate both the **Admin Panel** and the **Storefront**, or only the **Admin Panel**?
> 3. **Acronyms/Technical Terms**: Should we keep technical terms like "API", "URL", "SSL", "SKU", "IP", and "PDF" in English/Latin characters, or transliterate them into Persian? (Our regex filter can preserve these automatically if desired).

---

## Proposed Changes

We propose implementing a PowerShell script that directly updates the SQL Server database. This is cleaner and more reliable than manual Excel exports/imports.

### Database Translation Script

#### [NEW] [translate_resources.ps1](file:///C:/Users/Administrator/.gemini/antigravity-ide/brain/48494522-b223-4feb-ac34-49c3dfb28722/scratch/translate_resources.ps1)
A script that performs the following steps:
1. Connects to the SQL Express `nopCommerce490` database.
2. Selects all resource strings for `LanguageId = 2` (Persian) where:
   - The value contains no Persian/Arabic characters (`[\u0600-\u06FF]`).
   - The value is not empty or just numeric.
3. Groups the resources into batches (e.g., 100 strings per batch) to avoid API limits.
4. Translates the batch to Persian (preserving HTML tags and placeholders like `{0}`, `{1}`).
5. Updates the database `LocaleStringResource` table with the translated values.
6. Saves progress to a JSON log file in the `scratch/` directory so the process can be paused and resumed without losing progress.

---

## Verification Plan

### Automated Verification
- Run a verification query to count the number of untranslated resources remaining in `LocaleStringResource` for `LanguageId = 2`.
- Check that the placeholders (like `{0}`, `{1}`) are correctly preserved in the translated text.

### Manual Verification
- Open the nopCommerce admin panel, switch the language to Persian, and verify that fields, buttons, and settings are rendered in Farsi.
