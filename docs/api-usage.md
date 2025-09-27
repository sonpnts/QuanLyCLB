# Hướng dẫn sử dụng API hệ thống Quản Lý CLB

Tài liệu này mô tả chi tiết các API được cung cấp bởi dịch vụ `QuanLyCLB.Api`, cách xác thực, phân quyền và luồng xử lý nghiệp vụ chính. Các đường dẫn dưới đây đều được ghép với `https://<host>/api` khi triển khai chính thức. Ở môi trường phát triển, ứng dụng mặc định khởi chạy tại `https://localhost:5001` với bộ tài liệu Swagger.

## 1. Xác thực và phân quyền

### 1.1 Đăng nhập bằng Google
- **Phương thức**: `POST /Auth/google`
- **Yêu cầu**: `IdToken` nhận được từ Google Identity (OAuth 2.0 / Sign-In).
- **Luồng xử lý**:
  1. API sử dụng `IGoogleTokenValidator` để kiểm tra tính hợp lệ của `IdToken` với Google.【F:src/QuanLyCLB.Api/Controllers/AuthController.cs†L20-L61】
  2. Nếu hợp lệ, hệ thống gọi `IInstructorService.SyncGoogleAccountAsync` để đồng bộ thông tin giảng viên (tạo mới khi chưa tồn tại hoặc cập nhật lại tên/subject).【F:src/QuanLyCLB.Api/Controllers/AuthController.cs†L34-L44】
  3. Dựa trên danh sách role trả về, API tạo JWT với thời hạn `ExpirationDays` cấu hình trong `appsettings.json`.【F:src/QuanLyCLB.Api/Controllers/AuthController.cs†L46-L57】
  4. Kết quả trả về bao gồm `AccessToken`, `ExpiresAtUtc`, thông tin giảng viên và danh sách vai trò.
- **Mẫu yêu cầu**:
  ```json
  {
    "idToken": "<google-id-token>"
  }
  ```
- **Mẫu đáp ứng**:
  ```json
  {
    "accessToken": "<jwt-token>",
    "expiresAtUtc": "2024-06-15T08:12:30Z",
    "instructor": {
      "id": "...",
      "fullName": "Nguyễn Văn A",
      "email": "user@example.com",
      "phoneNumber": "0123456789",
      "hourlyRate": 150000,
      "isActive": true
    },
    "roles": ["Instructor"]
  }
  ```

### 1.2 Sử dụng JWT trong các API khác
- Thêm header `Authorization: Bearer <jwt-token>` trong mọi yêu cầu đã đăng nhập.
- Chính sách phân quyền:
  - `AdminOnly`: yêu cầu role `Admin`.
  - `InstructorOnly`: chấp nhận `Instructor`, `Admin` hoặc `TeachingAssistant`.
- Các chính sách được cấu hình trong `Program.cs` và áp dụng cho từng controller qua `[Authorize(Policy = ...)]` hoặc `[Authorize]`.

## 2. Quản lý giảng viên

| Hành động | Phương thức & Đường dẫn | Quyền | Mô tả |
|-----------|------------------------|-------|-------|
| Lấy danh sách giảng viên | `GET /Instructors` | AdminOnly | Trả về toàn bộ giảng viên đang quản lý. |
| Xem chi tiết | `GET /Instructors/{id}` | AdminOnly | Trả về chi tiết giảng viên theo `id`. |
| Tạo mới | `POST /Instructors` | AdminOnly | Nhận `CreateInstructorRequest` và trả về giảng viên vừa tạo. |
| Cập nhật | `PUT /Instructors/{id}` | AdminOnly | Cập nhật thông tin giảng viên bằng `UpdateInstructorRequest`. |
| Xóa | `DELETE /Instructors/{id}` | AdminOnly | Xóa giảng viên, trả về `204 No Content` khi thành công. |

- **Luồng xử lý chính**: Các thao tác đều gọi `IInstructorService` tương ứng để thao tác với dữ liệu và trả kết quả chuẩn REST (200/201/204/404).【F:src/QuanLyCLB.Api/Controllers/InstructorsController.cs†L8-L48】
- **Mẫu tạo mới**:
  ```json
  {
    "fullName": "Nguyễn Văn B",
    "email": "other@example.com",
    "phoneNumber": "0987654321",
    "hourlyRate": 200000
  }
  ```
- **Mẫu cập nhật**:
  ```json
  {
    "fullName": "Nguyễn Văn B",
    "phoneNumber": "0912345678",
    "hourlyRate": 220000,
    "isActive": true
  }
  ```

