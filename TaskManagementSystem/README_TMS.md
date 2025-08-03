# 📌 Task Management System (TMS)

A web-based Task Management System built using **.NET**, designed to help users efficiently manage tasks with features like task creation, status tracking, and user management.

---

## 🚀 Features

✅ Create, update, and delete tasks  
✅ Track task statuses (e.g., pending, in progress, completed)  
✅ User authentication and session management  
✅ Middleware-based session restoration (`SessionRestoreMiddleware`)  
✅ Configuration via `appsettings.json`  
✅ Clean MVC architecture with dependency injection

---

## 🛠️ Technologies Used

- **.NET 7/8** (verify your actual .NET version)
- **C#**
- ASP.NET Core MVC/Web API
- Entity Framework Core (if used – you can add this if relevant)
- SQL Server (if applicable)

---

## 📂 Project Structure

```
TaskManagementSystem/
├── Program.cs                  # App entry point
├── SessionRestoreMiddleware.cs # Custom middleware for restoring sessions
├── appsettings.json            # App configuration
├── TaskManagementSystem.csproj # Project file
├── TaskManagementSystem.sln    # Solution file
└── wwwroot/                    # Static files (if any)
```

---

## ⚙️ Getting Started

### 1️⃣ Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (7.0 or newer)
- Visual Studio / VS Code

### 2️⃣ Clone the repository

```bash
git clone https://github.com/Shiven0204/TMS-TaskManagementSystem.git
cd TaskManagementSystem
```

### 3️⃣ Restore dependencies

```bash
dotnet restore
```

### 4️⃣ Build the project

```bash
dotnet build
```

### 5️⃣ Run the application

```bash
dotnet run
```

The application will start on `https://localhost:5001` (or the port configured in `launchSettings.json`).

---

## 🖥️ Usage

- Open your browser and navigate to `https://localhost:5001`.
- Log in (if authentication is implemented).
- Create, update, and track your tasks through the web interface.

---

## 📝 Configuration

Modify **appsettings.json** to update settings like database connection strings, app URLs, logging levels, etc.

---

## 🤝 Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you’d like to change.
