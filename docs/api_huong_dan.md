# Hướng dẫn sử dụng API hệ thống Quản Lý Câu Lạc Bộ

Tài liệu này mô tả chi tiết các bước tích hợp với dịch vụ backend của hệ thống quản lý câu lạc bộ, bao gồm luồng xác thực, phân quyền và mẫu gọi cho từng API.

## 1. Thông tin chung

- **Base URL**: tùy thuộc vào môi trường triển khai. Ví dụ: `https://quanlyclb.yourdomain.com`.
- **Chuẩn JSON**: tất cả request/response sử dụng `Content-Type: application/json`.
- **Múi giờ**: thời gian được trả về theo UTC (thể hiện bằng hậu tố `Z`). Các trường `DateOnly` và `TimeOnly` là theo múi giờ hệ thống.
- **Quy ước format ngày giờ**:
  - `DateOnly`: `YYYY-MM-DD`.
  - `TimeOnly`: `HH:mm:ss`.
  - `DateTime`: ISO-8601 (`YYYY-MM-DDTHH:mm:ssZ`).

## 2. Luồng xác thực & phân quyền

1. Người dùng đăng nhập bằng Google để lấy `id_token` (OAuth2 Google Sign-In).
2. Gửi `id_token` tới API `POST /api/auth/google`.
3. Backend xác thực token với Google, đồng bộ thông tin giảng viên và sinh roles tương ứng.
4. Backend trả về `accessToken` (JWT) cùng ngày hết hạn. Các request tiếp theo cần gắn header:
   ```http
   Authorization: Bearer <accessToken>
   ```
5. JWT chứa các role (`Admin`, `Instructor`, `TeachingAssistant`) và được kiểm soát bởi hai policy:
   - `AdminOnly`: chỉ role `Admin`.
   - `InstructorOnly`: `Instructor`, `TeachingAssistant` hoặc `Admin`.

Các endpoint dưới đây yêu cầu token hợp lệ trừ khi ghi chú khác.

## 3. API xác thực

### POST `/api/auth/google`
- **Mục đích**: Đăng nhập bằng Google và nhận JWT.
- **Phân quyền**: Không yêu cầu (AllowAnonymous).
- **Request body**:
  ```json
  {
    "idToken": "<id_token_google>"
  }
  ```
- **Response 200**:
  ```json
  {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAtUtc": "2024-04-01T08:15:00Z",
    "instructor": {
      "id": "a3ad18a8-3be8-4f4c-8dc6-25c3f3b9a5cb",
      "fullName": "Nguyễn Văn A",
      "email": "giaovien@example.com",
      "phoneNumber": "+84901234567",
      "hourlyRate": 150000,
      "isActive": true
    },
    "roles": [
      "Instructor"
    ]
  }
  ```
- **Response 401**: token Google không hợp lệ hoặc tài khoản bị từ chối.

## 4. API giảng viên (`/api/instructors`)

> Các API này yêu cầu role `Admin`.

### GET `/api/instructors`
- Lấy danh sách toàn bộ giảng viên.
- **Response 200**: Mảng `InstructorDto`.

### GET `/api/instructors/{id}`
- Lấy chi tiết một giảng viên.
- **Response 200**:
  ```json
  {
    "id": "...",
    "fullName": "...",
    "email": "...",
    "phoneNumber": "...",
    "hourlyRate": 150000,
    "isActive": true
  }
  ```
- **Response 404**: không tìm thấy.

### POST `/api/instructors`
- Tạo giảng viên mới.
- **Request**:
  ```json
  {
    "fullName": "Nguyễn Văn B",
    "email": "gvb@example.com",
    "phoneNumber": "+84987654321",
    "hourlyRate": 180000
  }
  ```
- **Response 201**: đối tượng `InstructorDto` vừa tạo.

### PUT `/api/instructors/{id}`
- Cập nhật thông tin giảng viên.
- **Request**:
  ```json
  {
    "fullName": "Nguyễn Văn B",
    "phoneNumber": "+84987654321",
    "hourlyRate": 200000,
    "isActive": true
  }
  ```
- **Response 200**: thông tin sau cập nhật. `404` nếu không tồn tại.

### DELETE `/api/instructors/{id}`
- Xóa giảng viên.
- **Response 204** nếu thành công, `404` nếu không tồn tại.

## 5. API lớp học (`/api/classes`)

> Yêu cầu role `Admin` cho tất cả các endpoint.

### GET `/api/classes`
- Lấy toàn bộ lớp đang quản lý.
- **Response**: mảng `TrainingClassDto`.

### GET `/api/classes/{id}`
- Lấy thông tin lớp.

### POST `/api/classes`
- **Request**:
  ```json
  {
    "code": "YOGA-01",
    "name": "Yoga Cơ Bản",
    "description": "Lớp yoga cho người mới bắt đầu",
    "startDate": "2024-04-01",
    "endDate": "2024-06-30",
    "maxStudents": 20,
    "instructorId": "..."
  }
  ```
- **Response 201**: `TrainingClassDto`.

### PUT `/api/classes/{id}`
- Cập nhật lớp học. 

### DELETE `/api/classes/{id}`
- Xóa lớp học.

### GET `/api/classes/{classId}/schedules`
- Lấy toàn bộ lịch học của lớp.