## 3. Quản lý lớp học (Training Classes)

### 3.1 CRUD lớp học

| Phương thức | Đường dẫn | Quyền | Mục đích |
|-------------|-----------|-------|----------|
| GET | `/Classes` | AdminOnly | Lấy tất cả lớp học. |
| GET | `/Classes/{id}` | AdminOnly | Lấy chi tiết lớp. |
| POST | `/Classes` | AdminOnly | Tạo lớp mới từ `CreateTrainingClassRequest`. |
| PUT | `/Classes/{id}` | AdminOnly | Cập nhật thông tin lớp từ `UpdateTrainingClassRequest`. |
| DELETE | `/Classes/{id}` | AdminOnly | Xóa lớp học. |

- **Luồng xử lý**: Controller ủy quyền cho `ITrainingClassService` thực thi nghiệp vụ và trả mã trạng thái phù hợp (`201 Created` khi tạo mới, `404` khi không tìm thấy, v.v.).【F:src/QuanLyCLB.Api/Controllers/ClassesController.cs†L8-L50】

### 3.2 Lịch học theo lớp
- **GET `/Classes/{classId}/schedules` (AdminOnly)**: gọi `IScheduleService.GetByClassAsync` và trả danh sách lịch theo lớp.【F:src/QuanLyCLB.Api/Controllers/ClassesController.cs†L52-L60】
- **POST `/Classes/{classId}/schedules` (AdminOnly)**: nhận `BulkCreateScheduleRequest` để tạo hàng loạt lịch học. API kiểm tra `classId` trên URL trùng với `TrainingClassId` trong payload trước khi thực thi. Nếu không khớp trả `400 Bad Request`.
- **Luồng xử lý Bulk create**:
  1. Xác thực quyền Admin.
  2. Kiểm tra tính khớp `classId`.
  3. Gọi `IScheduleService.BulkCreateAsync` để sinh lịch học định kỳ dựa trên các ngày trong tuần và khoảng thời gian cung cấp.【F:src/QuanLyCLB.Api/Controllers/ClassesController.cs†L62-L75】

## 4. Quản lý lịch học (Schedules)

Ngoài các thao tác theo lớp, `SchedulesController` cung cấp CRUD trực tiếp:

| Phương thức | Đường dẫn | Quyền | Mô tả |
|-------------|-----------|-------|-------|
| GET | `/Schedules/{id}` | AdminOnly | Lấy chi tiết lịch học. |
| POST | `/Schedules` | AdminOnly | Tạo lịch đơn lẻ từ `CreateClassScheduleRequest`. |
| PUT | `/Schedules/{id}` | AdminOnly | Cập nhật lịch từ `UpdateClassScheduleRequest`. |
| DELETE | `/Schedules/{id}` | AdminOnly | Xóa lịch học. |

- **Luồng xử lý**: Các thao tác gọi `IScheduleService` tương ứng, trả về đối tượng lịch hoặc `404` nếu không tồn tại.【F:src/QuanLyCLB.Api/Controllers/SchedulesController.cs†L8-L39】

## 5. Điểm danh (Attendance)

### 5.1 Check-in trực tiếp
- **Phương thức**: `POST /Attendance/check-in`
- **Quyền**: `InstructorOnly`
- **Payload**: `CheckInRequest` gồm `classScheduleId`, `instructorId`, thời điểm và tọa độ check-in.
- **Luồng xử lý**:
  1. API so sánh `instructorId` trong payload với `ClaimTypes.NameIdentifier` của người dùng đăng nhập. Nếu khác, trả về `403 Forbid`.【F:src/QuanLyCLB.Api/Controllers/AttendanceController.cs†L18-L35】
  2. Nếu hợp lệ, gọi `IAttendanceService.CheckInAsync` để ghi nhận và trả lại `AttendanceRecordDto`.

### 5.2 Tạo bản ghi thủ công
- **Phương thức**: `POST /Attendance/manual`
- **Quyền**: `AdminOnly`
- **Payload**: `ManualAttendanceRequest` (bao gồm trạng thái điểm danh và ghi chú).
- **Luồng xử lý**: Gọi `CreateManualAttendanceAsync` và trả bản ghi kết quả.【F:src/QuanLyCLB.Api/Controllers/AttendanceController.cs†L37-L44】

