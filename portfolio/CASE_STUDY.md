# ⚡ Case Study: nopCommerce 4.90 Enterprise E-Commerce & AI Marketplace

## 1. Executive Summary

This project showcases the architecture, engineering, and deployment of an **enterprise-grade e-commerce platform and multi-vendor marketplace** built on **nopCommerce 4.90 (.NET 9 / C# / ASP.NET Core)**.

Engineered with a **strict Zero-Core-Modification standard**, all advanced business capabilities—including a viral **Social Commerce / Group Purchase Engine**, **Multimodal AI Visual & Voice Search**, a **24/7 AI Customer Support Chatbot**, **Multi-Vendor Seller Marketing Ads**, **3-Tier Conditional Logistics**, and **Role-Based SMS 2FA**—were architected entirely as modular, decoupled, and high-performance **nopCommerce Plugins**.

---

## 2. System Architecture & Plugin Ecosystem

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│                             PRESENTATION LAYER                              │
│  - ASP.NET Core Razor Pages & ViewComponents (Desktop & Mobile Storefront)  │
│  - RTL Localization & High-Impact Promotional Landing Pages (Amazing Deals) │
│  - Customer Account Dashboard Extensions (Group Hub, Wallet, Club Lottery)  │
│  - Responsive Admin Panel Extensions (DataTables, Moderation Queues)        │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          RESTFUL MOBILE API LAYER                           │
│  - /api/group-purchase/* (Cart conversion, unique tokens, wallet & history)  │
│  - /api/ai/* (Visual image search, voice transcription, semantic query)     │
│  - /api/seller-marketing/* (Sponsored ad submissions, daily budget control) │
│  - /api/notifications/* (System announcements & active popup stream)        │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        SERVICE & APPLICATION LAYER                          │
│  - DependencyRegistrar: Auto-wired DI (INopStartup, Autofac / ASP.NET DI)   │
│  - GroupRewardCalculationService: Atomic reward evaluation & rule matching   │
│  - WalletService & LotteryService: Double-entry ledger balance transactions │
│  - AiService: AvalAI API orchestration, embeddings, vector cosine ranking    │
│  - ConditionalShippingService: 3-tier logistics matrix evaluation           │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     EVENT-DRIVEN ASYNCHRONOUS CONSUMERS                     │
│  - OrderPlacedEventConsumer: Post-checkout reward calculation & deposits    │
│  - ProductCreated/UpdatedConsumer: AI duplicate detection & embeddings sync │
│  - CustomerRegisteredConsumer: Loyalty points & onboarding allocation       │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           DATA & PERSISTENCE LAYER                          │
│  - FluentMigrator Schema Migrations (Zero core database pollution)          │
│  - SQL Server / Linq2Db Repository Pattern with Optimistic Concurrency       │
│  - Tables: GP_GroupPurchase, GP_RewardRule, GP_CustomerWallet, GP_LegalLog  │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Technology Highlights

- **Backend Framework:** .NET 9, C#, ASP.NET Core, nopCommerce 4.90 Architecture.
- **AI & Computer Vision:** AvalAI Cloud API, Multimodal Visual Vector Matching, Audio Waveform Speech-to-Text Transcription, Cosine Similarity Attribute Embeddings.
- **Database & Migrations:** Microsoft SQL Server, FluentMigrator, Linq2Db, Entity Framework Core abstractions.
- **Security:** SMS Two-Factor Authentication (`Nop.Plugin.MultiFactorAuth.SMS`), Device Fingerprint Whitelisting Tokens, IP Address Range Validation.
- **Architecture Pattern:** Clean Architecture, Domain-Driven Design (DDD), In-Memory Event-Driven Message Handlers (`IConsumer<T>`), Widget Zones & Dependency Injection (`INopStartup`).

---

## 3. Key Feature Modules & Domain Implementations

### 🛒 1. Social Commerce & Group Purchase Engine (`Nop.Plugin.Misc.GroupPurchase`)

- **Cart Aggregation & Token Sharing:** Enables shoppers to convert active carts into collaborative Group Purchases. The system generates a cryptographic `UniqueCode` allowing teammates to join via direct URL or app code entry.
- **Legal Compliance & Disclaimer Logs:** Enforces mandatory terms acceptance for Group Leaders (accepting delivery responsibility) and subgroup members, logging timestamp, IP address, and displayed legal text into `GP_LegalConfirmationLog`.
- **Dynamic Multi-Tier Reward Engine:** Calculates incentives post-order via `GroupRewardCalculationService`. Supports:
  - Fixed cash values vs. percentage of cart total vs. percentage of net product margin.
  - Per-category rule prioritization (e.g., higher margins on Fashion vs. Commodities).
  - Configurable minimum cart thresholds and member count prerequisites.
- **Customer Digital Wallet & Gamified Lottery System:**
  - `CustomerWallet` with dual sub-balances: Regular Balance vs. Group Reward Balance.
  - `LotteryTransaction` gamification tracking points earned from purchases and team leadership, with redemption into discount coupons or prize entries.
- **Privacy-Preserving Dashboards:** Dedicated customer panels (`LeaderGroups` and `SubgroupHistory`) with item visibility controls (Full, Limited 5 items, or Total Price Only).

---

### 🧠 2. Multimodal AI Search & Intelligent Assistant (`Nop.Plugin.Misc.ArtificialIntelligence`)

- **Multimodal Visual Image Search:** Allows shoppers to capture or upload product photos. Analyzes image attributes using AvalAI vision embeddings and matches against catalog vectors in real time.
- **Voice Query Processing:** Speech-to-text audio waveform transcription allowing hands-free product discovery across web and mobile apps.
- **AI Semantic Text Search:** Evaluates deep product specifications, Persian/English synonym embeddings, and unstructured descriptions to return intent-based search results.
- **24/7 AI Customer Support Chatbot:** Conversational customer assistant trained on store policies, product catalogs, and shipment tracking, equipped with real-time confidence scoring and automated handoff to human support agents.
- **AI Duplicate Product Detection & Catalog Moderation:** Automated vendor submission scanner comparing photo perceptual hashes and text embeddings against the global catalog, redirecting duplicate attempts into an administrative dispute and approval queue (`/Admin/AiAdmin/DuplicateQueueList`).

---

### 📢 3. Multi-Vendor Seller Marketing & Sponsored Ads (`Nop.Plugin.Misc.SellerMarketing`)

- **Vendor Campaign Portal:** Vendors configure sponsored listings with daily budget limits, date ranges, and target categories.
- **Administrative Moderation Workflow:** Dedicated review queue allowing store administrators to inspect, approve, or reject vendor campaign submissions before ad units go live.
- **REST API Integration:** Dedicated endpoints (`/api/seller-marketing/submit`, `/api/seller-marketing/my-requests`) enabling vendor apps and warehouse portals to interact seamlessly with marketing services.

---

### 🚚 4. Advanced Conditional Logistics Engine (`Nop.Plugin.Shipping.ConditionalMethods`)

- **3-Tier Priority Evaluation Matrix:** Evaluates shipping carrier availability using strict hierarchical precedence:
  $$\text{1. City Courier Coverage} \longrightarrow \text{2. Product Dimension Constraints} \longrightarrow \text{3. Warehouse Origin}$$
- **4 Distinct Shipping Methods:** Courier (intra-city fast delivery), Heavy Freight / Transport (bulk cargo), Air Cargo, and Express Dispatch.
- **Multi-Warehouse Split Shipping:** Automatically detects orders containing products sourced from distributed warehouses in different cities, calculating accurate combined split-shipment invoices.

---

### 🔒 5. Enterprise Security & SMS 2FA (`Nop.Plugin.MultiFactorAuth.SMS`)

- **Role-Based Two-Factor Authentication:** Enforces mandatory SMS OTP verification for sensitive roles (`Force2FAForAdmins`, `Force2FAForVendors`).
- **Device Whitelisting & Binding:** Web-feasible security layer validating registered browser fingerprint tokens and client IP whitelists to prevent administrative credential hijacking over HTTPS.

---

## 4. Engineering Challenges & Solutions

| Challenge | Engineering Solution |
| :--- | :--- |
| **Zero Core Modification Mandate** | Implemented 100% of custom business domains as decoupled nopCommerce plugins using `INopStartup` for dependency injection, `IWidgetPlugin` for UI injection, and `IConsumer<OrderPlacedEvent>` for event-driven orchestration. |
| **Preventing Double-Reward Processing** | Architected atomic database transactions in `GroupRewardCalculationService` with idempotency verification flags, ensuring rewards are calculated exactly once even under rapid webhooks or concurrent checkouts. |
| **Sub-Second Multimodal AI Search** | Utilized pre-computed embedding vectors and lightweight API gateways with AvalAI, executing visual and voice query resolution in under 400ms without degrading storefront page load times. |
| **Multi-Warehouse Split Shipping Complexities** | Built a matrix evaluation engine resolving warehouse origins per cart item, dynamically computing partitioned shipping options and presenting unified checkouts. |
| **Clean Mobile REST API Authentication** | Developed custom API controllers throwing native HTTP 401 Unauthorized JSON payloads without triggering ASP.NET Core HTML cookie login challenge redirects. |

---

## 5. Deliverables & Technical Assets

- **Repository Architecture:** Modular solution containing core nopCommerce 4.90 engines and 7 custom enterprise plugins (`GroupPurchase`, `ArtificialIntelligence`, `SellerMarketing`, `UserNotifications`, `AmazingDiscounts`, `ConditionalShipping`, `SMS2FA`).
- **Database Schema:** 12+ custom tables created via FluentMigrator with zero modifications to native nopCommerce tables.
- **API Documentation:** RESTful JSON endpoints documented and ready for iOS, Android, and warehouse management integrations.
