Below is a **production-ready structural design** for your nopCommerce 4.90 implementation.

This includes:

1. ✅ Exact Database Schema (SQL Server – compatible with nopCommerce)
2. ✅ Class Diagram (Clean Architecture aligned with nop plugin pattern)
3. ✅ Recommended Folder Structure Template

All designed to avoid core modification and stay fully plugin-compliant.

---

# ✅ 1️⃣ EXACT DATABASE SCHEMA (SQL SERVER)

> Prefix used: `GP_` (Group Purchase)
> You can change prefix if needed.

---

# 🔹 GROUP PURCHASE CORE TABLES

---

## 1. GP_GroupPurchase

```sql
CREATE TABLE [dbo].[GP_GroupPurchase] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [LeaderCustomerId] INT NOT NULL,
    [UniqueCode] NVARCHAR(100) NOT NULL,
    [Status] INT NOT NULL, -- 0=Open,1=Completed,2=Cancelled
    [DeliveryCity] NVARCHAR(200) NULL,
    [DeliveryAddress] NVARCHAR(MAX) NULL,
    [TotalCombinedAmount] DECIMAL(18,4) NOT NULL DEFAULT 0,
    [TotalMembers] INT NOT NULL DEFAULT 1,
    [CreatedOnUtc] DATETIME2 NOT NULL,
    [CompletedOnUtc] DATETIME2 NULL
);

CREATE UNIQUE INDEX IX_GP_GroupPurchase_UniqueCode 
ON [dbo].[GP_GroupPurchase]([UniqueCode]);
```

---

## 2. GP_GroupPurchaseMember

```sql
CREATE TABLE [dbo].[GP_GroupPurchaseMember] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [GroupPurchaseId] INT NOT NULL,
    [CustomerId] INT NOT NULL,
    [IsLeader] BIT NOT NULL DEFAULT 0,
    [AcceptedTerms] BIT NOT NULL DEFAULT 0,
    [AcceptedOnUtc] DATETIME2 NULL,
    [VisibilityType] INT NOT NULL DEFAULT 0, -- 0=Full,1=Limited,2=None
    [CartTotal] DECIMAL(18,4) NOT NULL DEFAULT 0,
    [ParticipationCount] INT NOT NULL DEFAULT 0
);

CREATE INDEX IX_GP_Member_GroupPurchaseId 
ON [dbo].[GP_GroupPurchaseMember]([GroupPurchaseId]);

CREATE INDEX IX_GP_Member_CustomerId 
ON [dbo].[GP_GroupPurchaseMember]([CustomerId]);
```

---

## 3. GP_LegalConfirmationLog

```sql
CREATE TABLE [dbo].[GP_LegalConfirmationLog] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [GroupPurchaseId] INT NOT NULL,
    [Role] INT NOT NULL, -- 0=Leader,1=Subgroup
    [MessageShown] NVARCHAR(MAX) NOT NULL,
    [AcceptedOnUtc] DATETIME2 NOT NULL,
    [IPAddress] NVARCHAR(100) NULL
);

CREATE INDEX IX_GP_Legal_GroupPurchaseId
ON [dbo].[GP_LegalConfirmationLog]([GroupPurchaseId]);
```

---

# 🔹 REWARD SYSTEM TABLES

---

## 4. GP_RewardRule

```sql
CREATE TABLE [dbo].[GP_RewardRule] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Role] INT NOT NULL, -- 0=Leader,1=Subgroup
    [RewardType] INT NOT NULL, 
    -- 0=Wallet,1=Discount,2=NextDiscount,3=Gift,4=GiftCard,5=FreeShipping,6=Subscription

    [CalculationType] INT NOT NULL, 
    -- 0=Fixed,1=PercentCartTotal,2=PercentNetProfit

    [Value] DECIMAL(18,4) NOT NULL,
    [CategoryId] INT NULL,
    [MinCartAmount] DECIMAL(18,4) NULL,
    [MinMembers] INT NULL,
    [MaxCapAmount] DECIMAL(18,4) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedOnUtc] DATETIME2 NOT NULL
);
```

---

## 5. GP_GroupPurchaseReward

```sql
CREATE TABLE [dbo].[GP_GroupPurchaseReward] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [GroupPurchaseId] INT NOT NULL,
    [CustomerId] INT NOT NULL,
    [RewardRuleId] INT NOT NULL,
    [RewardType] INT NOT NULL,
    [CalculatedAmount] DECIMAL(18,4) NOT NULL,
    [CreatedOnUtc] DATETIME2 NOT NULL
);

CREATE INDEX IX_GP_Reward_GroupPurchaseId
ON [dbo].[GP_GroupPurchaseReward]([GroupPurchaseId]);
```

---

# 🔹 WALLET SYSTEM

---

## 6. GP_CustomerWallet

```sql
CREATE TABLE [dbo].[GP_CustomerWallet] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [AvailableBalance] DECIMAL(18,4) NOT NULL DEFAULT 0,
    [GroupRewardBalance] DECIMAL(18,4) NOT NULL DEFAULT 0,
    [LotteryPoints] INT NOT NULL DEFAULT 0,
    [UpdatedOnUtc] DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_GP_Wallet_CustomerId
ON [dbo].[GP_CustomerWallet]([CustomerId]);
```

