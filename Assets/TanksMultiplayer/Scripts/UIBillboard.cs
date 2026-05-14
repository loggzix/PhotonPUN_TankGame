/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Định hướng gameobject được gắn script này luôn hướng về phía camera.
    /// </summary>
    public class UIBillboard : MonoBehaviour
    {
        /// <summary>
        /// Nếu được bật, sẽ tỷ lệ hóa đối tượng này để luôn giữ cùng một kích thước,
        /// bất kể vị trí trong scene, tức là khoảng cách tới camera.
        /// </summary>
        public bool scaleWithDistance = false;

        /// <summary>
        /// Hệ số nhân áp dụng cho việc tính toán tỷ lệ theo khoảng cách.
        /// </summary>
        public float scaleMultiplier = 1f;

        //tối ưu hóa các lệnh gọi GetComponent:
        //lưu tạm tham chiếu đến transform của camera
        private Transform camTrans;
        
        //lưu tạm tham chiếu đến transform này
        private Transform trans;

        //kích thước được tính toán tùy thuộc vào khoảng cách camera
        private float size;


        //khởi tạo các biến
        void Awake()
        {
            camTrans = Camera.main.transform;
            trans = transform;
        }


        //luôn hướng về phía camera mỗi frame
        void Update()
        {
            transform.LookAt(trans.position + camTrans.rotation * Vector3.forward, camTrans.rotation * Vector3.up);

            if (!scaleWithDistance) return;
            size = (camTrans.position - transform.position).magnitude;
            transform.localScale = Vector3.one * (size * (scaleMultiplier / 100f));
        }
    }
}
