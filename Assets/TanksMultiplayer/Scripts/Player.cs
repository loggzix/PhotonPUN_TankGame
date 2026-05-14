/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using TMPro;

namespace TanksMP
{
    /// <summary>
    /// Lớp người chơi được nối mạng thực hiện điều khiển di chuyển và bắn súng.
    /// Chứa cả logic server và máy khách theo cách tiếp cận có thẩm quyền (authoritative).
    /// </summary>
    public class Player : MonoBehaviourPunCallbacks, IPunObservable
    {
        /// <summary>
        /// Văn bản UI hiển thị tên người chơi.
        /// </summary>    
        public TMP_Text label;

        /// <summary>
        /// Giá trị sức khỏe tối đa khi bắt đầu trò chơi.
        /// </summary>
        public int maxHealth = 10;

        /// <summary>
        /// Độ xoay hiện tại của tháp pháo và hướng bắn.
        /// </summary>
        [HideInInspector]
        public short turretRotation;

        /// <summary>
        /// Độ trễ giữa các lần bắn.
        /// </summary>
        public float fireRate = 0.75f;

        /// <summary>
        /// Tốc độ di chuyển theo mọi hướng.
        /// </summary>
        public float moveSpeed = 8f;

        /// <summary>
        /// Thanh trượt UI hiển thị giá trị sức khỏe.
        /// </summary>
        public Slider healthSlider;

        /// <summary>
        /// Thanh trượt UI hiển thị giá trị khiên.
        /// </summary>
        public Slider shieldSlider;

        /// <summary>
        /// Clip âm thanh phát ra khi bắn súng.
        /// </summary>
        public AudioClip shotClip;

        /// <summary>
        /// Clip âm thanh phát ra khi người chơi chết.
        /// </summary>
        public AudioClip explosionClip;

        /// <summary>
        /// Đối tượng được tạo ra khi bắn.
        /// </summary>
        public GameObject shotFX;

        /// <summary>
        /// Đối tượng được tạo ra khi người chơi chết.
        /// </summary>
        public GameObject explosionFX;

        /// <summary>
        /// Tháp pháo xoay theo hướng nhìn.
        /// </summary>
        public Transform turret;

        /// <summary>
        /// Vị trí để tạo đạn mới.
        /// </summary>
        public Transform shotPos;

        /// <summary>
        /// Mảng các loại đạn có sẵn để bắn.
        /// </summary>
        public GameObject[] bullets;

        /// <summary>
        /// Các MeshRenderer nên được làm nổi bật bằng màu của đội.
        /// </summary>
        public MeshRenderer[] renderers;

        /// <summary>
        /// Đối tượng người chơi cuối cùng đã giết người chơi này.
        /// </summary>
        [HideInInspector]
        public GameObject killedBy;

        /// <summary>
        /// Tham chiếu đến thành phần theo dõi của camera.
        /// </summary>
        [HideInInspector]
        public FollowTarget camFollow;

        //dấu thời gian khi lần bắn tiếp theo có thể thực hiện
        private float nextFire;
        
        //tham chiếu đến rigidbody này
        #pragma warning disable 0649
		private Rigidbody rb;
		#pragma warning restore 0649

        //đầu vào di chuyển được đọc trực tiếp từ bàn phím/joystick di động
        private Vector2 moveInput;
        //đầu vào xoay được đọc trực tiếp từ chuột/joystick di động
        private Vector2 viewInput;
        //liệu người chơi hiện đang nhấn đầu vào để bắn hay không
        private bool shootInput;


        //khởi tạo các giá trị server cho người chơi này
        void Awake()
        {
            //chỉ để master thực hiện khởi tạo
            if(!PhotonNetwork.IsMasterClient)
                return;
            
            //đặt giá trị sức khỏe hiện tại của người chơi sau khi tham gia
            GetView().SetHealth(maxHealth);
        }


