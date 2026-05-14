/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.OnScreen;
using Photon.Pun;
using TMPro;

namespace TanksMP
{
    /// <summary>
    /// Script UI cho tất cả các phần tử, sự kiện đội và tương tác của người dùng trong game scene.
    /// </summary>
    public class UIGame : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// Các thành phần Joystick điều khiển chuyển động và hành động của người chơi trên thiết bị di động.
        /// </summary>
        public OnScreenStick[] controls;

        /// <summary>
        /// Các slider UI hiển thị lấp đầy của mỗi đội bằng cách sử dụng các giá trị tuyệt đối.
        /// </summary>
        public Slider[] teamSize;

        /// <summary>
        /// Các văn bản UI hiển thị điểm hạ gục cho mỗi đội.
        /// </summary>
        public TMP_Text[] teamScore;

        /// <summary>
        /// Các văn bản UI hiển thị điểm hạ gục cho người chơi địa phương này.
        /// [0] = Số lần hạ gục, [1] = Số lần hy sinh
        /// </summary>
        public TMP_Text[] killCounter;

        /// <summary>
        /// Chỉ báo ngắm bắn trên di động cho người chơi địa phương.
        /// </summary>
        public GameObject aimIndicator;

        /// <summary>
        /// Văn bản UI để chỉ báo cái chết của người chơi và ai đã hạ gục người chơi này.
        /// </summary>
        public TMP_Text deathText;

        /// <summary>
        /// Văn bản UI hiển thị thời gian tính bằng giây còn lại cho đến khi người chơi respawn.
        /// </summary>
        public TMP_Text spawnDelayText;

        /// <summary>
        /// Văn bản UI để chỉ báo kết thúc trò chơi và đội nào đã thắng vòng đấu.
        /// </summary>
        public TMP_Text gameOverText;

        /// <summary>
        /// Gameobject cửa sổ UI được kích hoạt khi kết thúc trò chơi, cung cấp các nút chia sẻ và chơi lại.
        /// </summary>
        public GameObject gameOverMenu;


        //khởi tạo các biến
        IEnumerator Start()
        {
			//đợi cho đến khi mạng đã sẵn sàng
            while (GameManager.GetInstance() == null || GameManager.GetInstance().localPlayer == null)
                yield return null;

            //trong editor, chúng ta để các điều khiển joystick hiển thị nhưng phải vô hiệu hóa
            //các thành phần của chúng vì nếu không chúng sẽ tự thêm mình làm đầu vào gamepad vào sơ đồ đầu vào (input scheme)
            #if UNITY_EDITOR
                for(int i = 0; i < controls.Length; i++)
                    controls[i].enabled = false;
            #endif
            //khi chạy trên các thiết bị không phải di động, hãy ẩn các điều khiển joystick
            #if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_WEBGL)
                ToggleControls(false);
            #endif

            //trên các thiết bị di động, bật thêm chỉ báo ngắm bắn bổ sung
            #if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
            if (aimIndicator != null)
            {
                Transform indicator = Instantiate(aimIndicator).transform;
                indicator.SetParent(GameManager.GetInstance().localPlayer.shotPos);
                indicator.localPosition = new Vector3(0f, 0f, 3f);
            }
            #endif

            //phát nhạc nền
            AudioManager.PlayMusic(1);
        }


