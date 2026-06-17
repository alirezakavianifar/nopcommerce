# Phases 3–6 Implementation Status & Testing Guide

## Implementation Verification

All four phases have been **fully implemented** and the plugin builds with **0 errors**.

| Phase | Feature | Status | Key Files |
|-------|---------|--------|-----------|
| 3 | Reward Engine | ✅ Complete | `GroupRewardCalculationService.cs`, `OrderPlacedEventConsumer.cs`, `RewardRuleController.cs` |
| 4 | Wallet & Lottery | ✅ Complete | `WalletService.cs`, `LotteryService.cs`, `CustomerWalletAdminController.cs` |
| 5 | Leader Dashboard | ✅ Complete | `CustomerDashboardController.cs`, `Views/CustomerDashboard/`, `CustomerDashboardNavigationViewComponent.cs` |
| 6 | Mobile API | ✅ Complete | `GroupPurchaseApiController.cs` (6 endpoints) |

---

## Prerequisites

- .NET 9 SDK installed
- SQL Server (or SQLite for development) accessible and connection string configured in `src/Presentation/Nop.Web/appsettings.json`
- (Optional for Phase 6) Postman or cURL for API testing

---

## Running the Application

### Step 1 — Build the solution

```powershell
cd "e:\projects\nopCommerce_4.90.3_Source"
dotnet build src\NopCommerce.sln
```

Expected output: `Build succeeded. 0 Error(s)`

### Step 2 — Start the web app

```powershell
dotnet run --project src\Presentation\Nop.Web\Nop.Web.csproj
```

The app listens on:
- `http://localhost:5000`
- `https://localhost:5001`

Keep this terminal open throughout testing.

### Step 3 — Install the plugin

1. Open `https://localhost:5001` in your browser.
2. Log in as **Admin** (default: `admin@yourstore.com` / the password set during nopCommerce setup).
3. Go to **Configuration → Local plugins**.
4. Find **Misc.GroupPurchase** and click **Install**.
5. After the page reloads, click **Restart application** if prompted.

> The install process runs all database migrations automatically, creating the following tables:
> `GroupPurchase`, `GroupPurchaseMember`, `GroupPurchaseReward`, `RewardRule`,
> `CustomerWallet`, `WalletTransaction`, `LotteryPointTransaction`, `LegalConfirmationLog`

---

## Test Accounts Setup

Create two customer accounts before testing:

| Account | Role | Email example |
|---------|------|---------------|
| Customer A | Group Leader | `leader@test.com` |
| Customer B | Group Member (subgroup) | `member@test.com` |

Register both via the public storefront **Register** page.

---

## Phase 2 Quick Recap (Required Before Testing Phases 3–6)

These steps are prerequisites for reward triggers in Phases 3 and 4.

1. Log in as **Customer A**, add any product to the cart, go to the cart page, and click **"Start Group Purchase"** (injected widget at the bottom of the cart).
2. Accept the terms popup. Note the **Unique Code** shown (e.g., `A9F1B2C3`).
3. In a separate browser / incognito window, log in as **Customer B**, add a product to the cart, enter Customer A's code in the **"Join Group"** field, and accept terms.

---

## Phase 3 — Reward Engine

### What Was Implemented

- `RewardRule` domain entity — admin-configurable rules governing who earns what.
- `GroupRewardCalculationService` — evaluates rules against each order at checkout.
- `OrderPlacedEventConsumer` — hooks into `OrderPlacedEvent` without modifying core; calls the calculation service.
- `RewardRuleController` — full admin CRUD (List / Create / Edit / Delete).
- Views in `Views/RewardRule/` — `List.cshtml`, `Create.cshtml`, `Edit.cshtml`, `_CreateOrUpdate.cshtml`.

### Admin: Configure Reward Rules

1. Log in as **Admin**.
2. Navigate to **Configuration → Local plugins → Group Purchase → Manage Reward Rules**
   (Direct URL: `https://localhost:5001/Admin/RewardRule/List`).
3. Click **Add new rule** and create at least two rules:

**Rule 1 — Leader gets a fixed wallet credit:**

