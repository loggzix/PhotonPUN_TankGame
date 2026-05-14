/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Script của Camera để theo dõi người chơi hoặc một mục tiêu transform khác.
    /// Mở rộng với khả năng ẩn một số layer nhất định (ví dụ: UI) khi ở "chế độ theo dõi".
    /// </summary>
    public class FollowTarget : MonoBehaviour
    {
        /// <summary>
        /// Mục tiêu camera cần theo dõi.
        /// Tự động lấy được trong LateUpdate().
        /// </summary>
        public Transform target;
        
        /// <summary>
        /// Các layer cần ẩn sau khi gọi HideMask().
        /// </summary>
        public LayerMask respawnMask;

        /// <summary>
        /// Khoảng cách giới hạn trong mặt phẳng x-z tới mục tiêu.
        /// </summary>
        public float distance = 10.0f;
        
        /// <summary>
        /// Chiều cao giới hạn mà camera nên ở phía trên mục tiêu.
        /// </summary>
        public float height = 5.0f;

        /// <summary>
        /// Tham chiếu đến thành phần Camera.
        /// </summary>
        [HideInInspector]
        public Camera cam;
        
        /// <summary>
        /// Tham chiếu đến Transform của camera.
        /// </summary>
        [HideInInspector]
        public Transform camTransform;
        
        
        //khởi tạo các biến
        void Start()
        {
            cam = GetComponent<Camera>();
            camTransform = transform;

            //AudioListener cho scene này không được gắn trực tiếp vào camera này,
            //mà gắn vào một gameobject riêng biệt làm con của camera. Điều này là do
            //camera thường được đặt phía trên người chơi, tuy nhiên AudioListener
            //nên xem xét các clip âm thanh từ vị trí của người chơi trong không gian 3D.
            //vì vậy ở đây chúng ta đặt đối tượng con AudioListener tại vị trí mục tiêu.
            //Lưu ý: đặt AudioListener làm con của người chơi không hiệu quả, vì
            //nó bị vô hiệu hóa khi chết và do đó ngừng phát âm thanh hoàn toàn
            Transform listener = GetComponentInChildren<AudioListener>().transform;
            listener.position = transform.position + transform.forward * distance;
        }


        //đặt vị trí camera trong mỗi frame
        void LateUpdate()
        {
            //hủy nếu không có mục tiêu
            if (!target)
                return;

            //chuyển đổi góc transform của camera thành một vòng xoay
            Quaternion currentRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            //đặt vị trí của camera trên mặt phẳng x-z tại:
            //các đơn vị khoảng cách phía sau mục tiêu, các đơn vị chiều cao phía trên mục tiêu
            Vector3 pos = target.position;
            pos -= currentRotation * Vector3.forward * Mathf.Abs(distance);
            pos.y = target.position.y + Mathf.Abs(height);
            transform.position = pos;

            //nhìn vào mục tiêu
            transform.LookAt(target);

            //giới hạn khoảng cách
            transform.position = target.position - (transform.forward * Mathf.Abs(distance));
        }
        
        
        /// <summary>
        /// Loại bỏ các layer được chỉ định của 'respawnMask' khỏi camera.
        /// </summary>
        public void HideMask(bool shouldHide)
        {
            if(shouldHide) cam.cullingMask &= ~respawnMask;
            else cam.cullingMask |= respawnMask;
        }
    }
}