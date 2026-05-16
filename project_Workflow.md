# PharmacyManagement — Luồng hoạt động chức năng

Tài liệu này **chỉ** mô tả **luồng hoạt động** (bước nghiệp vụ / logic xử lý). Cây menu, giao diện, ánh xạ form, cấu trúc thư mục, công nghệ và nghiệp vụ tổng thể nằm trong **`project_Context.md`**.

---

## 1. Đăng nhập và vào hệ thống

1. `Program` áp dụng cấu hình (`ConnectionSettings.ApplyFromJsonFile()`), culture `vi-VN` nếu đã thống nhất.
2. Mở `FrmLogin` → người dùng nhập tài khoản → `AuthService` / stored procedure `sp_DangNhap` (DAL).
3. Thành công: gán `UserSession` (vai trò, thông tin hiển thị) → mở **`FrmMain`** (shell); chọn **Dashboard** nhúng **`FrmDashboard`**.
4. Lỗi CSDL: bắt `SqlException`, thông báo gợi ý (thiếu DB `PharmacyManagement`, sai server…) — đồng bộ với `project_Context.md`.

---

## 2. Dashboard (Admin — và phần hiển thị theo vai trò)

1. Sau đăng nhập (hoặc chọn mục Dashboard trên shell), tải dữ liệu qua `ReportService.LayDashboardHienThi()` (BLL → DAL → view/SQL).
2. Bind KPI, biểu đồ cột doanh thu tuần, biểu đồ tròn trạng thái hóa đơn, lưới hóa đơn gần đây, cảnh báo.
3. Với **Kho** (nếu shell cho phép vào dashboard tương lai): chỉ bind chỉ số tồn + cảnh báo — không bind doanh thu/hóa đơn nhạy cảm (theo phân quyền trong Context).

---

## 3. FEFO (First Expired, First Out) — khi bán / xuất kho

**Nguyên tắc**: một mã thuốc có **nhiều lô** → ưu tiên **xuất trừ lô sắp hết hạn trước** (HSD nhỏ nhất trong các lô còn đủ SL), không xuất lô mới khi lô cũ vẫn đủ SL và còn hạn.

**Luồng (BLL điều phối)**:

1. Người dùng chọn thuốc + số lượng bán (kê đơn / bán theo toa).
2. Truy vấn tồn theo lô: các dòng `SoLuongTon > 0` và (theo quy định) chưa quá hạn hoặc xử lý ngoại lệ có kiểm soát.
3. Sắp xếp lô theo `HanSuDung` tăng dần (cùng HSD có thể thêm `NgayNhap` hoặc ID lô để thứ tự ổn định).
4. Phân bổ SL: trừ lần lượt từ đầu danh sách đến khi đủ; không đủ tổng tồn → từ chối hoặc bán một phần theo UX đã thống nhất.
5. Ghi **chi tiết hóa đơn theo từng lô** (truy vết, đổi trả, kiểm kê).
6. Trong **một transaction**: cập nhật tồn từng lô, ghi `LichSuKho` (hoặc tương đương), ghi audit nếu là thao tác nhạy cảm.

**UI**: có thể gợi ý lô tự chọn; nếu cho đổi lô thủ công, vẫn validate không vượt SL lô và đúng chính sách.

---

## 4. Danh mục dược quốc gia (DQG)

**Mục tiêu**: chuẩn hóa theo DQG (số đăng ký, mã DQG, hoạt chất, hàm lượng, quy cách…).

### 4.1. Tầng gọi API

- Gọi API / nguồn DQG qua **BLL/DAL** (service bọc endpoint); GUI không gọi trực tiếp endpoint nếu team chuẩn hóa.

### 4.2. Nhập kho — thuốc chưa có trong hệ thống

1. Tại **Danh sách hàng nhập kho**, chọn thêm dòng từ DQG.
2. Nhập số đăng ký hoặc từ khóa tên → service tra cứu DQG → danh sách kết quả.
3. Người dùng chọn **một** bản ghi chuẩn.
4. Hệ thống bind hoạt chất, hàm lượng, đóng gói, hãng SX, nước SX, mã DQG… (phần sửa sau bind theo quy ước dự án).
5. Bổ sung: nhóm hàng, SL, lô, HSD, vị trí kệ, đơn vị nhập, VAT, giá nhập, giá bán (giá bán **tự gợi ý** khi nhập giá nhập theo `GiaBanBoYTeHelper`; có thể sửa tay).
6. Lưu phiếu (chưa nhập kho): tạo mới `Thuoc` + `ChiTietPhieuNhap`; **chưa** cộng tồn.

### 4.3. Nhập kho — thuốc đã có

1. Tìm mã/tên `Thuoc`.
2. Hiển thị tên, đơn vị, giá bán, tồn, mã DQG (nếu có).
3. Nhập SL, giá nhập (→ gợi ý giá bán BYT), lô, HSD, VAT… → lưu chi tiết; chủ yếu thêm **lô mới**.

### 4.4. Thêm hàng hóa ngoài phiếu nhập

- **TH1 — DQG / cho phép liên thông**: bật “Cho phép liên thông” → bắt buộc: số đăng ký, mã DQG, hoạt chất, hàm lượng, đơn vị, hãng SX; lưu `ChoPhepLienThong = 1` (hoặc tương đương).
- **TH2 — Không DQG** (mỹ phẩm, TPCN, vật tư…): không bắt buộc DQG/số đăng ký BYT; tối thiểu: tên hàng, đơn vị, giá bán, giá mua, VAT, nhóm hàng.

