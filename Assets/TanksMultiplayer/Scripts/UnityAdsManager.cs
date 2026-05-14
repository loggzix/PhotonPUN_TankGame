/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace TanksMP
{
    /// <summary>
    /// Manager xử lý toàn bộ quy trình hiển thị quảng cáo video, phản hồi
    /// các lượt xem video hoàn thành/thất bại và các sự kiện kết quả. Triển khai một
    /// cơ hội hiển thị quảng cáo tùy chỉnh dựa trên tỷ lệ phần trăm. Sử dụng Unity Ads.
    /// </summary>
    #if UNITY_ADS
    public class UnityAdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    #else
    public class UnityAdsManager : MonoBehaviour
    #endif
    {
        #if UNITY_ADS
        /// <summary>
        /// Được kích hoạt bất cứ khi nào một lượt xem hoàn tất, cung cấp kết quả.
        /// </summary>
        public static event Action<ShowResult> adResultEvent;
        
        //tham chiếu đến instance của script này
        private static UnityAdsManager instance;

        /// <summary>
        /// Android Game ID được hiển thị trong bảng điều khiển Unity Ads.
        /// </summary>
        public string gameIdAndroid;

        /// <summary>
        /// IOS Game ID được hiển thị trong bảng điều khiển Unity Ads.
        /// </summary>
        public string gameIdIOS;

        /// <summary>
        /// Android placement ID được hiển thị trong bảng điều khiển Unity Ads.
        /// </summary>
        public string placementIdAndroid = "Interstitial_Android";

        /// <summary>
        /// IOS placement ID được hiển thị trong bảng điều khiển Unity Ads.
        /// </summary>
        public string placementIdIOS = "Interstitial_iOS";

        /// <summary>
        /// Có yêu cầu quảng cáo sandbox hay production hay không.
        /// </summary>
        public bool sandbox = false;
        
        //bộ đếm các lần thử hiển thị quảng cáo
        private static int counter = 0;
        
        //quảng cáo đã được hiển thị trong một vòng chơi hay chưa
        private static bool adShown = false;
        
        
        /// <summary>
        /// Trả về tham chiếu đến instance của script này.
        /// </summary>
        public static UnityAdsManager GetInstance()
        {
            return instance;
        }


        //thiết lập tham chiếu instance, nếu chưa được thiết lập,
        // và tiếp tục lắng nghe các thay đổi scene.
        void Awake()
        {
            if (instance != null)
                return;

            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (Advertisement.isSupported)
            {
                #if UNITY_ANDROID
                Advertisement.Initialize(gameIdAndroid, sandbox, this);
                #elif UNITY_IOS
                Advertisement.Initialize(gameIdIOS, sandbox, this);
                #endif
            }    
        }
        
        
        //đặt lại trạng thái quảng cáo khi chuyển đổi scene
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            counter = 0;
            adShown = false;
        }


        //cố gắng load một quảng cáo mới và lưu trữ nó để hiển thị khi cần thiết
        private void LoadAd()
        {
            if (!Advertisement.isInitialized)
            {
                HandleResult(ShowResult.Failed);
                return;
            }

            #if UNITY_ANDROID
            Advertisement.Load(placementIdAndroid, this);
            #elif UNITY_IOS
            Advertisement.Load(placementIdIOS, this);
            #endif
        }
            
        
        /// <summary>
        /// Cố gắng hiển thị quảng cáo video. Điều này có thể thất bại nếu quảng cáo không khả dụng hoặc chưa sẵn sàng,
        /// người chơi yêu cầu quảng cáo đang làm host - trong trường hợp đó, việc hiển thị quảng cáo sẽ làm
        /// tạm dừng trò chơi cho tất cả các client - quảng cáo đã được hiển thị hoặc cơ hội
        /// dựa trên tỷ lệ phần trăm đã tính toán không hiển thị quảng cáo cho lần thử này. Tất cả các kiểm tra này có thể
        /// được bỏ qua bằng cách đặt đối số truyền vào thành true, ép buộc hiển thị quảng cáo.
        /// </summary>
        public static bool ShowAd(bool force = false)
        {
			if(force || !GameManager.isMaster() && !adShown && GetInstance().shouldShowAd())
            {
                //lần thử này nên hiển thị quảng cáo: khởi tạo quảng cáo và chuyển kết quả cho phương thức HandleResult
                GetInstance().LoadAd();
                return true;
            }
            
            //tại thời điểm này chúng ta chưa thể hiển thị quảng cáo.
            //nếu lần thử quảng cáo này không được ép buộc, hãy tăng bộ đếm số lần thử
            //để lần thử tiếp theo có xác suất hiển thị quảng cáo cao hơn
            if(!force) counter++;
                
            return false;
        }
        
        
        /// <summary>
        /// Trả về việc quảng cáo đã được hiển thị hay chưa.
        /// </summary>
        public static bool didShowAd()
        {
            return adShown;
        }


        //được gọi bởi Unity Ads khi hoàn tất lượt xem video
        private void HandleResult(ShowResult result)
        {
            //nếu kết quả là hoàn thành hoặc bỏ qua,
            //quảng cáo thực sự đã được hiển thị trong trò chơi
            switch(result)
            {
              case ShowResult.Finished:
                  adShown = true;
                  break;
            }
            
            //chuyển kết quả cho các trình lắng nghe (listeners)
            adResultEvent?.Invoke(result);
        }
        
        
        //tính toán cơ hội hiển thị quảng cáo,
        //dựa trên tổng số lần thử
        private bool shouldShowAd()
        {   
            //nhân cơ hội với số lần thử theo các bước 20%
            //ví dụ: lần thử thứ 1 = 0%, thứ 2 = 20%, thứ 3 = 40%, thứ 4 = 60%, thứ 5 = 80%, thứ 6 = 100%
            //sau đó chúng ta nên hiển thị quảng cáo nếu giá trị ngẫu nhiên nằm trong phạm vi này
            //điều này được gọi khi người chơi hy sinh, có nghĩa là sau 6 lần hy sinh, cơ hội là 100%
            float adChance = Mathf.Clamp01(counter * 0.2f);
            float adValue = UnityEngine.Random.value;
            bool adTrigger = adValue <= adChance;
            
            //Debug.Log("Chance: " + adChance + ", Random: " + adValue + " -> " + adTrigger);
            return adTrigger;
        }


        /// <summary>
        /// Khởi tạo Unity Ads hoàn tất.
        /// </summary>
        public void OnInitializationComplete()
        {
            //not implemented
        }


        /// <summary>
        /// Khởi tạo Unity Ads thất bại.
        /// </summary>
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            //not implemented
        }


        /// <summary>
        /// Đã load Unity Ads placement. Hiển thị quảng cáo ngay lập tức.
        /// </summary>
        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            Advertisement.Show(adUnitId, this);
        }


        /// <summary>
        /// Lỗi Unity Ads khi load placement.
        /// </summary>
        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            HandleResult(ShowResult.Failed);
        }


        /// <summary>
        /// Bắt đầu hiển thị Unity Ads placement.
        /// </summary>
        public void OnUnityAdsShowStart(string adUnitId)
        {
            //not implemented
        }


        /// <summary>
        /// Unity Ads placement đã được click.
        /// </summary>
        public void OnUnityAdsShowClick(string adUnitId)
        {
            //not implemented
        }


        /// <summary>
        /// Unity Ads placement đã được hiển thị xong.
        /// </summary>
        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            //chúng ta không phân biệt giữa hoàn thành và bỏ qua
            //dù thế nào đi nữa, một quảng cáo đã được hiển thị nên thế là đủ rồi
            HandleResult(ShowResult.Finished);
        }


        /// Lỗi Unity Ads khi hiển thị placement đã load.
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            HandleResult(ShowResult.Failed);
        }
        #endif
    }
}