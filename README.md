# Pharmacy Management ALN

**Đề tài**: quản lý nhà thuốc — **bán thuốc theo toa**. Ứng dụng **Windows Forms**, kiến trúc **GUI → BLL → DAL → SQL Server**, **ADO.NET** và stored procedure (`sp_DangNhap`, `sp_NhapKho`, `sp_BanThuoc`…), view báo cáo trong `SQL/View_PharmacyManagement.sql`.

Tài liệu chi tiết được tách theo vai trò:

| Tài liệu | Nội dung |
|----------|----------|
| [`project_Context.md`](project_Context.md) | Nghiệp vụ tổng thể, menu & ánh xạ form, cấu trúc thư mục, UX (màu/font), công nghệ, tích hợp (DQG…), bảo mật, kiến trúc tầng, ma trận phân quyền, gợi ý bảng dữ liệu |
| [`project_Workflow.md`](project_Workflow.md) | Luồng bước: đăng nhập, dashboard, **FEFO**, **DQG**, nhập kho (transaction), kê đơn, doanh thu, nhân viên, báo cáo, audit |

README này là **bản tổng hợp ngắn**; khi sửa chức năng hoặc luồng logic, cập nhật đúng file trong bảng trên (xem mục [Quy ước tài liệu](#quy-ước-tài-liệu)).

---

## Yêu cầu môi trường

- **Windows** (WinForms).
- **[.NET SDK](https://dotnet.microsoft.com/download)** khớp `TargetFramework` trong `PharmacyManagement/PharmacyManagement.csproj` (ví dụ `net10.0-windows`).
- **SQL Server** hoặc **LocalDB** để chạy script và kết nối ứng dụng.
- IDE gợi ý: **Visual Studio 2022** hoặc **VS Code** + C# Dev Kit.

---

## Kiến trúc & cấu trúc solution

**Tầng**: Presentation (WinForms, không SQL trực tiếp trong form) → **Pharmacy.BLL** (nghiệp vụ, transaction, kiểm tra quyền) → **Pharmacy.DAL** (`Microsoft.Data.SqlClient`) → SQL Server. **Pharmacy.DTO** truyền dữ liệu; **Pharmacy.Common** (`UserSession`, BCrypt, validate, log…).

| Project | Vai trò |
|---------|---------|
| `PharmacyManagement` | WinForms — `FrmLogin`, shell `FrmMain` (sidebar + nội dung), `FrmDashboard` (KPI/biểu đồ qua `ReportService.LayDashboardHienThi()`), các form nghiệp vụ theo menu |
| `Pharmacy.BLL` | `AuthService`, `MedicineService`, `InventoryService`, `SalesService`, `ReportService`, `AuditService`, `NhanVienAdminService`… |
| `Pharmacy.DAL` | Repository, `DbContextDAL`, kết nối qua `appsettings.json` |
| `Pharmacy.DTO` | DTO theo thực thể (nhân viên, thuốc, phiếu nhập, hóa đơn, audit…) |
| `Pharmacy.Common` | Session, mật khẩu, validate, logging |

File solution: `PharmacyManagement.slnx`. **Pharmacy.GUI** là mục tiêu tách sau; hiện form có thể gộp trong `PharmacyManagement/` — cây `Forms/` chuẩn (Auth, Main, Inventory, Product, Sales, Finance, Admin, Report) mô tả đầy đủ trong Context.

---

## Menu đề tài (tóm tắt)

1. **Dashboard** — Admin  
2. **Quản lý kho** — Admin, Kho (phiếu nhập kho · danh sách hàng nhập kho)  
3. **Thêm hàng hóa** — Admin, Kho (tab: Thông tin chung · Thuộc tính · Đơn vị)  
4. **Kê đơn bán thuốc** — Dược sĩ  
5. **Quản lý doanh thu** — Admin  
6. **Quản lý nhân viên** — Admin  
7. **Báo cáo** — cảnh báo hết hàng/hết hạn (Admin, Kho, Dược sĩ) · báo cáo thuốc (Admin)  
8. **Audit log** — Admin  

Sidebar ẩn/hiện theo `UserSession.TenVaiTro`; **kiểm tra quyền thực sự** tại **BLL** (không chỉ dựa vào ẩn menu).

**Ma trận quyền (rút gọn)**

| Chức năng | Admin | Kho | Dược sĩ |
|-----------|:-----:|:---:|:-------:|
| Dashboard | ✓ | — | — |
| Kho, Thêm hàng | ✓ | ✓ | — |
| Kê đơn | — | — | ✓ |
| Doanh thu, Nhân viên, Báo cáo thuốc, Audit | ✓ | — | — |
| Cảnh báo hết hàng/hết hạn | ✓ | ✓ | ✓ |

Chi tiết ánh xạ menu → form (`FrmDashboard`, `FrmQuanLyKho`, `FrmKeDonBanThuoc`…) xem Context §2.2.

---

## Luồng nghiệp vụ (tổng hợp từ Workflow)

**Đăng nhập**: `Program` → `ConnectionSettings.ApplyFromJsonFile()` (culture `vi-VN` nếu thống nhất) → `FrmLogin` → `AuthService` / `sp_DangNhap` → gán `UserSession` → `FrmMain`.

**Dashboard**: `ReportService.LayDashboardHienThi()` (BLL → DAL → view/SQL) — KPI, biểu đồ doanh thu tuần, trạng thái hóa đơn, lưới hóa đơn gần đây, cảnh báo. Kho (nếu sau này vào dashboard): chỉ tồn + cảnh báo, không doanh thu chi tiết.

**FEFO** (nhiều lô): truy vấn tồn theo lô còn hạn → sắp `HanSuDung` tăng dần → phân bổ số lượng lần lượt → ghi chi tiết hóa đơn theo lô; toàn bộ cập nhật tồn + lịch sử kho (+ audit nếu cần) trong **một transaction**.

**DQG**: tra cứu qua BLL/DAL; thuốc mới từ DQG bind dữ liệu chuẩn rồi bổ sung lô/HSD/giá; thuốc đã có chủ yếu thêm lô. Thêm hàng ngoài phiếu: nhánh DQG/liên thông vs hàng không DQG (tối thiểu trường khác nhau) — chi tiết Workflow §4.

**Nhập kho**: (1) Lập `PhieuNhapKho` chưa “Đã nhập kho” — **chưa** cộng tồn → (2) `ChiTietPhieuNhap` → (3) lưới chỉnh sửa khi phiếu chưa hoàn tất → (4) nút **Nhập kho**: trong transaction cập nhật tồn, `LichSuKho`, trạng thái phiếu, audit.

**Kê đơn (Dược sĩ)**: ngữ cảnh toa/bệnh nhân → thêm dòng thuốc (validate tồn, HSD) → **FEFO** → hóa đơn + transaction trừ tồn / lịch sử / in nếu có.

**Doanh thu / Nhân viên / Báo cáo / Audit**: lọc thời gian hoặc tiêu chí → BLL + view/SP (không SQL trong form); nhân viên: BCrypt, audit khi đổi quyền; audit log: chỉ Admin qua `AuditService`.

---

## Công nghệ & bảo mật (tóm tắt)

- **UI**: WinForms; palette & font: Context §3.  
- **CSDL**: SQL Server; script `SQL/PharmacyManagement.sql`, `SQL/Trigger_PharmacyManagemnt.sql`, `SQL/View_PharmacyManagement.sql`.  
- **Mật khẩu**: BCrypt; **chuỗi kết nối**: `PharmacyManagement/appsettings.json` — **không** commit secret thật.  
- **Báo cáo / Excel / logging**: ReportViewer/RDLC, ClosedXML/EPPlus, Serilog/NLog — bảng đầy đủ và quy ước thêm package: Context §5.

Lỗi *Cannot open database 'PharmacyManagement'*: tạo DB đúng instance hoặc chỉnh connection string (đồng bộ với Context).

---

## Cơ sở dữ liệu

1. Kết nối SQL Server / LocalDB đúng instance.  
2. Chạy script theo thứ tự phụ thuộc (thường):  
   - `SQL/PharmacyManagement.sql`  
   - `SQL/Trigger_PharmacyManagemnt.sql`  
   - `SQL/View_PharmacyManagement.sql`  

---

## Cấu hình chuỗi kết nối

- File: `PharmacyManagement/appsettings.json`, key `ConnectionStrings:PharmacyManagement`.  
- File được copy ra thư mục output khi build (`CopyToOutputDirectory`).  
- Dùng giá trị local hoặc file mẫu `.example` nếu team thống nhất; không đẩy mật khẩu máy chủ thật lên git.

---

## Build & chạy

Từ thư mục gốc repo:

```bash
dotnet build PharmacyManagement.slnx
dotnet run --project PharmacyManagement/PharmacyManagement.csproj
```

Hoặc mở `PharmacyManagement.slnx` trong Visual Studio và **Start** (F5).

---

## Quy ước phát triển

1. Đọc [`project_Context.md`](project_Context.md) và phần luồng liên quan trong [`project_Workflow.md`](project_Workflow.md) trước khi sửa code.  
2. Form **không** ghi SQL trực tiếp — chỉ gọi **BLL**.  
3. Đổi schema: cập nhật `SQL/*.sql` đồng bộ với code.  
4. Nhập/bán/điều chỉnh tồn: **transaction**; thao tác nhạy cảm: **audit**.  
5. UI mới: tuân màu/font/bố cục trong Context §3.  
6. Thư viện mới: cập nhật Context §5 + mô tả trong PR/commit.

---

## Quy ước tài liệu

| Thay đổi | Cập nhật |
|----------|----------|
| Menu, form, thư mục, UX, stack, ma trận quyền, nghiệp vụ & bảng dữ liệu tĩnh | `project_Context.md` |
| Thứ tự bước, FEFO/DQG/nhập kho/bán/audit… | `project_Workflow.md` |
| Cả menu và luồng | **Cả hai** |

---

## Giấy phép

*(Bổ sung khi dự án có quy định license.)*
