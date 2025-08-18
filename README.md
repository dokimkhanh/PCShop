<!-- License -->
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![.NET](https://img.shields.io/badge/.NET-Framework%204.8-blue)
![GitHub issues](https://img.shields.io/github/issues/dokimkhanh/PCShop)
![GitHub stars](https://img.shields.io/github/stars/dokimkhanh/PCShop?style=social)


# PCShop

## 📖 Overview

PCShop is an ASP.NET MVC application built with **Entity Framework 6**
to manage products, customers, employees, and orders for a computer
shop.\
It includes authentication, role-based authorization, payment
integration (VNPAY, QR, Banking), and admin/customer/employee
dashboards.

------------------------------------------------------------------------

## 📑 Table of Contents

-   [Features](#-features)
-   [Requirements](#-requirements)
-   [Setup Guide](#-setup-guide)
-   [Database Setup](#-database-setup)
-   [Email Configuration](#-email-configuration)
-   [Payment Integration (VNPAY)](#-payment-integration-vnpay)
-   [Run the Project](#-run-the-project)
-   [License](#-license)

------------------------------------------------------------------------

## 🚀 Features

-   Live previews
-   Payment (Banking, QR, VNPAY)
-   Pages for Employee, Admin, Customer
-   Multi upload product images
-   Fast performance

------------------------------------------------------------------------

## 📦 Requirements

-   Visual Studio 2019+\
-   .NET Framework 4.7.2\
-   SQL Server Express (or higher)\
-   Entity Framework 6

------------------------------------------------------------------------

## ⚙️ Setup Guide

### 1️⃣ Clone Repository

``` bash
git clone https://github.com/dokimkhanh/PCShop.git
cd PCShop
```

### 2️⃣ Database Setup

1.  Open **SQL Server Management Studio (SSMS)**\

2.  Create an empty database named **PCShop**\

3.  Update **Web.config** with your SQL connection:

    ``` xml
    <connectionStrings>
      <add name="DefaultConnection"
           connectionString="Data Source=DESKTOP-YOURNAME\SQLEXPRESS;Initial Catalog=PCShop;Integrated Security=True"
           providerName="System.Data.SqlClient" />
    </connectionStrings>
    ```

4.  Open **Package Manager Console** in Visual Studio and run:

    ``` powershell
    Update-Database
    ```

    > This will apply migrations and create all tables.

### 3️⃣ Email Configuration

To use Gmail SMTP, follow these steps:\
1. Turn off **Enhanced Safe Browsing** → [Google
Settings](https://myaccount.google.com/account-enhanced-safe-browsing)\
2. Create an **App Password** → [Google App
Passwords](https://myaccount.google.com/apppasswords)\
3. Replace in **Web.config**:
`xml    <appSettings>      <add key="YOUR_MAIL" value="yourmail@gmail.com" />      <add key="YOUR_APP_KEY" value="your_app_key_password" />    </appSettings>`

### 4️⃣ Payment Integration (VNPAY)

1.  Go to [VNPAY Sandbox](https://sandbox.vnpayment.vn/devreg) and
    register\
2.  Check your email for:
    -   **vnp_TmnCode**
    -   **vnp_HashSecret**\
3.  Replace in **Web.config** variables for sandbox testing.

### 5️⃣ Run the Project

-   Open solution in **Visual Studio**
-   Set **PCShop.Web** as startup project
-   Press **F5** to run

------------------------------------------------------------------------

## 📜 License

[MIT](https://choosealicense.com/licenses/mit/)

