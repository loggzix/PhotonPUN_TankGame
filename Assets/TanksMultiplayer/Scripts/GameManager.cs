/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System.Collections;
using UnityEngine;
using Photon.Pun;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace TanksMP
{
    /// <summary>
    /// Quản lý luồng công việc của trò chơi và cung cấp quyền truy cập cấp cao vào logic mạng trong suốt trò chơi.
    /// Nó quản lý các chức năng như lấp đầy đội, điểm số và kết thúc trò chơi, cũng như kết quả quảng cáo video.
    /// </summary>
    public class GameManager : MonoBehaviourPun
    {
        //tham chiếu đến instance của script này
        private static GameManager instance;

        /// <summary>
        /// Instance người chơi cục bộ được tạo cho máy khách này.
        /// </summary>
        [HideInInspector]
        public Player localPlayer;

        /// <summary>
        /// Chế độ trò chơi đang hoạt động trong scene hiện tại.
        /// </summary>
        public GameMode gameMode = GameMode.TDM;

        /// <summary>
        /// Tham chiếu đến script UI hiển thị số liệu thống kê trò chơi.
        /// </summary>
        public UIGame ui;

        /// <summary>
        /// Định nghĩa các đội chơi với các thuộc tính bổ sung.
        /// </summary>
        public Team[] teams;

        /// <summary>
        /// Số lượng mạng tiêu diệt tối đa cần đạt được trước khi kết thúc trò chơi.
        /// </summary>
        public int maxScore = 30;

        /// <summary>
        /// Độ trễ tính bằng giây trước khi hồi sinh một người chơi sau khi bị tiêu diệt.
        /// </summary>
        public int respawnTime = 5;

        /// <summary>
        /// Bật hoặc tắt sát thương đồng đội. Điều này được xác minh trong script Bullet khi va chạm.
        /// </summary>
        public bool friendlyFire = false;


        //khởi tạo các biến
        void Awake()
        {
            instance = this;

            //nếu Unity Ads được bật, hãy kết nối callback kết quả của nó
            #if UNITY_ADS
                UnityAdsManager.adResultEvent += HandleAdResult;
            #endif
        }


        /// <summary>
        /// Trả về tham chiếu đến instance của script này.
        /// </summary>
        public static GameManager GetInstance()
        {
            return instance;
        }
        
        
        /// <summary>
        /// Kiểm tra toàn cục xem máy khách này có phải là master của trận đấu hay không.
        /// </summary>
        public static bool isMaster()
        {
            return PhotonNetwork.IsMasterClient;
        }


        /// <summary>
        /// Trả về chỉ số đội tiếp theo mà người chơi nên được gán vào.
        /// </summary>
        public int GetTeamFill()
        {
            //khởi tạo các biến
            int[] size = PhotonNetwork.CurrentRoom.GetSize();
            int teamNo = 0;

            int min = size[0];
            //lặp qua các đội để tìm đội có ít người nhất
            for (int i = 0; i < teams.Length; i++)
            {
                //nếu số lượng ít hơn giá trị trước đó
                //lưu số lượng và đội mới cho lần lặp tiếp theo
                if (size[i] < min)
                {
                    min = size[i];
                    teamNo = i;
                }
            }

            //trả về chỉ số của đội có ít người nhất
            return teamNo;
        }


        /// <summary>
        /// Trả về một vị trí spawn ngẫu nhiên trong khu vực spawn của đội.
        /// </summary>
        public Vector3 GetSpawnPosition(int teamIndex)
        {
            //khởi tạo các biến
            Vector3 pos = teams[teamIndex].spawn.position;
            BoxCollider col = teams[teamIndex].spawn.GetComponent<BoxCollider>();

            if(col != null)
            {
                //tìm một vị trí trong phạm vi box collider, trước tiên đặt vị trí y cố định
                //biến counter xác định tần suất chúng ta tính toán lại vị trí mới nếu nằm ngoài phạm vi
                pos.y = col.transform.position.y;
                int counter = 10;
                
                //cố gắng lấy vị trí ngẫu nhiên trong giới hạn collider
                //nếu không nằm trong giới hạn, thực hiện một lần lặp khác
                do
                {
                    pos.x = UnityEngine.Random.Range(col.bounds.min.x, col.bounds.max.x);
                    pos.z = UnityEngine.Random.Range(col.bounds.min.z, col.bounds.max.z);
                    counter--;
                }
                while(!col.bounds.Contains(pos) && counter > 0);
            }
            
            return pos;
        }


        //thực hiện những việc cần làm khi xem xong quảng cáo
        #if UNITY_ADS
        void HandleAdResult(ShowResult result)
        {
            switch (result)
            {
                //trong trường hợp người chơi xem xong quảng cáo thành công,
                //nó sẽ gửi một yêu cầu để được hồi sinh
                case ShowResult.Finished:
                    localPlayer.CmdRespawn();
                    break;
                
                //trong trường hợp quảng cáo không thể hiển thị, chỉ cần xử lý nó
                //giống như chúng ta chưa thử hiển thị quảng cáo video
                //với bộ đếm ngược cái chết thông thường (buộc bỏ qua quảng cáo)
                case ShowResult.Failed:
                    DisplayDeath(true);
                    break;
            }
        }
        #endif


        /// <summary>
        /// Cộng điểm cho đội mục tiêu tùy thuộc vào chế độ trò chơi và loại điểm tương ứng.
        /// Điều này cho phép chúng ta cấp lượng điểm khác nhau cho các hành động ghi điểm khác nhau.
        /// </summary>
        public void AddScore(ScoreType scoreType, int teamIndex)
        {
            //phân biệt giữa các chế độ trò chơi
            switch(gameMode)
            {
                //trong TDM, chúng ta chỉ cấp điểm cho việc tiêu diệt
                case GameMode.TDM:
                    switch(scoreType)
                    {
                        case ScoreType.Kill:
                            PhotonNetwork.CurrentRoom.AddScore(teamIndex, 1);
                            break;
                    }
                break;

                //trong CTF, chúng ta cấp điểm cho cả việc tiêu diệt và chiếm cờ
                case GameMode.CTF:
                    switch(scoreType)
                    {
                        case ScoreType.Kill:
                            PhotonNetwork.CurrentRoom.AddScore(teamIndex, 1);
                            break;

                        case ScoreType.Capture:
                            PhotonNetwork.CurrentRoom.AddScore(teamIndex, 10);
                            break;
                    }
                break;
            }
        }
        

        /// <summary>
        /// Trả về việc một đội đã đạt đến điểm số tối đa của trò chơi hay chưa.
        /// </summary>
        public bool IsGameOver()
        {
            //khởi tạo các biến
            bool isOver = false;
            int[] score = PhotonNetwork.CurrentRoom.GetScore();
            
            //lặp qua các đội để tìm điểm số cao nhất
            for(int i = 0; i < teams.Length; i++)
            {
                //điểm số lớn hơn hoặc bằng điểm tối đa,
                //nghĩa là trò chơi đã kết thúc
                if(score[i] >= maxScore)
                {
                    isOver = true;
                    break;
                }
            }
            
            //trả về kết quả
            return isOver;
        }
        
        
        /// <summary>
        /// Chỉ dành cho người chơi này: thiết lập văn bản báo tử cho biết kẻ giết người khi chết.
        /// Nếu Unity Ads được bật, cố gắng hiển thị quảng cáo trong thời gian chờ hồi sinh.
        /// Bằng cách sử dụng tham số 'skipAd', có thể buộc bỏ qua quảng cáo.
        /// </summary>
        public void DisplayDeath(bool skipAd = false)
        {
            //lấy thành phần player đã giết chúng ta
            Player other = localPlayer;
            string killedByName = "YOURSELF";
            if(localPlayer.killedBy != null)
                other = localPlayer.killedBy.GetComponent<Player>();

            //tự sát hay bị giết bình thường?
            if (other != localPlayer)
            {
                killedByName = other.GetView().GetName();
                //tăng bộ đếm mạng tiêu diệt cục bộ cho trò chơi này
                ui.killCounter[1].text = (int.Parse(ui.killCounter[1].text) + 1).ToString();
                ui.killCounter[1].GetComponent<Animator>().Play("Animation");
            }

            //tính toán xem chúng ta có nên hiển thị quảng cáo video không
            #if UNITY_ADS
            if (!skipAd && UnityAdsManager.ShowAd())
                return;
            #endif

            //khi không có quảng cáo nào được hiển thị, hãy đặt văn bản báo tử
            //và bắt đầu chờ đợi độ trễ hồi sinh ngay lập tức
            ui.SetDeathText(killedByName, teams[other.GetView().GetTeam()]);
            StartCoroutine(SpawnRoutine());
        }


        //coroutine hồi sinh người chơi sau một khoảng thời gian chờ hồi sinh
        IEnumerator SpawnRoutine()
        {
            //tính toán thời điểm hồi sinh
            float targetTime = Time.time + respawnTime;

            //chờ cho đến khi việc hồi sinh kết thúc,
            //trong khi chờ đợi hãy cập nhật bộ đếm ngược hồi sinh
            while (targetTime - Time.time > 0)
            {
                ui.SetSpawnDelay(targetTime - Time.time);
                yield return null;
            }

            //hồi sinh ngay bây giờ: gửi yêu cầu đến server
            ui.DisableDeath();
            localPlayer.CmdRespawn();
        }


        /// <summary>
        /// Chỉ dành cho người chơi này: thiết lập văn bản kết thúc trò chơi cho biết đội chiến thắng.
        /// Vô hiệu hóa việc di chuyển của người chơi để không có bản cập nhật nào được gửi qua mạng.
        /// </summary>
        public void DisplayGameOver(int teamIndex)
        {
            //PhotonNetwork.isMessageQueueRunning = false;
            localPlayer.enabled = false;
            localPlayer.camFollow.HideMask(true);
            ui.SetGameOverText(teams[teamIndex]);

            //bắt đầu coroutine để hiển thị cửa sổ kết thúc trò chơi
            StopCoroutine(SpawnRoutine());
            StartCoroutine(DisplayGameOver());
        }


        //hiển thị cửa sổ kết thúc trò chơi sau một khoảng trễ ngắn
        IEnumerator DisplayGameOver()
        {
            //cho người dùng cơ hội đọc xem đội nào đã thắng trò chơi
            //trước khi bật màn hình kết thúc trò chơi
            yield return new WaitForSeconds(3);

            //hiển thị cửa sổ kết thúc trò chơi (vẫn còn kết nối tại thời điểm đó)
            ui.ShowGameOver();
        }


        //dọn dẹp các callback khi chuyển đổi scene
        void OnDestroy()
        {
            #if UNITY_ADS
                UnityAdsManager.adResultEvent -= HandleAdResult;
            #endif
        }
    }


    /// <summary>
    /// Định nghĩa các thuộc tính của một đội.
    /// </summary>
    [System.Serializable]
    public class Team
    {
        /// <summary>
        /// Tên của đội được hiển thị khi kết thúc trò chơi.
        /// </summary>
        public string name;

        /// <summary>
        /// Màu sắc của một đội cho UI và các prefab người chơi.
        /// </summary>   
        public Material material;

        /// <summary>
        /// Điểm spawn của một đội trong scene. Trong trường hợp nó có một thành phần BoxCollider
        /// đi kèm, một điểm trong giới hạn của collider sẽ được sử dụng.
        /// </summary>
        public Transform spawn;
    }


    /// <summary>
    /// Định nghĩa các loại hành động có thể cấp điểm cho người chơi hoặc đội.
    /// Được sử dụng trong phương thức AddScore() để lọc.
    /// </summary>
    public enum ScoreType
    {
        Kill,
        Capture
    }


    /// <summary>
    /// Các chế độ trò chơi có sẵn được chọn theo từng scene.
    /// Được sử dụng trong phương thức AddScore() để lọc.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Chế độ Team Deathmatch
        /// </summary>
        TDM,

        /// <summary>
        /// Chế độ Capture The Flag
        /// </summary>
        CTF
    }
}