/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

#if UNITY_IAP
using Unity.Services.Core;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif
#if IAPGUARD
using FLOBUK.IAPGUARD;
#endif

using System;
using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Manager xử lý toàn bộ quy trình mua hàng trong ứng dụng (IAP),
    /// cấp quyền mua và bắt lỗi bằng Unity IAP.
    /// </summary>
    #if UNITY_IAP
    public class UnityIAPManager : MonoBehaviour, IDetailedStoreListener
    #else
    public class UnityIAPManager : MonoBehaviour
    #endif
    {
        #pragma warning disable 0067
        /// <summary>
        /// Kích hoạt khi việc mua hàng thất bại để cung cấp mã định danh sản phẩm.
        /// </summary>
        public static event Action<string> purchaseFailedEvent;
        #pragma warning restore 0067

        #if UNITY_IAP
        //vô hiệu hóa các cảnh báo dành riêng cho nền tảng, vì Unity thường đưa ra chúng
        //cho các biến không sử dụng, tuy nhiên chúng được sử dụng trong ngữ cảnh này
        #pragma warning disable 0414
        private static ConfigurationBuilder builder;
        private static IStoreController controller;
        private static IExtensionProvider extensions;
        #pragma warning restore 0414



        void Start()
        {
            //xây dựng instance mua hàng IAP và thêm Google Play public key vào đó
            builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            //lặp qua tất cả các IAPProduct tìm thấy trong scene và thêm id của chúng để Unity IAP tra cứu
            IAPProduct[] products = Resources.FindObjectsOfTypeAll<IAPProduct>();
            foreach(IAPProduct product in products)
            {
				if(product.buyable)
                	builder.AddProduct(product.id, product.type);
            }

            Initialize();
        }


        /// <summary>
        /// Khởi tạo các dịch vụ cốt lõi và Unity IAP.
        /// </summary>
        public async void Initialize()
        {
            try
            {
                //Yêu cầu các Dịch vụ Trò chơi của Unity trước
                await UnityServices.InitializeAsync();

                //bây giờ chúng ta đã sẵn sàng khởi tạo Unity IAP
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                OnInitializeFailed(InitializationFailureReason.PurchasingUnavailable);
            }
        }


        /// <summary>
        /// Được gọi khi Unity IAP sẵn sàng thực hiện mua hàng, cung cấp store controller
        /// (chứa tất cả các sản phẩm trực tuyến) và các extension dành riêng cho nền tảng
        /// </summary>
        public void OnInitialized (IStoreController ctrl, IExtensionProvider ext)
        {
            //lưu trữ các tham chiếu (cache)
            controller = ctrl;
            extensions = ext;

            #if IAPGUARD
                IAPGuard.Instance.Initialize(controller, builder);
                IAPGuard.purchaseCallback += OnPurchaseResult;
            #endif
        }


        /// <summary>
        /// Được gọi khi người dùng nhấn nút 'Buy' trên một IAPProduct.
        /// </summary>
        public static void PurchaseProduct(string productId)
        {
            if(controller != null)
               controller.InitiatePurchase(productId);
        }


        /// <summary>
        /// Được gọi khi Unity IAP gặp lỗi khởi tạo không thể phục hồi.
        /// </summary>
        public void OnInitializeFailed (InitializationFailureReason error)
        {
			OnInitializeFailed(error, string.Empty);
        }


        /// <summary>
        /// Được gọi khi Unity IAP gặp lỗi khởi tạo không thể phục hồi.
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log(error + ": " + message);
        }


        /// <summary>
        /// Được gọi khi một giao dịch mua hoàn tất sau khi được mua.
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs e)
        {
            Product product = e.purchasedProduct;

            #if IAPGUARD
                PurchaseState state = IAPGuard.Instance.RequestPurchase(product);
                //xử lý những gì sẽ xảy ra với sản phẩm tiếp theo
                switch (state)
                {
                    case PurchaseState.Pending:
                        return PurchaseProcessingResult.Pending;
                    case PurchaseState.Failed:
                        product = null;
                        break;
                }                
            #endif

            //với giao dịch đã hoàn thành, chỉ cần gọi phương thức mua hàng của chúng ta
            if(product != null)
            { 
                OnPurchaseSuccess(product);
            }

            //trả về rằng chúng ta đã hoàn tất việc xử lý giao dịch
            return PurchaseProcessingResult.Complete;
        }


        #if IAPGUARD
        void OnPurchaseResult(bool success, SimpleJSON.JSONNode data)
        {
            if (!success) return;

            Product product = controller.products.WithID(data["data"]["productId"]);
            OnPurchaseSuccess(product);
        }
        #endif


        void OnPurchaseSuccess(Product purchasedProduct)
        {
            //lấy tất cả tham chiếu IAPProduct trong scene, sau đó lặp qua chúng
            IAPProduct[] products = FindObjectsOfType(typeof(IAPProduct)) as IAPProduct[];
            foreach (IAPProduct product in products)
            {
                //chúng ta đã tìm thấy instance IAPProduct mà chúng ta vừa mua xong
                if (product.id == purchasedProduct.definition.id)
                {
                    //đặt sản phẩm thành đã mua
                    //nếu nó có thể chọn được, hiển thị nút chọn của nó
                    product.Purchased();
                    if (product.selectButton)
                        product.IsSelected(true);
                    break;
                }
            }

            //lưu mã định danh được mã hóa của sản phẩm này trên thiết bị để giữ nó ở trạng thái đã mua
            PlayerPrefs.SetString(Encryptor.Encrypt(purchasedProduct.definition.id), "");
            PlayerPrefs.Save();
        }


        /// <summary>
        /// Được gọi khi một giao dịch mua thất bại, cung cấp sản phẩm và lý do.
        /// </summary>
        public void OnPurchaseFailed (Product product, PurchaseFailureReason reason)
        {
            purchaseFailedEvent?.Invoke(reason.ToString());
        }


        /// <summary>
        /// Được gọi khi một giao dịch mua thất bại, cung cấp sản phẩm và mô tả (bao gồm cả lý do).
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
        {
            purchaseFailedEvent?.Invoke(description.reason.ToString() + ": " + description.message);
        }


        /// <summary>
        /// Phương thức khôi phục các giao dịch (yêu cầu mật khẩu trên iOS).
        /// </summary>
        public static void RestoreTransactions()
        {
            foreach (Product p in controller.products.all)
            {	
                if(!PlayerPrefs.HasKey(Encryptor.Encrypt(p.definition.id)))
                    PlayerPrefs.DeleteKey(Encryptor.Encrypt(p.definition.id));
            }

            #if UNITY_IOS
            if(extensions != null)
			    extensions.GetExtension<IAppleExtensions>().RestoreTransactions(null);
            #elif IAPGUARD
                IAPGuard.Instance.RequestRestore();
            #endif
        }
        #endif
    }
}