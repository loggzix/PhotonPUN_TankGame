/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

namespace TanksMP
{
    /// <summary>
    /// Triển khai powerup tùy chỉnh để thêm điểm giáp cho người chơi.
    /// </summary>
	public class PowerupShield : Collectible
    {
        /// <summary>
        /// Số điểm giáp cần thêm cho mỗi lần tiêu thụ.
        /// </summary>
        public int amount = 3;


        /// <summary>
        /// Ghi đè hành vi mặc định bằng một triển khai tùy chỉnh.
        /// Kiểm tra giáp hiện tại và thêm điểm giáp bổ sung.
        /// </summary>
		public override bool Apply(Player p)
        {
            if (p == null)
                return false;

            int value = p.GetView().GetShield();

            //không thêm giáp nếu nó đã ở mức tối đa rồi
            if (value == amount)
                return false;

            //gán điểm giáp tuyệt đối cho người chơi
            //chúng ta không thể vượt quá mức tối đa nên không cần kiểm tra ở đây
            p.GetView().SetShield(amount);

            //trả về thu thập thành công
            return true;
        }
    }
}
