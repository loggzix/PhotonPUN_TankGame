/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

namespace TanksMP
{
    /// <summary>
    /// Triển khai powerup tùy chỉnh để thay đổi đạn của người chơi.
    /// </summary>
	public class PowerupBullet : Collectible
    {
        /// <summary>
        /// Số phát bắn trước khi quay lại loại đạn mặc định.
        /// </summary>
        public int amount = 5;

        /// <summary>
        /// Chỉ số (index) của loại đạn mới, trên script Player, cần được gán.
        /// </summary>
        public int bulletIndex = 1;


        /// <summary>
        /// Ghi đè hành vi mặc định bằng một triển khai tùy chỉnh.
        /// Kiểm tra đạn hiện tại và nạp lại đạn.
        /// </summary>
		public override bool Apply(Player p)
        {
            if (p == null)
                return false;

            int value = p.GetView().GetAmmo();
            int index = p.GetView().GetBullet();

            //không tiêu thụ powerup nếu người chơi đã sở hữu loại đạn mới rồi
            //và số lượng đạn đang ở mức tối đa sẵn có
            if (value == amount && index == bulletIndex)
                return false;

            //ngược lại gán đạn mới và nạp lại đạn
            p.GetView().SetAmmo(amount, bulletIndex);

            //trả về thu thập thành công
            return true;
        }
    }
}