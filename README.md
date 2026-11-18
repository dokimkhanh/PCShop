<!-- Giấy phép -->
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![.NET](https://img.shields.io/badge/.NET-Framework%204.8-blue)
![GitHub issues](https://img.shields.io/github/issues/dokimkhanh/PCShop)
![GitHub stars](https://img.shields.io/github/stars/dokimkhanh/PCShop?style=social)


# PCShop

## 📖 Giới thiệu

PCShop là website bán máy tính viết bằng **ASP.NET MVC 5** và **Entity Framework 6**,
quản lý sản phẩm, khách hàng, nhân viên và đơn hàng.\
Hệ thống có đăng nhập, phân quyền theo vai trò, tích hợp thanh toán
(VNPAY, QR, ngân hàng) và khu vực quản trị cho Admin / Nhân viên / Khách hàng.

------------------------------------------------------------------------

## 📑 Mục lục

-   [Tính năng](#-tính-năng)
-   [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
-   [Hướng dẫn cài đặt](#-hướng-dẫn-cài-đặt)
-   [Cấu hình CSDL](#2️⃣-cấu-hình-csdl)
-   [Cấu hình email (Brevo)](#3️⃣-cấu-hình-email-brevo)
-   [Tích hợp thanh toán VNPAY](#4️⃣-tích-hợp-thanh-toán-vnpay)
-   [Cấu hình bí mật](#5️⃣-cấu-hình-bí-mật)
-   [Bật/tắt thanh toán trực tuyến](#6️⃣-bậttắt-thanh-toán-trực-tuyến)
-   [Chạy project](#7️⃣-chạy-project)
-   [Giấy phép](#-giấy-phép)

------------------------------------------------------------------------

## 🚀 Tính năng

-   Xem trước sản phẩm, tìm kiếm, lọc theo danh mục
-   Thanh toán (chuyển khoản ngân hàng, QR, VNPAY, COD)
-   Trang riêng cho Khách hàng, Nhân viên, Admin
-   Upload nhiều ảnh cho một sản phẩm
-   Gửi email xác nhận đơn hàng tự động
-   Hiệu năng nhanh, giao diện responsive

------------------------------------------------------------------------

## 📦 Yêu cầu hệ thống

-   Visual Studio 2019 trở lên\
-   .NET Framework 4.7.2\
-   SQL Server Express (hoặc bản cao hơn)\
-   Entity Framework 6

------------------------------------------------------------------------

## ⚙️ Hướng dẫn cài đặt

### 1️⃣ Clone mã nguồn

``` bash
git clone https://github.com/dokimkhanh/PCShop.git
cd PCShop
```

### 2️⃣ Cấu hình CSDL

1.  Mở **SQL Server Management Studio (SSMS)**\

2.  Tạo database rỗng tên **PCShop**\

3.  Tạo file `connections.config` từ file mẫu rồi điền chuỗi kết nối
    (file thật **không** commit lên git):

    ``` bash
    copy connections.config.example connections.config
    ```

    ``` xml
    <!-- connections.config -->
    <connectionStrings>
      <add name="DefaultConnection"
           connectionString="Data Source=DESKTOP-TENMAY\SQLEXPRESS;Initial Catalog=PCShop;Integrated Security=True"
           providerName="System.Data.SqlClient" />
    </connectionStrings>
    ```

    > `Web.config` chỉ khai báo `<connectionStrings configSource="connections.config" />`.
    > Thiếu file này web sẽ báo lỗi 500 ngay khi khởi động — nhớ copy lên cả server khi deploy.

    > Có thể tạo bảng bằng script `DB/PCShop.sql` (mở trong SSMS và chạy),
    > hoặc dùng lệnh `Update-Database` ở bước dưới.

4.  Mở **Package Manager Console** trong Visual Studio và chạy:

    ``` powershell
    Update-Database
    ```

    > Lệnh này áp dụng migration và tạo toàn bộ bảng.

### 3️⃣ Cấu hình email (Brevo)

Hệ thống gửi email giao dịch qua **Brevo Transactional Email API v3**
(không dùng SMTP Gmail nữa). Cách cấu hình:

1.  Tạo tài khoản [Brevo](https://www.brevo.com/) → **Senders & Domains** →
    thêm và xác thực (verify) email hoặc domain người gửi.\
2.  Vào **SMTP & API** → **API Keys** → tạo API key dạng `xkeysib-...`.\
3.  Điền các giá trị sau vào `secrets.config` (xem mục *Cấu hình bí mật*):

    ``` xml
    <add key="Brevo_ApiKey" value="xkeysib-..." />
    <add key="Brevo_FromEmail" value="email-da-verify@domain.com" />
    ```

4.  `Web.config` đã có sẵn `Brevo_FromName` và `Brevo_ApiUrl`
    (`https://api.brevo.com/v3/smtp/email`).

> Email gửi đi phải dùng địa chỉ/domain **đã verify** trên Brevo, nếu không
> API sẽ trả lỗi `sender not valid`.

> Nếu Brevo bật chế độ chặn IP, nhớ thêm IP của server hosting
> (và IP máy dev) vào danh sách cho phép trong mục **Security → Authorised IPs**.

### 4️⃣ Tích hợp thanh toán VNPAY

1.  Vào [VNPAY Sandbox](https://sandbox.vnpayment.vn/devreg) và đăng ký\
2.  Kiểm tra email để lấy:
    -   **vnp_TmnCode**
    -   **vnp_HashSecret**\
3.  Điền `vnp_TmnCode`, `vnp_Url`, `vnp_Returnurl` trong **Web.config** và
    `vnp_HashSecret` trong **secrets.config** (không commit).

### 5️⃣ Cấu hình bí mật

Các giá trị bí mật (chuỗi kết nối CSDL, hash secret VNPAY, API key Brevo)
**không** lưu trong `Web.config` mà nằm ở các file đã bị `.gitignore`,
không commit lên git:

| File | Nội dung | Khai báo trong Web.config |
|---|---|---|
| `connections.config` | Chuỗi kết nối CSDL (`DefaultConnection`) | `<connectionStrings configSource="connections.config" />` |
| `secrets.config` | `vnp_HashSecret`, `Brevo_ApiKey`, `Brevo_FromEmail`, `vnp_Returnurl` | `<appSettings file="secrets.config">` |

1.  Sao chép file mẫu:

    ``` bash
    copy connections.config.example connections.config
    copy secrets.config.example secrets.config
    ```

2.  Mở từng file và điền giá trị thật:

    -   `connections.config` – `Data Source`, `Initial Catalog`, user/password
    -   `vnp_HashSecret` – hash secret do VNPAY cấp
    -   `Brevo_ApiKey` – API key của Brevo
    -   `Brevo_FromEmail` – email người gửi đã xác thực trên Brevo

3.  Khi deploy phải copy **cả 2 file** này lên server.

> **Lưu ý bảo mật:** hash secret VNPAY từng nằm trong git history, nên cần
> **cấp lại (rotate) secret** trên cổng VNPAY rồi cập nhật lại `secrets.config`.

### 6️⃣ Bật/tắt thanh toán trực tuyến

Mặc định hệ thống **chỉ nhận thanh toán khi nhận hàng (COD)**. Code VNPAY vẫn
giữ nguyên nhưng bị ẩn khỏi giao diện và chặn ở phía server. Muốn bật lại, thêm
vào `secrets.config` (hoặc `Web.config`):

``` xml
<add key="EnableOnlinePayment" value="true" />
```

### 7️⃣ Chạy project

-   Mở solution bằng **Visual Studio**
-   Chọn project **PCShop** làm startup project
-   Nhấn **F5** để chạy

------------------------------------------------------------------------

## 📜 Giấy phép

[MIT](https://choosealicense.com/licenses/mit/)
