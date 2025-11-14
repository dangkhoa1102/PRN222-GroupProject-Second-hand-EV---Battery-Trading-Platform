# Second-hand EV & Battery Trading Platform – Luồng Hoạt Động & Services

Tài liệu này tổng hợp cách các luồng nghiệp vụ chính vận hành và mô tả các service (bao gồm cả background service) đang hoạt động trong dự án ASP.NET Core `GroupProject`.

## 1. Tổng quan luồng nghiệp vụ

### 1.1 Luồng quản lý tin đăng & kiểm duyệt
- **Tạo tin**: khách hàng chọn loại sản phẩm (xe/pin), nhập hãng, model, năm, tình trạng, giá bán, mô tả chi tiết và tải bộ hình ảnh minh họa.
- **Gửi duyệt**: tin được lưu ở trạng thái `Pending` cùng thời gian gửi.
- **Staff kiểm duyệt**:
  - Kiểm tra nội dung, hình ảnh, mức giá và dấu hiệu bất thường/lừa đảo.
  - Quyết định:
    - `Approve`: tin chuyển sang `Active`, hiển thị công khai trên trang chủ và kết quả tìm kiếm.
    - `Reject`: ghi rõ lý do, gửi thông báo cho khách hàng.
    - `RequestChanges`: trả về cho khách chỉnh sửa rồi gửi lại.
- **Sau khi duyệt**:
  - Người đăng có thể tạm ẩn, chỉnh sửa giá hoặc xóa tin khi đã bán.
  - Staff tiếp tục theo dõi, nhận báo cáo và gỡ tin vi phạm chính sách.

### 1.2 Luồng mua bán
1. **Khách tìm kiếm**: sử dụng bộ lọc, xem chi tiết tin (bao gồm lịch sử đánh giá của người bán).
2. **Đặt hàng**: buyer tạo order → hệ thống giữ chỗ và thông báo cho seller.
3. **Seller phản hồi**: xác nhận hoặc từ chối; khi xác nhận, order chuyển `Confirmed`.
4. **Giao dịch**: quá trình thanh toán/giao hàng cập nhật qua các trạng thái (`Shipped`, `Delivered`).
5. **Hoàn tất**:
   - Buyer xác nhận đã nhận hàng → order `Completed`.
   - Nếu buyer im lặng quá 24h, background service tự động chuyển `Cancelled`.

### 1.3 Luồng đánh giá & xây dựng uy tín
- Sau khi order hoàn tất, buyer và seller có thể để lại review.
- `ReviewService` ghi nhận điểm số + nhận xét, cập nhật thống kê `UserReviewSummary`.
- Điểm trung bình hiển thị trên trang listing nhằm tăng tính minh bạch.

## 2. Danh sách application services chính (BLL/Services)

| Service | Vai trò | File chính |
|---------|---------|-----------|
| `AuthService` | Đăng ký, đăng nhập, phát JWT/refresh token, quản lý vai trò | `BLL/Services/AuthService.cs` |
| `VehicleListingService`, `BatteryListingService` | CRUD tin đăng, tìm kiếm, thay đổi trạng thái listing | `BLL/Services/VehicleListingService.cs`, `BatteryListingService.cs` |
| `BuyerOrderService` | Tạo order, kiểm tra tồn tại, tính toán phí, liên kết buyer/seller | `BLL/Services/BuyerOrderService.cs` |
| `OrderService` | Cập nhật vòng đời order (Pending → Confirmed → Completed/Cancelled) | `BLL/Services/OrderService.cs` |
| `ReviewService` | Ghi nhận review, thống kê điểm uy tín, trả về dữ liệu cho UI | `BLL/Services/ReviewService.cs` |
| `AdminReviewService` | Dashboard kiểm duyệt tin, phê duyệt/từ chối/ yêu cầu sửa | `BLL/Services/AdminReviewService.cs` |
| `AdminTransactionService` | Giám sát giao dịch, báo cáo doanh số, xử lý tranh chấp | `BLL/Services/AdminTransactionService.cs` |
| `BuyerService` / `Seller` flows | Cung cấp dữ liệu hồ sơ, lịch sử giao dịch cho Razor Pages | `BLL/Services/BuyerService.cs` (và các service liên quan) |
| `NotificationService` + `NotificationHub` | Đẩy thông báo realtime (SignalR) khi tin/đơn/đánh giá thay đổi | `GroupProject/Services/NotificationService.cs`, `GroupProject/Hubs/NotificationHub.cs` |

