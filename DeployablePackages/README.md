# nopCommerce 4.90 Modular Plugin Packages

This directory contains standalone, isolated installable `.zip` packages for nopCommerce 4.90. These packages are completely self-contained and ready to be uploaded and installed on any standard nopCommerce 4.90 store.

---

## 📦 Included Module Packages

| Package File | Module Title | Primary Functionality |
| :--- | :--- | :--- |
| **`Misc.UserNotifications.zip`** | **User Notifications & Mobile API** | System-wide & admin announcements, storefront top ticker bar, popup modals, customer inbox, and mobile REST API endpoints. |
| **`Misc.AmazingDiscounts.zip`** | **Amazing Discounts & Mobile Nav** | Dedicated `/amazing-discounts` landing page, countdown timers, stock progress bars, and mobile footer navigation integration. |
| **`Misc.GroupPurchase.zip`** | **Advanced Group Buying & Wallet** | Group cart assembly, leader liability confirmation modals, legal audit logs, and customer wallet milestone rewards. |
| **`Shipping.ConditionalMethods.zip`** | **Conditional Shipping Engine** | Multi-tier shipping rules, city/warehouse cart mapping, express shipping rules, and multi-city warning modals. |
| **`Misc.ArtificialIntelligence.zip`** | **AvalAI AI Integration** | AI-driven visual/voice search, intelligent customer support chatbot, and duplicate product prevention. |
| **`Misc.SellerMarketing.zip`** | **Vendor Support & Marketing** | Vendor portal for uploading products, marketing assets, and admin approval workflows. |

---

## 🚀 Installation Instructions

### Method 1: Web Admin Upload (Recommended — No Server Access Needed)

1. Log in to the **nopCommerce Admin Panel** (`/admin`).
2. In the left navigation menu, navigate to **Configuration** → **Local plugins**.
3. In the upper-right corner of the page, click **Upload plugin or theme**.
4. In the file picker dialog, select the `.zip` package you wish to install (e.g., `Misc.UserNotifications.zip`).
5. After the file finishes uploading, nopCommerce will prompt you to restart the application. Click **Restart application to apply changes**.
6. After restart, locate the newly uploaded module in the **Local plugins** table.
7. Click the **Install** button next to the plugin.
8. Click **Restart application to apply changes** at the top of the page to finalize the installation.

> [!NOTE]
> All database tables, schema migrations, permissions, and localized string resources are created automatically when you click **Install**. No manual SQL scripts are required.

---

### Method 2: Manual / FTP Upload (Server File System)

1. Extract the `.zip` archive on your computer. You will get a folder matching the module name (e.g., `Misc.UserNotifications`).
2. Connect to your web server via FTP, SSH, or Remote Desktop.
3. Upload the extracted folder directly into the store's `/Plugins/` directory (e.g., `/Plugins/Misc.UserNotifications`).
4. Log into the nopCommerce Admin Panel and go to **Configuration** → **Local plugins**.
5. Click **Reload list of plugins** in the top-right corner (or restart the application pool).
6. Find the module in the grid, click **Install**, and restart the application.

---

## ⚙️ Post-Installation Navigation

Once installed, the modules will automatically appear in the appropriate admin sections:
- **User Notifications**: Admin Menu → **Promotions** → **Announcements & Notifications**
- **Amazing Discounts**: Admin Menu → **Promotions** → **Amazing Discounts**
- **Group Purchase**: Admin Menu → **Promotions** → **Group Purchases & Rewards**
- **Conditional Shipping**: Admin Menu → **Configuration** → **Shipping** → **Shipping providers**