        /// <summary>
        /// Khởi tạo các giá trị được đồng bộ trên mọi máy khách.
        /// Khởi tạo camera và đầu vào cho máy khách cục bộ này.
        /// </summary>
        void Start()
        {           
			//lấy đội tương ứng và tô màu các renderer theo màu của đội
            Team team = GameManager.GetInstance().teams[GetView().GetTeam()];
            for(int i = 0; i < renderers.Length; i++)
                renderers[i].material = team.material;

            //đặt tên trong nhãn (label)
            label.text = GetView().GetName();
            //gọi các hook thủ công để cập nhật
            OnHealthChange(GetView().GetHealth());
            OnShieldChange(GetView().GetShield());

            //chỉ được gọi cho máy khách này
            if (!photonView.IsMine)
                return;

			//đặt một tham chiếu toàn cục đến người chơi cục bộ
            GameManager.GetInstance().localPlayer = this;

			//lấy các thành phần và đặt mục tiêu cho camera
            rb = GetComponent<Rigidbody>();
            camFollow = Camera.main.GetComponent<FollowTarget>();
            camFollow.target = turret;

            //khởi tạo các điều khiển đầu vào
            PlayerInput.GetPlayerByIndex(0).onActionTriggered += OnAction;
        }


        /// <summary>
        /// Phương thức này được gọi bất cứ khi nào các thuộc tính của người chơi được thay đổi trên mạng.
        /// </summary>
        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player player, ExitGames.Client.Photon.Hashtable playerAndUpdatedProps)
        {
            //chỉ phản hồi khi có sự thay đổi thuộc tính cho người chơi này
            if(player != photonView.Owner)
                return;

            //cập nhật các giá trị có thể thay đổi bất cứ lúc nào để việc hiển thị luôn được cập nhật
            OnHealthChange(player.GetHealth());
            OnShieldChange(player.GetShield());
        }

        
        //phương thức này được gọi nhiều lần mỗi giây, ít nhất 10 lần hoặc hơn
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {        
            if (stream.IsWriting)
            {             
                //tại đây chúng ta gửi góc xoay tháp pháo đến các máy khách khác
                stream.SendNext(turretRotation);
            }
            else
            {   
                //tại đây chúng ta nhận góc xoay tháp pháo từ những người khác và áp dụng nó
                this.turretRotation = (short)stream.ReceiveNext();
                OnTurretRotation();
            }
        }


        /// <summary>
        /// Phương thức hỗ trợ để lấy chủ sở hữu đối tượng hiện tại.
        /// </summary>
        public PhotonView GetView()
        {
            return this.photonView;
        }


        //xử lý các giá trị đầu vào
        void Update()
        {
            if(!photonView.IsMine)
                return;

            ApplyMovement();
            ApplyRotation();
            if(shootInput) Shoot();
        }


        //đồng bộ hóa các vòng xoay tháp pháo từ những người chơi khác
        void FixedUpdate()
		{
			//bỏ qua các cuộc gọi tiếp theo cho các máy khách từ xa    
            if (!photonView.IsMine)
            {
                //luôn cập nhật độ xoay tháp pháo cho tất cả các máy khách
                OnTurretRotation();
                return;
            }
        }
            
      
        //liên tục kiểm tra đầu vào
        void OnAction(InputAction.CallbackContext context)
        {
            if(!gameObject.activeInHierarchy)
                return;

            switch(context.action.name)
            {
                //đọc các hướng di chuyển và tính toán lực di chuyển
                case "Move":
                    moveInput = context.ReadValue<Vector2>();

                    //tái hiện đầu vào cho các điều khiển di động vì mục đích minh họa
                    #if UNITY_EDITOR
				        RectTransform moveStick = GameManager.GetInstance().ui.controls[0].GetComponent<RectTransform>();
                        moveStick.anchoredPosition = moveInput * GameManager.GetInstance().ui.controls[0].movementRange;
			        #endif
                    break;

                case "View":
                    viewInput = context.ReadValue<Vector2>();

                    //chuẩn hóa từ không gian màn hình chuột sang phạm vi từ -1 đến 1
                    if(context.control.device.name == "Mouse")
                    {
                        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                        Vector2 normalized = (viewInput - screenCenter) / screenCenter;
                        normalized = Vector2.ClampMagnitude(normalized, 1f);
                        viewInput = normalized;
                    }

                    //trên thiết bị di động, bắt đầu bắn khi sử dụng cần điều khiển bên phải
                    if(context.control.device.name != "Mouse")
                    {
                        bool isDragActive = viewInput != Vector2.zero;
                        //đặt một khoảng trễ nhỏ cho phát bắn đầu tiên
                        if(isDragActive && !shootInput)
                            nextFire = Time.time + 0.25f;
                        
                        shootInput = isDragActive;
                    }

                    //tái hiện đầu vào cho các điều khiển di động vì mục đích minh họa
                    #if UNITY_EDITOR
				        RectTransform viewStick = GameManager.GetInstance().ui.controls[1].GetComponent<RectTransform>();
                        viewStick.anchoredPosition = viewInput * GameManager.GetInstance().ui.controls[1].movementRange;
			        #endif
                    break;

                //bắn đạn khi nhấp chuột trái
                case "Shoot":
                    shootInput = context.control.IsPressed();
                    break;
            }
        }


