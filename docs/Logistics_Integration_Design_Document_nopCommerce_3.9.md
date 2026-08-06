# Integration Analysis and Design Document
## nopCommerce 3.9 with Logistics, Warehouse, Courier, and Delivery System
**Proposed Version:** 1.0  
**Document Scope:** Shipping method inquiry, multi-warehouse grouping, delivery scheduling, estimated cost calculation, and final selection logging  
**Creation Date:** 2026 / 1405  

---

## Table of Contents
1. [Objective and Scope](#1-objective-and-scope)
2. [Architectural Principles and System Responsibilities](#2-architectural-principles-and-system-responsibilities)
3. [Glossary and Core Entities](#3-glossary-and-core-entities)
4. [End-to-End Cart-to-Delivery Workflow](#4-end-to-end-cart-to-delivery-workflow)
5. [Identifier Mapping and Reference Data](#5-identifier-mapping-and-reference-data)
6. [Shipping Options Inquiry API Contract](#6-shipping-options-inquiry-api-contract)
7. [Multi-Warehouse Model and Shipment Groups](#7-multi-warehouse-model-and-shipment-groups)
8. [Selection, Reservation, and Re-Validation](#8-selection-reservation-and-re-validation)
9. [Logistics Order Creation and Status Tracking](#9-logistics-order-creation-and-status-tracking)
10. [Errors, Security, Retry, and Idempotency](#10-errors-security-retry-and-idempotency)
11. [Sample Scenarios](#11-sample-scenarios)
12. [Test Scenarios and Acceptance Criteria](#12-test-scenarios-and-acceptance-criteria)
13. [Current Module Refactoring Plan](#13-current-module-refactoring-plan)

---

## 1. Objective and Scope

The objective of this document is to define the standard contract and workflow between the nopCommerce 3.9-based storefront and the dedicated logistics system. The store module must collect accurate cart details and customer destination data, forwarding them to the logistics system for decision-making. The logistics system calculates and returns valid shipping options based on warehouse locations, inventory, destination, courier capacity, carriers, holidays, and item restrictions.

### Expected Output for the User
* Selectable shipping methods such as dedicated courier, standard post, Tipax, or special freight.
* Approximate delivery date or time range for each option.
* Selectable time slots (e.g., Tir 2nd between 12:00 and 16:00).
* Estimated or fixed shipping cost for each option.
* Transparent information regarding multi-shipment orders and item allocation per shipment.
* Clear messaging if no valid shipping method exists for a portion of the cart.

### Out of Scope for Version 1
* Real-time driver route optimization and driver mobile application.
* Financial settlement with carriers.
* Comprehensive return management; however, necessary identifiers and workflows are anticipated for future development.

---

## 2. Architectural Principles and System Responsibilities

The core principle of this design is that the nopCommerce module acts strictly as an **Adapter**, preventing the replication of logistics logic within the storefront.

| System | Core Responsibilities |
| :--- | :--- |
| **nopCommerce / Integration Plugin** | Reading cart, customer, and address data; transforming data into the integration contract; displaying options; storing user selections; sending final orders and receiving status updates. |
| **Logistics System** | Warehouse resolution and shipment grouping; evaluating carrier eligibility; calculating dates, time slots, and costs; reserving capacity; creating shipments; managing delivery status. |
| **Customer** | Selecting address, shipping method, and time slot; confirming costs; placing orders and completing payments. |
| **Store Manager** | Managing mappings, connection settings, monitoring errors, and executing retry operations. |

### Decision-Making Rule
The storefront must not independently decide whether a destination is serviceable by courier, whether a time slot has capacity, or what the shipping cost is. All such decisions must derive from the valid response of the logistics system.

---

## 3. Glossary and Core Entities

| Term | Definition |
| :--- | :--- |
| **Cart** | Current shopping cart of the user in nopCommerce. |
| **Cart Item** | A single item row with selected quantity and attributes. |
| **Warehouse** | Warehouse or fulfillment supply point. |
| **Shipment Group** | A group of items eligible for shipping in a single shipment or shared route. |
| **Delivery Option** | A selectable option at the level of the entire cart or a shipment group. |
| **Time Slot** | Reservable date and time window for delivery. |
| **Quote** | Temporary shipping inquiry result with an expiration time. |
| **Selection** | User's choice among returned options and time slots. |
| **Logistics Order** | Final order record within the logistics system. |
| **External Code** | Persistent identifier between systems for city, province, warehouse, and shipping method. |

---

## 4. End-to-End Cart-to-Delivery Workflow

1. Customer adds items to the cart.
2. Delivery address is selected or entered on the cart or checkout page.
3. The module prepares cart information, physical item characteristics, warehouse IDs, and destination data.
4. The module sends a Quote request to the logistics system.
5. Logistics groups items based on warehouses and operational rules.
6. Valid options including method, date, time slot, and cost are returned.
7. Customer selects an option; a Time Slot is chosen if required.
8. Before order placement or payment, the Quote is re-validated.
9. Upon successful store order placement, Logistics Orders and Shipments are created.
10. Shipping status changes are transmitted to the store via Polling or Webhook.

### Textual Workflow Diagram
```text
Customer Cart + Address
        |
        v
nopCommerce 3.9 Integration Plugin
        |
        | POST /delivery-quotes
        v
Logistics Engine
  - Warehouse resolution
  - Shipment grouping
  - Method eligibility
  - Capacity / calendar
  - Cost estimation
        |
        v
Delivery Options + Time Slots + Estimated Cost
        |
        v
User Selection -> Validate -> Create Logistics Order
```

---

## 5. Identifier Mapping and Reference Data

Issues from previous versions—such as incorrect city/province returns or mixing warehouse IDs with addresses—demonstrate that direct dependency on internal database IDs is hazardous. Internal nopCommerce and logistics identifiers must not be assumed equal.

### Proposed Mapping Structure

| Type | nopCommerce ID | Persistent External Code | Logistics ID | Example |
| :--- | :--- | :--- | :--- | :--- |
| **Province** | `StateProvinceId` | `IR-TEH` | 8 | Tehran |
| **City** | `CityId` or Attribute | `IR-TEH-TEHRAN` | 110 | Tehran |
| **Warehouse** | `WarehouseId` | `WH-TEH-01` | 7 | Tehran Central Warehouse |
| **Shipping Method** | `ShippingMethodId` | `COURIER` | 3 | Dedicated Courier |

### Reference Data Requirements
* Province and city synchronization endpoints must return ID, ExternalCode, Persian name, and active status.
* Warehouses must be returned as independent objects containing `warehouseId`, `externalCode`, and `address`; warehouse IDs must not be embedded inside address fields.
* Mappings must be viewable and editable within the module management panel.
* In the absence of a mapping, the system must return a clear, traceable error; guessing values is strictly prohibited.

```json
{
  "warehouseId": 7,
  "externalCode": "WH-TEH-01",
  "name": "انبار مرکزی تهران",
  "address": {
    "provinceCode": "IR-TEH",
    "cityCode": "IR-TEH-TEHRAN",
    "postalCode": "...",
    "addressLine": "تهران، ...",
    "latitude": 35.70,
    "longitude": 51.40
  }
}
```

---

## 6. Shipping Options Inquiry API Contract

### Proposed Endpoint
`POST /api/v1/integration/delivery-quotes`

**Objective:** Receive cart and destination details and return all valid shipping combinations instantly.

### Sample Request
```json
{
  "requestId": "c69492d1-63bd-4f08-91db-14a5831ef001",
  "storeId": "STORE-1",
  "cartId": "CART-9001",
  "customer": {
    "customerId": "852",
    "mobile": "09121234567"
  },
  "destination": {
    "addressId": "990",
    "receiverName": "علی رضایی",
    "mobile": "09121234567",
    "provinceCode": "IR-TEH",
    "provinceName": "تهران",
    "cityCode": "IR-TEH-TEHRAN",
    "cityName": "تهران",
    "postalCode": "1234567890",
    "addressLine": "خیابان مثال، پلاک ۱۰",
    "latitude": 35.7001,
    "longitude": 51.4002
  },
  "currency": "IRR",
  "items": [
    {
      "cartItemId": "501",
      "productId": "145",
      "sku": "SKU-145",
      "productName": "کالای نمونه A",
      "quantity": 2,
      "warehouseCode": "WH-TEH-01",
      "unitWeightGrams": 1500,
      "dimensionsCm": { "length": 30, "width": 20, "height": 15 },
      "unitPrice": 25000000,
      "requiresShipping": true,
      "isFragile": false
    },
    {
      "cartItemId": "502",
      "productId": "220",
      "sku": "SKU-220",
      "productName": "کالای نمونه B",
      "quantity": 1,
      "warehouseCode": "WH-KRJ-01",
      "unitWeightGrams": 800,
      "dimensionsCm": { "length": 20, "width": 15, "height": 10 },
      "unitPrice": 18000000,
      "requiresShipping": true,
      "isFragile": true
    }
  ]
}
```

### Sample Response (Cart Level)
```json
{
  "isSuccess": true,
  "quoteId": "QT-20260721-10025",
  "expiresAtUtc": "2026-07-21T13:30:00Z",
  "currency": "IRR",
  "options": [
    {
      "optionId": "OPT-ECONOMY-1",
      "title": "ارسال اقتصادی",
      "description": "دو مرسوله؛ پست و تیپاکس",
      "totalEstimatedCost": 4400000,
      "estimatedDeliveryFrom": "2026-07-23",
      "estimatedDeliveryTo": "2026-07-26",
      "requiresTimeSlot": false,
      "shipments": [
        {
          "shipmentGroupId": "SG-1",
          "warehouseCode": "WH-TEH-01",
          "cartItemIds": ["501"],
          "methodCode": "POST",
          "methodTitle": "پست پیشتاز",
          "estimatedCost": 1800000
        },
        {
          "shipmentGroupId": "SG-2",
          "warehouseCode": "WH-KRJ-01",
          "cartItemIds": ["502"],
          "methodCode": "TIPAX",
          "methodTitle": "تیپاکس",
          "estimatedCost": 2600000
        }
      ]
    },
    {
      "optionId": "OPT-FAST-1",
      "title": "ارسال سریع",
      "totalEstimatedCost": 3200000,
      "estimatedDeliveryFrom": "2026-07-22",
      "estimatedDeliveryTo": "2026-07-22",
      "requiresTimeSlot": true,
      "timeSlots": [
        {
          "slotId": "SLOT-101",
          "date": "2026-07-22",
          "from": "12:00",
          "to": "16:00",
          "additionalCost": 0,
          "capacityStatus": "Available"
        },
        {
          "slotId": "SLOT-102",
          "date": "2026-07-22",
          "from": "16:00",
          "to": "20:00",
          "additionalCost": 200000,
          "capacityStatus": "Limited"
        }
      ]
    }
  ],
  "warnings": []
}
```

### Required Response Fields

| Field | Requirement | Explanation |
| :--- | :--- | :--- |
| `quoteId` | Mandatory | Unique inquiry result identifier. |
| `expiresAtUtc` | Mandatory | Expiration timestamp for price and capacity. |
| `optionId` | Mandatory | Selectable option identifier. |
| `totalEstimatedCost` | Mandatory | Total displayable cost. |
| `shipments` | Mandatory for multi-warehouse carts | Breakdown of groups and items. |
| `timeSlots` | Conditional | Required when the method requires time slot reservation. |
| `warnings` | Optional | Non-blocking warnings. |
| `unavailableItems` | Conditional | Items that cannot be shipped and the reason why. |

---

## 7. Multi-Warehouse Model and Shipment Groups

A cart can be fulfilled from multiple warehouses. The logistics system must specify which items fall into a single shipment and whether consolidation is possible.

### UX Recommendation
In Version 1, instead of separate selections for each warehouse, the user views combined options at the entire cart level, such as "Economic", "Fast", and "Scheduled Delivery". Detailed shipments remain visible beneath each option.

| Scenario | Proposed Behavior |
| :--- | :--- |
| Two items from one warehouse | Single Shipment Group; shared methods and timing. |
| Two items from two nearby warehouses | If consolidation is possible, both single-shipment and multi-shipment options can be offered. |
| Bulky item + Normal item | Two independent groups; special freight for bulky and post/courier for normal. |
| Single unshippable item | The entire quote may fail or offer a separate purchase option; the decision must be product-driven and consistent. |

```json
{
  "shipmentGroups": [
    {
      "shipmentGroupId": "SG-1",
      "warehouseCode": "WH-TEH-01",
      "cartItemIds": ["501", "503"],
      "constraints": []
    },
    {
      "shipmentGroupId": "SG-2",
      "warehouseCode": "WH-ESF-01",
      "cartItemIds": ["502"],
      "constraints": ["FRAGILE"]
    }
  ]
}
```

---

## 8. Selection, Reservation, and Re-Validation

Upon user selection, the module must store `quoteId`, `optionId`, and `slotId` in the session or a dedicated module table. This selection remains valid until expiration, and any change to address, quantities, or cart items invalidates it.

### Temporary Selection Recording
`POST /api/v1/integration/delivery-selections`

```json
{
  "cartId": "CART-9001",
  "quoteId": "QT-20260721-10025",
  "optionId": "OPT-FAST-1",
  "timeSlotId": "SLOT-101"
}
```

### Pre-Order Validation
`POST /api/v1/integration/delivery-quotes/QT-20260721-10025/validate`

```json
{
  "cartId": "CART-9001",
  "optionId": "OPT-FAST-1",
  "timeSlotId": "SLOT-101",
  "cartHash": "sha256:..."
}
```

### Sample Response on Capacity Exhaustion
```json
{
  "isValid": false,
  "code": "TIME_SLOT_CAPACITY_EXHAUSTED",
  "message": "بازه انتخاب‌شده دیگر ظرفیت ندارد.",
  "requiresRequote": true
}
```

---

## 9. Logistics Order Creation and Status Tracking

Following successful order creation in nopCommerce, the module must submit an idempotent request to create the logistics order. This operation is recommended post-successful payment or per store commercial policy.

`POST /api/v1/integration/logistics-orders`  
`Idempotency-Key: nop-order-78512`

```json
{
  "storeOrderId": "78512",
  "storeOrderNumber": "ORD-20260721-78512",
  "quoteId": "QT-20260721-10025",
  "optionId": "OPT-FAST-1",
  "timeSlotId": "SLOT-101",
  "paymentStatus": "Paid",
  "destination": { "...": "same validated address" },
  "items": [ "...final order items..." ],
  "shippingAmount": 3200000,
  "currency": "IRR"
}
```

```json
{
  "isSuccess": true,
  "logisticsOrderId": "LG-800025",
  "shipments": [
    {
      "shipmentId": "SHP-1001",
      "shipmentGroupId": "SG-1",
      "trackingCode": null,
      "status": "PendingAllocation"
    },
    {
      "shipmentId": "SHP-1002",
      "shipmentGroupId": "SG-2",
      "trackingCode": null,
      "status": "PendingAllocation"
    }
  ]
}
```

### Proposed Status Codes

| Status Code | Persian Title | Explanation |
| :--- | :--- | :--- |
| `PendingAllocation` | در انتظار تخصیص | Order registered but courier or carrier not yet assigned. |
| `ReadyForPickup` | آماده برداشت | Warehouse ready for courier pickup. |
| `PickedUp` | تحویل به حمل‌کننده | Shipment dispatched from warehouse. |
| `InTransit` | در مسیر | Shipment in transit. |
| `OutForDelivery` | در حال توزیع | Out for final customer delivery. |
| `Delivered` | تحویل‌شده | Delivery completed successfully. |
| `DeliveryFailed` | ناموفق | Delivery failed, action required. |
| `Canceled` | لغوشده | Shipment canceled. |

---

## 10. Errors, Security, Retry, and Idempotency

### Proposed HTTP Status Codes

| HTTP | Usage | Sample Domain Code |
| :--- | :--- | :--- |
| `200` | Successful operation | `QUOTE_CREATED` |
| `400` | Invalid structure or value | `INVALID_ADDRESS` |
| `401/403` | Invalid authentication or authz | `INVALID_API_KEY` |
| `404` | ID or Mapping not found | `WAREHOUSE_MAPPING_NOT_FOUND` |
| `409` | Conflict or expired quote | `QUOTE_EXPIRED` |
| `422` | Valid request but unprocessable | `NO_DELIVERY_OPTION` |
| `429` | Rate limit exceeded | `RATE_LIMITED` |
| `502/503` | Temporary dependency service failure | `CARRIER_UNAVAILABLE` |

### Standard Error Sample
```json
{
  "isSuccess": false,
  "statusCode": 422,
  "code": "NO_DELIVERY_OPTION",
  "message": "برای مقصد و اقلام انتخاب‌شده روش ارسال فعالی وجود ندارد.",
  "traceId": "01J...",
  "details": [
    {
      "cartItemId": "502",
      "reasonCode": "OUT_OF_SERVICE_AREA",
      "message": "کالای حجیم به این شهر ارسال نمی‌شود."
    }
  ]
}
```

### Technical Requirements
* All requests must run over HTTPS using API Key or service-to-service JWT.
* Credentials must never appear in general configuration files or logs.
* Use `Idempotency-Key` when creating Logistics Orders.
* Retries permitted solely for temporary errors 429, 502, 503, and timeouts with exponential backoff.
* `requestId` and `traceId` must be logged and searchable across both systems.
* Sensitive customer information in logs must be masked.
* Quote endpoints must enforce a defined timeout (e.g., 3 to 5 seconds) with a fail-safe UX design.

---

## 11. Sample Scenarios

* **Scenario A: Destination Tehran and all items in Tehran warehouse**  
  Cart contains two items from `WH-TEH-01`, and the customer is within courier coverage. Logistics returns three options: scheduled courier, instant courier, and post. The user selects the 12–16 courier. Cost is 120k Tomans, and the slot is reserved for 10 minutes.
* **Scenario B: Destination in a city without courier coverage**  
  Cart is identical, but destination is Mashhad. Courier option is dropped, leaving only Post and Tipax with estimated dates and independent costs.
* **Scenario C: Multi-warehouse cart**  
  Item A ships from Tehran and Item B from Karaj. Logistics creates two shipment groups and returns an all-cart "Fast" option for a total cost of 320k Tomans. The user sees a single selection, but details indicate the order fulfills in two shipments.
* **Scenario D: Cart modification after selection**  
  User changes item A's quantity after selecting a slot. The module detects a changed `cartHash`, invalidates the previous selection, and fetches a new quote.
* **Scenario E: Unshippable single item**  
  Bulky item ships only within Tehran, but destination is Shiraz. Response 422 includes `cartItemId` and `reasonCode`. The site highlights the specific item and reason, guiding the user to remove or change the address.

---

## 12. Test Scenarios and Acceptance Criteria

| ID | Test Scenario | Expected Result |
| :--- | :--- | :--- |
| `T01` | Single item, single warehouse, destination inside courier coverage | Courier displayed with Time Slot and cost. |
| `T02` | Destination outside courier coverage | Courier hidden; Post/Tipax available. |
| `T03` | Two warehouses | Shipment groups and total cost correct. |
| `T04` | City without mapping | Error `CITY_MAPPING_NOT_FOUND` with clear message. |
| `T05` | Warehouse without external code | Error `WAREHOUSE_MAPPING_NOT_FOUND`; no guessed ID usage. |
| `T06` | Expired quote | Validate returns `QUOTE_EXPIRED` and requires re-quote. |
| `T07` | Filled time slot | Selection rejected with alternative slots suggested. |
| `T08` | Item quantity change | Previous quote invalidated. |
| `T09` | Duplicate order creation submission | With `Idempotency-Key`, only one Logistics Order is created. |
| `T10` | Logistics timeout | Temporary message and retry option; log `TraceId`. |
| `T11` | Persian province and city | Correct name and external code visible in request. |
| `T12` | Warehouse and address | `warehouseId` and `address` reside in independent fields. |

### Version 1 Acceptance Criteria
* All primary checkout flows operate without local shipping method calculation.
* Each option clearly indicates method, date, cost, and time slot when applicable.
* City, province, and warehouse mappings are testable and manageable.
* User selections automatically invalidate upon cart or address modification.
* Duplicate logistics orders are prevented.
* All module and logistics endpoints documented in Swagger/OpenAPI with objective, mandatory/optional parameters, request/response samples, status codes, errors, and complete workflow.

---

## 13. Current Module Refactoring Plan

1. Audit previous module code and extract all endpoints, tables, and checkout hooks.
2. Refactor city, province, and warehouse models to include persistent `ExternalCode`.
3. Create settings and mapping tables within the nopCommerce module.
4. Implement `DeliveryQuoteClient` with timeout, retry, and standard logging.
5. Integrate with Cart/Checkout page and re-quote upon cart or address changes.
6. Display options and time slots, storing selections in session/DB.
7. Final pre-order validation.
8. Send idempotent final order to logistics.
9. Implement error management page, manual retry, and `TraceId` viewing.
10. Execute stage environment acceptance tests with single- and multi-warehouse scenarios.

### Proposed Module Tables

| Table | Usage |
| :--- | :--- |
| `LogisticsIntegrationSettings` | API URL, key, timeout, and retry policy. |
| `LogisticsLocationMapping` | Country, province, and city mappings. |
| `LogisticsWarehouseMapping` | Mapping nopCommerce warehouse to logistics code. |
| `LogisticsCartSelection` | Quote and temporary user selection. |
| `LogisticsOrderLink` | Linking store order to logistics order and shipments. |
| `LogisticsIntegrationLog` | Summary log of requests, results, errors, and trace IDs excluding sensitive data. |

---

## Conclusion
The nopCommerce 3.9 module serves as a reliable integration layer: collecting correct cart and destination data, delegating decisions to logistics, displaying selectable options without alteration, and recording final selections securely and idempotently. Through this separation, rules for courier, post, Tipax, capacity, calendar, cost, and multi-warehouse fulfillment reside in a single location—the logistics system—eliminating previous errors stemming from ambiguous identifiers and scattered logic.
