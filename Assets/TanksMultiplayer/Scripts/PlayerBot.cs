/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

namespace TanksMP
{          
    /// <summary>
    /// Triển khai các bot AI bằng cách ghi đè (override) các phương thức của lớp Player.
    /// </summary>
	public class PlayerBot : Player
    {
        //các thuộc tính tùy chỉnh (custom properties) cho mỗi PhotonPlayer không hoạt động trong chế độ offline
        //(thực tế chúng có hoạt động, nhưng đối với các đối tượng được spawn bởi master client,
        //PhotonPlayer luôn là local master client. Điều này có nghĩa là việc
        //thiết lập các thuộc tính người chơi tùy chỉnh sẽ áp dụng cho tất cả các đối tượng)
        [HideInInspector] public string myName;
        [HideInInspector] public int teamIndex;
        [HideInInspector] public int health;
        [HideInInspector] public int shield;
        [HideInInspector] public int ammo;
        [HideInInspector] public int currentBullet;

        /// <summary>
        /// Bán kính tính bằng đơn vị để phát hiện những người chơi khác.
        /// </summary>
        public float range = 6f;

        //danh sách những người chơi kẻ thù nằm trong phạm vi của bot này
        private List<GameObject> inRange = new List<GameObject>();

        //tham chiếu đến thành phần agent (NavMeshAgent)
        private NavMeshAgent agent;

        //điểm đến hiện tại trên navigation mesh
        private Vector3 targetPoint;

        //dấu thời gian (timestamp) khi phát bắn tiếp theo sẽ xảy ra
        private float nextShot;

        //công tắc bật/tắt cho logic cập nhật (update)
        private bool isDead = false;
        
        
        //được gọi trước khi cập nhật SyncVar
        void Start()
        {           
            //lấy các thành phần và thiết lập mục tiêu camera
            camFollow = Camera.main.GetComponent<FollowTarget>();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;

            //lấy đội tương ứng và tô màu các renderer theo màu đội
            targetPoint = GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam());
            agent.Warp(targetPoint);

            Team team = GameManager.GetInstance().teams[GetView().GetTeam()];
            for(int i = 0; i < renderers.Length; i++)
                renderers[i].material = team.material;
            
			//đặt tên trong nhãn (label)
            label.text = myName = "Bot" + System.String.Format("{0:0000}", Random.Range(1, 9999));
            //gọi các hook thủ công để cập nhật
            OnHealthChange(GetView().GetHealth());
            OnShieldChange(GetView().GetShield());

            //bắt đầu quy trình phát hiện kẻ thù
            StartCoroutine(DetectPlayers());
        }
        
        
        //thiết lập danh sách inRange để phát hiện người chơi
        IEnumerator DetectPlayers()
        {
            //đợi khởi tạo xong
            yield return new WaitForEndOfFrame();
            
            //logic phát hiện
            while(true)
            {
                //xóa danh sách trong mỗi lần lặp (iteration)
                inRange.Clear();

                //bắn một hình cầu (sphere) để phát hiện các đối tượng người chơi khác trong bán kính hình cầu
                Collider[] cols = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Player"));
                //lặp qua các người chơi được tìm thấy trong bán kính của bot
                for (int i = 0; i < cols.Length; i++)
                {
                    //lấy thành phần Player khác
                    //chỉ thêm người chơi vào danh sách nếu họ không cùng đội này
                    Player p = cols[i].gameObject.GetComponent<Player>();
                    if(p.GetView().GetTeam() != GetView().GetTeam() && !inRange.Contains(cols[i].gameObject))
                    {
                        inRange.Add(cols[i].gameObject);   
                    }
                }
                
                //đợi một giây trước khi thực hiện kiểm tra phạm vi tiếp theo
                yield return new WaitForSeconds(1);
            }
        }
        
        
        //tính toán điểm ngẫu nhiên để di chuyển trên navigation mesh
        private void RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            //xóa điểm mục tiêu trước đó
            result = Vector3.zero;
            
