/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using System.Collections;
using Photon.Pun;

namespace TanksMP
{
    /// <summary>
    /// Quản lý việc tạo (spawn) các prefab được đồng bộ qua mạng, trong trường hợp này là các vật phẩm thu thập và hỗ trợ (powerups).
    /// Với thời gian hồi sinh được đồng bộ trên tất cả các máy khách, nó cũng hỗ trợ việc chuyển đổi chủ phòng (host migration).
    /// </summary>
    public class ObjectSpawner : MonoBehaviourPunCallbacks
	{
        /// <summary>
        /// Prefab để đồng bộ việc khởi tạo qua mạng.
        /// </summary>
		public GameObject prefab;

        /// <summary>
        /// Ô đánh dấu xem đối tượng có nên được hồi sinh sau khi bị hủy hay không.
        /// </summary>
        public bool respawn;

        /// <summary>
        /// Khoảng thời gian chờ cho đến khi hồi sinh lại đối tượng sau khi nó bị hủy.
        /// </summary>
        public int respawnTime;

        /// <summary>
        /// Tham chiếu đến instance của prefab đã được tạo trong scene.
        /// </summary>
        [HideInInspector]
        public GameObject obj;

        /// <summary>
        /// Loại vật phẩm thu thập mà bộ tạo này nên sử dụng. Cái này được thiết lập tự động,
        /// do đó bị ẩn trong inspector. Gửi các thông điệp mạng tùy thuộc vào loại.
        /// </summary>
        [HideInInspector]
        public CollectionType colType = CollectionType.Use;

        //giá trị thời gian khi lần hồi sinh tiếp theo sẽ diễn ra, được tính theo thời gian trong trò chơi
        private float nextSpawn;


        //khi vào scene game lần đầu tiên với tư cách là master client,
        //master client nên tạo đối tượng trong scene cho tất cả các client khác
        void Start()
        {
            if(PhotonNetwork.IsMasterClient)
                OnMasterClientSwitched(PhotonNetwork.LocalPlayer);
        }
        
        
        /// <summary>
        /// Đồng bộ hóa trạng thái hoạt động hiện tại của đối tượng cho các người chơi mới tham gia.
        /// </summary>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player player)
        {
            //không thực thi nếu không phải master, và cũng không thực thi cho chính master client
            if(!PhotonNetwork.IsMasterClient || player.IsMasterClient)
                return;

            //đối tượng đang hoạt động trong scene trên master. Do đó gửi một lời gọi khởi tạo
            //đến người chơi mới tham gia để đối tượng cũng được kích hoạt/khởi tạo trên máy khách đó
            if (obj != null && obj.activeInHierarchy)
            {
                this.photonView.RPC("Instantiate", player);
            }

            //xác định các trường hợp mà phương thức SetRespawn nên được gọi thay thế
            switch (colType)
            {
                case CollectionType.Use:
                    //trên master, đối tượng không hoạt động trong scene. Với tư cách là client, chúng ta phải biết
                    //thời gian hồi sinh còn lại để có thể tiếp quản trong kịch bản chuyển đổi chủ phòng
                    if (obj == null || !obj.activeInHierarchy)
                    {
                        this.photonView.RPC("SetRespawn", player, nextSpawn);
                    }
                    break;

                case CollectionType.Pickup:
                    //ngoài việc kiểm tra ở trên, ở đây chúng ta cũng kiểm tra trạng thái hiện tại
                    //nếu vật phẩm bị rơi, master client cũng nên gửi thời gian hồi sinh đã cập nhật
                    if (obj == null || !obj.activeInHierarchy ||
                      (obj.transform.parent != PoolManager.GetPool(obj).transform && obj.transform.position != transform.position))
                    {
                        this.photonView.RPC("SetRespawn", player, nextSpawn);
                    }
                    break;
            }
        }


