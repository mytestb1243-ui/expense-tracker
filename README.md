# Expense Tracker

[![Build and Test](https://github.com/mytestb1243-ui/expense-tracker/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mytestb1243-ui/expense-tracker/actions/workflows/build-and-test.yml)

A small command-line expense tracker built as a learning project in C# / .NET 10.

It demonstrates a bunch of modern C# features in a real (if tiny) app:
records and `with` expressions, nullable reference types, generics with delegates,
LINQ (`GroupBy`, `Sum`, `Average`, `MaxBy`), switch expressions, `async`/`await`
file I/O, custom exceptions, and coding against an interface for persistence.

## Features

- Add, view, edit, and delete expenses
- Spend summary: total, average, biggest, and a breakdown by category
- Filter by category or by date range
- Data is persisted to `expenses.json` between runs

## Project layout

```
First_Console_shafi/              # the app
├── Program.cs                    # menu loop + all the commands
├── Models/
│   ├── Expense.cs                # the Expense record
│   └── Category.cs               # expense category enum
├── Data/
│   ├── IExpenseStore.cs          # persistence abstraction
│   └── JsonFileExpenseStore.cs   # JSON-file implementation
├── Services/
│   └── ExpenseService.cs         # validation / business rules (UI-free, testable)
└── Exceptions/
    └── InvalidExpenseException.cs

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
