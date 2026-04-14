# SpendWise 💳

### **Personal Finance & Subscription Tracker**

**SpendWise** is a lightweight, console-based backend application designed to help users monitor their monthly **"burn rate."** It distinguishes between one-time expenses (like food or transport) and recurring subscriptions (like Netflix or Gym memberships) to provide a clear view of financial health.

---

## 🚀 Key Features

* **Polymorphic Data Handling:** Manages two distinct transaction types (`OneTimeExpense` and `Subscription`) within a unified system.
* **Automated Monthly Forecasting:** Uses a predictive engine to calculate **fixed costs** for the next 30 days based on active billing cycles.
* **Data Persistence:** Implements a custom `System.IO` File I/O layer to save and load transaction history automatically in **CSV format**.
* **Advanced Filtering (LINQ):** Generates real-time category reports (**Food, Transport, Tech, Utilities, Entertainment**) and total spending summaries.
* **Robust Error Handling:** Features a **"Defensive Coding"** architecture with custom exceptions (`InsufficientDataException`) and input validation to prevent crashes.

---

## 🛠️ Technical Stack & Patterns

* **Language:** C# | .NET 10
* **Architecture:** **N-Tier / Clean Architecture** (Separation of UI, Service, and Models)
* **OOP Principles:**
    * **Interfaces:** Decoupled logic using `ITransactionService`.
    * **Inheritance:** Shared base properties between different transaction models.
    * **Polymorphism:** Dynamic object reconstruction during file loading using **Type Discriminators**.
* **Querying:** **LINQ** (Language Integrated Query) for data aggregation and filtering.

---

## 📂 Project Structure

```text
SpendWise/
├── Models/        # Domain entities and Enums (Category, Transaction)
├── Interfaces/    # Contracts for services (ITransactionService)
├── Services/      # Business logic and File I/O implementation
├── Exceptions/    # Custom error handling (InsufficientDataException)
├── UI/            # Console-based User Interface and input logic
└── Program.cs     # Application entry point
