# Quản Lý CLB Võ Thuật

Dự án minh họa backend quản lý câu lạc bộ võ với .NET 8, xây dựng theo mô hình 3 lớp (API, Application, Infrastructure) và sử dụng Entity Framework Core.

## Tính năng chính

- Quản lý huấn luyện viên với mức lương theo giờ và tài khoản Google.
- Quản lý lớp học, lịch học chuẩn hóa theo từng ngày/buổi (mỗi bản ghi ứng với một buổi học).
- Quản lý chấm công bằng tọa độ GPS (Google Geocoding API được tích hợp thông qua validator token Google để đảm bảo danh tính).
- Tạo ticket chấm công thủ công và phê duyệt.
- Sinh bảng lương hàng tháng dựa trên dữ liệu chấm công và lịch học.
- Đăng nhập bằng Google ID Token, phát hành JWT có hạn 30 ngày và phân quyền theo vai trò `Admin`/`Instructor`.

## Cấu trúc thư mục

```
QuanLyCLB.sln
└── src
    ├── QuanLyCLB.Api             # Lớp trình bày (ASP.NET Core Web API)
    ├── QuanLyCLB.Application     # Lớp nghiệp vụ, DTOs, interfaces
    └── QuanLyCLB.Infrastructure  # Lớp hạ tầng: EF Core, services, JWT, Google validator
```

## Cấu hình

- `appsettings.json` chứa chuỗi kết nối SQLite mặc định (`club-management.db`), cấu hình JWT và danh sách Google Client ID.
- Khóa bí mật JWT (`SecretKey`) cần được thay thế bằng giá trị mạnh trong môi trường thực tế.
- Danh sách quản trị (`Authorization:Admins`) bao gồm các email được gán thêm vai trò `Admin`.

## Chạy dự án

1. Cài đặt .NET SDK 8.0.
2. Khôi phục và build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Cập nhật `appsettings.json` với Google Client ID hợp lệ và cấu hình kết nối cơ sở dữ liệu mong muốn.
4. Chạy API:
   ```bash
   dotnet run --project src/QuanLyCLB.Api
   ```
5. Truy cập Swagger UI ở `https://localhost:5001/swagger` (hoặc cổng được chỉ định) để thử nghiệm các API.

## Mở rộng

- Thiết lập migrations EF Core (`dotnet ef migrations add InitialCreate`) và áp dụng (`dotnet ef database update`).
- Tích hợp Google Geocoding API phía client để lấy lat/lng, gửi kèm khi chấm công.
- Bổ sung unit test và cơ chế background job để tự động sinh bảng lương định kỳ.
