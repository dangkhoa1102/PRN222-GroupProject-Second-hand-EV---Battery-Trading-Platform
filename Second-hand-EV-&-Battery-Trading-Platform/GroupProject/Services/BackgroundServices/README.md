# Background Services

## OrderAutoCancelService

### Mô tả
Background Service tự động hủy đơn hàng sau 24 giờ nếu buyer không nhận hàng sau khi seller đã xác nhận.

### Vị trí
- **File**: `GroupProject/Services/BackgroundServices/OrderAutoCancelService.cs`
- **Namespace**: `GroupProject.Services.BackgroundServices`

### Cách hoạt động

1. **Chạy định kỳ**: Service chạy định kỳ mỗi 1 giờ để kiểm tra các đơn hàng
2. **Điều kiện hủy**: 
   - OrderStatus = "Confirmed" (seller đã xác nhận)
   - CompletedDate (ngày seller confirm) đã qua hơn 24 giờ
   - DeliveryDate = null (buyer chưa confirm nhận hàng)
3. **Hành động**: 
   - Cập nhật OrderStatus = "Cancelled"
   - Set CancellationReason = "Người mua không nhận hàng sau 24 giờ"
   - Ghi log để theo dõi

### Đăng ký Service

Service được đăng ký trong `Program.cs`:

```csharp
builder.Services.AddHostedService<OrderAutoCancelService>();
```

### Cấu hình

- **Check Interval**: Mỗi 1 giờ (có thể thay đổi trong code)
- **Cancel After**: 24 giờ (có thể thay đổi trong code)

### Logging

Service sử dụng ILogger để ghi log:
- Log thông tin khi service start/stop
- Log khi tìm thấy đơn hàng cần hủy
- Log lỗi nếu có vấn đề xảy ra

### Lưu ý

- Service chạy độc lập với HTTP requests
- Service sẽ tự động restart nếu có lỗi xảy ra
- Service không ảnh hưởng đến performance của ứng dụng vì chạy trong background thread riêng

