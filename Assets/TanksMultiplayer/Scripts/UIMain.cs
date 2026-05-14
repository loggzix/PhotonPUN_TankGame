/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TanksMP
{
    /// <summary>
    /// Script UI cho tất cả các phần tử, cài đặt và tương tác của người dùng trong menu scene.
    /// </summary>
    public class UIMain : MonoBehaviour
    {
        /// <summary>
        /// Đối tượng cửa sổ cho màn hình tải giữa lúc đang kết nối và chuyển đổi scene.
        /// </summary>
        public GameObject loadingWindow;

        /// <summary>
        /// Đối tượng cửa sổ để hiển thị các lỗi kết nối hoặc hết thời gian chờ (timeout).
        /// </summary>
        public GameObject connectionErrorWindow;

        /// <summary>
        /// Đối tượng cửa sổ để hiển thị các lỗi liên quan đến hành động thanh toán.
        /// </summary>
        public GameObject billingErrorWindow;

        /// <summary>
		/// Cài đặt: trường nhập liệu cho tên người chơi.
		/// </summary>
		public TMPro.TMP_InputField nameField;

        /// <summary>
        /// Cài đặt: lựa chọn dropdown cho chế độ mạng.
        /// </summary>
        public TMPro.TMP_Dropdown networkDrop;

        /// <summary>
        /// Lựa chọn dropdown cho chế độ chơi ưa thích.
        /// </summary>
        public TMPro.TMP_Dropdown gameModeDrop;

        /// <summary>
		/// Cài đặt: trường nhập liệu cho địa chỉ máy chủ thủ công,
        /// lưu trữ máy chủ trong mạng riêng tư (chỉ dành cho Photon).
		/// </summary>
		public TMPro.TMP_InputField serverField;

        /// <summary>
        /// Cài đặt: checkbox để phát nhạc nền.
        /// </summary>
        public Toggle musicToggle;

        /// <summary>
        /// Cài đặt: slider để điều chỉnh âm lượng âm thanh trò chơi.
        /// </summary>
        public Slider volumeSlider;


        //khởi tạo lựa chọn của người chơi trong cửa sổ Cài đặt
        //nếu đây là lần đầu tiên khởi chạy trò chơi, hãy đặt các giá trị ban đầu
        void Start()
        {
            //đặt giá trị ban đầu cho tất cả các cài đặt
            if (!PlayerPrefs.HasKey(PrefsKeys.playerName)) PlayerPrefs.SetString(PrefsKeys.playerName, "User" + System.String.Format("{0:0000}", Random.Range(1, 9999)));
            if (!PlayerPrefs.HasKey(PrefsKeys.networkMode)) PlayerPrefs.SetInt(PrefsKeys.networkMode, 0);
            if (!PlayerPrefs.HasKey(PrefsKeys.gameMode)) PlayerPrefs.SetInt(PrefsKeys.gameMode, 0);
            if (!PlayerPrefs.HasKey(PrefsKeys.serverAddress)) PlayerPrefs.SetString(PrefsKeys.serverAddress, "127.0.0.1");
            if (!PlayerPrefs.HasKey(PrefsKeys.playMusic)) PlayerPrefs.SetString(PrefsKeys.playMusic, "true");
            if (!PlayerPrefs.HasKey(PrefsKeys.appVolume)) PlayerPrefs.SetFloat(PrefsKeys.appVolume, 1f);
            if (!PlayerPrefs.HasKey(PrefsKeys.activeTank)) PlayerPrefs.SetString(PrefsKeys.activeTank, Encryptor.Encrypt("0"));

            PlayerPrefs.Save();

            //đọc các lựa chọn và đặt chúng vào các phần tử UI tương ứng
            nameField.text = PlayerPrefs.GetString(PrefsKeys.playerName);
            networkDrop.value = PlayerPrefs.GetInt(PrefsKeys.networkMode);
            gameModeDrop.value = PlayerPrefs.GetInt(PrefsKeys.gameMode);
            serverField.text = PlayerPrefs.GetString(PrefsKeys.serverAddress);
            musicToggle.isOn = bool.Parse(PlayerPrefs.GetString(PrefsKeys.playMusic));
            volumeSlider.value = PlayerPrefs.GetFloat(PrefsKeys.appVolume);

            //gọi các callback onValueChanged một lần với các giá trị đã lưu của chúng
            OnMusicChanged(musicToggle.isOn);
            OnVolumeChanged(volumeSlider.value);

            //lắng nghe các lỗi kết nối mạng và thanh toán IAP
            NetworkManagerCustom.connectionFailedEvent += OnConnectionError;
            UnityIAPManager.purchaseFailedEvent += OnBillingError;
        }


        /// <summary>
        /// Cố gắng vào game scene. Kích hoạt màn hình tải trong khi kết nối với
        /// Matchmaker và đồng thời bắt đầu coroutine xử lý hết thời gian chờ (timeout).
        /// </summary>
        public void Play()
        {
            loadingWindow.SetActive(true);
            NetworkManagerCustom.StartMatch((NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode));
            StartCoroutine(HandleTimeout());
        }


        //coroutine đợi 10 giây trước khi hủy việc tham gia trận đấu
        IEnumerator HandleTimeout()
        {
            yield return new WaitForSeconds(10);

            //đã hết thời gian chờ, chúng ta muốn dừng việc tham gia trò chơi ngay bây giờ
            Photon.Pun.PhotonNetwork.Disconnect();
            //hiển thị cửa sổ lỗi kết nối
            OnConnectionError();
        }


        //kích hoạt cửa sổ lỗi kết nối để nó hiển thị
        void OnConnectionError()
        {
            //trò chơi đã tắt hoàn toàn
            if (this == null)
                return;

            StopAllCoroutines();
            loadingWindow.SetActive(false);
            connectionErrorWindow.SetActive(true);
        }


        //kích hoạt cửa sổ lỗi thanh toán để nó hiển thị
        void OnBillingError(string error)
        {
            //lấy nhãn văn bản để hiển thị lý do thanh toán thất bại
            TMP_Text errorLabel = billingErrorWindow.GetComponentInChildren<TMP_Text>();
            if (errorLabel)
                errorLabel.text = "Purchase failed.\n" + error;

            billingErrorWindow.SetActive(true);
        }


        /// <summary>
        /// Chỉ cho phép nhập thêm địa chỉ máy chủ trong chế độ mạng LAN.
        /// Nếu không, trường nhập liệu sẽ bị ẩn trong phần cài đặt (chỉ dành cho Photon).
        /// </summary>
        public void OnNetworkChanged(int value)
        {
            serverField.gameObject.SetActive((NetworkMode)value == NetworkMode.LAN ? true : false);
        }


        /// <summary>
        /// Lưu giá trị GameMode mới được chọn vào PlayerPrefs để kiểm tra sau.
        /// Được gọi bởi sự kiện onValueChanged của DropDown.
        /// </summary>
        public void OnGameModeChanged(int value)
        {
            PlayerPrefs.SetInt(PrefsKeys.gameMode, value);
            PlayerPrefs.Save();
        }


        /// <summary>
        /// Chỉnh sửa AudioSource nhạc dựa trên lựa chọn của người chơi.
        /// Được gọi bởi sự kiện onValueChanged của Toggle.
        /// </summary>
        public void OnMusicChanged(bool value)
        {
            AudioManager.GetInstance().musicSource.enabled = musicToggle.isOn;
            AudioManager.PlayMusic(0);
        }


        /// <summary>
        /// Chỉnh sửa âm lượng trò chơi tổng thể dựa trên lựa chọn của người chơi.
        /// Được gọi bởi sự kiện onValueChanged của Slider.
        /// </summary>
        public void OnVolumeChanged(float value)
        {
            volumeSlider.value = value;
            AudioListener.volume = value;
        }


        /// <summary>
        /// Lưu tất cả các lựa chọn của người chơi trong cửa sổ Cài đặt vào thiết bị.
        /// </summary>
        public void CloseSettings()
        {
            PlayerPrefs.SetString(PrefsKeys.playerName, nameField.text);
            PlayerPrefs.SetInt(PrefsKeys.networkMode, networkDrop.value);
            PlayerPrefs.SetString(PrefsKeys.serverAddress, serverField.text);
            PlayerPrefs.SetString(PrefsKeys.playMusic, musicToggle.isOn.ToString());
            PlayerPrefs.SetFloat(PrefsKeys.appVolume, volumeSlider.value);
            PlayerPrefs.Save();
        }

		
        /// <summary>
        /// Mở cửa sổ trình duyệt đến trang App Store cho ứng dụng này.
        /// </summary>
        public void RateApp()
        {
            //UnityAnalyticsManager.RateStart();
            
            //url ứng dụng mặc định trên các nền tảng không phải di động
            //thay thế bằng trang web của bạn, ví dụ vậy
			string url = "";
			
			#if UNITY_ANDROID
				url = "http://play.google.com/store/apps/details?id=" + Application.identifier;
			#elif UNITY_IPHONE
				url = "https://itunes.apple.com/app/idXXXXXXXXX";
			#endif
			
			if(string.IsNullOrEmpty(url) || url.EndsWith("XXXXXX"))
            {
                Debug.LogWarning("UIMain: You didn't replace your app links!");
                return;
            }
			
			Application.OpenURL(url);
        }
    }
}