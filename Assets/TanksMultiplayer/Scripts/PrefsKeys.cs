/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

namespace TanksMP
{
    /// <summary>
    /// Danh sách tất cả các key được lưu trên thiết bị của người dùng, cho dù là cài đặt hay lựa chọn.
    /// </summary>
    public class PrefsKeys
    {
        /// <summary>
		/// Key PlayerPrefs cho tên người chơi: UserXXXX
		/// </summary>
        public const string playerName = "TM_playerName";

        /// <summary>
        /// Key PlayerPrefs cho chế độ mạng đã chọn: 0, 1 hoặc 2
        /// </summary>
        public const string networkMode = "TM_networkMode";

        /// <summary>
        /// Key PlayerPrefs cho chế độ chơi đã chọn.
        /// </summary>
        public const string gameMode = "TM_gameMode";

        /// <summary>
        /// Địa chỉ máy chủ để kết nối thủ công, ví dụ: trong các trò chơi LAN.
        /// Cái này chỉ được sử dụng khi sử dụng Photon Networking, vì Netcode
        /// có hỗ trợ broadcast và tự động tìm kiếm máy chủ.
        /// </summary>
        public const string serverAddress = "TM_serverAddress";

        /// <summary>
        /// Key PlayerPrefs cho trạng thái nhạc nền: true/false
        /// </summary>
        public const string playMusic = "TM_playMusic";

        /// <summary>
        /// Key PlayerPrefs cho âm lượng ứng dụng tổng thể: phạm vi 0-1
        /// </summary>
        public const string appVolume = "TM_appVolume";
      
        /// <summary>
        /// Key PlayerPrefs cho mẫu người chơi đã chọn: 0/1/2 v.v.
        /// </summary>
        public const string activeTank = "TM_activeTank";
    }
}