### 4.5. Cache & lỗi mạng (khuyến nghị)

- Cache tra cứu theo session hoặc TTL ngắn.
- Timeout / lỗi API: thông báo rõ; nếu chính sách cho phép, nhập tạm thủ công và đánh dấu đối soát sau.

---

## 5. Nhập kho (Quản lý kho) — tách chứng từ và cộng tồn

**Ý tưởng**: lập chứng từ trước; chỉ khi **“Nhập kho”** mới cộng tồn + lịch sử.

### Bước 1 — Lập phiếu (`PhieuNhapKho`)

- Nhập thông tin chứng từ (NCC, VAT, chiết khấu, công nợ, ngày, nhân viên, kho…); `TrangThai` chưa “Đã nhập kho” → **chưa** cộng tồn.

### Bước 2 — Chi tiết (`ChiTietPhieuNhap`)

- Một phiếu — nhiều dòng chi tiết; áp dụng luồng DQG mục 4 cho thuốc mới/cũ.

### Bước 3 — Lưới danh sách

- Thêm / sửa / xóa dòng khi phiếu **chưa** “Đã nhập kho”.

### Bước 4 — Xác nhận “Nhập kho”

Trong **một transaction**:

1. Cập nhật tồn (theo thiết kế bảng tồn/lô).
2. Ghi `LichSuKho` (hoặc tương đương).
3. `PhieuNhapKho.TrangThai` → **Đã nhập kho**.
4. Ghi audit (ví dụ: nhập kho phiếu PNxxx).

**Tham chiếu cột gợi ý** `PhieuNhapKho` / `ChiTietPhieuNhap`: xem bảng trong **`project_Context.md`** (nghiệp vụ dữ liệu nhập kho).

---

## 6. Kê đơn bán thuốc (Dược sĩ)

1. Mở màn kê đơn → nhập ngữ cảnh toa / bệnh nhân tối thiểu theo thiết kế.
2. Thêm dòng thuốc: tìm `Thuoc` → hiển thị tồn, cảnh báo HSD (màu theo chuẩn UI trong Context).
3. Nhập liều dùng, số ngày, SL — validate (không vượt tồn, quy tắc thuốc kiểm soát nếu có).
4. Áp dụng **FEFO** (mục 3) khi ghi nhận xuất lô.
5. Lập hóa đơn / phiếu bán + chi tiết; tính tiền (VAT, chiết khấu nếu có).
6. Transaction: trừ tồn theo lô đã phân bổ, ghi lịch sử kho, audit nếu cần.
7. In / xem trước chứng từ (theo stack báo cáo/in trong Context).

---

## 7. Quản lý doanh thu (Admin)

1. Chọn khoảng thời gian (ngày / tháng / năm).
2. Truy vấn hóa đơn hoàn tất (join chi tiết nếu cần lợi nhuận).
3. Tổng hợp doanh thu, giá vốn (nếu có), lợi nhuận gộp.
4. Xuất Excel nếu có chức năng (ClosedXML / EPPlus theo Context).

---

## 8. Quản lý nhân viên (Admin)

1. CRUD nhân viên, gán vai trò (Admin / Kho / Dược sĩ; legacy “Quản lý” map theo Context).
2. Tạo / đổi mật khẩu: hash BCrypt, không plaintext (theo Context).
3. Ghi audit khi đổi quyền hoặc vô hiệu hóa tài khoản.

---

## 9. Báo cáo — cảnh báo & báo cáo thuốc

### 9.1. Cảnh báo hết hàng / hết hạn

1. Nạp dữ liệu từ view/query tồn — ngưỡng tồn, HSD (ví dụ `SQL/View_PharmacyManagement.sql` khi đã triển khai).
2. Phân loại hiển thị: tồn thấp, sắp hết hạn, đã hết hạn (màu theo Context).

### 9.2. Báo cáo thuốc (Admin)

1. Chọn tiêu chí báo cáo (danh mục, tồn lô, bán chạy/chậm…).
2. Gọi view/SP báo cáo qua BLL; không nhúng SQL trong form.

---

## 10. Audit log (Admin / Quản lý)

1. Mở **Audit log** trong shell → `Forms/Admin/FrmAuditLog.cs` (nhúng trong `FrmMain`).
2. Gọi `AuditService`: dữ liệu từ `vw_AuditLogChiTiet`; lọc tham số hóa (khoảng ngày, `MaNhanVien`, `HanhDong`, từ khóa `CHARINDEX` trên nội dung / mã bản ghi / tên bảng); phân trang `OFFSET/FETCH`.
3. `BLL` từ chối nếu không phải **Admin** hoặc **Quản lý** (`BllAuthorization`).
4. Xuất tra cứu: CSV UTF-8 BOM (mở bằng Excel); cập nhật view thêm `MaNhanVien` khi áp script `SQL/View_PharmacyManagement.sql`.
---

## 11. Ghi chú đồng bộ tài liệu

Thay đổi **bước luồng**, thứ tự xử lý, FEFO/DQG/nhập kho/bán/audit → cập nhật **file này**. Thay đổi **menu, form, thư mục, stack, bảo mật, bảng nghiệp vụ tĩnh** → cập nhật **`project_Context.md`**.

---

*Phiên bản Workflow: 2.0 — chỉ còn luồng hoạt động; menu/UX/cấu trúc chuyển sang `project_Context.md`.*
