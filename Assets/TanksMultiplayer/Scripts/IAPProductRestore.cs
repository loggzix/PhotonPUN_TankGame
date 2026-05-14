/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Script đơn giản để xử lý việc khôi phục các giao dịch mua trên iOS. Khôi phục giao dịch mua là một
    /// yêu cầu của Apple và ứng dụng của bạn sẽ bị từ chối nếu bạn không cung cấp tính năng này.
    /// </summary>
    public class IAPProductRestore : MonoBehaviour
    {
        //chỉ hiển thị nút khôi phục trên iOS
        void Start()
        {
            #if !UNITY_IPHONE
                gameObject.SetActive(false);
            #endif
        }


        /// <summary>
        /// Gọi phương thức RestoreTransactions của Unity IAP.
        /// Việc thêm phương thức này vào một sự kiện nút UI là hợp lý.
        /// </summary>
        public void Restore()
        {
            #if UNITY_IAP
            UnityIAPManager.RestoreTransactions();
            #endif
        }
    }
}
