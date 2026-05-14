/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace TanksMP
{
    /// <summary>
    /// Lớp này mở rộng đối tượng PhotonPlayer của Photon bằng các thuộc tính tùy chỉnh (custom properties).
    /// Cung cấp một số phương thức để thiết lập và lấy các biến từ chúng.
    /// </summary>
    public static class PlayerExtensions
    {
        //các key để lưu và truy cập các giá trị trong Hashtable custom properties
        public const string team = "team";
        public const string health = "health";
        public const string shield = "shield";
        public const string ammo = "ammo";
        public const string bullet = "bullet";


        /// <summary>
        /// Trả về biệt danh của người chơi qua mạng.
        /// Offline: tên bot. Online: tên PhotonPlayer.
        /// </summary>
        public static string GetName(this PhotonView player)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    return bot.myName;
                }
            }

            return player.Owner.NickName;
        }

        /// <summary>
        /// Offline: trả về số đội của một bot được lưu trữ trong PlayerBot.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static int GetTeam(this PhotonView player)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    return bot.teamIndex;
                }
            }

            return player.Owner.GetTeam();
        }

        /// <summary>
        /// Online: trả về số đội qua mạng của người chơi từ custom properties.
        /// </summary>
        public static int GetTeam(this Photon.Realtime.Player player)
        {
            return System.Convert.ToInt32(player.CustomProperties[team]);
        }

        /// <summary>
        /// Offline: đồng bộ hóa số đội của một PlayerBot cục bộ.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static void SetTeam(this PhotonView player, int teamIndex)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    bot.teamIndex = teamIndex;
                    return;
                }
            }

            player.Owner.SetTeam(teamIndex);
        }

        /// <summary>
        /// Online: đồng bộ hóa số đội của người chơi cho tất cả người chơi thông qua custom properties.
        /// </summary>
        public static void SetTeam(this Photon.Realtime.Player player, int teamIndex)
        {
            player.SetCustomProperties(new Hashtable() { { team, (byte)teamIndex } });
        }

        /// <summary>
        /// Offline: trả về giá trị máu của một bot được lưu trữ trong PlayerBot.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static int GetHealth(this PhotonView player)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    return bot.health;
                }
            }

            return player.Owner.GetHealth();
        }

        /// <summary>
        /// Online: trả về giá trị máu qua mạng của người chơi từ custom properties.
        /// </summary>
        public static int GetHealth(this Photon.Realtime.Player player)
        {
            return System.Convert.ToInt32(player.CustomProperties[health]);
        }

        /// <summary>
        /// Offline: đồng bộ hóa giá trị máu của một PlayerBot cục bộ.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static void SetHealth(this PhotonView player, int value)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    bot.health = value;
                    return;
                }
            }

            player.Owner.SetHealth(value);
        }

        /// <summary>
        /// Online: đồng bộ hóa giá trị máu của người chơi cho tất cả người chơi thông qua custom properties.
        /// </summary>
        public static void SetHealth(this Photon.Realtime.Player player, int value)
        {
            player.SetCustomProperties(new Hashtable() { { health, (byte)value } });
        }

        /// <summary>
        /// Offline: trả về giá trị giáp của một bot được lưu trữ trong PlayerBot.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static int GetShield(this PhotonView player)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    return bot.shield;
                }
            }

            return player.Owner.GetShield();
        }

        /// <summary>
        /// Online: trả về giá trị giáp qua mạng của người chơi từ custom properties.
        /// </summary>
        public static int GetShield(this Photon.Realtime.Player player)
        {
            return System.Convert.ToInt32(player.CustomProperties[shield]);
        }

        /// <summary>
        /// Offline: đồng bộ hóa giá trị giáp của một PlayerBot cục bộ.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static void SetShield(this PhotonView player, int value)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    bot.shield = value;
                    return;
                }
            }

            player.Owner.SetShield(value);
        }

        /// <summary>
        /// Online: đồng bộ hóa giá trị giáp của người chơi cho tất cả người chơi thông qua custom properties.
        /// </summary>
        public static void SetShield(this Photon.Realtime.Player player, int value)
        {
            player.SetCustomProperties(new Hashtable() { { shield, (byte)value } });
        }

        /// <summary>
        /// Giảm giá trị giáp qua mạng của người chơi hoặc bot theo lượng được truyền vào.
        /// </summary>
        public static int DecreaseShield(this PhotonView player, int value)
        {
            int newShield = player.GetShield();
            newShield -= value;

            player.SetShield(newShield);
            return newShield;
        }

        /// <summary>
        /// Offline: trả về giá trị đạn của một bot được lưu trữ trong PlayerBot.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static int GetAmmo(this PhotonView player)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    return bot.ammo;
                }
            }

            return player.Owner.GetAmmo();
        }

        /// <summary>
        /// Online: trả về giá trị đạn qua mạng của người chơi từ custom properties.
        /// </summary>
        public static int GetAmmo(this Photon.Realtime.Player player)
        {
            return System.Convert.ToInt32(player.CustomProperties[ammo]);
        }

        /// <summary>
        /// Offline: đồng bộ hóa số lượng đạn của một PlayerBot cục bộ.
        /// Cung cấp một tham số chỉ số tùy chọn để thiết lập đạn mới và số đạn cùng nhau.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static void SetAmmo(this PhotonView player, int value, int index = -1)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    bot.ammo = value;
                    if (index >= 0)
                        bot.currentBullet = index;
                    return;
                }
            }

            player.Owner.SetAmmo(value, index);
        }

        /// <summary>
        /// Online: đồng bộ hóa số lượng đạn của người chơi cho tất cả người chơi thông qua custom properties.
        /// Cung cấp một tham số chỉ số tùy chọn để thiết lập đạn mới và số đạn cùng nhau.
        /// </summary>
        public static void SetAmmo(this Photon.Realtime.Player player, int value, int index = -1)
        {
            Hashtable hash = new Hashtable();
            hash.Add(ammo, (byte)value);
            if (index >= 0)
                hash.Add(bullet, (byte)index);

            player.SetCustomProperties(hash);
        }

        /// <summary>
        /// Giảm giá trị đạn qua mạng của người chơi hoặc bot theo lượng được truyền vào.
        /// Nếu người chơi hết đạn, chỉ số đạn sẽ tự động được đặt về mặc định.
        /// </summary>
        public static int DecreaseAmmo(this PhotonView player, int value)
        {
            int newAmmo = player.GetAmmo();
            newAmmo -= value;

            if (newAmmo <= 0)
                player.SetAmmo(newAmmo, 0);
            else
                player.SetAmmo(newAmmo);

            return newAmmo;
        }

        /// <summary>
        /// Offline: trả về chỉ số đạn (bullet index) của một bot được lưu trữ trong PlayerBot.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static int GetBullet(this PhotonView player)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    return bot.currentBullet;
                }
            }

            return player.Owner.GetBullet();
        }

        /// <summary>
        /// Online: trả về chỉ số đạn qua mạng của người chơi từ custom properties.
        /// </summary>
        public static int GetBullet(this Photon.Realtime.Player player)
        {
            return System.Convert.ToInt32(player.CustomProperties[bullet]);
        }

        /// <summary>
        /// Offline: đồng bộ hóa loại đạn hiện đang được chọn của một PlayerBot cục bộ.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static void SetBullet(this PhotonView player, int value)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    bot.currentBullet = value;
                    return;
                }
            }

            player.Owner.SetBullet(value);
        }

        /// <summary>
        /// Online: Đồng bộ hóa loại đạn hiện đang được chọn của người chơi cho tất cả người chơi thông qua custom properties.
        /// </summary>
        public static void SetBullet(this Photon.Realtime.Player player, int value)
        {
            player.SetCustomProperties(new Hashtable() { { bullet, (byte)value } });
        }


        /// <summary>
        /// Offline: xóa tất cả các thuộc tính của một PlayerBot cục bộ.
        /// Dự phòng sang chế độ online cho master hoặc trong trường hợp chế độ offline đã được tắt.
        /// </summary>
        public static void Clear(this PhotonView player)
        {
            if (PhotonNetwork.OfflineMode == true)
            {
                PlayerBot bot = player.GetComponent<PlayerBot>();
                if (bot != null)
                {
                    bot.currentBullet = 0;
                    bot.health = 0;
                    bot.shield = 0;
                    return;
                }
            }

            player.Owner.Clear();
        }


        /// <summary>
        /// Online: Xóa tất cả các biến qua mạng của người chơi thông qua custom properties trong một lệnh duy nhất.
        /// </summary>
        public static void Clear(this Photon.Realtime.Player player)
        {
            player.SetCustomProperties(new Hashtable() { { PlayerExtensions.bullet, (byte)0 },
                                                         { PlayerExtensions.health, (byte)0 },
                                                         { PlayerExtensions.shield, (byte)0 } });
        }
    }
}
