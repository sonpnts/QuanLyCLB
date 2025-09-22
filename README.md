# QuanLyCLB

Backend quản lý câu lạc bộ được viết trên nền tảng ASP.NET Core 8 với mô hình 3 lớp (Domain, Application, Infrastructure) và Entity Framework Core. Hệ thống cung cấp các API để:

- Quản lý học viên (thêm, cập nhật, đổi trạng thái, tra cứu).
- Quản lý lớp học (tạo mới, chỉnh sửa, lưu trữ/khôi phục, sao chép lớp, thiết lập thời khóa biểu).
- Ghi danh, chuyển lớp học viên.
- Điểm danh (tạo buổi học, đánh dấu trạng thái tham gia, lưu ảnh buổi học).
- Thu học phí, chốt sổ cho huấn luyện viên/ trợ giảng.
- Báo cáo học phí, danh sách học viên theo lớp hiện tại.
- Phân quyền tài khoản, đăng nhập bằng mật khẩu hoặc Google, phát hành JWT.

## Cấu trúc giải pháp

```
QuanLyClb.sln
└── src
    ├── QuanLyClb.Domain          # Định nghĩa entity, enum nghiệp vụ
    ├── QuanLyClb.Application     # DTO, request/response, interface nghiệp vụ
    ├── QuanLyClb.Infrastructure  # DbContext, EF Core, triển khai service, JWT, Google auth
    └── QuanLyClb.Api             # ASP.NET Core Web API, controller và cấu hình DI
```

## Cấu hình & chạy thử

1. Cập nhật khoá ký JWT (`Jwt:SigningKey`) và `GoogleAuth:ClientId` trong `src/QuanLyClb.Api/appsettings.json`.
2. Khởi tạo & chạy API:

   ```bash
   dotnet restore
   dotnet run --project src/QuanLyClb.Api/QuanLyClb.Api.csproj
   ```

3. API mặc định chạy tại `https://localhost:5001` và `http://localhost:5000`. Swagger UI bật ở chế độ Development.

Cơ sở dữ liệu mặc định dùng SQLite file `quanlyclb.db` đặt cùng thư mục chạy. Có thể đổi chuỗi kết nối `ConnectionStrings:Default`.

## Ghi chú

- JWT yêu cầu gửi header `Authorization: Bearer <token>` cho các API cần phân quyền.
- Để điểm danh có ảnh, client có thể tải ảnh lên dịch vụ lưu trữ riêng và truyền URL vào `CreateAttendanceSessionRequest.PhotoUrl`.
- Hệ thống mặc định gán vai trò `Coach` cho tài khoản đăng nhập lần đầu qua Google.
