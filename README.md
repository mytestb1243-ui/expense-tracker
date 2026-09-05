# Expense Tracker

[![Build and Test](https://github.com/mytestb1243-ui/expense-tracker/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mytestb1243-ui/expense-tracker/actions/workflows/build-and-test.yml)

A small command-line expense tracker built as a learning project in C# / .NET 10.

It demonstrates a bunch of modern C# features in a real (if tiny) app:
records and `with` expressions, nullable reference types, generics with delegates,
LINQ (`GroupBy`, `Sum`, `Average`, `MaxBy`), switch expressions, `async`/`await`
file I/O, custom exceptions, coding against an interface for persistence, and
PBKDF2 password hashing for local accounts.

## Features

- Local accounts: register/log in with a username and password (PBKDF2-hashed,
  never stored in plaintext); each account's data is kept separate
- Add, view, edit, and delete expenses
- Spend summary: total, average, biggest, and a breakdown by category
- Filter by category or by date range, or search description/notes for text
- Monthly budgets per category, with a warning when you go over
- Monthly spending report with a trend indicator
- Export your expenses to CSV
- Data is persisted between runs as `users.json`, `expenses.<username>.json`,
  and `budgets.<username>.json`

## Project layout

```
First_Console_shafi/              # the app
├── Program.cs                    # menu loop + all the commands
├── Models/
│   ├── Expense.cs                # the Expense record
│   ├── Category.cs               # expense category enum
│   ├── User.cs                   # a local account
│   └── Budget.cs                 # a per-category monthly limit
├── Data/
│   ├── IExpenseStore.cs / JsonFileExpenseStore.cs   # expense persistence
│   ├── IUserStore.cs / JsonFileUserStore.cs         # account persistence
│   └── IBudgetStore.cs / JsonFileBudgetStore.cs     # budget persistence
├── Services/
│   ├── ExpenseService.cs         # expense validation (UI-free, testable)
│   ├── AuthService.cs            # register/login + password hashing
│   ├── BudgetService.cs          # budget validation + status checks
│   └── CsvExportService.cs       # CSV formatting/export
└── Exceptions/
    ├── InvalidExpenseException.cs
    ├── AuthenticationException.cs
    └── InvalidBudgetException.cs

First_Console_shafi.Tests/        # xUnit test project
```

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later

## Run

```bash
dotnet run --project First_Console_shafi
```

## Build

```bash
dotnet build
```

## Test

```bash
dotnet test
```