| Field | Value |
|-------|-------|
| Target Role | `Leader` |
| Reward Type | `WalletCredit` |
| Calculation Type | `Fixed` |
| Value | `10.00` |
| Min Cart Amount | `0` |
| Min Members | `1` |

**Rule 2 — Member earns lottery points:**

| Field | Value |
|-------|-------|
| Target Role | `Subgroup` |
| Reward Type | `LotteryPoints` |
| Calculation Type | `PercentageOfCartTotal` |
| Value | `5` (meaning 5%) |
| Min Cart Amount | `0` |
| Min Members | `1` |

### Trigger: Complete Checkouts

1. As **Customer B**, complete the checkout for the joined cart.
2. As **Customer A**, complete their checkout.

### Verification

- In your database, query `GroupPurchaseReward` — you should see two rows:
  - One for Customer A with `RewardType = WalletCredit`, `Amount = 10.00`
  - One for Customer B with `RewardType = LotteryPoints`, `Amount = (5% of cart total)`
- No changes should exist in any core nopCommerce table (`Order`, `OrderItem`, etc.).

---

## Phase 4 — Wallet & Lottery System

### What Was Implemented

- `CustomerWallet` & `WalletTransaction` domains — tracks balances separated by `WalletType` (`Regular` vs `GroupReward`).
- `LotteryPointTransaction` domain — cumulative points per customer.
- `WalletService` / `LotteryService` — atomic deposit operations called by the reward engine.
- `CustomerWalletAdminController` — admin list view of all live customer wallet balances.
- Views in `Views/CustomerWalletAdmin/` — `List.cshtml`.

### Customer Verification — Wallet Balance

1. Log in as **Customer A**.
2. Go to **My Account → My Wallet**
   (Direct URL: `https://localhost:5001/customer/wallet`).
3. Expected result:

```
Regular Balance:       £0.00
Group Reward Balance:  £10.00
```

### Customer Verification — Lottery Points

1. Log in as **Customer B**.
2. Go to **My Account → My Lottery Points**
   (Direct URL: `https://localhost:5001/customer/lottery`).
3. Expected result:

```
Total Lottery Points:  [5% of Customer B's cart total, as integer points]
```

### Admin Verification — Wallet Overview

1. Log in as **Admin**.
2. Navigate to `https://localhost:5001/Admin/CustomerWallet/List`.
3. A DataTables grid displays all customers with their wallet balances.
4. Confirm Customer A appears with a `GroupReward` balance of `10.00`.

---

## Phase 5 — Leader Dashboard

### What Was Implemented

- `CustomerDashboardController` — 4 actions: `Wallet`, `Lottery`, `LeaderGroups`, `SubgroupHistory`.
- Razor views for each action in `Views/CustomerDashboard/`.
- `CustomerDashboardNavigationViewComponent` — injected into the `AccountNavigationAfter` widget zone, adding dashboard links to the standard "My Account" sidebar for all registered users.
- Routes: `/customer/wallet`, `/customer/lottery`, `/customer/leader-groups`, `/customer/subgroup-history`.

### Test: Leader View

1. Log in as **Customer A** (the leader).
2. In the **My Account** sidebar, you should see four new navigation items:
   - My Wallet
   - My Lottery Points
   - My Leader Groups
   - My Subgroup History
3. Click **My Leader Groups** (`https://localhost:5001/customer/leader-groups`).
4. Expected result: A table listing the group Customer A created, showing:
   - Unique Code
   - Status (`Active`)
   - Created date
   - Delivery city
   - Member count (at least 1 — Customer B)

### Test: Subgroup Member View

1. Log in as **Customer B** (the member).
2. Click **My Subgroup History** (`https://localhost:5001/customer/subgroup-history`).
3. Expected result: A row showing:
   - The group's Unique Code
   - Join date / accepted date
   - Visibility type
   - Leader email (shown if `VisibilityType = Full`, otherwise `Hidden`)

### Test: Auth Guard

1. Open an incognito window (not logged in).
2. Navigate to `https://localhost:5001/customer/wallet`.
3. Expected result: Redirected to the nopCommerce login page (standard MVC `Challenge()` response).

---

## Phase 6 — Mobile API Integration

### What Was Implemented

