/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TanksMP
{
    /// <summary>
    /// Thành phần Joystick để điều khiển chuyển động và hành động của người chơi bằng các sự kiện Unity UI.
    /// Có thể có nhiều joystick trên màn hình cùng một lúc, thực hiện các callback khác nhau.
    /// </summary>
    public class UIJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// Callback được kích hoạt khi joystick bắt đầu di chuyển do đầu vào của người dùng.
        /// </summary>
        public event Action onDragBegin;
        
        /// <summary>
        /// Callback được kích hoạt khi joystick đang di chuyển hoặc được giữ xuống.
        /// </summary>
        public event Action<Vector2> onDrag;
        
        /// <summary>
        /// Callback được kích hoạt khi đầu vào của joystick được thả ra.
        /// </summary>
        public event Action onDragEnd;
       
        /// <summary>
        /// Đối tượng mục tiêu, tức là nút joystick (thumb) đang được người dùng kéo.
        /// </summary>
        public Transform target;

        /// <summary>
        /// Bán kính tối đa mà đối tượng mục tiêu có thể di chuyển so với tâm.
        /// </summary>
        public float radius = 50f;
        
        /// <summary>
        /// Vị trí hiện tại của đối tượng mục tiêu trên trục x và y trong không gian 2D.
        /// Các giá trị được tính toán trong phạm vi [-1, 1] tương ứng với trái/dưới và phải/trên.
        /// </summary>
        public Vector2 position;
        
        //theo dõi trạng thái kéo hiện tại
        private bool isDragging = false;
        
        //tham chiếu đến nút joystick đang được kéo quanh
		private RectTransform thumb;


        //khởi tạo các biến
		void Start()
		{
			thumb = target.GetComponent<RectTransform>();

			//trong editor, vô hiệu hóa đầu vào nhận được bởi đồ họa joystick:
            //chúng ta muốn chúng hiển thị nhưng không nhận hoặc chặn bất kỳ đầu vào nào
			#if UNITY_EDITOR
				Graphic[] graphics = GetComponentsInChildren<Graphic>();
				for(int i = 0; i < graphics.Length; i++)
					graphics[i].raycastTarget = false;
			#endif
		}


        /// <summary>
        /// Sự kiện được kích hoạt bởi UI Eventsystem khi bắt đầu kéo.
        /// </summary>
        public void OnBeginDrag(PointerEventData data)
        {
            isDragging = true;
            if(onDragBegin != null)
                onDragBegin();
        }


        /// <summary>
        /// Sự kiện được kích hoạt bởi UI Eventsystem khi đang kéo.
        /// </summary>
        public void OnDrag(PointerEventData data)
        {
            //lấy các RectTransform của các thành phần liên quan
            RectTransform draggingPlane = transform as RectTransform;
            Vector3 mousePos;

            //kiểm tra xem vị trí được kéo có nằm trong hình chữ nhật kéo hay không,
            //sau đó đặt vị trí chuột toàn cục và gán nó cho nút joystick
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out mousePos))
            {
                thumb.position = mousePos;
            }

            //độ dài của vector chạm (độ lớn - magnitude)
            //được tính toán từ vị trí tương đối của nút joystick
            float length = target.localPosition.magnitude;

            //nếu nút joystick rời khỏi ranh giới của joystick,
            //hãy giới hạn nó ở bán kính tối đa
            if (length > radius)
            {
                target.localPosition = Vector3.ClampMagnitude(target.localPosition, radius);
            }

            //thiết lập vị trí Vector2 của nút dựa trên vị trí sprite thực tế
            position = target.localPosition;
            //nội suy mượt mà (lerp) vị trí Vector2 của nút dựa trên các vị trí cũ
            position = position / radius * Mathf.InverseLerp(radius, 2, 1);
        }
        
        
        //đặt vị trí nút joystick thành vị trí kéo trong mỗi frame
        void Update()
        {
            //trong editor, vị trí joystick không di chuyển, chúng ta phải mô phỏng nó
			//phản chiếu đầu vào của người chơi vào vị trí joystick và tính toán vị trí nút từ đó
			#if UNITY_EDITOR
				target.localPosition =  position * radius;
				target.localPosition = Vector3.ClampMagnitude(target.localPosition, radius);
			#endif

            //kiểm tra trạng thái kéo thực tế và kích hoạt callback. Chúng ta thực hiện việc này trong Update(),
            //không phải OnDrag, vì OnDrag chỉ được gọi khi joystick đang di chuyển. Nhưng chúng ta
            //thực sự muốn tiếp tục di chuyển người chơi ngay cả khi joystick đang được giữ xuống
            if(isDragging && onDrag != null)
                onDrag(position);
        }


        /// <summary>
        /// Sự kiện được kích hoạt bởi UI Eventsystem khi kết thúc kéo.
        /// </summary>
        public void OnEndDrag(PointerEventData data)
        {
            //chúng ta không còn kéo nữa, đặt lại về vị trí mặc định
            position = Vector2.zero;
            target.position = transform.position;
            
            //đặt dragging thành false và kích hoạt callback
            isDragging = false;
            if (onDragEnd != null)
                onDragEnd();
        }
    }
}