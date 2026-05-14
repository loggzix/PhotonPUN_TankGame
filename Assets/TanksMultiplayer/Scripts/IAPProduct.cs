/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using UnityEngine.UI;
#if UNITY_IAP
using UnityEngine.Purchasing;
#endif

namespace TanksMP
{
    /// <summary>
    /// Mô tả một sản phẩm mua trong ứng dụng có thể mua bằng Unity IAP.
    /// Chứa một số phần tử UI và logic để chọn/bỏ chọn.
    /// </summary>
    public class IAPProduct : MonoBehaviour
    {
		/// <summary>
		/// Liệu sản phẩm này có nên được đăng ký với Unity IAP hay không.
        /// Cái này chỉ nên được đặt thành true nếu sản phẩm tồn tại trên App Store.
		/// </summary>
		public bool buyable = true;

        /// <summary>
        /// Định danh duy nhất cho sản phẩm này.
        /// Đối với các sản phẩm đang hoạt động, định danh này nên khớp với id trên App Store.
        /// </summary>
        public string id;

		/// <summary>
		/// Giá trị duy nhất được lưu cho các sản phẩm có thể chọn để xác định lựa chọn hiện tại.
		/// </summary>
		public int value;

        #if UNITY_IAP
        /// <summary>
        /// Loại mua trong ứng dụng nên khớp với loại sản phẩm trên App Store.
        /// </summary>
        public ProductType type = ProductType.NonConsumable;
        #endif

        /// <summary>
        /// Nút UI kích hoạt quy trình mua hàng qua Unity IAP.
        /// </summary>
        public GameObject buyButton;

        /// <summary>
        /// Các thành phần tùy chọn sẽ được bật nếu sản phẩm này đã được bán.
        /// </summary>
        public GameObject sold;

        /// <summary>
        /// Nút UI kích hoạt việc chọn sản phẩm này trong cửa hàng.
        /// Nếu một group đã được gán cho Toggle của nó, các sản phẩm khác sẽ bị bỏ chọn.
        /// </summary>
        public GameObject selectButton;

        /// <summary>
        /// Các thành phần tùy chọn sẽ được bật nếu sản phẩm này đã được chọn.
        /// </summary>
        public GameObject selected;


        //thiết lập trạng thái mua/chọn ban đầu
        void Awake()
        {
            //sản phẩm này đã được mua rồi
            if (PlayerPrefs.HasKey(Encryptor.Encrypt(id)))
                Purchased();
            else if (!buyable)
            {
                //sản phẩm chưa được mua, nhưng nó cũng không được đánh dấu là có thể mua
                //trên App Store. Nghĩa là chúng ta ẩn nút mua và hiển thị trực tiếp
                //nút chọn cho nó thay thế.
                buyButton.SetActive(false);
                selectButton.SetActive(true);
            }
        }


        //xác thực giá trị đã lưu trên thiết bị với giá trị của sản phẩm này: nếu chúng khớp nhau,
        //điều này có nghĩa là trước đó chúng ta đã chọn sản phẩm này và khởi tạo lại nó là đã chọn
        //một lần nữa
        void Start()
        {
            if (Encryptor.Decrypt(PlayerPrefs.GetString(PrefsKeys.activeTank)) == value.ToString())
                IsSelected(true);
        }


        /// <summary>
        /// Thử mở hộp thoại mua sản phẩm này qua Unity IAP.
        /// </summary>
        public void Purchase()
        {
            #if UNITY_IAP
            if (!buyable) return;
            UnityIAPManager.PurchaseProduct(id);
            #endif
        }


        /// <summary>
        /// Thiết lập trạng thái UI của sản phẩm này thành 'đã mua', ẩn nút mua
        /// và hiển thị gameobject 'sold' nếu được chỉ định.
        /// </summary>
        public void Purchased()
        {
            buyButton.SetActive(false);
            if (sold) sold.SetActive(true);
        }


        /// <summary>
        /// Đối với các sản phẩm đã mua: thiết lập trạng thái UI của sản phẩm thành 'đã chọn' và lưu
        /// giá trị lựa chọn hiện tại trên thiết bị. Nếu một sản phẩm được chọn, phương thức này cũng
        /// được gọi cho tất cả các sản phẩm khác trong cùng một nhóm với giá trị boolean là false.
        /// Do đó, cả logic cho việc chọn và bỏ chọn đều được xử lý trong phương thức này.
        /// Được gọi bởi sự kiện onValueChanged trong inspector của nút chọn.
        /// </summary>
        public void IsSelected(bool thisSelect)
        {
            //chúng ta cần mua sản phẩm này trước
            if (buyButton.activeInHierarchy)
                return;

            //nếu đối tượng này đã được chọn
            if (thisSelect)
            {  
                //lấy tham chiếu đến thành phần Toggle trên nút chọn
                Toggle toggle = selectButton.GetComponent<Toggle>();

                //trong trường hợp sản phẩm này là một phần của một nhóm vật phẩm
                if (toggle.group)
                {
                    //vì các thành phần Toggle trên các gameobject bị vô hiệu hóa không nhận được sự kiện onValueChanged,
                    //ở đây chúng ta triển khai một cách "hacky" để bỏ chọn tất cả các Toggle khác, ngay cả những cái đã bị vô hiệu hóa
                    IAPProduct[] others = toggle.group.GetComponentsInChildren<IAPProduct>(true);
                    for (int i = 0; i < others.Length; i++)
                    {
                        //bỏ chọn sản phẩm được lặp qua nếu nó không phải là sản phẩm được chọn.
                        if (others[i].selectButton != null && others[i] != this)
                        {
                            others[i].IsSelected(false);
                        }
                    }
                }

                //hiển thị rằng sản phẩm này đã được chọn
                toggle.isOn = true;
                selectButton.SetActive(false);
                if (selected) selected.SetActive(true);

                //lưu giá trị lựa chọn vào thiết bị
				PlayerPrefs.SetString(PrefsKeys.activeTank, Encryptor.Encrypt(value.ToString()));
            }
            else
            {
                //nếu một đối tượng khác đã được chọn, hiển thị nút chọn
                //cho sản phẩm này và bỏ trạng thái 'đã chọn'
                if (selectButton) selectButton.SetActive(true);
                if (selected) selected.SetActive(false);
            }
        }
    }
}
