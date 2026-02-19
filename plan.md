Below is a **practical, phase-by-phase implementation plan** tailored specifically for a **nopCommerce 4.90** project (ASP.NET Core).
Architecture is based on the official nopCommerce plugin system from the documentation you shared.

---

# 🔷 OVERALL ARCHITECTURE STRATEGY

Because your changes affect:

* Shopping cart logic
* Order processing
* Wallet & reward calculations
* API (mobile app)
* Customer dashboard
* Admin configuration
* Legal confirmations storage

You should **NOT modify core nopCommerce directly**.
Everything must be implemented via:

* ✔ Custom Plugins
* ✔ Services (DI-based)
* ✔ Domain Extensions (via custom tables)
* ✔ Event Consumers
* ✔ API Extensions
* ✔ Overridden Views via Widget Zones

---

# 📦 PLUGIN STRUCTURE OVERVIEW

You will create **3 main plugins**:

```
Plugins/
 ├── Nop.Plugin.Misc.UserNotifications
 ├── Nop.Plugin.Misc.AmazingDiscounts
 └── Nop.Plugin.Misc.GroupPurchase
```

Each plugin follows:

* Plugin.cs
* Install/Uninstall logic
* Controllers
* Services
* Domain models
* Data mappings
* Admin views
* Public views
* API extensions

---

# 1️⃣ USER NOTIFICATION MODULE

## 🔹 Goal

System-wide announcements synced with:

* Website
* Mobile App API

---

## 🔹 Database Design

Create table:

### NotificationAnnouncement

| Field        | Type     |
| ------------ | -------- |
| Id           | int      |
| Title        | string   |
| Body         | string   |
| StartDateUtc | datetime |
| EndDateUtc   | datetime |
| IsPublished  | bool     |
| CreatedOnUtc | datetime |

Optional:

* Target roles
* Popup or banner type

---

## 🔹 Backend Implementation

### 1. Create Plugin

`Nop.Plugin.Misc.UserNotifications`

### 2. Create Service

`INotificationService`

Functions:

* GetActiveAnnouncements()
* InsertAnnouncement()
* DeleteAnnouncement()

---

## 🔹 Admin UI

Admin panel page:

```
Admin → Promotions → System Announcements
```

CRUD functionality.

---

## 🔹 Public Display

Use:

```
IWidgetPlugin
```

Inject into:

* Header
* Homepage
* Mobile API endpoint

---

## 🔹 API Extension

If mobile uses:

* Nop REST API plugin
* Custom API

Add endpoint:

```
GET /api/notifications/active
```

Return active announcements.

---

# 2️⃣ AMAZING DISCOUNTS PAGE

---

## 🔹 Goal

Dedicated offers page:

* Mobile app
* Website
* Footer link
* Attractive UI

---

## 🔹 Implementation Strategy

Create plugin:

```
Nop.Plugin.Misc.AmazingDiscounts
```

---

## 🔹 Logic

You have two options:

### Option A (Simple)

Use Product Tags:

* Tag products as "Amazing"
* Display by tag

### Option B (Better)

Create table:

AmazingDiscountProduct:

* ProductId
* StartDate
* EndDate
* Priority
* CustomLabel

---

## 🔹 Frontend Page

Create route:

```
/amazing-discounts
```

Controller:

```
AmazingDiscountController
```

Return View:

```
Views/AmazingDiscount/List.cshtml
```

Use:

* Custom theme override
* Banner slider
* Countdown timer

---

## 🔹 Add to Footer

Use:

```
IWidgetPlugin
```

Inject link into:

```
PublicWidgetZones.Footer
```

---

## 🔹 Mobile App

Add API:

```
GET /api/amazing-discounts
```

---

# 3️⃣ GROUP PURCHASE MODULE (CORE SYSTEM)

This is the largest part.

---

# 🏗 ARCHITECTURE DESIGN

Create plugin:

```
Nop.Plugin.Misc.GroupPurchase
```

---

# 🔹 DATABASE STRUCTURE

### GroupPurchase

| Field            | Type     |
| ---------------- | -------- |
| Id               | int      |
| LeaderCustomerId | int      |
| UniqueCode       | string   |
| Status           | enum     |
| CreatedOnUtc     | datetime |
| DeliveryCity     | string   |
| DeliveryAddress  | string   |

---

### GroupPurchaseMember

| Field           | Type     |
| --------------- | -------- |
| Id              | int      |
| GroupPurchaseId | int      |
| CustomerId      | int      |
| CartId          | int      |
| IsLeader        | bool     |
| AcceptedTerms   | bool     |
| AcceptedOnUtc   | datetime |
| VisibilityType  | enum     |

---

### GroupPurchaseReward

| Field           | Type    |
| --------------- | ------- |
| Id              | int     |
| GroupPurchaseId | int     |
| CustomerId      | int     |
| RewardType      | enum    |
| Amount          | decimal |
| CalculationType | enum    |
| CategoryId      | int     |

