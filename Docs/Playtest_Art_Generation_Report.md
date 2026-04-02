# Bộ art Playtest — 20/10/2026

Đã tạo và lưu đủ **13/13 ảnh** bằng công cụ **painted tích hợp**. Các bản cuối được chép nguyên byte từ đầu ra công cụ, không resize, thêm lề, cắt ảnh hay xóa nền bằng code.

- [Bảng xem nhanh 13 ảnh](Playtest_Art_Gallery.html).
- [Prompt gốc và ảnh tham chiếu](Playtest_Art_Prompts_ALL.txt).
- [Các hướng dẫn bổ sung khi sửa ảnh](Playtest_Art_Prompt_Adjustments.json).
- [Nguồn ảnh, hash và kiểm tra định dạng/alpha](Playtest_Art_Provenance.json).
- [Nhật ký tiến độ](Playtest_Art_Progress.txt).

Theo lựa chọn của chủ dự án, giữ kích thước công cụ xuất: **11 sheet RGBA 1254×1254**, **2 ảnh nền RGB 1672×941**. Hub_Stations dùng bố cục **4 cột × 5 hàng**, hai trạng thái bụi/phục hồi nằm cạnh nhau.

Các sheet có nền alpha thật và chủ thể nhìn thấy cách mép canvas. Đầu ra vẫn có nhiễu alpha cực nhẹ ở biên: đa số tối đa 1/255; Hub_Stations tối đa 8/255. Hai góc của UI_Panels và Env_LampHall có alpha 1/255 thay vì 0 tuyệt đối. Số đo chi tiết nằm trong JSON; không chỉnh alpha bằng code để giữ nguyên đầu ra.

Đã kiểm tra giải mã PNG, kích thước, mode màu, lề chủ thể với ngưỡng alpha >16/255 và hash trùng nguồn. Đây là bộ ảnh để duyệt và tích hợp; chưa thực hiện cắt sprite, thiết lập 9-slice hoặc thay UI runtime trong đợt này.

| Ảnh | File |
| --- | --- |
| UI_Buttons | [PNG](../Assets/ArcaneBrigade/Art/Playtest/UI/UI_Buttons_v1.png) |
| UI_Panels | [PNG](../Assets/ArcaneBrigade/Art/Playtest/UI/UI_Panels_v1.png) |
| UI_Controls | [PNG](../Assets/ArcaneBrigade/Art/Playtest/UI/UI_Controls_v1.png) |
| UI_HUD | [PNG](../Assets/ArcaneBrigade/Art/Playtest/UI/UI_HUD_v1.png) |
| UI_BloodPrice | [PNG](../Assets/ArcaneBrigade/Art/Playtest/UI/UI_BloodPrice_v1.png) |
| UI_RoleBadges | [PNG](../Assets/ArcaneBrigade/Art/Playtest/UI/UI_RoleBadges_v1.png) |
| UI_MenuBackdrop | [PNG](../Assets/ArcaneBrigade/Art/Playtest/UI/UI_MenuBackdrop_v1.png) |
| Env_WaitingRoom | [PNG](../Assets/ArcaneBrigade/Art/Playtest/Environment/Env_WaitingRoom_v1.png) |
| Env_PostOffice | [PNG](../Assets/ArcaneBrigade/Art/Playtest/Environment/Env_PostOffice_v1.png) |
| Env_LampHall | [PNG](../Assets/ArcaneBrigade/Art/Playtest/Environment/Env_LampHall_v1.png) |
| Env_ArchiveVault | [PNG](../Assets/ArcaneBrigade/Art/Playtest/Environment/Env_ArchiveVault_v1.png) |
| Hub_WaitingRoom | [PNG](../Assets/ArcaneBrigade/Art/Playtest/Hub/Hub_WaitingRoom_v1.png) |
| Hub_Stations | [PNG](../Assets/ArcaneBrigade/Art/Playtest/Hub/Hub_Stations_v1.png) |
