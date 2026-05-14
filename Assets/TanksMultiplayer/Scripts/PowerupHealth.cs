/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Triển khai powerup tùy chỉnh để thêm điểm máu cho người chơi.
    /// </summary>
	public class PowerupHealth : Collectible
    {
        /// <summary>
        /// Số điểm máu cần thêm cho mỗi lần tiêu thụ.
        /// </summary>
        public int amount = 5;


        /// <summary>
        /// Ghi đè hành vi mặc định bằng một triển khai tùy chỉnh.
        /// Kiểm tra máu hiện tại và thêm máu bổ sung.
        /// </summary>
        public override bool Apply(Player p)
        {
            if (p == null)
                return false;

            int value = p.GetView().GetHealth();

            //không thêm máu nếu nó đã ở mức tối đa rồi
            if (value == p.maxHealth)
                return false;

            //lấy giá trị máu hiện tại và thêm lượng máu vào đó
            value += amount;

            //chúng ta phải giới hạn (clamp) máu ở mức tối đa, để
            //chúng ta không vô tình vượt quá giới hạn. Sau đó gán
            //giá trị máu mới lại cho người chơi
            value = Mathf.Clamp(value, value, p.maxHealth);
            p.GetView().SetHealth(value);

            //trả về thu thập thành công
            return true;
        }
    }
}