Tất cả service được đăng ký tại `Program.cs` thông qua DI container, tận dụng repository trong `DAL/Repository` để tách biệt logic nghiệp vụ và tầng dữ liệu.

## 3. Background services & tiến trình hỗ trợ

### 3.1 OrderAutoCancelService
- **Vị trí**: `GroupProject/Services/BackgroundServices/OrderAutoCancelService.cs`
- **Mục tiêu**: Bảo vệ người bán khỏi đơn treo quá lâu, đảm bảo SLA giao dịch.
- **Cách chạy**:
  1. Dùng `PeriodicTimer` quét mỗi 1 giờ.
  2. Điều kiện hủy:
     - `OrderStatus == Confirmed`
     - `CompletedDate` đã quá 24 giờ
     - `DeliveryDate == null`
  3. Hành động: cập nhật trạng thái `Cancelled`, set `CancellationReason = "Người mua không nhận hàng sau 24 giờ"`, log thông tin.
- **Đăng ký**:
  ```csharp
  builder.Services.AddHostedService<OrderAutoCancelService>();
  ```
- **Đặc điểm**: Chạy nền, tự restart khi lỗi, không chặn HTTP thread, toàn bộ hoạt động được log qua `ILogger`.

### 3.2 Notification flow (SignalR)
- `NotificationService` bắn sự kiện (tin duyệt, order cập nhật, yêu cầu sửa).
- `NotificationHub` phát realtime tới client đang kết nối; client offline sẽ nhận thông báo khi login nhờ dữ liệu lưu DB.

## 4. Mapping luồng ↔ services
- **Listing flow**: Razor Pages gọi `VehicleListingService`/`BatteryListingService` → repository đọc/ghi DB → NotificationService báo cho staff/buyer.
- **Moderation**: `AdminReviewService` xử lý các tin trạng thái `Pending`, áp dụng `ListingStatus` trong `BLL/Constants/ListingStatus.cs`, ghi lý do vào `ListingActionResultDto`.
- **Purchase flow**: `BuyerOrderService` tạo order, `OrderService` duy trì vòng đời, `OrderAutoCancelService` xử lý timeout.
- **Review flow**: `ReviewService` ghi nhận đánh giá, trả summary qua `UserReviewSummaryDto`, cập nhật điểm hiển thị trên listing.
- **Realtime update**: Notification Service/Hub giúp buyer, seller, staff nhận thông tin lập tức để phản hồi nhanh.

## 5. Import database từ file `.bacpac`

### 5.1 Vị trí file
- File backup nằm ở thư mục gốc của repo: `EVTradingPlatform.bacpac`.

### 5.2 Import bằng SQL Server Management Studio (SSMS)
1. Mở SSMS và kết nối đến SQL Server (LocalDB hoặc instance thật).
2. Chuột phải vào `Databases` → `Import Data-tier Application`.
3. Chọn **Import from local disk**, duyệt tới file `EVTradingPlatform.bacpac`.
4. Đặt tên database (ví dụ `EVTradingPlatform`) và đường dẫn lưu data/log nếu cần.
5. Xem lại cấu hình ở bước Summary → `Finish`. Sau khi hoàn tất, toàn bộ schema + dữ liệu mẫu sẽ được tạo trên server.

### 5.3 Import nhanh bằng dòng lệnh (tùy chọn)
Nếu đã cài `sqlpackage`, có thể chạy:
```
sqlpackage /Action:Import /SourceFile:"EVTradingPlatform.bacpac" /TargetServerName:"(localdb)\MSSQLLocalDB" /TargetDatabaseName:"EVTradingPlatform"
```
Tùy chỉnh `TargetServerName` và `TargetDatabaseName` sao cho khớp môi trường của bạn.

---

Khi bổ sung luồng mới hoặc chỉnh sửa service, hãy cập nhật README để mọi thành viên nắm rõ cách hệ thống phối hợp giữa UI, service, repository và background processes.