- `GroupPurchaseApiController` at route prefix `/api/group-purchase`.
- 6 endpoints:

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/group-purchase/create` | Creates a new group purchase for the current customer |
| `POST` | `/api/group-purchase/join/{code}` | Joins an existing group by unique code |
| `GET`  | `/api/group-purchase/wallet` | Returns wallet balances as JSON |
| `GET`  | `/api/group-purchase/lottery` | Returns total lottery points as JSON |
| `GET`  | `/api/group-purchase/leader-groups` | Returns leader's group list as JSON |
| `GET`  | `/api/group-purchase/subgroup-history` | Returns member's join history as JSON |

- All endpoints return pure JSON — no HTML redirects.
- Unauthenticated requests return strict HTTP `401 Unauthorized`.

### Setup: Authenticate in Postman

nopCommerce uses cookie-based auth. To get a valid session in Postman:

1. In Postman, create a `POST` request to `https://localhost:5001/login`.
2. Body (form-data):
   - `Email` = `leader@test.com`
   - `Password` = `<password>`
   - `RememberMe` = `false`
3. Send the request — Postman will store the session cookie automatically.
4. All subsequent requests in the same Postman collection will be authenticated.

> **TLS Note:** If using `https://localhost:5001`, disable SSL certificate verification in Postman settings (Settings → General → SSL Certificate Verification → OFF).

### Test All Endpoints (as Customer A)

**GET wallet balances:**
```
GET https://localhost:5001/api/group-purchase/wallet
```
Expected response (`200 OK`):
```json
{
  "regularBalance": 0.00,
  "groupRewardBalance": 10.00
}
```

---

**GET lottery points:**
```
GET https://localhost:5001/api/group-purchase/lottery
```
Expected response (`200 OK`):
```json
{
  "totalPoints": 0
}
```
(Customer A earned wallet credit, not lottery points — Customer B would have points here.)

---

**GET leader groups:**
```
GET https://localhost:5001/api/group-purchase/leader-groups
```
Expected response (`200 OK`):
```json
[
  {
    "id": 1,
    "uniqueCode": "A9F1B2C3",
    "status": "Active",
    "createdOnUtc": "2026-02-24T10:00:00Z",
    "deliveryCity": "...",
    "membersCount": 1
  }
]
```

---

**GET subgroup history** (as Customer B):

Re-authenticate Postman as Customer B, then:
```
GET https://localhost:5001/api/group-purchase/subgroup-history
```
Expected response (`200 OK`):
```json
[
  {
    "id": 1,
    "uniqueCode": "A9F1B2C3",
    "status": "Active",
    "joinedOnUtc": "2026-02-24T11:00:00Z",
    "visibilityType": "Full",
    "leaderEmail": "leader@test.com"
  }
]
```

---

**POST create group** (as Customer A with a fresh cart):
```
POST https://localhost:5001/api/group-purchase/create
```
Expected response (`200 OK`):
```json
{
  "success": true,
  "code": "XXXXXXXX"
}
```

---

**POST join group** (as Customer B):
```
POST https://localhost:5001/api/group-purchase/join/XXXXXXXX
```
Expected response (`200 OK`):
```json
{
  "success": true
}
```

---

### Test: Authorization Failsafe

Clear cookies in Postman (or open a new collection with no session), then call any endpoint:

```
GET https://localhost:5001/api/group-purchase/wallet
```
Expected response:
```
HTTP 401 Unauthorized
```
Confirm: **no HTML body** is returned — only a bare 401 status. This is the headless-safe behavior required by mobile clients.

---

## Quick Route Reference

| Feature | URL |
|---------|-----|
| Admin — Group Purchase list | `/Admin/GroupPurchase/List` |
| Admin — Reward Rules | `/Admin/RewardRule/List` |
| Admin — Customer Wallets | `/Admin/CustomerWallet/List` |
| Public — Convert to Group Purchase | `/GroupPurchase/Convert` |
| Public — Join Group Purchase | `/GroupPurchase/Join` |
| Customer — My Wallet | `/customer/wallet` |
| Customer — My Lottery Points | `/customer/lottery` |
| Customer — My Leader Groups | `/customer/leader-groups` |
| Customer — My Subgroup History | `/customer/subgroup-history` |
| API — Group Purchase Base | `/api/group-purchase/` |