        /// <summary>
        /// Phương thức này được gọi bất cứ khi nào thuộc tính phòng thay đổi trên mạng.
        /// Cập nhật sĩ số đội và hiển thị UI điểm số trong suốt trò chơi.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			OnTeamSizeChanged(PhotonNetwork.CurrentRoom.GetSize());
			OnTeamScoreChanged(PhotonNetwork.CurrentRoom.GetScore());
		}


        /// <summary>
        /// Đây là một triển khai cho các thay đổi đối với việc lấp đầy đội,
        /// cập nhật các giá trị slider (cập nhật hiển thị UI về sĩ số đội).
        /// </summary>
        public void OnTeamSizeChanged(int[] size)
        {
            //lặp qua các giá trị slider và gán nó
			for(int i = 0; i < size.Length; i++)
            	teamSize[i].value = size[i];
        }


        /// <summary>
        /// Đây là một triển khai cho các thay đổi đối với điểm số đội,
        /// cập nhật các giá trị văn bản (cập nhật hiển thị UI về điểm số đội).
        /// </summary>
        public void OnTeamScoreChanged(int[] score)
        {
            //lặp qua các văn bản
			for(int i = 0; i < score.Length; i++)
            {
                //phát hiện nếu điểm số đã tăng lên, sau đó thêm hiệu ứng hoạt hình đẹp mắt
                if(score[i] > int.Parse(teamScore[i].text))
                    teamScore[i].GetComponent<Animator>().Play("Animation");

                //gán giá trị điểm số vào văn bản
                teamScore[i].text = score[i].ToString();
            }
        }


        /// <summary>
        /// Bật hoặc tắt khả năng hiển thị của các điều khiển joystick.
        /// </summary>
        public void ToggleControls(bool state)
        {
            for (int i = 0; i < controls.Length; i++)
                controls[i].transform.parent.gameObject.SetActive(state);
        }


        /// <summary>
        /// Thiết lập văn bản báo tử cho thấy ai đã hạ gục người chơi theo màu đội của họ.
        /// Tham số: tên kẻ hạ gục, đội của kẻ hạ gục
        /// </summary>
        public void SetDeathText(string playerName, Team team)
        {
            //ẩn các điều khiển joystick trong khi hiển thị văn bản báo tử
            #if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
                ToggleControls(false);
            #endif
            
            //hiển thị tên kẻ hạ gục và tô màu tên bằng cách chuyển đổi màu đội của nó sang giá trị hex HTML RGB cho định dạng UI
            deathText.text = "KILLED BY\n<color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + playerName + "</color>";
        }
        
        
        /// <summary>
        /// Thiết lập giá trị độ trễ respawn hiển thị theo giá trị thời gian tuyệt đối nhận được.
        /// Giá trị thời gian còn lại được tính toán trong một coroutine bởi GameManager.
        /// </summary>
        public void SetSpawnDelay(float time)
        {                
            spawnDelayText.text = Mathf.Ceil(time) + "";
        }
        
        
        /// <summary>
        /// Ẩn bất kỳ thành phần UI nào liên quan đến cái chết của người chơi sau khi respawn.
        /// </summary>
        public void DisableDeath()
        {
            //hiển thị các điều khiển joystick sau khi vô hiệu hóa văn bản báo tử
            #if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
                ToggleControls(true);
            #endif
            
            //xóa giá trị các thành phần văn bản
            deathText.text = string.Empty;
            spawnDelayText.text = string.Empty;
        }


        /// <summary>
        /// Thiết lập văn bản kết thúc trò chơi và hiển thị đội chiến thắng theo màu đội của họ.
        /// </summary>
        public void SetGameOverText(Team team)
        {
            //ẩn các điều khiển joystick trong khi hiển thị văn bản kết thúc trò chơi
            #if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
                ToggleControls(false);
            #endif
            
            //hiển thị đội thắng và tô màu bằng cách chuyển đổi màu đội sang giá trị hex HTML RGB cho định dạng UI
            gameOverText.text = "TEAM <color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + team.name + "</color> WINS!";
        }


        /// <summary>
        /// Hiển thị màn hình kết thúc của trò chơi. Được gọi bởi GameManager sau vài giây trì hoãn.
        /// Cố gắng hiển thị một quảng cáo video, nếu chưa được hiển thị.
        /// </summary>
        public void ShowGameOver()
        {       
            //ẩn văn bản nhưng bật cửa sổ kết thúc trò chơi
            gameOverText.gameObject.SetActive(false);
            gameOverMenu.SetActive(true);
            
            //kiểm tra xem quảng cáo đã được hiển thị trong khi chơi chưa
            //nếu không có quảng cáo nào được hiển thị trong cả vòng đấu, chúng ta yêu cầu một cái ở đây
            #if UNITY_ADS
            if(!UnityAdsManager.didShowAd())
                UnityAdsManager.ShowAd(true);
            #endif
        }


        /// <summary>
        /// Quay lại scene bắt đầu và yêu cầu ngay một phiên trò chơi khác.
        /// Ở scene bắt đầu, chúng ta đã thiết lập màn hình tải và xử lý ngắt kết nối rồi,
        /// vì vậy điều này giúp chúng ta tiết kiệm thêm công sức thực hiện cùng một logic hai lần trong game scene.
        /// Yêu cầu chơi lại được triển khai trong một gameobject khác tồn tại xuyên suốt quá trình thay đổi scene.
        /// </summary>
        public void Restart()
        {
            GameObject gObj = new GameObject("RestartNow");
            gObj.AddComponent<UIRestartButton>();
            DontDestroyOnLoad(gObj);
            
            Disconnect();
        }


        /// <summary>
        /// Ngừng nhận thêm các bản cập nhật mạng bằng cách ngắt kết nối cứng, sau đó tải scene bắt đầu.
        /// </summary>
        public void Disconnect()
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
        }


        /// <summary>
        /// Tải scene bắt đầu. Việc ngắt kết nối đã xảy ra khi hiển thị màn hình GameOver.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(NetworkManagerCustom.GetInstance().offlineSceneIndex);
        }
    }
}
