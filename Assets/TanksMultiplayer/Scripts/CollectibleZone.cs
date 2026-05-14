/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using Photon.Pun;

namespace TanksMP
{
    /// <summary>
    /// Thành phần đóng vai trò là một khu vực để kích hoạt việc thu thập các vật phẩm CollectibleTeam.
    /// Ví dụ: cần thiết cho các căn cứ của đội trong chế độ Cướp Cờ (Capture The Flag). Cần một Collider để kích hoạt.
    /// </summary>
	public class CollectibleZone : MonoBehaviour
    {
        /// <summary>
        /// Chỉ số đội mà khu vực này thuộc về.
        /// Các đội được định nghĩa trong inspector của script GameManager.
        /// </summary>
        public int teamIndex = 0;

        /// <summary>
        /// Tùy chọn: Vật phẩm thu thập khác cần phải ở vị trí spawn của nó để khu vực này
        /// kích hoạt thu thập thành công. Một ví dụ là chế độ Cướp Cờ, nơi
        /// cờ đỏ cần phải ở điểm spawn của đội đỏ để có thể thu thập thành công cờ xanh.
        /// </summary>
        public ObjectSpawner requireObject;

        /// <summary>
        /// Clip âm thanh phát ra khi một vật phẩm CollectibleTeam được mang đến khu vực này.
        /// </summary>
        public AudioClip scoreClip;


        /// <summary>
        /// Chỉ dành cho server: kiểm tra các vật phẩm thu thập va chạm với khu vực này.
        /// Các va chạm có thể xảy ra được xác định trong Physics Matrix. 
        /// </summary>
        public void OnTriggerEnter(Collider col)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            //trò chơi đã kết thúc nên không làm gì thêm
            if (GameManager.GetInstance().IsGameOver()) return;

            //kiểm tra vật phẩm được yêu cầu
            //tiếp tục nếu nó không được gán ngay từ đầu
            if (requireObject != null)
            {
                //vật phẩm yêu cầu chưa được khởi tạo
                if (requireObject.obj == null)
                    return;

                //vật phẩm yêu cầu hoặc không có thành phần CollectibleTeam,
                //hoặc vẫn đang bị mang đi hoặc chưa quay lại vị trí spawn của nó
                CollectibleTeam colReq = requireObject.obj.GetComponent<CollectibleTeam>();
                if (colReq == null || colReq.carrierId >= 0 ||
                    colReq.transform.position != requireObject.transform.position)
                    return;
            }

            CollectibleTeam colOther = col.gameObject.GetComponent<CollectibleTeam>();

            //một vật phẩm của đội, không phải của chính chúng ta, đã được mang đến khu vực này
            if (colOther != null && colOther.teamIndex != teamIndex)
            {
                if (scoreClip) AudioManager.Play3D(scoreClip, transform.position);

                //thêm điểm cho loại ghi điểm này vào đội chính xác
                GameManager.GetInstance().AddScore(ScoreType.Capture, teamIndex);
                //điểm số tối đa đã đạt được ngay bây giờ
                if (GameManager.GetInstance().IsGameOver())
                {
                    //đóng phòng đối với người chơi mới tham gia
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    //thông báo cho tất cả các máy khách đội chiến thắng
                    GameManager.GetInstance().localPlayer.photonView.RPC("RpcGameOver", RpcTarget.All, (byte)teamIndex);
                    return;
                }

                //xóa các thông điệp mạng về Vật phẩm thu thập vì nó sắp bị hủy
                PhotonNetwork.RemoveRPCs(colOther.spawner.photonView);
                colOther.spawner.photonView.RPC("Destroy", RpcTarget.All);
            }
        }
    }
}