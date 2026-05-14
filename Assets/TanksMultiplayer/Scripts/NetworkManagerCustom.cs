/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace TanksMP
{
    /// <summary>
    /// Triển khai tùy chỉnh cho hầu hết các trình xử lý callback của Photon cho luồng công việc mạng. Script này
    /// chịu trách nhiệm kết nối với Cloud của Photon, tạo người chơi và xử lý ngắt kết nối.
    /// </summary>
	public class NetworkManagerCustom : MonoBehaviourPunCallbacks
    {
        //tham chiếu đến instance của script này
        private static NetworkManagerCustom instance;

        /// <summary>
        /// Chỉ số scene được tải khi ngắt kết nối khỏi trò chơi.
        /// </summary>
        public int offlineSceneIndex = 0;

        /// <summary>
        /// Chỉ số scene được tải sau khi kết nối đã được thiết lập.
        /// Sẽ bị ghi đè bởi scene ghép trận ngẫu nhiên khi sử dụng bộ lọc GameMode.
        /// </summary>
        public int onlineSceneIndex = 1;

        /// <summary>
        /// Số lượng người chơi tối đa trong một phòng.
        /// </summary>
        public int maxPlayers = 12;

        /// <summary>
        /// Tham chiếu đến các prefab người chơi có sẵn nằm trong thư mục Resources.
        /// </summary>
        public GameObject[] playerPrefabs;

        /// <summary>
        /// Sự kiện được kích hoạt khi kết nối với dịch vụ matchmaker thất bại.
        /// </summary>
        public static event Action connectionFailedEvent;


        //khởi tạo network view
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            //thêm một view vào gameobject này với một viewID duy nhất
            //điều này để tránh việc có cùng một ID trong một scene
            PhotonView view = gameObject.AddComponent<PhotonView>();
            view.ViewID = 999;
        }


        /// <summary>
        /// Trả về tham chiếu đến instance của script này.
        /// </summary>
        public static NetworkManagerCustom GetInstance()
        {
            return instance;
        }


        /// <summary>
        /// Bắt đầu khởi tạo và kết nối với trò chơi. Tùy thuộc vào chế độ mạng đã chọn.
        /// Thiết lập tên người chơi hiện tại trước khi kết nối với máy chủ.
        /// </summary>
        public static void StartMatch(NetworkMode mode)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.NickName = PlayerPrefs.GetString(PrefsKeys.playerName);

            switch (mode)
            {
                //kết nối với một cloud game có sẵn trên máy chủ Photon
                case NetworkMode.Online:
                    PhotonNetwork.ConnectUsingSettings();
                    break;

                //tìm kiếm các trò chơi LAN đang mở trên mạng hiện tại, nếu không có hãy mở một trò chơi mới
                case NetworkMode.LAN:
                    PhotonNetwork.ConnectToMaster(PlayerPrefs.GetString(PrefsKeys.serverAddress), 5055, PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
                    break;

                //bật chế độ offline của Photon để không gửi bất kỳ thông điệp mạng nào cả
                case NetworkMode.Offline:
                    PhotonNetwork.OfflineMode = true;
                    break;
            }
        }


        /// <summary>
        /// Được gọi nếu lệnh gọi kết nối tới máy chủ Photon thất bại trước hoặc sau khi kết nối được thiết lập.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            if (connectionFailedEvent != null)
                connectionFailedEvent();

            //không chuyển đổi scene tự động khi màn hình kết thúc trò chơi đang được hiển thị
            if (GameManager.GetInstance() != null && GameManager.GetInstance().ui.gameOverMenu.activeInHierarchy)
                return;

            //chuyển từ scene online sang scene offline sau khi kết nối được đóng
            if (SceneManager.GetActiveScene().buildIndex != offlineSceneIndex)
                SceneManager.LoadScene(offlineSceneIndex);
        }


        /// <summary>
        /// Được gọi sau khi kết nối với master được thiết lập.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            //đặt tên của chính mình và thử tham gia một trò chơi
            PhotonNetwork.NickName = PlayerPrefs.GetString(PrefsKeys.playerName);

            //sử dụng cái này để xác định các lựa chọn ghép trận theo từng chế độ thay vì tham gia các phòng ngẫu nhiên (cũng xem phương thức OnPhotonRandomJoinFailed())
            //https://doc.photonengine.com/en-us/realtime/current/reference/matchmaking-and-lobby#not_so_random_matchmaking
            Hashtable expectedCustomRoomProperties = new Hashtable() { { "mode", (byte)PlayerPrefs.GetInt(PrefsKeys.gameMode) } };

            //đối với việc ghép trận thực sự ngẫu nhiên, bạn sẽ sử dụng lệnh gọi này mà không có thuộc tính
            //PhotonNetwork.JoinRandomRoom();
            PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, (byte)this.maxPlayers);
        }


        /// <summary>
        /// Được gọi khi việc tham gia một phòng ngẫu nhiên thất bại.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Photon không tìm thấy bất kỳ trận đấu nào trên Master Client mà chúng ta đang kết nối. Đang tạo phòng riêng của chúng ta...");

            //tham gia thất bại nên thử tạo phòng riêng của chúng ta
            RoomOptions roomOptions = new RoomOptions();

            //tương tự như trong phương thức OnConnectedToMaster() ở trên, tại đây chúng ta đang thiết lập các thuộc tính phòng để ghép trận
            //comment lại để ghép trận hoàn toàn ngẫu nhiên
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "mode" };
            roomOptions.CustomRoomProperties = new Hashtable() { { "mode", (byte)PlayerPrefs.GetInt(PrefsKeys.gameMode) } };

            roomOptions.MaxPlayers = (byte)this.maxPlayers;
            roomOptions.CleanupCacheOnLeave = false;
            roomOptions.BroadcastPropsChangeToAll = false;
            PhotonNetwork.CreateRoom(null, roomOptions, null);
        }


        /// <summary>
        /// Được gọi khi việc tạo phòng thất bại.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            if (connectionFailedEvent != null)
                connectionFailedEvent();
        }


        /// <summary>
        /// Được gọi khi máy khách này đã tạo một phòng và tham gia vào đó.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnCreatedRoom()
        {
            //kích thước đội ban đầu của trò chơi cho máy chủ tạo phòng mới.
            //rất tiếc cái này không thể được thiết lập thông qua GameManager vì nó chưa tồn tại tại thời điểm đó
            short initialArrayLength;
            //lấy chế độ trò chơi đã chọn từ PlayerPrefs
            GameMode activeGameMode = ((GameMode)PlayerPrefs.GetInt(PrefsKeys.gameMode));

            //thiết lập khởi tạo kích thước mảng phòng ban đầu dựa trên chế độ trò chơi
            switch(activeGameMode)
            {
                case GameMode.CTF:
                    initialArrayLength = 2;
                    break;
                default:
                    initialArrayLength = 4;
                    break;
            }

            //chúng ta đã tạo một phòng nên chúng ta phải thiết lập các thuộc tính phòng ban đầu cho phòng này,
            //chẳng hạn như điền vào mảng lấp đầy đội và mảng điểm số
            Hashtable roomProps = new Hashtable();
            roomProps.Add(RoomExtensions.size, new int[initialArrayLength]);
            roomProps.Add(RoomExtensions.score, new int[initialArrayLength]);
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

            //tải scene online ngẫu nhiên từ tất cả các scene có sẵn cho chế độ trò chơi đã chọn
            //chúng ta đang kiểm tra quy ước đặt tên ở đây, nếu một scene bắt đầu bằng chữ viết tắt của chế độ trò chơi
            List<int> matchingScenes = new List<int>();
            for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string[] scenePath = SceneUtility.GetScenePathByBuildIndex(i).Split('/');
                if (scenePath[scenePath.Length - 1].StartsWith(activeGameMode.ToString()))
                {
                    matchingScenes.Add(i);
                }
            }
			
			//kiểm tra xem scene của bạn có bắt đầu bằng chữ viết tắt của chế độ trò chơi không
			if(matchingScenes.Count == 0)
            {
                Debug.LogWarning("No Scene for selected Game Mode found in Build Settings!");
                return;
            }

            //lấy scene ngẫu nhiên trong số các scene có sẵn và gán nó làm scene online
            onlineSceneIndex = matchingScenes[UnityEngine.Random.Range(0, matchingScenes.Count)];
            //sau đó tải nó
            PhotonNetwork.LoadLevel(onlineSceneIndex);
        }


        /// <summary>
        /// Được gọi khi tham gia sảnh chờ (lobby) trên Master Server.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnJoinedLobby()
        {
            //khi kết nối với master, hãy thử tham gia một phòng
            PhotonNetwork.JoinRandomRoom();
        }


        /// <summary>
        /// Được gọi khi tham gia một phòng (bằng cách tạo hoặc tham gia).
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnJoinedRoom()
        {
            //chúng ta đã tham gia một phòng đã kết thúc, hãy ngắt kết nối ngay lập tức
            if (GameManager.GetInstance() != null && GameManager.GetInstance().IsGameOver())
            {
                PhotonNetwork.Disconnect();
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
                return;

            //thêm chính mình vào trò chơi. Cái này chỉ được gọi cho máy khách master
            //vì các máy khách khác sẽ kích hoạt callback OnPhotonPlayerConnected trực tiếp
            StartCoroutine(WaitForSceneChange());
        }


        //routine chờ này là cần thiết trong chế độ offline để chờ thay đổi scene hoàn tất,
        //vì trong chế độ offline, Photon không tạm dừng các thông điệp mạng. Nhưng việc
        //để lại cái này cho tất cả các chế độ mạng khác cũng không gây hại gì
        IEnumerator WaitForSceneChange()
        {
            while (SceneManager.GetActiveScene().buildIndex != onlineSceneIndex)
            {
                yield return null;
            }

            //chúng ta đã tự kết nối mình
            OnPlayerEnteredRoom(PhotonNetwork.LocalPlayer);
        }


        /// <summary>
        /// Được gọi khi một người chơi từ xa tham gia phòng.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player player)
        {
            //chỉ để máy khách master xử lý kết nối này
            if (!PhotonNetwork.IsMasterClient)
                return;

            //lấy chỉ số đội tiếp theo mà người chơi nên thuộc về
            //gán nó cho người chơi và cập nhật các thuộc tính người chơi
            int teamIndex = GameManager.GetInstance().GetTeamFill();
            PhotonNetwork.CurrentRoom.AddSize(teamIndex, +1);
            player.SetTeam(teamIndex);

            //ngoài ra các thuộc tính người chơi không được xóa khi ngắt kết nối và kết nối
            //tự động, vì vậy chúng ta phải đặt tất cả các thuộc tính hiện có thành null
            //các giá trị mặc định này sẽ sớm bị ghi đè bởi dữ liệu chính xác
            player.Clear();

            //máy khách master gửi một hướng dẫn đến người chơi này để thêm họ vào trò chơi
            this.photonView.RPC("AddPlayer", player);
        }


        //nhận được từ máy khách master, cho người chơi này, sau khi tham gia trò chơi thành công
		[PunRPC]
		void AddPlayer()
		{
            //lấy chỉ số prefab người chơi đã chọn của chúng ta
			int prefabId = int.Parse(Encryptor.Decrypt(PlayerPrefs.GetString(PrefsKeys.activeTank)));
            
            //lấy vị trí spawn nơi prefab người chơi của chúng ta nên được khởi tạo, tùy thuộc vào đội được gán
            //nếu chúng ta không thể lấy được vị trí, hãy tạo nó ở giữa khu vực đội đó - nếu không hãy sử dụng vị trí đã tính toán
			Transform startPos = GameManager.GetInstance().teams[PhotonNetwork.LocalPlayer.GetTeam()].spawn;
			if (startPos != null) PhotonNetwork.Instantiate(playerPrefabs[prefabId].name, startPos.position, startPos.rotation, 0);
			else PhotonNetwork.Instantiate(playerPrefabs[prefabId].name, Vector3.zero, Quaternion.identity, 0);
		}


        /// <summary>
        /// Được gọi khi một người chơi từ xa rời phòng.
        /// Xem tài liệu chính thức của Photon để biết thêm chi tiết.
        /// </summary>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player player)
        {
            //chỉ để máy khách master xử lý kết nối này
            if (!PhotonNetwork.IsMasterClient)
				return;

            //lấy game object do người chơi điều khiển từ người chơi đã ngắt kết nối
            GameObject targetPlayer = GetPlayerGameObject(player);

            //xử lý bất kỳ vật phẩm thu thập nào được gán cho người chơi đó
            if(targetPlayer != null)
            {
                Collectible[] collectibles = targetPlayer.GetComponentsInChildren<Collectible>(true);
                for (int i = 0; i < collectibles.Length; i++)
                {
                    //để người chơi thả vật phẩm thu thập xuống
                    PhotonNetwork.RemoveRPCs(collectibles[i].spawner.photonView);
                    collectibles[i].spawner.photonView.RPC("Drop", RpcTarget.AllBuffered, targetPlayer.transform.position);
                }
            }

            //dọn dẹp các instance sau khi xử lý người chơi rời đi
            PhotonNetwork.DestroyPlayerObjects(player);
            //giảm lượng lấp đầy đội cho đội của người chơi rời đi và cập nhật thuộc tính phòng
            PhotonNetwork.CurrentRoom.AddSize(player.GetTeam(), -1);
        }


        /// <summary>
        /// Tìm game object Player được điều khiển từ xa của một người chơi cụ thể,
        /// bằng cách lặp qua tất cả các thành phần Player và tìm kiếm người tạo phù hợp.
        /// </summary>
        public GameObject GetPlayerGameObject(Photon.Realtime.Player player)
        {
            GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
            List<Player> playerList = new List<Player>();

            //lấy tất cả các thành phần Player từ các đối tượng gốc
            for (int i = 0; i < rootObjs.Length; i++)
            {
                Player p = rootObjs[i].GetComponentInChildren<Player>(true);
                if (p != null) playerList.Add(p);
            }

            //tìm game object nơi người tạo khớp với ID người chơi cụ thể này
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].photonView.CreatorActorNr == player.ActorNumber)
                {
                    return playerList[i].gameObject;
                }
            }

            return null;
        }
    }


    /// <summary>
    /// Lựa chọn Chế độ Mạng cho loại mạng ưa thích.
    /// </summary>
    public enum NetworkMode
    {
        Online = 0,
        LAN = 1,
        Offline = 2
    }


    /// <summary>
    /// Lớp này mở rộng đối tượng Room của Photon bằng các thuộc tính tùy chỉnh.
    /// Cung cấp một số phương thức để thiết lập và lấy các biến từ chúng.
    /// </summary>
    public static class RoomExtensions
    {       
        /// <summary>
        /// Khóa để truy cập lượng lấp đầy đội theo từng đội từ các thuộc tính phòng.
        /// </summary>
        public const string size = "size";
        
        /// <summary>
        /// Khóa để truy cập điểm số của người chơi theo từng đội từ các thuộc tính phòng.
        /// </summary>
        public const string score = "score";
        
        
        /// <summary>
        /// Trả về lượng lấp đầy đội được nối mạng cho tất cả các đội từ các thuộc tính.
        /// </summary>
        public static int[] GetSize(this Room room)
        {
            return (int[])room.CustomProperties[size];
        }
        
        /// <summary>
        /// Tăng lượng lấp đầy đội cho một đội thêm một khi có người chơi mới tham gia trò chơi.
        /// Cái này cũng được sử dụng khi người chơi ngắt kết nối bằng cách sử dụng giá trị âm.
        /// </summary>
        public static int[] AddSize(this Room room, int teamIndex, int value)
        {
            int[] sizes = room.GetSize();
            sizes[teamIndex] += value;

            room.SetCustomProperties(new Hashtable() {{size, sizes}});
            return sizes;
        }
        
        /// <summary>
        /// Trả về điểm số đội được nối mạng cho tất cả các đội từ các thuộc tính.
        /// </summary>
        public static int[] GetScore(this Room room)
        {
            return (int[])room.CustomProperties[score];
        }
        
        /// <summary>
        /// Tăng điểm số cho một đội thêm một khi một người chơi mới ghi điểm cho đội của mình.
        /// </summary>
        public static int[] AddScore(this Room room, int teamIndex, int value)
        {
            int[] scores = room.GetScore();
            scores[teamIndex] += value;
            
            room.SetCustomProperties(new Hashtable() {{score, scores}});
            return scores;
        }
    }
}