# Quản Lý CLB Võ Thuật

Dự án minh họa backend quản lý câu lạc bộ võ với .NET 8, xây dựng theo mô hình 3 lớp (API, Application, Infrastructure) và sử dụng Entity Framework Core.

## Tính năng chính

- Quản lý huấn luyện viên (Coach) sử dụng tài khoản người dùng chung, bao gồm thông tin trình độ (`SkillLevel`) và chứng chỉ (`Certification`).
- Quản lý chi nhánh/địa điểm với bán kính điểm danh, thông tin Google Maps (Place ID, URL nhúng).
- Quản lý lớp học với lịch học cố định theo từng ngày trong tuần (mỗi bản ghi đại diện cho một ngày trong tuần và liên kết tới chi nhánh).
- Quản lý phân công trợ giảng (Assistant) cho lớp/lịch học thông qua bảng `ClassAssistantAssignments`.
- Quản lý chấm công bằng tọa độ GPS dựa trên thông tin chi nhánh (Google API phục vụ tìm kiếm địa điểm được tích hợp ở tầng client).
- Tạo ticket chấm công thủ công và phê duyệt.
- Thiết lập quy định tiền lương (`PayrollRules`) theo vai trò và trình độ, sinh bảng lương hàng tháng dựa trên dữ liệu chấm công và lịch học.
- Đăng nhập bằng Google ID Token, phát hành JWT có hạn 30 ngày và phân quyền theo vai trò `Admin`/`Coach`/`Assistant`/`Student`.

## Cấu trúc thư mục & mô hình 3 lớp

```
QuanLyCLB.sln
└── src
    ├── QuanLyCLB.Api             # Lớp trình bày (Presentation): cấu hình API, DI và middleware
    ├── QuanLyCLB.Application     # Lớp nghiệp vụ (Business): DTOs, Entities, Interfaces, logic xử lý chính
    └── QuanLyCLB.Infrastructure  # Lớp hạ tầng (Data/Infrastructure): EF Core, truy cập DB, dịch vụ ngoài (JWT, Google)
```

Các lớp chỉ phụ thuộc "xuôi chiều": `Api` gọi `Application`, `Application` định nghĩa hợp đồng, `Infrastructure` hiện thực hóa các hợp đồng và cung cấp DbContext.

## Cấu hình

- `appsettings.json` chứa chuỗi kết nối SQL Server mặc định (`Server=(localdb)\\MSSQLLocalDB;Database=QuanLyCLB;...`), cấu hình JWT và danh sách Google Client ID.
- Thay đổi `ConnectionStrings:Default` nếu bạn sử dụng SQL Server khác (ví dụ SQL Server Express hoặc Azure SQL).
- Khóa bí mật JWT (`SecretKey`) cần được thay thế bằng giá trị mạnh trong môi trường thực tế.
- Danh sách quản trị (`Authorization:Admins`) bao gồm các email được gán thêm vai trò `Admin`.

## Hướng dẫn tạo cơ sở dữ liệu SQL Server

1. Cài đặt SQL Server hoặc sử dụng `LocalDB` đi kèm Visual Studio.
2. Mở SQL Server Management Studio (SSMS) hoặc `sqlcmd` và chạy script `docs/sql/create_database.sql` để tạo database, bảng và ràng buộc:
   ```sql
   :r docs/sql/create_database.sql
   ```
   > Script kiểm tra sự tồn tại trước khi tạo nên có thể chạy nhiều lần.
3. Cập nhật lại chuỗi kết nối trong `appsettings.json` (hoặc biến môi trường `ConnectionStrings__Default`) tương ứng với server của bạn.
4. Nếu muốn dùng EF Core migration tự động, cài đặt công cụ và tạo migration:
   ```bash
   dotnet tool install --global dotnet-ef      # chạy 1 lần nếu chưa có
   dotnet ef migrations add InitialCreate -p src/QuanLyCLB.Infrastructure -s src/QuanLyCLB.Api
   dotnet ef database update -p src/QuanLyCLB.Infrastructure -s src/QuanLyCLB.Api
   ```
   > Migration không bắt buộc khi đã dùng script, nhưng hữu ích để đồng bộ schema khi mở rộng.

## Chạy dự án

1. Cài đặt .NET SDK 8.0.
2. Khôi phục và build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Cập nhật `appsettings.json` với Google Client ID hợp lệ và chuỗi kết nối SQL Server tương ứng.
4. Chạy API:
   ```bash
   dotnet run --project src/QuanLyCLB.Api
   ```
5. Truy cập Swagger UI ở `https://localhost:5001/swagger` (hoặc cổng được chỉ định) để thử nghiệm các API.

## Mở rộng

- Thiết lập migrations EF Core (`dotnet ef migrations add InitialCreate`) và áp dụng (`dotnet ef database update`).
- Tích hợp Google Geocoding API phía client để lấy lat/lng, gửi kèm khi chấm công.
- Bổ sung unit test và cơ chế background job để tự động sinh bảng lương định kỳ.