---

## 7. GP_WalletTransaction

```sql
CREATE TABLE [dbo].[GP_WalletTransaction] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [Amount] DECIMAL(18,4) NOT NULL,
    [TransactionType] INT NOT NULL, 
    -- 0=Credit,1=Debit

    [SourceType] INT NOT NULL, 
    -- 0=GroupReward,1=Conversion,2=Manual,3=LotteryConversion

    [GroupPurchaseId] INT NULL,
    [Description] NVARCHAR(500) NULL,
    [CreatedOnUtc] DATETIME2 NOT NULL
);

CREATE INDEX IX_GP_WalletTransaction_CustomerId
ON [dbo].[GP_WalletTransaction]([CustomerId]);
```

---

# 🔹 LOTTERY SYSTEM

---

## 8. GP_LotteryTransaction

```sql
CREATE TABLE [dbo].[GP_LotteryTransaction] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [Points] INT NOT NULL,
    [SourceType] INT NOT NULL,
    -- 0=GroupPurchase,1=Referral,2=Manual,3=Conversion

    [GroupPurchaseId] INT NULL,
    [CreatedOnUtc] DATETIME2 NOT NULL
);

CREATE INDEX IX_GP_Lottery_CustomerId
ON [dbo].[GP_LotteryTransaction]([CustomerId]);
```

---

# ✅ 2️⃣ CLASS DIAGRAM (SIMPLIFIED ARCHITECTURE)

Below is a clean architectural diagram description.

---

## 🔷 Core Domain Layer

```
GroupPurchase
 ├── Id
 ├── LeaderCustomerId
 ├── UniqueCode
 ├── Status
 ├── Members (Collection<GroupPurchaseMember>)
 └── Rewards (Collection<GroupPurchaseReward>)

GroupPurchaseMember
 ├── CustomerId
 ├── IsLeader
 ├── VisibilityType
 └── CartTotal

RewardRule
 ├── Role
 ├── RewardType
 ├── CalculationType
 └── Value

GroupPurchaseReward
 ├── GroupPurchaseId
 ├── CustomerId
 └── CalculatedAmount

CustomerWallet
 ├── AvailableBalance
 ├── GroupRewardBalance
 └── LotteryPoints
```

---

## 🔷 Service Layer

```
IGroupPurchaseService
 ├── CreateGroup()
 ├── JoinGroup()
 ├── CompleteGroup()
 ├── GetGroupByCode()

IRewardCalculationService
 ├── CalculateRewards()
 ├── ApplyReward()

IWalletService
 ├── Credit()
 ├── Debit()
 ├── ConvertToLottery()

ILotteryService
 ├── AddPoints()
 ├── ConvertPointsToWallet()
```

---

## 🔷 Event Layer

```
GroupPurchaseOrderPlacedConsumer : IConsumer<OrderPlacedEvent>
 └── Trigger reward calculation
```

---

## 🔷 Controller Layer

```
GroupPurchaseController (Public)
GroupPurchaseApiController (Mobile)
GroupPurchaseAdminController (Admin)
WalletController
LotteryController
```

---

# ✅ 3️⃣ FOLDER STRUCTURE TEMPLATE (NOP PLUGIN)

Below is production-grade structure:

```
Plugins
 └── Nop.Plugin.Misc.GroupPurchase
      │
      ├── Controllers
      │     ├── GroupPurchaseController.cs
      │     ├── GroupPurchaseApiController.cs
      │     ├── GroupPurchaseAdminController.cs
      │     ├── WalletController.cs
      │
      ├── Domain
      │     ├── GroupPurchase.cs
      │     ├── GroupPurchaseMember.cs
      │     ├── RewardRule.cs
      │     ├── GroupPurchaseReward.cs
      │     ├── CustomerWallet.cs
      │     ├── WalletTransaction.cs
      │     ├── LotteryTransaction.cs
      │     └── LegalConfirmationLog.cs
      │
      ├── Data
      │     ├── GroupPurchaseBuilder.cs
      │     ├── RewardRuleBuilder.cs
      │     ├── WalletBuilder.cs
      │     └── Migration.cs
      │
      ├── Services
      │     ├── GroupPurchaseService.cs
      │     ├── RewardCalculationService.cs
      │     ├── WalletService.cs
      │     ├── LotteryService.cs
      │     └── LegalLogService.cs
      │
      ├── EventConsumers
      │     └── OrderPlacedConsumer.cs
      │
      ├── Models
      │     ├── GroupPurchaseViewModel.cs
      │     ├── LeaderDashboardModel.cs
      │     ├── WalletModel.cs
      │
      ├── Views
      │     ├── Public
      │     ├── Admin
      │     └── Shared
      │
      ├── Infrastructure
      │     ├── DependencyRegistrar.cs
      │     ├── RouteProvider.cs
      │     └── Permissions.cs
      │
      ├── plugin.json
      └── GroupPurchasePlugin.cs
```

---

# 🔥 BEST PRACTICES FOR NOP 4.90

* Use `INopStartup` for DI registration
* Use FluentMigrator for schema creation
* Use Event Consumers for order logic
* Always wrap reward logic in DB transaction
* Prevent duplicate reward processing with flag column
* Cache reward rules
* Log all legal confirmations