            //thử tìm một điểm hợp lệ trên navmesh với giới hạn tối đa (10 lần)
            for (int i = 0; i < 10; i++)
            {
                //tìm một điểm trong bán kính di chuyển
                Vector3 randomPoint = center + (Vector3)Random.insideUnitCircle * range;
                randomPoint.y = 0;
                NavMeshHit hit;

                //nếu điểm tìm thấy là điểm mục tiêu hợp lệ, hãy thiết lập nó và tiếp tục
                if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas)) 
                {
                    result = hit.position;
                    break;
                }
            }
            
            //thiết lập điểm mục tiêu làm điểm đến mới
            agent.SetDestination(result);
        }
        
        
        void FixedUpdate()
        {
            //không thực hiện gì nếu trò chơi đã kết thúc,
            //nhưng chấm dứt agent và các quy trình tìm đường (path finding)
            if(GameManager.GetInstance().IsGameOver())
            {
                agent.isStopped = true;
                StopAllCoroutines();
                enabled = false;
                return;
            }
            
            //không tiếp tục nếu bot này được đánh dấu là đã chết
            if(isDead) return;

            //trực quan hóa chỉ số (stat visualization) không tự động cập nhật
            OnHealthChange(health);
            OnShieldChange(shield);

            //không có người chơi kẻ thù nào trong phạm vi
            if(inRange.Count == 0)
            {
                //nếu bot này đã đạt đến điểm ngẫu nhiên trên navigation mesh,
                //thì tính toán một điểm ngẫu nhiên khác trên navmesh để tiếp tục di chuyển xung quanh
                //khi không có người chơi khác trong phạm vi, AI sẽ đi lang thang từ điểm spawn của đội này sang điểm spawn của đội kia
                if(Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
                {
                    int teamCount = GameManager.GetInstance().teams.Length;
                    RandomPoint(GameManager.GetInstance().teams[Random.Range(0, teamCount)].spawn.position, range, out targetPoint);
                }
            }
            else
            {
                //nếu chúng ta đã đạt đến điểm mục tiêu, hãy tính toán một điểm mới xung quanh kẻ thù
                //điều này mô phỏng chuyển động "nhảy múa" trôi chảy hơn để tránh bị bắn trúng dễ dàng
                if(Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
                {
                    RandomPoint(inRange[0].transform.position, range * 2, out targetPoint);
                }
                
                //vòng lặp bắn súng
                for(int i = 0; i < inRange.Count; i++)
                {
                    RaycastHit hit;
                    //raycast để phát hiện kẻ thù hữu hình và bắn vào vị trí hiện tại của họ
                    if (Physics.Linecast(transform.position, inRange[i].transform.position, out hit))
                    {
                        //lấy vị trí kẻ thù hiện tại và xoay turret này
                        Vector3 lookPos = inRange[i].transform.position;
                        turret.LookAt(lookPos);
                        turret.eulerAngles = new Vector3(0, turret.eulerAngles.y, 0);
                        turretRotation = (short)turret.eulerAngles.y;

                        //tìm hướng bắn và bắn vào đó
                        Vector3 shotDir = lookPos - transform.position;
                        Shoot(new Vector2(shotDir.x, shotDir.z));
                        break;
                    }
                }
            }
        }

        
        /// <summary>
        /// Ghi đè phương thức cơ sở để xử lý respawn của bot một cách riêng biệt.
        /// </summary>
        [PunRPC]
        protected override void RpcRespawn(short senderId)
        {
            StartCoroutine(Respawn(senderId));
        }
        
        
        //quy trình respawn thực tế
        IEnumerator Respawn(short senderId)
        {   
            //dừng cập nhật AI
            isDead = true;
            inRange.Clear();
            agent.isStopped = true;
            killedBy = null;

            //tìm game object của người gửi ban đầu (killedBy)
            PhotonView senderView = senderId > 0 ? PhotonView.Find(senderId) : null;
            if (senderView != null && senderView.gameObject != null) killedBy = senderView.gameObject;

            //phát hiện xem người dùng hiện tại có chịu trách nhiệm cho lượt hạ gục này không
            //đúng, đó là lượt hạ gục của tôi: tăng bộ đếm lượt hạ gục địa phương
            if (killedBy == GameManager.GetInstance().localPlayer.gameObject)
            {
                GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
                GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
            }

            if (explosionFX)
            {
			     //spawn các hạt hiệu ứng khi chết cục bộ bằng pooling và tô màu chúng theo màu đội của người chơi
                 GameObject particle = PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
                 ParticleColor pColor = particle.GetComponent<ParticleColor>();
                 if(pColor) pColor.SetColor(GameManager.GetInstance().teams[GetView().GetTeam()].material.color);
            }
				
			//phát đoạn âm thanh khi người chơi hy sinh
            if(explosionClip) AudioManager.Play3D(explosionClip, transform.position);

            //tắt hiển thị cho tất cả các phần dựng hình (rendering)
            ToggleComponents(false);
            //đợi thời gian trễ respawn toàn cục cho đến khi tái kích hoạt
            yield return new WaitForSeconds(GameManager.GetInstance().respawnTime);
            //bật lại hiển thị (on)
            ToggleComponents(true);

            //respawn và tiếp tục tìm đường
            targetPoint = GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam());
            transform.position = targetPoint;
            agent.Warp(targetPoint);
            agent.isStopped = false;
            isDead = false;
        }


        //vô hiệu hóa các thành phần dựng hình hoặc chặn đường (blocking)
        void ToggleComponents(bool state)
        {
            GetComponent<Rigidbody>().isKinematic = state;
            GetComponent<Collider>().enabled = state;

            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(state);
        }
    }
}