        //di chuyển rigidbody theo hướng đầu vào di chuyển
        void ApplyMovement()
        {
            //nếu hướng khác không, hãy xoay người chơi theo hướng di chuyển so với camera
            if (moveInput == Vector2.zero)
                return;

            transform.rotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0, moveInput.y))
                                    * Quaternion.Euler(0, camFollow.camTransform.eulerAngles.y, 0);
            
            //tạo vector di chuyển dựa trên độ xoay và tốc độ hiện tại
            Vector3 moveDir = transform.forward * moveSpeed * Time.deltaTime;
            //áp dụng vector vào vị trí rigidbody
            rb.MovePosition(rb.position + moveDir);
        }


        //xoay tháp pháo theo hướng đầu vào xoay nhìn
        void ApplyRotation()
        {
			//không xoay nếu không có giá trị đầu vào
            if(viewInput == Vector2.zero)
                return;

            //lấy giá trị xoay dưới dạng góc từ hướng chúng ta nhận được
            turretRotation = (short)(Quaternion.LookRotation(new Vector3(viewInput.x, 0, viewInput.y)).eulerAngles.y + camFollow.camTransform.eulerAngles.y);
            OnTurretRotation();
        }


        //shoots a bullet in the direction passed in
        //we do not rely on the current turret rotation here, because we send the direction
        //along with the shot request to the server to absolutely ensure a synced shot position
        protected void Shoot(Vector2 direction = default(Vector2))
        {
            //nếu độ trễ bắn đã hết  
            if (Time.time > nextFire)
            {
                //đặt dấu thời gian cho phát bắn tiếp theo
                nextFire = Time.time + fireRate;
                
                //gửi vị trí máy khách hiện tại và vòng xoay tháp pháo đi kèm để đồng bộ vị trí bắn
                //ngoài ra chúng ta gửi nó dưới dạng mảng short (chỉ x, z - bỏ qua y) để tiết kiệm thêm băng thông
                short[] pos = new short[] { (short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10)};
                //gửi yêu cầu bắn kèm theo nguồn gốc tới server
                this.photonView.RPC("CmdShoot", RpcTarget.AllViaServer, pos, turretRotation);
            }
        }
        
        
        //được gọi trên server trước nhưng được chuyển tiếp đến tất cả các máy khách
        [PunRPC]
        protected void CmdShoot(short[] position, short angle)
        {   
            //lấy loại đạn hiện tại
            int currentBullet = GetView().GetBullet();

            //tính toán điểm giữa của vị trí bắn được gửi và vị trí hiện tại trên server (hệ số 0.6f = 40% máy khách, 60% server)
            //điều này được thực hiện để bù đắp độ trễ mạng và làm mượt nó giữa cả hai vị trí máy khách/server
            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(position[0]/10f, shotPos.position.y, position[1]/10f), 0.6f);
            Quaternion syncedRot = turret.rotation = Quaternion.Euler(0, angle, 0);

            //tạo đạn sử dụng pooling
            GameObject obj = PoolManager.Spawn(bullets[currentBullet], shotCenter, syncedRot);
            obj.GetComponent<Bullet>().owner = gameObject;

            //kiểm tra lượng đạn hiện tại
            //để server giảm lượng đạn đặc biệt, nếu có
            if (PhotonNetwork.IsMasterClient && currentBullet != 0)
            {
                //nếu hết đạn: tự động đặt lại loại đạn mặc định
                GetView().DecreaseAmmo(1);
            }

            //gửi sự kiện tới tất cả các máy khách để tạo hiệu ứng bắn
            if (shotFX || shotClip)
                RpcOnShot();
        }


        //được gọi trên tất cả các máy khách sau khi tạo đạn
        //tạo hiệu ứng hoặc âm thanh tại địa phương, nếu được thiết lập
        protected void RpcOnShot()
        {
            if (shotFX) PoolManager.Spawn(shotFX, shotPos.position, Quaternion.identity);
            if (shotClip) AudioManager.Play3D(shotClip, shotPos.position, 0.1f);
        }


        //hook để cập nhật vòng xoay tháp pháo tại máy khách cục bộ
        void OnTurretRotation()
        {
            //chúng ta không cần kiểm tra quyền sở hữu cục bộ khi đặt turretRotation,
            //vì OnPhotonSerializeView PhotonStream.isWriting == true chỉ áp dụng cho chủ sở hữu
            turret.rotation = Quaternion.Euler(0, turretRotation, 0);
        }


        //hook để cập nhật sức khỏe tại địa phương
        //(giá trị thực tế cập nhật thông qua player properties)
        protected void OnHealthChange(int value)
        {
            healthSlider.value = (float)value / maxHealth;
        }


        //hook để cập nhật khiên tại địa phương
        //(giá trị thực tế cập nhật thông qua player properties)
        protected void OnShieldChange(int value)
        {
            shieldSlider.value = value;
        }


        /// <summary>
        /// Chỉ dành cho server: tính toán sát thương mà Người chơi phải nhận,
		/// kích hoạt tăng điểm và quy trình hồi sinh khi chết.
        /// </summary>
        public void TakeDamage(Bullet bullet)
        {
            //lưu trữ tạm thời các biến mạng
            int health = GetView().GetHealth();
            int shield = GetView().GetShield();

            //giảm khiên khi bị trúng đạn
            if (shield > 0)
            {
                GetView().DecreaseShield(1);
                return;
            }

            //trừ sức khỏe theo sát thương
            //tạm thời tại địa phương, để chỉ cập nhật một lần duy nhất sau đó
            health -= bullet.damage;

            //đạn đã tiêu diệt người chơi
            if (health <= 0)
            {
                //trò chơi đã kết thúc nên không làm gì thêm
                if(GameManager.GetInstance().IsGameOver()) return;

                //lấy kẻ giết người và tăng điểm cho đội đối phương đó
                Player other = bullet.owner.GetComponent<Player>();
                int otherTeam = other.GetView().GetTeam();
                if(GetView().GetTeam() != otherTeam)
                    GameManager.GetInstance().AddScore(ScoreType.Kill, otherTeam);

                //điểm số tối đa đã đạt được ngay bây giờ
                if(GameManager.GetInstance().IsGameOver())
                {
                    //đóng phòng đối với những người chơi mới tham gia
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    //thông báo cho tất cả các máy khách đội chiến thắng
                    this.photonView.RPC("RpcGameOver", RpcTarget.All, (byte)otherTeam);
                    return;
                }

                //trò chơi vẫn chưa kết thúc, hãy đặt lại các giá trị thời gian thực
                //đồng thời thông báo cho tất cả các máy khách hủy đối tượng người chơi này
                GetView().SetHealth(maxHealth);
                GetView().SetBullet(0);

                //dọn dẹp các vật phẩm thu thập trên người chơi này bằng cách để chúng rơi xuống
                Collectible[] collectibles = GetComponentsInChildren<Collectible>(true);
                for (int i = 0; i < collectibles.Length; i++)
                {
                    PhotonNetwork.RemoveRPCs(collectibles[i].spawner.photonView);
                    collectibles[i].spawner.photonView.RPC("Drop", RpcTarget.AllBuffered, transform.position);
                }

                //cho người chơi đã chết biết ai đã giết họ (chủ sở hữu viên đạn)
                short senderId = 0;
                if (bullet.owner != null)
                    senderId = (short)bullet.owner.GetComponent<PhotonView>().ViewID;

                this.photonView.RPC("RpcRespawn", RpcTarget.All, senderId);
            }
            else
            {
                //we didn't die, set health to new value
                GetView().SetHealth(health);
            }
        }


        //được gọi trên tất cả các máy khách khi người chơi chết và hồi sinh
        //khác biệt duy nhất là khi hồi sinh, máy khách sẽ gửi yêu cầu
        [PunRPC]
        protected virtual void RpcRespawn(short senderId)
        {
            //chuyển đổi khả năng hiển thị cho gameobject người chơi (bật/tắt)
            gameObject.SetActive(!gameObject.activeInHierarchy);
            bool isActive = gameObject.activeInHierarchy;
            killedBy = null;

            //người chơi đã bị giết
            if (!isActive)
            {
                //tìm game object gửi ban đầu (killedBy)
                PhotonView senderView = senderId > 0 ? PhotonView.Find(senderId) : null;
                if (senderView != null && senderView.gameObject != null) killedBy = senderView.gameObject;

                //phát hiện xem người dùng hiện tại có chịu trách nhiệm cho mạng tiêu diệt này không, nhưng không phải tự sát
                //đúng, đó là mạng tiêu diệt của tôi: tăng bộ đếm mạng tiêu diệt tại địa phương
                if (this != GameManager.GetInstance().localPlayer && killedBy == GameManager.GetInstance().localPlayer.gameObject)
                {
                    GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
                    GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
                }

                if (explosionFX)
                {
                    //tạo các hạt hiệu ứng chết tại địa phương bằng cách sử dụng pooling và tô màu chúng theo màu đội của người chơi
                    GameObject particle = PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
                    ParticleColor pColor = particle.GetComponent<ParticleColor>();
                    if (pColor) pColor.SetColor(GameManager.GetInstance().teams[GetView().GetTeam()].material.color);
                }

                //phát clip âm thanh khi người chơi chết
                if (explosionClip) AudioManager.Play3D(explosionClip, transform.position);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                //gửi người chơi trở lại khu vực đội, điều này sẽ bị ghi đè bởi vị trí chính xác từ chính máy khách sau đó
                //chúng ta làm điều này để tránh việc người chơi "đột ngột hiện ra" từ vị trí họ đã chết và sau đó dịch chuyển đến khu vực đội ngay lập tức
                //điều này đang thao túng bộ đệm PhotonTransformView nội bộ để cập nhật biến networkPosition
                GetComponent<PhotonTransformView>().OnPhotonSerializeView(new PhotonStream(false, new object[] { GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam()),
                                                                                                                 Vector3.zero, Quaternion.identity }), new PhotonMessageInfo());
            }

            //các thay đổi tiếp theo chỉ ảnh hưởng đến máy khách cục bộ
            if (!photonView.IsMine)
                return;

            //người chơi cục bộ đã được hồi sinh nên hãy đặt lại các trạng thái
            if (isActive == true)
                ResetPosition();
            else
            {
                //người chơi cục bộ đã bị tiêu diệt, hãy đặt camera theo dõi kẻ giết người
                if (killedBy != null) camFollow.target = killedBy.transform;
                //ẩn các điều khiển đầu vào và các phần tử HUD khác
                camFollow.HideMask(true);
                //hiển thị cửa sổ hồi sinh (chỉ dành cho người chơi cục bộ)
                GameManager.GetInstance().DisplayDeath();
            }
        }


        /// <summary>
        /// Lệnh thông báo cho server và tất cả những người khác rằng máy khách này đã sẵn sàng để hồi sinh.
        /// Điều này diễn ra khi độ trễ hồi sinh đã hết hoặc một quảng cáo video đã được xem.
        /// </summary>
        public void CmdRespawn()
        {
            this.photonView.RPC("RpcRespawn", RpcTarget.AllViaServer, (short)0);
        }


        /// <summary>
        /// Định vị lại trong khu vực đội và đặt lại camera cũng như các biến đầu vào.
        /// Việc này chỉ nên được gọi cho người chơi cục bộ.
        /// </summary>
        public void ResetPosition()
        {
            //bắt đầu theo dõi lại người chơi cục bộ
            camFollow.target = turret;
            camFollow.HideMask(false);

            //lấy khu vực đội và định vị lại ở đó
            transform.position = GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam());

            //đặt lại các đầu vào còn sót lại
            moveInput = Vector2.zero;
            viewInput = Vector2.zero;
            shootInput = false;
            //cũng trên các điều khiển joystick
            GameManager.GetInstance().ui.controls[0].OnPointerUp(null);
            GameManager.GetInstance().ui.controls[1].OnPointerUp(null);

            //đặt lại các lực bị thay đổi bởi đầu vào
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }


        //được gọi trên tất cả các máy khách khi kết thúc trò chơi cung cấp đội chiến thắng
        [PunRPC]
        protected void RpcGameOver(byte teamIndex)
        {
            //display game over window
            GameManager.GetInstance().DisplayGameOver(teamIndex);
        }
    }
}