        /// <summary>
        /// Được gọi sau khi chuyển sang một MasterClient mới khi cái hiện tại rời đi.
        /// Tại đây, master mới phải quyết định có kích hoạt đối tượng trong scene hay không.
        /// </summary>
		public override void OnMasterClientSwitched(Photon.Realtime.Player newMaster)
		{         
            //chỉ thực thi trên máy khách master mới
            if(PhotonNetwork.LocalPlayer != newMaster)
                return;

            //xác định các trường hợp mà SpawnRoutine nên được bỏ qua
            switch (colType)
            {
                case CollectionType.Use:
                    //đối tượng đã hoạt động nên không kích hoạt coroutine hồi sinh
                    if (obj != null && obj.activeInHierarchy)
                        return;
                    break;
                case CollectionType.Pickup:
                    //ngoài việc kiểm tra ở trên, ở đây chúng ta cũng kiểm tra trạng thái hiện tại
                    //nếu vật phẩm không bị mang đi và đang ở căn cứ, chúng ta có thể bỏ qua việc hồi sinh
                    if (obj != null && obj.activeInHierarchy &&
                        obj.transform.parent == PoolManager.GetPool(obj).transform &&
                        obj.transform.position == transform.position)
                        return;
                    break;
            }

            StartCoroutine(SpawnRoutine());
        }


        //tính toán thời gian còn lại cho đến lần hồi sinh tiếp theo,
        //chờ đợi độ trễ trôi qua và sau đó khởi tạo đối tượng
        IEnumerator SpawnRoutine()
		{
            yield return new WaitForEndOfFrame();
            float delay = Mathf.Clamp(nextSpawn - (float)PhotonNetwork.Time, 0, respawnTime);
			yield return new WaitForSeconds(delay);

            if (PhotonNetwork.IsConnected)
            {
                //phân biệt giữa các loại CollectionType
                if(colType == CollectionType.Pickup && obj != null)
                {
                    //nếu vật phẩm thuộc loại Pickup, nó không nên bị hủy sau khi
                    //routine kết thúc mà nên được trả lại vị trí ban đầu
                    PhotonNetwork.RemoveRPCs(this.photonView);
                    this.photonView.RPC("Return", RpcTarget.All);
                }
                else
                {
                    //khởi tạo một bản sao mới trên tất cả các máy khách
                    this.photonView.RPC("Instantiate", RpcTarget.All);
                }
            }
        }
		
        
        /// <summary>
        /// Khởi tạo đối tượng trong scene bằng chức năng của PoolManager.
        /// </summary>
        [PunRPC]
		public void Instantiate()
		{
            //kiểm tra để đảm bảo trong trường hợp đã có một đối tượng đang hoạt động
            if (obj != null)
                return;

			obj = PoolManager.Spawn(prefab, transform.position, transform.rotation);
            //thiết lập tham chiếu trên đối tượng đã được khởi tạo để tham chiếu chéo
            Collectible colItem = obj.GetComponent<Collectible>();
            if(colItem != null)
            {
                //thiết lập tham chiếu chéo
                colItem.spawner = this;
                //tự động thiết lập loại vật phẩm nội bộ
                if (colItem is CollectibleTeam) colType = CollectionType.Pickup;
                else colType = CollectionType.Use;
            }
		}


        /// <summary>
        /// Thu thập đối tượng và gán nó cho người chơi có view tương ứng.
        /// </summary>
        [PunRPC]
        public void Pickup(short viewId)
        {
            //trong trường hợp lời gọi phương thức này được nhận qua mạng sớm hơn việc
            //khởi tạo bộ tạo, ở đây chúng ta đảm bảo bắt kịp và khởi tạo nó trực tiếp
            if (obj == null)
                Instantiate();

            //lấy transform của view mục tiêu để làm cha
            PhotonView view = PhotonView.Find(viewId);
            obj.transform.parent = view.transform;
            obj.transform.localPosition = Vector3.zero + new Vector3(0, 2, 0);
            
            //gán người mang cho vật phẩm thu thập
            Collectible colItem = obj.GetComponent<Collectible>();
            if (colItem != null)
            {
                colItem.carrierId = viewId;
                colItem.OnPickup();
            }

            //hủy bỏ bộ đếm thời gian trả về vì đối tượng này hiện đang được mang đi
            if (PhotonNetwork.IsMasterClient)
                StopAllCoroutines();
        }


