/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using Photon.Pun;

namespace TanksMP
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các vật phẩm thu thập (máu, khiên, v.v.) được tiêu thụ hoặc mang theo.
    /// Kế thừa lớp này để tạo ra Vật phẩm thu thập tùy chỉnh cao với các chức năng cụ thể.
    /// </summary>
	public class Collectible : MonoBehaviour
	{	    
        /// <summary>
        /// Clip âm thanh phát ra khi Vật phẩm thu thập này được người chơi tiêu thụ.
        /// </summary>
        public AudioClip useClip;

        /// <summary>
        /// Tham chiếu đến đối tượng cục bộ (script) đã tạo ra Vật phẩm thu thập này.
        /// </summary>
        [HideInInspector]
        public ObjectSpawner spawner;

        /// <summary>
        /// ID mạng (PhotonView) cố định của Người chơi đã nhặt Vật phẩm thu thập này.
        /// </summary>
        [HideInInspector]
        public int carrierId = -1;
        
                  
        /// <summary>
        /// Chỉ dành cho server: kiểm tra các người chơi va chạm với vật phẩm này.
        /// Các va chạm có thể xảy ra được xác định trong Physics Matrix.
        /// </summary>
        public virtual void OnTriggerEnter(Collider col)
		{
            if (!PhotonNetwork.IsMasterClient)
                return;
            
    		GameObject obj = col.gameObject;
			Player player = obj.GetComponent<Player>();

            //thử áp dụng vật phẩm thu thập cho người chơi, kết quả nên là true
            if (Apply(player))
            {
                //hủy sau khi sử dụng
                spawner.photonView.RPC("Destroy", RpcTarget.All);           
            }
		}


        /// <summary>
        /// Thử áp dụng Vật phẩm thu thập cho người chơi va chạm. Trả về 'true' nếu đã tiêu thụ.
        /// Ghi đè phương thức này trong script Collectible của riêng bạn để triển khai hành vi tùy chỉnh.
        /// </summary>
        public virtual bool Apply(Player p)
		{
            //làm gì đó với người chơi
            if (p == null)
                return false;
            else
                return true;
		}


        /// <summary>
        /// Triển khai ảo được gọi khi Vật phẩm thu thập này được nhặt lên.
        /// Cái này chỉ được gọi cho các vật phẩm có CollectionType = Pickup.
        /// </summary>
        public virtual void OnPickup()
        {
        }


        /// <summary>
        /// Triển khai ảo được gọi khi Vật phẩm thu thập này bị rơi khi người chơi chết.
        /// Cái này chỉ được gọi cho các vật phẩm có CollectionType = Pickup.
        /// </summary>
        public virtual void OnDrop()
        {
        }


        /// <summary>
        /// Triển khai ảo được gọi khi Vật phẩm thu thập này được trả lại.
        /// Cái này chỉ được gọi cho các vật phẩm có CollectionType = Pickup.
        /// </summary>
        public virtual void OnReturn()
        {
        }


        //nếu đã tiêu thụ, phát clip âm thanh. Bây giờ khi Vật phẩm thu thập đã biến mất,
        //thiết lập thời gian hồi sinh tiếp theo trên script quản lý ObjectSpawner
        void OnDespawn()
        {
            if (useClip) AudioManager.Play3D(useClip, transform.position);
            carrierId = -1;
            spawner.SetRespawn();
        }
    }
}
