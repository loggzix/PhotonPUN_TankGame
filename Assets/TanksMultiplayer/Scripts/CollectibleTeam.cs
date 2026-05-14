/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using Photon.Pun;

namespace TanksMP
{
    /// <summary>
    /// Triển khai Vật phẩm thu thập tùy chỉnh cho các vật phẩm thuộc quyền sở hữu của scene (chưa gán) hoặc thuộc quyền sở hữu của đội.
    /// Ví dụ: cho phép nhặt vật phẩm 'Rambo', vật phẩm Cướp cờ (Capture the Flag), v.v.
    /// </summary>
	public class CollectibleTeam : Collectible
    {
        /// <summary>
        /// Chỉ số đội mà Vật phẩm thu thập này thuộc về, hoặc -1 nếu chưa gán.
        /// Các đội được định nghĩa trong inspector của script GameManager.
        /// </summary>
        public int teamIndex = -1;

        /// <summary>
        /// Tùy chọn: Material nên được gán lại nếu Vật phẩm thu thập này bị rơi hoặc được trả lại.
        /// </summary>
        public Material baseMaterial;

        /// <summary>
        /// Tùy chọn: Renderer mà trên đó material nên được sửa đổi tùy thuộc vào đội đang mang.
        /// </summary>
        public MeshRenderer targetRenderer;


        /// <summary>
        /// Chỉ dành cho server: kiểm tra các người chơi va chạm với powerup.
        /// Các va chạm có thể xảy ra được xác định trong Physics Matrix.
        /// </summary>
        public override void OnTriggerEnter(Collider col)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            GameObject obj = col.gameObject;
            Player player = obj.GetComponent<Player>();

            //thử áp dụng vật phẩm thu thập cho người chơi, kết quả nên là true
            if (Apply(player))
            {
                //dọn dẹp các RPC đã đệm trước đó để chúng ta chỉ giữ lại cái gần đây nhất
                PhotonNetwork.RemoveRPCs(spawner.photonView);

                //kiểm tra xem người chơi va chạm có thuộc cùng một đội với vật phẩm không
                if (teamIndex == player.GetView().GetTeam())
                {
                    //người chơi đã thu thập vật phẩm của đội, trả nó về căn cứ của đội
                    //chúng ta không cần gửi cái này dưới dạng buffered RPC vì đây là vị trí spawn mặc định
                    spawner.photonView.RPC("Return", RpcTarget.All);
                }
                else
                {
                    //người chơi đã nhặt vật phẩm từ đội khác, gửi đi buffered RPC để nó được ghi nhớ
                    spawner.photonView.RPC("Pickup", RpcTarget.AllBuffered, (short)player.GetView().ViewID);
                }
            }
        }


        /// <summary>
        /// Ghi đè hành vi mặc định bằng một triển khai tùy chỉnh.
        /// Kiểm tra người đang mang và vị trí vật phẩm để quyết định việc nhặt vật phẩm có hợp lệ hay không.
        /// </summary>
        public override bool Apply(Player p)
        {
            //không cho phép thu thập nếu vật phẩm đã được mang đi nơi khác
            //nhưng cũng bỏ qua bất kỳ xử lý nào nếu cờ của chúng ta đã ở căn cứ rồi
            if (p == null || carrierId > 0 ||
                teamIndex == p.GetView().GetTeam() && transform.position == spawner.transform.position)
                return false;

            //nếu một target renderer được thiết lập, gán material của đội
            Colorize(p.GetView().GetTeam());

            //trả về việc thu thập thành công
            return true;
        }


        /// <summary>
        /// Ghi đè hành vi mặc định bằng một triển khai tùy chỉnh.
        /// </summary>
        public override void OnDrop()
        {
            Colorize(this.teamIndex);
        }


        /// <summary>
        /// Ghi đè hành vi mặc định bằng một triển khai tùy chỉnh.
        /// </summary>
        public override void OnReturn()
        {
            Colorize(this.teamIndex);
        }


        //gán material dựa trên chỉ số đội được truyền vào
        void Colorize(int teamIndex)
        {
            if (targetRenderer != null)
            {
                if (teamIndex >= 0)
                    targetRenderer.material = GameManager.GetInstance().teams[teamIndex].material;
                else
                    targetRenderer.material = baseMaterial;
            }
        }
    }
}