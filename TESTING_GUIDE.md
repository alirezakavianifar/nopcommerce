# Running and Testing the NopCommerce Group Purchase Ecosystem

This guide explains how to spin up the nopCommerce application locally and perform end-to-end testing for all 6 phases of the Group Purchase integration as outlined in the `plan.md`.

---

## 1. Running the Application

1. Open a terminal (PowerShell or Command Prompt) and set your current directory to the project root:
   ```bash
   cd e:\projects\nopCommerce_4.90.3_Source\
   ```
2. Build and run the web presentation project:
   ```bash
   dotnet run --project src\Presentation\Nop.Web\Nop.Web.csproj
   ```
3. Wait for the host to start. It will typically listen on `http://localhost:5000` or `https://localhost:5001`.
4. Open the store in your browser.
5. **Plugin Installation (Crucial First Step):**
   - Log in with the standard **Admin** credentials.
   - Navigate to **Configuration → Local plugins**.
   - Find the custom plugins (`Misc.GroupPurchase`, `Misc.AmazingDiscounts`, `Misc.UserNotifications`).
   - Click the green **Install** button for each.
   - Restart the application to apply the routing and widget injection changes.

---

## 2. Testing Phase by Phase

### Phase 1: User Notifications & Amazing Discounts

**User Notifications**
1. **Admin Setup:** Navigate to `Admin → Promotions → System Announcements`.
2. Add a new announcement, set its active dates, and publish it.
3. **Verification:** Go to the public store homepage. You should see the announcement displayed correctly in the designated widget zone (e.g., Header or Homepage top).

**Amazing Discounts**
1. **Admin Setup:** Go to standard products and tag some as "Amazing". Alternatively, use the custom `AmazingDiscount` admin panel to list specific product priorities.
2. **Verification:** Navigate to the `/amazing-discounts` public route or click the auto-injected link in the website footer. Verify that the special promotional grid layout is rendering these products.

---

### Phase 2: Group Purchase Basic (Convert & Join)

1. **Convert to Group Purchase (The Leader):**
   - Log in as **Customer A**.
   - Add any product to the cart and proceed to the Shopping Cart page.
   - Scroll to the bottom widget area. Click the **"Start Group Purchase"** button.
   - Accept the legal terms in the popup.
   - **Result:** You will be assigned a Unique Group Code (e.g., `A9F1B2C3`), and your cart is now locked into this group.
2. **Join a Group Purchase (The Subgroup Member):**
   - Open a fresh incognito window and log in as **Customer B**.
   - Add a product to the cart.
   - On the Shopping Cart page, enter Customer A's Unique Code into the **"Join Group"** input field and submit.
   - Accept the terms.
   - **Result:** Customer B is successfully attached to Customer A's group purchase.

---

### Phase 3: Reward Engine

1. **Configure the Rules:**
   - Log in as Admin. Navigate to `Admin → Promotions → Misc.GroupPurchase → Manage Reward Rules`.
   - **Create Rule 1 (Leader):** Target Role: `Leader`, Reward Type: `WalletCredit`, Calc Type: `Fixed`, Value: `10.00`.
   - **Create Rule 2 (Member):** Target Role: `Subgroup`, Reward Type: `LotteryPoints`, Calc Type: `PercentageOfCartTotal`, Value: `5` (5% of cart).
2. **Trigger the Engine:**
   - Have **Customer B** complete their checkout.
   - Have **Customer A** complete their checkout.
3. **Verification:** The system seamlessly fired the `OrderPlacedEvent`. The `GroupRewardCalculationService` evaluated the rules and safely inserted `GroupPurchaseReward` logs into the database based on the users' roles.

---

### Phase 4: Wallet & Lottery System

Phase 4 automatically hooks into the Phase 3 backend to instantly deposit the earned rewards.

1. **Customer Dashboard Verification:**
   - **Customer A:** Navigate to **My Account → My Wallet**. The *Group Reward Balance* should correctly reflect the `10.00` deposit from Rule 1.
   - **Customer B:** Navigate to **My Account → My Lottery Points**. The points should reflect 5% of their order total from Rule 2.
2. **Admin Verification:**
   - Navigate to `Admin → Promotions → Misc.GroupPurchase → Manage Customer Wallets`.
   - The DataTables grid displays real-time balances for Customer A and Customer B.

---

### Phase 5: Leader Dashboard

1. **Leader Experience:**
   - Log in as **Customer A**. Go to **My Account → My Leader Groups**.
   - You will see the group you created, its unique code, execution status, and the number of members (Customer B) who joined.
2. **Subgroup Member Experience:**
   - Log in as **Customer B**. Go to **My Account → My Subgroup History**.
   - You will see the history of groups you joined, the timestamp, and Customer A's Email (if the visibility limits were configured to `Full`).

---

### Phase 6: Mobile API Integration

You can natively test the headless JSON endpoints using **Postman** or **cURL**. NopCommerce relies on Cookie-based or Token-based authentication, so ensure your REST client includes an active authenticated session.

1. **Test Native Data Fetching:**
   - Send `GET /api/group-purchase/wallet`
   - **Expected output:** HTTP `200 OK` with JSON: `{"regularBalance": 0.00, "groupRewardBalance": 10.00}`
   - Send `GET /api/group-purchase/lottery`
   - **Expected output:** HTTP `200 OK` with JSON: `{"totalPoints": 50}` (or whatever the 5% calculation yielded).
   - Send `GET /api/group-purchase/leader-groups`
   - **Expected output:** Triggers the native JSON serialization of the leader's group array without Razor views.

2. **Test Authorization Failsafes:**
   - Clear your cookies/tokens in Postman.
   - Send `POST /api/group-purchase/create`
   - **Expected output:** Strict HTTP `401 Unauthorized`. It will not return a bulky HTML redirect to the login page, proving it is a perfect headless responder for custom mobile applications.