### 5.3 Tra cứu điểm danh theo giảng viên
- **Phương thức**: `GET /Attendance/instructor/{instructorId}?fromDate=YYYY-MM-DD&toDate=YYYY-MM-DD`
- **Quyền**: Bất kỳ người dùng đăng nhập. Tuy nhiên API chỉ cho phép:
  - Admin xem mọi giảng viên.
  - Giảng viên/TA chỉ xem chính mình (kiểm tra `TryValidateInstructor`).【F:src/QuanLyCLB.Api/Controllers/AttendanceController.cs†L46-L59】
- **Luồng xử lý**: Sau khi kiểm tra quyền, hệ thống gọi `GetAttendanceByInstructorAsync` để trả về danh sách.

### 5.4 Quản lý phiếu điểm danh (ticket)
- **Tạo phiếu**: `POST /Attendance/tickets` (InstructorOnly) với `CreateTicketRequest`. API xác thực giảng viên là chủ sở hữu trước khi gọi `CreateTicketAsync` để tạo phiếu.【F:src/QuanLyCLB.Api/Controllers/AttendanceController.cs†L61-L72】
- **Duyệt phiếu**: `POST /Attendance/tickets/{ticketId}/approval` (AdminOnly) với `TicketApprovalRequest`. Nếu phiếu tồn tại, trả về thông tin cập nhật; nếu không, trả `404`.
- **Luồng xử lý chung**:
  1. Giảng viên tạo phiếu (ví dụ khi check-in lỗi hoặc nghỉ có lý do).
  2. Admin duyệt/ từ chối, API cập nhật trạng thái `IsApproved`, `ApprovedBy`, `ApprovedAt` dựa trên request.【F:src/QuanLyCLB.Api/Controllers/AttendanceController.cs†L74-L83】

## 6. Tính lương (Payroll)

### 6.1 Tạo bảng lương theo tháng
- **Phương thức**: `POST /Payroll/generate`
- **Quyền**: `AdminOnly`
- **Payload**: `GeneratePayrollRequest` gồm `instructorId`, `year`, `month`.
- **Luồng xử lý**: API gọi `IPayrollService.GeneratePayrollAsync` để tổng hợp giờ dạy, số tiền và chi tiết từng bản ghi điểm danh. Kết quả trả về `PayrollPeriodDto` chứa danh sách `details` cho từng buổi dạy.【F:src/QuanLyCLB.Api/Controllers/PayrollController.cs†L8-L25】

### 6.2 Xem bảng lương
- **Phương thức**: `GET /Payroll/instructor/{instructorId}`
- **Quyền**: người dùng đăng nhập. Admin xem mọi giảng viên, người dùng thường chỉ xem chính mình (kiểm tra `TryValidateInstructor`).【F:src/QuanLyCLB.Api/Controllers/PayrollController.cs†L27-L40】
- **Kết quả**: Danh sách `PayrollPeriodDto` của các kỳ đã tạo.

### 6.3 Xem chi tiết theo mã kỳ lương
- **Phương thức**: `GET /Payroll/{payrollId}`
- **Quyền**: `AdminOnly`
- **Luồng xử lý**: Gọi `GetPayrollByIdAsync`. Nếu không có dữ liệu trả về `404`.

## 7. Quy ước dữ liệu chung
- Tất cả GUID được định dạng dưới dạng chuỗi chuẩn UUID.
- `DateOnly`/`TimeOnly` được serialize theo ISO-8601 (`YYYY-MM-DD`, `HH:MM:SS`).
- Các trường tiền (`hourlyRate`, `totalAmount`, `amount`) là số thập phân.
- Hầu hết API trả về `404 Not Found` khi id không tồn tại và `400 Bad Request` khi dữ liệu không hợp lệ.

## 8. Luồng nghiệp vụ tổng thể
1. **Đăng nhập**: Giảng viên hoặc Admin đăng nhập bằng Google -> nhận JWT.
2. **Quản lý danh mục** (Admin): tạo giảng viên, gán lớp, thiết lập lịch học.
3. **Điểm danh**:
   - Giảng viên check-in trực tiếp theo lịch.
   - Nếu phát sinh sự cố, tạo ticket để Admin xử lý.
   - Admin có thể tạo bản ghi thủ công hoặc duyệt ticket.
4. **Báo cáo & lương**:
   - Giảng viên xem lịch sử điểm danh của bản thân.
   - Admin tạo bảng lương hàng tháng dựa trên dữ liệu điểm danh.
   - Giảng viên và Admin xem bảng lương đã tạo.

Tài liệu này nên được sử dụng kết hợp với Swagger UI để xem chi tiết schema và thử nghiệm trực tiếp từng API.