### POST `/api/classes/{classId}/schedules`
- Tạo lịch học hàng loạt cho lớp.
- **Request**:
  ```json
  {
    "trainingClassId": "<classId>",
    "fromDate": "2024-04-01",
    "toDate": "2024-04-30",
    "daysOfWeek": ["Monday", "Wednesday"],
    "startTime": "18:00:00",
    "endTime": "19:30:00",
    "locationName": "Phòng 201",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "allowedRadiusMeters": 50
  }
  ```
- **Response 200**: danh sách lịch đã tạo.
- **Lưu ý**: `classId` trong URL phải trùng `trainingClassId` trong body.

## 6. API lịch học (`/api/schedules`)

> Yêu cầu role `Admin`.

- `GET /api/schedules/{id}`: lấy chi tiết lịch học.
- `POST /api/schedules`: tạo lịch đơn lẻ với payload giống `CreateClassScheduleRequest`.
- `PUT /api/schedules/{id}`: cập nhật lịch.
- `DELETE /api/schedules/{id}`: xóa lịch.

## 7. API điểm danh (`/api/attendance`)

### POST `/api/attendance/check-in`
- **Phân quyền**: `InstructorOnly` (giảng viên tự điểm danh).
- **Request**:
  ```json
  {
    "classScheduleId": "...",
    "instructorId": "...",  // phải trùng với instructor trong token
    "checkedInAt": "2024-04-02T10:05:00Z",
    "latitude": 10.762700,
    "longitude": 106.660200
  }
  ```
- **Response 200**: `AttendanceRecordDto` chứa kết quả điểm danh.
- **Response 403**: nếu `instructorId` không khớp với token.

### POST `/api/attendance/manual`
- **Phân quyền**: `AdminOnly`.
- **Request**:
  ```json
  {
    "classScheduleId": "...",
    "instructorId": "...",
    "occurredAt": "2024-04-02T10:05:00Z",
    "status": "Present",
    "notes": "Điểm danh thủ công",
    "ticketId": "..." // có thể null nếu không liên quan ticket
  }
  ```

### GET `/api/attendance/instructor/{instructorId}?fromDate=YYYY-MM-DD&toDate=YYYY-MM-DD`
- **Phân quyền**: Người dùng phải là admin hoặc chính giảng viên đó.
- **Response 200**: danh sách bản ghi điểm danh theo khoảng ngày.

### POST `/api/attendance/tickets`
- **Phân quyền**: `InstructorOnly`.
- **Request**:
  ```json
  {
    "classScheduleId": "...",
    "instructorId": "...",
    "reason": "Vắng mặt do công tác",
    "createdBy": "Nguyễn Văn A",
    "createdByUserId": "..." // tùy chọn
  }
  ```
- **Response 200**: `AttendanceTicketDto` vừa tạo.

### POST `/api/attendance/tickets/{ticketId}/approval`
- **Phân quyền**: `AdminOnly`.
- **Request**:
  ```json
  {
    "approve": true,
    "approver": "Trần Thị C",
    "notes": "Đã xác minh lý do",
    "updatedByUserId": "..." // tùy chọn
  }
  ```
- **Response 200**: ticket sau cập nhật, `404` nếu không tồn tại.

## 8. API bảng lương (`/api/payroll`)

### POST `/api/payroll/generate`
- **Phân quyền**: `AdminOnly`.
- **Mục đích**: tổng hợp giờ dạy trong tháng cho một giảng viên.
- **Request**:
  ```json
  {
    "instructorId": "...",
    "year": 2024,
    "month": 3
  }
  ```
- **Response 200**: `PayrollPeriodDto` chứa tổng giờ, tổng tiền và chi tiết bản ghi điểm danh.

### GET `/api/payroll/instructor/{instructorId}`
- **Phân quyền**: Admin hoặc chính giảng viên đó.
- **Response**: danh sách các kỳ lương.

### GET `/api/payroll/{payrollId}`
- **Phân quyền**: `AdminOnly`.
- **Response**: chi tiết một kỳ lương, `404` nếu không tìm thấy.

## 9. Thực hành tích hợp nhanh

1. **Đăng nhập**: gọi `POST /api/auth/google` với `idToken` → nhận `accessToken`.
2. **Thêm lớp & lịch**: dùng token admin gọi các endpoint lớp học và lịch học.
3. **Điểm danh**: giảng viên dùng token của mình gọi `POST /api/attendance/check-in`.
4. **Xử lý vắng mặt**: giảng viên tạo ticket, admin duyệt.
5. **Tổng hợp lương**: admin tạo payroll cho từng tháng và theo dõi qua các API bảng lương.

## 10. Mẹo xử lý lỗi

- **401 Unauthorized**: kiểm tra lại header `Authorization` hoặc token đã hết hạn.
- **403 Forbidden**: role hiện tại không đủ quyền hoặc `instructorId` không khớp với token.
- **404 Not Found**: đối tượng không tồn tại hoặc đã bị xóa.
- **400 Bad Request**: dữ liệu không hợp lệ (ví dụ ngày không hợp lệ, `classId` không khớp body).

Luôn kiểm tra thông báo lỗi trả về trong body để hiển thị thông tin thích hợp cho người dùng cuối.
