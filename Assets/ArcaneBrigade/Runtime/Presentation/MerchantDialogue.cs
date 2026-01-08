using UnityEngine;

namespace ArcaneBrigade
{
    public enum MerchantMoment { Enter, Inspect, Buy, BloodBuy, Denied, Reroll, Fuse, Leave }
    public sealed class MerchantDialogue
    {
        private string line;
        private float began;
        private float expires;
        private int cursor;
        private string focus;
        private float nextInspect;
        private GUIStyle copy;
        private GUIStyle name;
        public int SpokenCount { get; private set; }
        public MerchantMoment LastMoment { get; private set; }
        public string CurrentLine => line;
        public float ExpiresAt => expires;
        public int VisibleGlyphs(float now, bool reduced) => line == null ? 0 : reduced ? line.Length : Mathf.Clamp(Mathf.FloorToInt((now - began) * 58), 0, line.Length);
        public bool IsSpeaking(float now) => line != null && now >= began && now < expires && VisibleGlyphs(now, false) < line.Length;

        public static string Pick(bool special, MerchantMoment moment, int variant)
        {
            string[] choices;
            if (special)
                choices = moment switch
                {
                    MerchantMoment.Enter => new[] { "Suỵt... giấc mơ vừa mở một con mắt.", "Vào đi. Ta đã thấy ngươi trong một quả trứng." },
                    MerchantMoment.Inspect => new[] { "Đừng nhìn quá lâu. Món đó cũng biết nhìn.", "Ngươi muốn mang nó về... hay để nó mang ngươi đi?", "Ta nhận trứng. Một vài món thích máu hơn." },
                    MerchantMoment.BloodBuy => new[] { "Một chút máu thôi. Phần còn lại giữ để mơ.", "Tim còn đập là giao dịch còn đẹp." },
                    MerchantMoment.Buy => new[] { "Vỏ trứng giữ lại. Giấc mơ bên trong thuộc về ngươi.", "Đã đổi chủ. Đừng để nó nhớ tên cũ." },
                    MerchantMoment.Denied => new[] { "Chưa đủ. Ta không mua nhịp tim cuối cùng.", "Ví trống cũng có tiếng vọng. Tiếc là ta không nhận." },
                    MerchantMoment.Reroll => new[] { "Chớp mắt đi. Ta thay giấc mơ bên dưới.", "Ba điều chưa xảy ra. Chọn một." },
                    MerchantMoment.Fuse => new[] { "Hai cái tên chìm xuống. Một cái tên tỉnh dậy." },
                    _ => new[] { "Đi đi. Đừng nhìn vào giấc mơ phía sau." }
                };
            else
                choices = moment switch
                {
                    MerchantMoment.Enter => new[] { "Keng! Chợ mở. Ba mắt ta đều thấy ví ngươi!", "Mời vào! Hôm nay biên lai còn tươi." },
                    MerchantMoment.Inspect => new[] { "Món đó chưa cắn ai... trong ca bán này.", "Cứ ngắm đi. Ta chưa tính phí nhìn!", "Một mắt xem hàng, hai mắt xem túi tiền." },
                    MerchantMoment.Buy => new[] { "Chốt! Ta nhai tiền, ngươi nhận điều kỳ diệu.", "Biên lai dài hơn đời quái vật. Cầm lấy!", "Keng keng! Lại thêm một giấc mơ có chủ." },
                    MerchantMoment.Denied => new[] { "Ví đang ngủ à? Đánh thức nó rồi quay lại!", "Ta nghe tiếng leng keng... nhưng đó là răng ta." },
                    MerchantMoment.Reroll => new[] { "Hàng cũ vào bụng, hàng mới lên bàn!", "Ba món mới! Mắt thứ ba ta tự chọn đấy." },
                    MerchantMoment.Fuse => new[] { "Hai vào một ra! Đừng hỏi phép toán của ta." },
                    _ => new[] { "Xuống dưới nhé! Ta không bảo hành ác mộng." }
                };
            return choices[Mathf.Abs(variant % choices.Length)];
        }

        public void Speak(bool special, MerchantMoment moment, float now)
        {
            LastMoment = moment; line = Pick(special, moment, cursor++); began = now; expires = now + 3.8f; SpokenCount++;
            nextInspect = now + (moment == MerchantMoment.Inspect ? 1.6f : 2.2f);
        }
        public void Observe(bool special, string station, float now)
        {
            if (focus == station) return;
            if (station == null) { focus = null; return; }
            if (now < nextInspect) return;
            focus = station;
            Speak(special, MerchantMoment.Inspect, now);
        }
        public void ResetFocus() { focus = null; }
        public void Draw(bool special, float now, bool reduced)
        {
            if (line == null || now >= expires) return;
            if (copy == null) { copy = ArcadeTypography.Style(19); copy.normal.textColor = ArcadeSkin.Ink; name = ArcadeTypography.Style(18, true); name.normal.textColor = ArcadeSkin.Ink; }
            float reveal = reduced ? 1 : Mathf.Clamp01((now - began) / 0.18f);
            float width = 370 * Mathf.SmoothStep(0.9f, 1, reveal);
            Rect bubble = new Rect(1180, 260 + (1 - reveal) * 8, width, 126);
            Color previous = GUI.color; GUI.color = new Color(1, 1, 1, reveal);
            ArcadeSkin.Ticket(bubble, ArcadeSkin.Cream, special ? new Color(0.7f, 0.45f, 0.85f) : ArcadeSkin.Accent);
            GUI.Label(new Rect(bubble.x + 18, bubble.y + 12, bubble.width - 36, 27), special ? "BÀ VỎ MỘNG" : "ÔNG NUỐT BIÊN LAI", name);
            int length = VisibleGlyphs(now, reduced);
            GUI.Label(new Rect(bubble.x + 18, bubble.y + 45, bubble.width - 36, 70), line.Substring(0, length), copy);
            GUI.color = previous;
        }
    }
}
