[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![.NET](https://img.shields.io/badge/.NET-Framework%204.7.2-blue)
![GitHub issues](https://img.shields.io/github/issues/dokimkhanh/PCShop)
![GitHub stars](https://img.shields.io/github/stars/dokimkhanh/PCShop?style=social)

# PCShop

## 📖 Giới thiệu

PCShop là website bán máy tính, laptop và linh kiện viết bằng **ASP.NET MVC 5** và **Entity Framework 6**, quản lý sản phẩm, danh mục cha–con, khách hàng, nhân viên và đơn hàng.
Hệ thống hỗ trợ đăng nhập, phân quyền chi tiết theo vai trò, thanh toán khi nhận hàng (COD), bình luận sản phẩm, gửi email xác nhận đơn hàng tự động và khu vực quản trị cho Admin / Nhân viên.

---

## 📑 Mục lục

- [Tính năng](#-tính-năng)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Hướng dẫn cài đặt](#-hướng-dẫn-cài-đặt)
- [Cấu hình CSDL](#2️⃣-cấu-hình-csdl)
- [Cấu hình email (Brevo)](#3️⃣-cấu-hình-email-brevo)
- [Cấu hình bí mật](#4️⃣-cấu-hình-bí-mật)
- [Chạy project](#5️⃣-chạy-project)
- [Giấy phép](#-giấy-phép)

---

## 🚀 Tính năng

- Mega-Menu danh mục sản phẩm 2 cấp (danh mục cha – con), tìm kiếm, lọc và sắp xếp sản phẩm
- Thanh toán khi nhận hàng (COD), quản lý bật/tắt và tuỳ chỉnh mô tả phương thức thanh toán trong trang Quản trị (`/Admin/PaymentMethod`)
- Phân quyền chi tiết theo vai trò cho Admin, Nhân viên và Khách hàng
- Upload nhiều ảnh cho sản phẩm (kéo thả, sắp xếp, chọn ảnh chính) và soạn thảo chi tiết qua CKEditor
- Hệ thống bình luận sản phẩm 2 cấp và quản lý bình luận trong Admin
- Gửi email xác nhận đơn hàng tự động qua Brevo Transactional Email API
- Endpoint kiểm tra sức khoẻ `/health` và `/health?db=1` hỗ trợ giám sát / keep-alive

---

## 📦 Yêu cầu hệ thống

- Visual Studio 2019 trở lên (hoặc MSBuild Build Tools)
- .NET Framework 4.7.2
- SQL Server Express (hoặc bản cao hơn)
- Entity Framework 6

---

## ⚙️ Hướng dẫn cài đặt

### 1️⃣ Clone mã nguồn

```bash
git clone https://github.com/dokimkhanh/PCShop.git
cd PCShop
```

### 2️⃣ Cấu hình CSDL

Chọn **1 trong 2 cách** dưới đây để khởi tạo database **PCShop** trong **SQL Server Management Studio (SSMS)**:

#### Cách A — Restore bản mẫu có sẵn dữ liệu (`DB/PCShop.bak`, khuyên dùng)

Phù hợp nếu muốn chạy ngay với đầy đủ dữ liệu mẫu (**35 danh mục cha–con**, **64 sản phẩm** kèm ảnh, **10 bài tin tức**, **banner trang chủ**, **bộ phân quyền** và **cấu hình cửa hàng** — đã làm sạch đơn hàng, bình luận và thông tin cá nhân):

1. Mở **SSMS** → chuột phải vào **Databases** → chọn **Restore Database...**
2. Ở mục **Source**, chọn **Device** → bấm nút `...` → **Add** → chọn file `DB/PCShop.bak` → bấm **OK**
3. Kiểm tra tên đích **Database** là `PCShop` → chuyển sang tab **Files** (nếu cần đổi thư mục lưu `.mdf/.ldf`, tích **Relocate all files to folder**) → bấm **OK**

> File `DB/PCShop.bak` được đóng gói ở chuẩn **SQL Server 2014 (12.0)** nên restore được trên **SQL Server 2014, 2016, 2017, 2019, 2022 và 2025**. Sau khi restore xong **không cần** chạy `DB/PCShop.sql` hay `Update-Database`.

#### Cách B — Tạo CSDL sạch bằng script DDL (`DB/PCShop.sql`) hoặc Migration

Phù hợp nếu muốn tự nhập dữ liệu từ đầu hoặc đang dùng phiên bản SQL Server khác:

- **Dùng script DDL:** Mở file `DB/PCShop.sql` trong SSMS và nhấn **Execute (F5)** để tạo sẵn database `PCShop`, đầy đủ bảng, phân quyền, 5 danh mục mẫu và tài khoản `admin`.
- **Hoặc dùng EF Migration:** Tạo database rỗng tên `PCShop`, hoàn tất bước cấu hình `connections.config` bên dưới, sau đó mở **Package Manager Console** trong Visual Studio và chạy:

    ```powershell
    Update-Database
    ```

#### Kết nối CSDL với ứng dụng (`connections.config`)

Tạo file `connections.config` từ file mẫu rồi điền chuỗi kết nối (file thật **không** commit lên git):

```bash
copy connections.config.example connections.config
```

```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Data Source=DESKTOP-TENMAY\SQLEXPRESS;Initial Catalog=PCShop;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> `Web.config` khai báo `<connectionStrings configSource="connections.config" />`.
> Thiếu file này web sẽ báo lỗi 500 khi khởi động — nhớ copy lên cả server khi deploy.

### 3️⃣ Cấu hình email (Brevo)

Hệ thống gửi email giao dịch qua **Brevo Transactional Email API v3**. Cách cấu hình:

1. Tạo tài khoản [Brevo](https://www.brevo.com/) → **Senders & Domains** → thêm và xác thực (verify) email hoặc domain người gửi.
2. Vào **SMTP & API** → **API Keys** → tạo API key dạng `xkeysib-...`.
3. Điền các giá trị sau vào `secrets.config`:

    ```xml
    <add key="Brevo_ApiKey" value="xkeysib-..." />
    <add key="Brevo_FromEmail" value="email-da-verify@domain.com" />
    ```

4. `Web.config` đã cấu hình sẵn `Brevo_FromName` và `Brevo_ApiUrl` (`https://api.brevo.com/v3/smtp/email`).

> Email gửi đi phải dùng địa chỉ/domain **đã verify** trên Brevo, nếu không API sẽ trả lỗi `sender not valid`.
> Nếu Brevo bật chế độ chặn IP, nhớ thêm IP của server hosting (và IP máy dev) vào danh sách cho phép trong mục **Security → Authorised IPs**.

### 4️⃣ Cấu hình bí mật

Các giá trị bí mật (chuỗi kết nối CSDL, API key Brevo) **không** lưu trong `Web.config` mà nằm ở các file đã được khai báo trong `.gitignore`:

| File | Nội dung | Khai báo trong Web.config |
| --- | --- | --- |
| `connections.config` | Chuỗi kết nối CSDL (`DefaultConnection`) | `<connectionStrings configSource="connections.config" />` |
| `secrets.config` | `Brevo_ApiKey`, `Brevo_FromEmail` | `<appSettings file="secrets.config">` |

1. Sao chép file mẫu:

    ```bash
    copy connections.config.example connections.config
    copy secrets.config.example secrets.config
    ```

2. Mở từng file và điền giá trị thật:
    - `connections.config` – `Data Source`, `Initial Catalog`, user/password
    - `Brevo_ApiKey` – API key của Brevo
    - `Brevo_FromEmail` – email người gửi đã xác thực trên Brevo
3. Khi deploy phải copy **cả 2 file** này lên server.

### 5️⃣ Chạy project

- Mở solution bằng **Visual Studio**, chọn project **PCShop** làm startup project và nhấn **F5** để chạy.
- Các phương thức thanh toán và thông tin cửa hàng (tên shop, địa chỉ, hotline, email, mạng xã hội, SEO) có thể tuỳ chỉnh trực tiếp trong khu vực quản trị `/Admin/PaymentMethod` và `/Admin/SystemSettings`.

---

## 📜 Giấy phép

[MIT](https://choosealicense.com/licenses/mit/)
