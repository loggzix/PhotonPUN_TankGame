/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Script làm cho các đối tượng con không bị hủy khi chuyển đổi scene.
    /// Chỉ giữ lại một instance duy nhất trong suốt toàn bộ trò chơi.
    /// </summary>
    public class DontDestroyManager : MonoBehaviour
    {
        //tham chiếu đến instance của script này
        private static DontDestroyManager instance;
        
        //đặt toàn bộ gameobject thành 'không hủy' (dont destroy),
        //hoặc hủy cái còn lại nếu có sự trùng lặp
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
                Destroy(gameObject);
        }
    }
}