# Trustesse Notification Service

A centralized service for sending and managing **emails**, **SMS**, and other **notifications** across Trustesse applications. Built for reliability, scalability, and flexibility, it supports both **direct sending** and **Hangfire background jobs** for queued delivery.

---

## Features

- Unified service for sending emails, SMS, and notifications
- Direct (synchronous) sending or queued background jobs
- Built on Hangfire for reliable scheduling, retries, and monitoring
- Easy integration with other Trustesse services

---

## Tech Stack

| Layer         | Technology                        |
|---------------|-----------------------------------|
| Backend       | ASP.NET Core 8 (Web API)          |
| Background    | Hangfire                         |
| Messaging     | Email (SMTP) / SMS Gateway        |
| Hosting       | Azure App Service / Azure SQL     |

---

## Architecture

- **API Layer** to receive notification requests
- **Background Jobs** with Hangfire to process and deliver messages
- **Provider Abstraction** for pluggable email and SMS providers
---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- mySql instance (local or remote)
- Command Line (cmd, PowerShell, etc.)

---

### Setup Instructions

1. Clone the repository:

```bash
git clone https://github.com/Trustesse-limited/trustesse-notification-service.git
cd trustesse-notification-service
