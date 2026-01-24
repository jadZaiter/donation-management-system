\# Donation Management System



A web-based \*\*Donation Management System\*\* developed using \*\*ASP.NET Core MVC\*\* following \*\*Clean Architecture principles\*\*.  

The system allows users to create donation cases, donate with proof submission, and enables admins to review and approve cases and payments with real-time notifications.



---



\## 🧱 Architecture Overview (Clean Architecture)



The project is structured into \*\*4 main layers\*\*, each with a clear responsibility:



\### 1️⃣ Domain Layer

\- Contains core business entities (DonationCase, Payment, Donation, Comment)

\- Enums for statuses (CaseStatus, PaymentStatus)

\- No dependency on other layers



\### 2️⃣ Application Layer

\- Business logic and workflows

\- Services, interfaces, DTOs, and ViewModels

\- Examples:

&nbsp; - `PaymentWorkflow`

&nbsp; - `DonationCaseWorkflow`

\- Validation rules and business constraints live here



\### 3️⃣ Infrastructure Layer

\- Data access implementation

\- Entity Framework Core (ApplicationDbContext)

\- Services implementation (PaymentService, DonationCaseService)

\- Logging (Serilog)

\- Database migrations



\### 4️⃣ Web Layer

\- ASP.NET Core MVC UI

\- Controllers, Views, Razor Pages

\- Authentication \& Authorization (ASP.NET Identity)

\- SignalR Hubs

\- ViewModels for UI interaction



---



\## 🛠️ Tech Stack



\- ASP.NET Core MVC (.NET 8)

\- Entity Framework Core

\- SQL Server

\- ASP.NET Identity (Authentication \& Roles)

\- SignalR (Real-time notifications)

\- Serilog (Structured logging)

\- Background Services

\- Clean Architecture

\- Unit Testing (Application layer)



---



\## ✨ Main Features



\### 👤 Users

\- Register / Login

\- Submit donation cases

\- Upload images for cases

\- Donate to approved cases

\- Upload payment proof

\- View own cases and payments

\- Personal dashboard



\### 🛡️ Admin

\- Review \& approve/reject donation cases

\- Review \& approve/reject payment proofs

\- Admin dashboard with statistics

\- Real-time notifications when users upload payment proof (SignalR)



\### ⚙️ System Features

\- Background service to auto-close donation cases when target is reached

\- Server-side \& client-side validation

\- Optimized database queries (AsNoTracking, projections)

\- Structured logging using Serilog

\- Unit tests for business workflows



---



\## 🔔 Real-Time Notifications (SignalR)



\- Admins receive \*\*instant notifications\*\* when a user uploads a payment proof

\- No page refresh required

\- Implemented using SignalR Hubs and role-based groups



---



\## 🔐 Demo Accounts (For Evaluation Only)



> ⚠️ These credentials are \*\*for academic demonstration purposes only\*\*



\### Admin Account

\- \*\*Email:\*\* jad\_wb@hotmail.com  

\- \*\*Password:\*\* Jad\_123456789  

\- \*\*Role:\*\* Admin  



> 🔒 Please change the password after evaluation.



\### User Account

\- Register a new user from the registration page.



---



\## ▶️ How to Run the Project



1️⃣ Clone the repository:

```bash

git clone https://github.com/jadZaiter/donation-management-system.git
2️⃣ Open the solution in Visual Studio

3️⃣ Update database connection string in:

appsettings.json


4️⃣ Apply database migrations:

Update-Database -Context ApplicationDbContext


5️⃣ Run the project (F5)


