# 🩺 Pulse Medical API (Backend)

An advanced, production-ready healthcare management API built with **ASP.NET Core** utilizing a clean **3-Tier Architecture**. The system supports full **Localization (Arabic/English)** and orchestrates complex business workflows including multi-patient bookings, automated free revisions, identity-based profiling, and automated conditional Stripe refunds.

---

## 🚀 Advanced Business Logic & Features

All core healthcare operations, rules, and validation pipelines are securely encapsulated inside the **Business Logic Layer (Services)**:

### 1. Smart Appointment Pricing (New vs. 7-Day Revision)
The `AppointmentService` automatically evaluates patient booking history using EF Core before processing a transaction:
* **New Appointment:** Requires an active payment via Stripe.
* **Free Revision Appointment (مراجعة مجانية):** If a patient schedules a session with the *same doctor* at the *same branch* within **7 days** of a completed appointment, the system dynamically sets the cost to `0.00` and bypasses the Stripe payment intent pipeline.

### 2. Strict 12-Hour Cancellation & Automated Refunds
Patients can cancel their appointments, with rules applied directly via the backend logic:
* **Allowed Refund:** If the cancellation request is made **more than 12 hours before** the appointment time, the service calls the Stripe API directly to trigger an **automated full refund** to the patient's card, and marks the slot as cancelled.
* **Restricted Refund:** If the cancellation is attempted **within the 12-hour window**, the system rejects the refund, enforcing the clinic's tight scheduling policy.

### 3. Identity-Based Profiling (National ID Auto-Fill)
To optimize the onboarding flow, patient records are bound to their **National ID (رقم الهوية)**:
* When a user inputs a National ID, the backend queries the database. If a matching record exists, it automatically retrieves and returns the profile data (Full Name, Age, Phone, Medical History), preventing redundant data entry.

### 4. Multi-Patient Booking & Payment Coupling
* An authenticated user account can act as a "Sponsor" or primary booker. The system allows a single user to book and pay for **multiple different patients** (e.g., family members) under one session while cleanly tracking individual medical charts.
* Payments are explicitly bound to the transaction owner, ensuring accurate financial logging.

### 5. Full System Localization
* Built-in global supportive response framework handling **Arabic and English** translations for validation messages, error exceptions, and email templates based on request headers.

---

## 🏗️ Architectural Overview (3-Tier Structure)

The project is split into three modular logical layers to ensure a clean separation of concerns:

1.  **Presentation Layer (Web API):** Controllers exposing RESTful endpoints, handling HTTP requests, culture/language headers, and user authentication/authorization.
2.  **Business Logic Layer (BLL - Services):** The heart of the application containing services (e.g., `AppointmentService`, `PatientService`, `PaymentService`) where the 12-hour rule, pricing algorithms, identity auto-fill queries, and direct Stripe SDK calls are processed.
3.  **Data Access Layer (DAL):** Driven directly by **Entity Framework Core (EF Core)**. It uses a centralized `ApplicationDbContext` to communicate with SQL Server, omitting the repository layer for maximum query optimization and clean LINQ operations.

---

## 🛠️ Tech Stack

* **Framework:** ASP.NET Core Web API (C#)
* **Architecture:** 3-Tier Architecture (API ➔ Services ➔ EF Core / DB)
* **Database & ORM:** SQL Server via Entity Framework Core (EF Core)
* **Payment Integration:** Stripe Dotnet SDK (Direct API Client-Side Confirmation Workflow)
* **Security:** JWT Authentication with Role-Based Access Control
* **Internationalization:** .NET Localization Capabilities (`IStringLocalizer`)

---

## 🔧 Getting Started & Local Setup

### Prerequisites
* .NET 8.0 SDK or later
* SQL Server Express / LocalDB

### Configuration (`appsettings.json`)
Configure your database connection and sensitive gateway keys in the main API project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=PulseMedicalDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "YOUR_SUPER_SECRET_JWT_KEY_GENERATED_FOR_VALIDATION",
    "Issuer": "PulseMedical",
    "Audience": "PulsePatients"
  },
  "Stripe": {
    "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY"
  }
}
