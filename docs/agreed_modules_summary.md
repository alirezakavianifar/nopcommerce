# Summary of Agreed Modules and Features

Based on the chronological chat records in [`docs/phase2/conversation.md`](file:///e:/projects/nopCommerce_4.90.3_Source/docs/phase2/conversation.md), the following modules and features were agreed upon across multiple development phases and budget revisions:

---

## Phase 1: Core Modules & Application Enhancements
**Initial Price:** 22,000,000 Tomans

1. **User Notifications Module & API**
   - Systemic and admin-driven notification posting mechanism targeting all users.
   - API endpoints supporting mobile application integration.

2. **Amazing Discounts Module (App Page & Layout Integration)**
   - Dedicated "Amazing Discounts" page inside the mobile application.
   - Dynamic links added to the footer/navbar with modern application visual styling.

3. **Group Buying Module**
   - **Cart Aggregation:** Customers can convert standard shopping carts into group buys, generating a unique shared Group Cart ID.
   - **Terms & Disclaimers:** Automated disclaimers for Group Leaders and subgroup members detailing delivery terms to the group leader's address.
   - **Admin Commission & Reward Rules:** Admin-configurable incentives (wallet top-ups, discounts on current/future purchases, gift cards, free shipping, or courier sub rewards).
   - **Flexible Calculation Logic:** Calculation based on fixed amounts, cart total percentages, or net profit percentages, with per-category percentage overrides (e.g., Supermarket vs. Digital Devices).
   - **Customer Club & Lottery System:** Earned group buy rewards can be converted into loyalty points and lottery entries.
   - **Profile Panels:** Dedicated profile tabs for "Group Leader" and "Subgroup Member" with privacy settings to toggle item visibility.

---

## Phase 2: Logistics & Advanced Shipping Expansion
**Added Price:** +5,000,000 Tomans  
**Cumulative Total:** 27,000,000 Tomans

4. **Advanced & Conditional Shipping Methods Module**
   - **4 Shipping Methods:** Courier, Freight/Transport, Cargo, and Express.
   - **Evaluation Priority:** System evaluates availability based on strict priority ordering:
     1. City Courier/Shipping Coverage
     2. Product Support
     3. Warehouse Support
   - **Dynamic Cost Calculation:** Admin-defined formulas incorporating fixed additions, percentage markups, min/max fee limits, or external API integrations.
   - **Multi-Warehouse / Multi-City Cart Handling:** Detection and handling of cart items originating from warehouses in different cities, prompting users or providing split shipping invoices.

---

## Phase 3: AI Capabilities, Marketplace Administration & Security
**Added Price:** +15,000,000 Tomans  
**Cumulative Total:** 42,000,000 Tomans

5. **Courier Web App Header Integration**
   - Dedicated header button and icon located in the main storefront web app to navigate directly to the Courier Web App.

6. **AI Visual & Voice Product Search**
   - Search engine supporting natural language text queries, voice input, and image uploads against indexed product metadata, specifications, and imagery.

7. **AI Duplicate Product Registration Prevention**
   - **Machine Learning Inspection:** Scans new vendor inventory submissions across names, photos, attributes, and descriptions.
   - **Conflict Workflow:** Prevents duplicate global product creation; flags existing matches to vendors/admins, pre-fills approved parameters, and allows vendor dispute/appeal submission for manual admin review.

8. **AI Online Live Chat Support Bot (Module D)**
   - Conversational AI trained on store products, courier policies, and order tracking.
   - Automated handoff capability to human support agents when queries exceed bot confidence or dataset limits.

9. **Vendor Panel Backup & Restore Module**
   - Vendor panel feature enabling marketplace sellers to generate and download backups of their own store inventory and panel data.
   - Admin review and approval mechanism prior to restoring vendor backups, with local disk storage management and automated retention purging.

10. **Two-Factor Authentication (2FA) & Admin Panel Access Control**
    - 2FA via SMS verification codes for user account authentication.
    - Admin panel access control enforcing IP address and MAC address whitelisting.

---

## Excluded / Postponed Items

- **Centralized Cross-Platform Ticketing System:** Although quoted during negotiations, the buyer explicitly postponed the ticketing module on July 3, 2026 (Message 445) to reduce accumulated costs prior to concluding current project deliverables.
