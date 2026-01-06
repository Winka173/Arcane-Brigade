# K7 — production hardening và nghiệm thu

2026-10-14. Windows candidate/rework đã bàn giao; production/device/human approval giữ OPEN. Checkpoint mới nhất: `K7_Final_Checkpoint.md` và `K7_Quality_Rework_Plan.md`; không tự ghi release complete.

1. K7.1: audio bus/ducking/priority/ensemble, composition/SFX original và audition package; sound approval riêng.
2. K7.2: controller movement/navigation, touch/reset/lifecycle và presentation options; profile migration.
3. K7.3: safe-area/font/transition/readability/GC hot paths; kiểm tra layout nhiều aspect trên Windows.
4. K7.4: native frame-time/memory/GC/GPU và video realtime; full-run/difficulty/Endless diagnostics, không giả bot là human feel test.
5. K7.5: regression/release candidate, dependency report Android/iOS/Steam, checklist art/audio/playtest và push.

Audit ban đầu: Unity6000.4.8f1 Windows có Windows/WebGL modules, thiếu Android/iOS; adb chỉ có emulator offline, không device online; chưa có Mac/Xcode/certificates/Steamworks credentials. Không cài thêm SDK lớn hoặc giả lập release approvals. Gate runtime Windows làm ngay; external gates giữ OPEN cho tới có evidence thật.

Nguyên tắc: không cap ally/damage để đạt FPS; không đổi RNG/combat/threat bằng accessibility/cosmetic/audio. Save diagnostics cô lập. Audio source/code/licensing đi kèm; waveform checks chỉ kỹ thuật, không musical approval.
