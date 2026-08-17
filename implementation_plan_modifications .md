# Phase-by-Phase Implementation Plan: Fix Issues in Already Implemented Modules

This plan addresses all issues, documentation errors, screenshot mismatches, and missing REST endpoints identified by the client in [`testing_guide_fa (2).pdf`](file:///e:/projects/nopCommerce_4.90.3_Source/testing_guide_fa%20(2).pdf) regarding the **already implemented modules**. It is structured in sequential phases, each ending with a dedicated verification phase.

---

## User Review Required

> [!NOTE]
> Each phase is self-contained and sequentially verifiable before moving to the next phase.

---

## Phase 1: REST API Enhancements & AI Semantic Text Search

### Objectives
1. Provide dedicated REST API endpoints for external applications (mobile app, warehouse panel, courier app) to consume AI search capabilities.
2. Implement AI Semantic Text Search (evaluating product titles, specifications, and descriptions via embeddings).
3. Provide REST API endpoints for sellers and warehouse keepers to submit and monitor sponsored advertising requests.

### Changes to Implement

#### 1. Artificial Intelligence Plugin (`Nop.Plugin.Misc.ArtificialIntelligence`)
- **[MODIFY] [AiStorefrontController.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Controllers/AiStorefrontController.cs)**
  - Add explicit REST API routes with JSON-formatted responses:
    - `POST /api/ai/visual-search` (accepts image file, returns matching products)
    - `POST /api/ai/voice-search` (accepts audio file, returns speech-to-text transcript + matching products)
    - `POST /api/ai/text-search` & `GET /api/ai/text-search` (accepts text query, returns semantic AI-ranked products)
- **[MODIFY] [AiService.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Services/AiService.cs)** & **[IAiService.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Services/IAiService.cs)**
  - Implement `TextSearchAsync(string query, int maxResults = 10)` to compute cosine similarity across product text/attribute embeddings for deeper attribute search.

#### 2. Seller Marketing Plugin (`Nop.Plugin.Misc.SellerMarketing`)
- **[NEW] `Nop.Plugin.Misc.SellerMarketing/Controllers/SellerMarketingApiController.cs`**
  - Expose API endpoints on route `/api/seller-marketing`:
    - `POST /api/seller-marketing/submit`: Submit a sponsored product/ad request with target product, daily budget, and date range.
    - `GET /api/seller-marketing/my-requests`: Fetch active and pending marketing submissions for the authenticated seller.

### 🔍 Phase 1 Verification
- [ ] Run `dotnet build src/NopCommerce.sln` to confirm 0 compilation errors across all plugin projects.
- [ ] Verify that the new API routes (`/api/ai/text-search`, `/api/ai/visual-search`, `/api/ai/voice-search`, `/api/seller-marketing/submit`, `/api/seller-marketing/my-requests`) are registered and return structured JSON.
- [ ] Confirm proper customer and vendor authentication handling on protected endpoints.

---

## Phase 2: Visual Asset & Screenshot Alignment

### Objectives
Fix the screenshot duplication bug in `docs/images/` where `step1_admin_discounts.png`, `step5_admin_seller_marketing.png`, `step6_admin_group_purchase.png`, and `step7_admin_reward_rules.png` all shared the identical 134,630-byte generic discount list image.

### Changes to Implement
- **[MODIFY] `docs/images/step5_admin_seller_marketing.png`**: Generate and save the true Admin Seller Marketing review and approval list view (`/Admin/SellerMarketing/List`).
- **[MODIFY] `docs/images/step6_admin_group_purchase.png`**: Generate and save the true Group Purchase catalog management view (`/Admin/GroupPurchase/List`).
- **[MODIFY] `docs/images/step7_admin_reward_rules.png`**: Generate and save the true Group Purchase Reward Rules configuration table (`/Admin/RewardRule/List`).

### 🔍 Phase 2 Verification
- [ ] Inspect file sizes and metadata of all 3 images in `docs/images/` to verify they are distinct, non-identical files.
- [ ] Visually confirm that each image clearly represents its corresponding admin section:
  - Step 5 shows Seller Marketing requests with approve/reject controls.
  - Step 6 shows products with Group Purchase enabled toggles.
  - Step 7 shows the Reward Rules table with % cart total and fixed wallet/lottery tier values.

---

## Phase 3: Persian Testing Guide Overhaul (`testing_guide_fa.tex`)

### Objectives
Update [testing_guide_fa.tex](file:///e:/projects/nopCommerce_4.90.3_Source/docs/testing_guide_fa.tex) to comprehensively address all 14 feedback points, clarify system behaviors, and update image figures.

### Changes to Implement
- **[MODIFY] [testing_guide_fa.tex](file:///e:/projects/nopCommerce_4.90.3_Source/docs/testing_guide_fa.tex)**:
  1. **Section 2 & Step 12 (AI Smart Search)**:
     - Add explicit technical and user explanation for **AI Smart Text Search** (evaluating product titles, specifications, and descriptions via embeddings) alongside visual and voice search.
     - Document the REST API endpoints available for mobile and logistics apps.
  2. **Step 5, 6, and 7 (Seller Marketing & Group Purchase)**:
     - Embed the corrected screenshots from Phase 2.
     - Explain how [GroupRewardCalculationService](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Services/GroupRewardCalculationService.cs) evaluates rewards post-order and deposits them into customer wallets/lotteries, operating completely independently from core cart discounts to prevent rule collisions.
  3. **Step 13 (AI Duplicate Product Detection)**:
     - Document the complete multi-vendor workflow: photo and attribute scanning, duplicate alerts, dispute explanations, and admin approval/rejection queue in `Admin/AiAdmin/DuplicateQueueList`.
  4. **Step 14 & Step 15 (SMS 2FA & Device Security)**:
     - Clarify role-based SMS 2FA enforcement (`Force2FAForAdmins`, `Force2FAForVendors`).
     - Explain why **Device Binding Tokens + IP Whitelisting** represent the web-feasible solution replacing physical MAC address filtering over browser HTTPS.
  5. **Step 16 (Conditional Shipping)**:
     - Document the 3-tier priority evaluation (**1. City Coverage $\rightarrow$ 2. Product Support $\rightarrow$ 3. Warehouse Support**) and the 4 shipping methods (**Courier, Freight, Cargo, Express**).

### 🔍 Phase 3 Verification
- [ ] Validate LaTeX syntax and Persian typography alignment in `testing_guide_fa.tex`.
- [ ] Confirm all `\includegraphics` commands point to the correct updated image files.
- [ ] Verify that all 14 feedback comments from `testing_guide_fa (2).pdf` have corresponding explanations in the document.

---

## Phase 4: Final Solution Build & End-to-End Verification

### Objectives
Perform full end-to-end build and integration verification across the entire solution.

### Verification Steps
1. **Compilation Check**:
   ```powershell
   dotnet build src/NopCommerce.sln -v minimal
   ```
   *Pass Criteria*: `Build succeeded. 0 Error(s)`.
2. **API Endpoint Verification**:
   - Verify all newly registered endpoints compile and serialize JSON properly.
3. **Artifact Consistency**:
   - Check that `docs/testing_guide_fa.tex` and `docs/images/` are synchronized with the codebase.
4. **Summary & Walkthrough**:
   - Generate a detailed [walkthrough.md](file:///C:/Users/Administrator/.gemini/antigravity-ide/brain/a7c23539-247a-47fb-a76f-b62b20a666b0/walkthrough.md) documenting all completed modifications and validation results.
