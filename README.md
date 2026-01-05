# HospitalTriageSystem – Hệ thống Tiếp nhận & Phân luồng Khám chữa bệnh (ASP.NET Core 8 MVC)

## 1. Stack / Kiến trúc
- **Backend**: ASP.NET Core 8
- **Frontend**: ASP.NET Core MVC + Razor Views (không dùng React)
- **Database**: SQL Server + EF Core (Code First + Migration)
- **Auth**: ASP.NET Core Identity (Cookie-based)
- **Authorization**: Role-based + custom filters:
  - `RequireLoginAttribute`
  - `RequireRoleAttribute`
- **Logging**: Serilog
- **Swagger**: chỉ cho API (`/swagger`)
- **Testing**: xUnit + EF InMemory + Moq (có sẵn package)
- **CI/CD**: GitHub Actions (Build → Test → Coverage >= 70%)

## 2. Solution Structure (Layered Architecture)
```
HospitalTriageSystem.sln
src/
  HospitalTriage.Domain/           (Entities, Enums)
  HospitalTriage.Application/      (Services, Interfaces, Models, Rules)
  HospitalTriage.Infrastructure/   (EF Core DbContext, Repositories, Seed, Migrations)
  HospitalTriage.Web/              (MVC Controllers, ApiServices, ViewModels, Razor Views)
tests/
  HospitalTriage.Tests/            (xUnit tests + InMemory)
```

## 3. Users/Role seed (mặc định)
> Khi chạy lần đầu, hệ thống tự `Migrate()` và seed dữ liệu.

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@hospital.local` | `Admin@12345` |
| Receptionist | `receptionist@hospital.local` | `Receptionist@12345` |
| Nurse | `nurse@hospital.local` | `Nurse@12345` |
| Manager | `manager@hospital.local` | `Manager@12345` |
| Doctor | `DR001@hospital.local` | `Doctor@12345` |

> Lưu ý: User doctor có `UserName = Doctor.Code` (ví dụ `DR001`) để phục vụ rule: Doctor chỉ xem dashboard khoa của mình.

## 4. Chạy project (Local)
### 4.1 Prerequisites
- .NET SDK 8
- SQL Server (LocalDB/Express/Developer đều được)

### 4.2 Cấu hình connection string
Mặc định trong `src/HospitalTriage.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=HospitalTriageDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 4.3 Run Web
```bash
dotnet run --project src/HospitalTriage.Web
```

- Web UI: `https://localhost:<port>/`
- Swagger API: `https://localhost:<port>/swagger`

## 5. EF Core Migration
Project đã có migration `InitialCreate`.

Nếu muốn chạy thủ công:
```bash
dotnet ef database update --project src/HospitalTriage.Infrastructure --startup-project src/HospitalTriage.Web
```

## 6. Test + Coverage
```bash
dotnet test HospitalTriageSystem.sln
```

Test sử dụng EF Core InMemory và coverlet. CI enforce coverage line >= 70%.

## 7. Module theo Sprint
### Sprint 1 – Danh mục + Auth
- Department CRUD + search (MVC + ApiService + ViewModel)
- Doctor CRUD + search + dropdown Department
- Identity + Roles + Seed Admin + Filters

### Sprint 2 – Tiếp nhận & Phân luồng
- Tiếp nhận: Upsert Patient + tạo Visit + sinh QueueNumber theo ngày
- Phân luồng: nhập symptoms, rule engine gợi ý khoa, workflow trạng thái, lưu `VisitStatusHistory`

### Sprint 3 – Dashboard & Báo cáo
- Dashboard: tổng bệnh nhân theo status + danh sách đang chờ
  - Doctor view bị giới hạn theo khoa của mình
- Reports: lượt khám theo ngày + thời gian chờ trung bình + export CSV

## 8. API Endpoints
- `GET/POST/PUT/DELETE /api/departments`
- `GET/POST/PUT/DELETE /api/doctors`
- `GET /api/patients/{id}`
- `POST /api/patients/upsert`
- `GET /api/visits/{id}`
- `GET /api/visits?status=WaitingTriage`
- `POST /api/visits`
- `POST /api/visits/{id}/status`
- `POST /api/triage`
- `GET /api/dashboard`
- `GET /api/reports`
- `GET /api/reports/csv`

---
Nếu cần mở rộng (phân quyền chi tiết hơn, mapping User ↔ Doctor chuẩn, lịch khám, kết quả khám...), có thể bổ sung bảng mapping mà không phá kiến trúc Layered hiện tại.
