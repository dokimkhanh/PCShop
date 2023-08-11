
# Welcome to PCShop Project

PCShop is a project to support website selling technology products, gear, ...




## Features

- Live previews
- Payment (Banking, QR, ...)
- Page for Employee, Admin, Customer
- Multil upload product image
- Fast performance
- ...

## Environment Variables

To run this project, you will need to add the following environment variables to your Web.config file first !!!

**DATABASE**:
- `DB_CONNECT` = Your SQL Server Name (Ex: DESKTOP-8S25GLP\KHANH) or IP
- `DB_NAME` = Database table Name

**EMAIL**:

You need turn off `account enhanced safe browsing` (https://myaccount.google.com/account-enhanced-safe-browsing)

Next, create `app key password` (https://myaccount.google.com/apppasswords)

- `YOUR_MAIL` = Your google email address (ex: abc@gmail.com)
- `YOUR_APP_KEY` = app_key_password

**VNPAY SETTINGS (PAYMENT)**
- For sandbox: 
    1. Go to https://sandbox.vnpayment.vn/devreg and register
    2. Go to Email and get `vnp_TmnCode` and `vnp_HashSecret`
    3. Replace variables in Web.config




## Demo

Live: http://levankhang-001-site1.ctempurl.com/
- Admin account: adminx/123456


## License

[MIT](https://choosealicense.com/licenses/mit/)