        /// <summary>
        /// Gỡ bỏ đối tượng khỏi bất kỳ người mang nào và thả nó tại vị trí mục tiêu.
        /// </summary>
        [PunRPC]
        public void Drop(Vector3 position)
        {
            //trong trường hợp lời gọi phương thức này được nhận qua mạng sớm hơn việc
            //khởi tạo bộ tạo, ở đây chúng ta đảm bảo bắt kịp và khởi tạo nó trực tiếp
            if (obj == null)
                Instantiate();

            //gán lại cha của đối tượng cho bộ tạo này
            obj.transform.parent = PoolManager.GetPool(obj).transform;
            obj.transform.position = position;

            //đặt lại người mang
            Collectible colItem = obj.GetComponent<Collectible>();
            if (colItem != null)
            {
                colItem.carrierId = -1;
                colItem.OnDrop();
            }

            //cập nhật bộ đếm hồi sinh cho một thời điểm trong tương lai
            SetRespawn();
            //nếu cơ chế hồi sinh được chọn, kích hoạt một coroutine mới
            if (PhotonNetwork.IsMasterClient && respawn)
            {
                StopAllCoroutines();
                StartCoroutine(SpawnRoutine());
            }
        }


        /// <summary>
        /// Trả đối tượng về vị trí của bộ tạo này. Ví dụ: trong chế độ Capture The Flag, điều này
        /// có thể xảy ra nếu một đội thu thập cờ của chính họ, hoặc cờ hết thời gian sau khi bị rơi. 
        /// </summary>
        [PunRPC]
        public void Return()
        {
            //gán lại cha của đối tượng cho bộ tạo này
            obj.transform.parent = PoolManager.GetPool(obj).transform;
            obj.transform.position = transform.position;

            //đặt lại người mang
            Collectible colItem = obj.GetComponent<Collectible>();
            if (colItem != null)
            {
                colItem.carrierId = -1;
                colItem.OnReturn();
            }

            //hủy bỏ bộ đếm thời gian trả về vì đối tượng hiện đã trở lại vị trí căn cứ của nó
            if (PhotonNetwork.IsMasterClient)
                StopAllCoroutines();
        }


        /// <summary>
        /// Được gọi bởi đối tượng được tạo để tự hủy trên thành phần quản lý này.
        /// Đây có thể là trường hợp khi nó được người chơi thu thập.
        /// </summary>
        [PunRPC]
		public void Destroy()
		{
            //hủy đối tượng và xóa các tham chiếu
			PoolManager.Despawn(obj);
            obj = null;
			
            //nếu nó nên hồi sinh lại, kích hoạt một coroutine mới
			if(PhotonNetwork.IsMasterClient && respawn)
                StartCoroutine(SpawnRoutine());
		}
        
        
        /// <summary>
        /// Được gọi bởi đối tượng được tạo để đặt lại bộ đếm hồi sinh khi nó bị hủy
        /// trong scene. Cũng được gọi trên tất cả các máy khách với bộ đếm hiện tại khi chuyển đổi chủ phòng.
        /// </summary>
        [PunRPC]
        public void SetRespawn(float init = 0f)
        {
            if(init > 0f)
                nextSpawn = init;
            else
                nextSpawn = (float)PhotonNetwork.Time + respawnTime;
        }
	}


    /// <summary>
    /// Loại vật phẩm thu thập được sử dụng trên ObjectSpawner, để xác định xem vật phẩm được tiêu thụ hay được nhặt lên.
    /// </summary>
    public enum CollectionType
    {
        Use,
        Pickup
    }
}