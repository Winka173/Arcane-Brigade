namespace ArcaneBrigade
{
    public static class ExpandedDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier)
        {
            string text = weapon.Family switch
            {
                ExpandedWeapon.Tea => "Trà tách thành hai giọt khi va chạm, mỗi giọt gây 35% sát thương và không tách tiếp. Giọt chính để lại vùng nóng 1,8 giây, gây sát thương mỗi 0,4 giây. Lót cốc mở rộng nhưng làm yếu vùng nóng. Mục tiêu đang cháy giúp búa tạo hơi nước.",
                ExpandedWeapon.TetherBow => "Mũi tên nảy tối đa " + (tier >= 2 ? 3 : 2) + " lần giữa các mục tiêu trong bán kính 3; mỗi lần còn 70% sát thương. Đòn đầu nối mục tiêu gần, gây 25% sát thương mỗi đường nối trong bán kính 2. Không đánh lặp mục tiêu cũ bằng cùng viên đạn.",
                ExpandedWeapon.Scissors => "Hai nhát cắt hẹp cách nhau 0,12 giây, mỗi nhát gây 65% sát thương. Chọn vị trí để trúng cả hai; không quét vòng tròn.",
                ExpandedWeapon.Camera => "Chớp sáng hình quạt trúng tối đa " + (tier >= 2 ? 4 : 3) + " mục tiêu, làm lộ điểm yếu trong 2 giây. Đòn đồng minh thêm 15% cơ hội chí mạng lên mục tiêu đó.",
                ExpandedWeapon.Fish => "Bay ra rồi quay về nguồn sau hit đầu hoặc0.7s khi trượt. Đường về có thể trúng mục tiêu khác; mỗi mục tiêu chỉ nhận một hit mỗi lần ném, không nhân damage khi quay về.",
                ExpandedWeapon.Iron => "Đòn phía trước để lại vệt nóng " + (tier >= 2 ? "rộng hơn " : "") + "trong 1,4 giây, gây sát thương mỗi 0,4 giây. Mực nhiệt nối các dấu đang còn hiệu lực.",
                ExpandedWeapon.Hammer => "Đòn thứ ba lấy đà lâu hơn 50%, chu kỳ dài hơn 35%, rồi gây sát thương vùng bằng 55%. Mục tiêu đang cháy tạo thêm hơi nước gây 30% sát thương.",
                ExpandedWeapon.Alarm => "Khóa một vị trí trên sàn, báo trước 0,9 giây rồi nổ " + (tier >= 2 ? "rộng hơn" : "một lần") + ". Kẻ địch có thể né; không bám mục tiêu sau khi bắn.",
                ExpandedWeapon.MusicBox => "Nốt nhạc nhẹ bám mục tiêu; mỗi nốt thứ ba kéo dài dấu đang có lên ít nhất 0,6 giây. Đĩa xước thêm đúng một tiếng vọng trễ.",
                _ => ""
            };
            return text + "\nHiệu ứng phụ không lặp vô hạn. Nhịp đánh bắt đầu lại mỗi màn; không cần nút kỹ năng, không giới hạn số đồng minh.";
        }
    }
}
