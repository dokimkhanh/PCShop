# PCShop Admin Design System

## Brief
- **Sản phẩm:** Hệ thống quản trị (Admin Portal) của PCShop (cửa hàng PC Gaming, laptop và linh kiện).
- **Người dùng:** Quản trị viên (`Admin`) và nhân viên vận hành (`Nhân viên`).
- **Việc chính:** Theo dõi doanh thu & đơn hàng trong ngày, cập nhật trạng thái thanh toán đơn hàng, quản lý sản phẩm (giá, tồn kho, ảnh), danh mục, tin tức và phân quyền.
- **Nền tảng:** Desktop / laptop là chính, hỗ trợ responsive trên tablet/mobile.

## Bộ mặt & Token (P13)
- **Bộ mặt:** Clean (`sidebar`, `classic`, Flat `P6`, Inter)
- **File token:** `Content/admin-clean.css`
- **Font:** `Inter` (400, 500, 600, 700), dòng `--app-font` tại `Content/admin-clean.css`
- **Màu nhấn:** `#2563eb` (`--primary`), hover `#1d4ed8` (`--primary-hover`), đạt chuẩn tương phản WCAG AA (>= 5.1:1 với chữ trắng)
- **Sắc:** Neutral (`P17`)
- **Dark mode:** Không

## Màn đã dựng
- Khung điều hướng (`_Layout.cshtml`, `_Sidebar.cshtml`): Sidebar sáng cố định 4 nhóm + trạng thái active theo controller + user footer; Topbar tối giản bỏ toàn bộ widget mẫu AdminLTE.
- Đăng nhập Admin (`_LayoutLogin.cshtml`, `Account/Login.cshtml`): Card tối giản giữa màn mang thương hiệu PCShop Admin.
- Thống kê tổng quan (`Home/Index.cshtml`): Phương án A — Dải 4 KPI liền khối + lưới 2:1 (biểu đồ cột doanh thu 7 ngày từ dữ liệu SQL thật + danh sách đơn chưa thanh toán & cảnh báo tồn kho thấp).
- Quản lý đơn hàng & Chi tiết (`Order/Index.cshtml`, `Order/View.cshtml`): Phương án A — Tab lọc trạng thái + ô tìm kiếm + bảng 6 cột + modal cập nhật thanh toán giữ sẵn trạng thái.
- Quản lý sản phẩm (`Product/Index.cshtml`, `Add.cshtml`, `Edit.cshtml`): Phương án A — Gộp từ 13 cột xuống 6 cột trọng tâm, cảnh báo tồn kho, thanh xoá hàng loạt ẩn khi chưa chọn dòng.
- Danh mục, Tin tức, Tài khoản, Phân quyền, Cấu hình (`Category/Index`, `News/Index`, `Account/Index`, `Role/Index`, `SystemSettings/Index`): Đồng nhất hệ thống bảng, thanh tìm kiếm, nút hành động và phân trang tiếng Việt.
- Phương thức thanh toán (`PaymentMethod/Index.cshtml`): Bố cục cài đặt một cột tối đa 920px theo bộ mặt Clean — danh sách phương thức thanh toán dạng hàng chia kẻ kèm công tắc lưu tức thì (24×44), modal chỉnh tên/mô tả hiển thị và banner cảnh báo khi tắt toàn bộ phương thức.
- Email xác nhận đơn hàng (`Content/template/mail1.html`): Biên lai tối giản 600px đồng bộ brand storefront (`#ffffff` trên nền `#f6f6f8`, viền `#e4e4e7`, điểm nhấn `#fe4c50`), thông tin shop động từ `SystemSettings`, trạng thái thanh toán rõ ràng và 1 nút CTA chính dẫn về lịch sử đơn hàng.
