# Personal Finance & Portfolio Manager

An ASP.NET Core web application for managing multi-asset investment portfolios, tracking cash flow, and monitoring net worth with live market valuations.

## Overview

Personal Finance & Portfolio Manager consolidates asset management into a centralized web dashboard. It allows users to track holdings across multiple asset classes (Cash, Stocks, and Cryptocurrencies), pulls live market quotes via Yahoo Finance, records financial transactions, and provides portfolio distribution insights.

## Features

- **Multi-Asset Portfolio:** Unified tracking for Cash reserves, Equities (Stocks), and Cryptocurrencies.
- **Real-Time Market Data:** Live quote updates for stocks and crypto via `YahooFinanceApi` and HTTP clients (`Flurl`).
- **Transaction Ledger:** Complete record of deposits, buys, and sells with automated balance and net worth recalculations.
- **Asset Comparison & Analytics:** Interactive dashboards comparing asset performance, portfolio weight, and sector allocations.
- **Modern Responsive Web UI:** Built with ASP.NET Core Razor Pages and Bootstrap 5 for seamless desktop and mobile use.

## Tech Stack

- **Backend:** C# / ASP.NET Core 8.0 (Razor Pages)
- **Data & APIs:** `YahooFinanceApi`, `Flurl.Http`, `CsvHelper`, `Newtonsoft.Json`
- **Frontend:** Bootstrap 5, HTML5/CSS3, JavaScript

## Project Structure

```text
Personal-Finance-and-Portfolio-Manager/
├── PersonalFinanceManager/
│   ├── Models/             # Domain entities (Asset, Cash, Stock, Crypto, Transaction)
│   ├── Pages/              # Razor Pages (Index, Markets, Comparison, Transactions)
│   ├── Services/           # Portfolio business logic and background workers
│   └── wwwroot/            # Static assets (Bootstrap, custom CSS/JS)
├── PersonalFinanceManager.sln
└── .gitignore
```

## Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Installation & Run
1. Clone the repository:
   ```bash
   git clone https://github.com/niksata-ivanovw/Personal-Finance-and-Portfolio-Manager.git
   ```
2. Navigate to the project directory:
   ```bash
   cd Personal-Finance-and-Portfolio-Manager/PersonalFinanceManager
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
4. Open your browser and navigate to `https://localhost:7214` (or the port shown in your terminal).\n
