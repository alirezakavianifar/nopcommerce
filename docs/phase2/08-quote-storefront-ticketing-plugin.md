# Quote: Native .NET Storefront Ticketing & Chat Plugin

**Document:** Phase 2 — Module H (Storefront Ticket Plugin Extension)  
**Quote validity:** 30 days from issue date  
**Prerequisite:** CRM API documentation (if integration required) and WebSocket server configuration

---

## Scope of Work

Implement an advanced ticketing and chat system as a native **nopCommerce 4.90.3 .NET Plugin**. This module will run directly within the storefront and admin panel, enabling customer-to-admin/vendor support workflows.

### Included

- **Storefront Support Center (Customer UI):**
  - Customer interface under "My Account" to view tickets.
  - "Submit Ticket" form with fields (Subject, Category, Priority, Message).
  - Secure file/image attachment upload.
  - Interactive thread history view.
- **Admin/Vendor Ticketing Desk:**
  - Standard admin list of all tickets with filtering (by Status, Priority, Vendor, Customer).
  - Ticket detail page for staff replies, internal notes, and reassigning ticket owners.
  - Vendor partitioning: Vendors can only see and reply to tickets relating to their products/orders.
- **Live Chat Capabilities:**
  - Real-time communication channel integrated via ASP.NET Core SignalR (using WebSockets/fallback protocols).
  - Real-time agent typing indicators and message status checks (read/unread).
- **Core Integrations:**
  - Sync with standard nopCommerce customer accounts, products, and order histories.
  - Standard email alerts to admin/vendor upon ticket opening, and to customers upon staff replies.

### Explicitly Excluded (Available under the broader Microservice/Laravel scope)

- The Flutter mobile client UI integrations (handled inside mobile developer scope).
- Hosting fees for dedicated WebSocket servers (if external SignalR hub services are chosen instead of local IIS WebSockets).

---

## Technical Approach

- **Extension approach:** Built as a standalone `Nop.Plugin.Misc.StorefrontTicketing` plugin, extending the nopCommerce database schema via `FluentMigrator` without modifying the core nopCommerce code.
- **Real-time framework:** Utilizes native ASP.NET Core SignalR hubs.
- **CRM Sync Bridge (Optional):** Exposes webhook callbacks or consumes background tasks (`IScheduledTask`) to push/pull updates between the nopCommerce database and the client's corporate CRM.

---

## Effort and Pricing

| Tier | Scope | Duration | Suggested Price |
|------|-------|----------|-----------------|
| **H1 — Basic Ticketing** | Ticketing system, UI in account/admin panel, email updates (No live chat/CRM). | 2 weeks | 6,000,000 Tomans |
| **H2 — Live Chat & WebSockets**| H1 + ASP.NET Core SignalR real-time chat interface for customers and admins. | 3–4 weeks | 10,000,000 Tomans |
| **H3 — CRM Integration** | H2 + two-way API synchronization bridge to external corporate CRM. | 4–5 weeks | 13,000,000 Tomans |

---

## Client Sign-Off

| Field | Response |
|-------|----------|
| Company | |
| Contact | |
| Date | |
| Selected Tier | ☐ H1 (Basic)  ☐ H2 (+ Live Chat)  ☐ H3 (+ CRM Sync) |
| Approved Budget | |
