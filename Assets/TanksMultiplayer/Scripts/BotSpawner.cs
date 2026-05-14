/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace TanksMP
{          
    /// <summary>
    /// Chịu trách nhiệm tạo các bot AI khi ở chế độ offline, nếu không sẽ bị vô hiệu hóa.
    /// </summary>
	public class BotSpawner : MonoBehaviour
    {                
        /// <summary>
        /// Số lượng bot cần tạo trên tất cả các đội.
        /// </summary>
        public int maxBots;
        
        /// <summary>
        /// Danh sách các prefab bot để lựa chọn.
        /// </summary>
        public GameObject[] prefabs;
        
        
        void Awake()
        {
            //vô hiệu hóa khi không ở chế độ offline
            if ((NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode) != NetworkMode.Offline)
                this.enabled = false;
        }


        IEnumerator Start()
        {
            //đợi một giây để tất cả script khởi tạo xong
            yield return new WaitForSeconds(1);

            //lặp qua số lượng bot
			for(int i = 0; i < maxBots; i++)
            {
                //chọn ngẫu nhiên bot từ mảng các prefab bot
                //tạo bot thông qua mạng riêng mô phỏng
                int randIndex = Random.Range(0, prefabs.Length);
                GameObject obj = PhotonNetwork.Instantiate(prefabs[randIndex].name, Vector3.zero, Quaternion.identity, 0);

                //để máy chủ cục bộ xác định việc phân chia đội
                Player p = obj.GetComponent<Player>();
                p.GetView().SetTeam(GameManager.GetInstance().GetTeamFill());

                //tăng kích thước đội tương ứng
                PhotonNetwork.CurrentRoom.AddSize(p.GetView().GetTeam(), +1);

                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