---

### LegalConfirmationLog

| Field           | Type            |
| --------------- | --------------- |
| Id              | int             |
| CustomerId      | int             |
| GroupPurchaseId | int             |
| Role            | Leader/Subgroup |
| MessageShown    | string          |
| AcceptedOnUtc   | datetime        |
| IPAddress       | string          |

---

# 🔹 CART MODIFICATION STRATEGY

You must hook into:

* IShoppingCartService
* IOrderProcessingService
* OrderPlacedEvent

Do NOT change core.

Use:

```
EventConsumer<OrderPlacedEvent>
```

---

# 🔹 GROUP CONVERSION FLOW

## When leader clicks "Convert to Group Purchase"

1. Show popup (admin configurable content)
2. Require checkbox "I Accept"
3. Store legal log
4. Generate UniqueCode
5. Save GroupPurchase
6. Tag cart with GroupPurchaseId

---

# 🔹 JOINING FLOW

User enters UniqueCode:

1. Show popup
2. Accept terms
3. Link cart to GroupPurchase
4. Store acceptance log

---

# 🔹 ORDER PROCESSING LOGIC

When checkout occurs:

1. Detect GroupPurchase
2. Combine logic for rewards
3. Calculate commissions
4. Store rewards
5. Apply wallet credit

---

# 💰 REWARD SYSTEM DESIGN

Create configuration entity:

### RewardRule

| Field           | Type            |
| --------------- | --------------- |
| Id              | int             |
| Role            | Leader/Subgroup |
| RewardType      | enum            |
| CalculationType | enum            |
| Value           | decimal         |
| CategoryId      | int             |
| MinCartAmount   | decimal         |
| MinMembers      | int             |

---

Admin can define unlimited rules.

---

# 🔹 CALCULATION ENGINE

Create:

```
IGroupRewardCalculationService
```

It should:

1. Fetch applicable rules
2. Filter by category
3. Calculate:

   * Fixed
   * Percentage of cart total
   * Percentage of net profit
4. Apply caps
5. Save reward

---

# 🔹 WALLET SYSTEM

If wallet already exists:

* Extend wallet transactions

If not:
Create:

CustomerWallet
CustomerWalletTransaction

Important:

* Separate balances:

  * Regular wallet
  * Group reward wallet
  * Lottery points

---

# 🎯 LOTTERY SYSTEM

Table:

LotteryPointTransaction

| Field           | Type |
| --------------- | ---- |
| CustomerId      | int  |
| Points          | int  |
| Source          | enum |
| GroupPurchaseId | int  |

Conversion rules configurable in admin.

---

# 👑 LEADER DASHBOARD

Add to:

```
Customer → My Account
```

Add tabs:

* Leader Groups
* Subgroup History
* Wallet
* Lottery Points

---

Display:

* Group date
* Members
* Cart totals
* Participation count
* Visibility rules

---

# 🔐 VISIBILITY LOGIC

Enum:

VisibilityType:

* Full
* Limited (5 items)
* None

Enforce in service layer.

---

# 📦 DELIVERY RULE LOGIC

During checkout:

* Validate same city
* If different → no free delivery
* Store delivery responsibility flag

---

# 🛠 ADMIN CONFIGURATION PANEL

Admin → Group Purchase Settings

Allow configuration:

* Reward types
* Calculation types
* Minimum cart amount
* Percentage per category
* Lottery rules
* Popup messages
* Conversion ratios

---

# 🔗 HOMEPAGE ACCESS

Use Widget plugin to inject:

* "Start Group Purchase"
* "Join Group Purchase"

---

# 🧪 TESTING STRATEGY

1. Unit tests for:

   * Reward calculations
   * Rule matching
   * Wallet updates

2. Integration tests:

   * Cart → Convert → Join → Checkout

3. Edge cases:

   * Member leaves
   * Cart updates after join
   * Leader cancels
   * Partial payment

---

# 🚀 DEPLOYMENT STRATEGY

1. Install plugins in staging
2. Run migrations
3. Verify:

   * Cart flow
   * Order creation
   * API response
   * Wallet transactions
4. Load test group purchases
5. Deploy production

---

# ⚠️ CRITICAL WARNINGS

* Never modify nopCommerce core files.
* Use event consumers for order logic.
* Always log legal confirmations.
* Use transactions for reward calculations.
* Prevent double reward processing.

---

# 🔥 DEVELOPMENT PHASE PLAN

### Phase 1

✔ User Notification Plugin
✔ Amazing Discounts Page

### Phase 2

✔ Group Purchase Basic (Convert + Join + Unique ID)

### Phase 3

✔ Reward Engine

### Phase 4

✔ Wallet + Lottery

### Phase 5

✔ Leader Dashboard

### Phase 6

✔ Mobile API Integration


