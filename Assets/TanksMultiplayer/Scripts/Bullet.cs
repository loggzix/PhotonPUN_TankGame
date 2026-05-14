/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

namespace TanksMP
{
    /// <summary>
    /// Script cho đạn của người chơi với logic va chạm/đánh trúng.
    /// </summary>
    public class Bullet : MonoBehaviourPun
    {
        /// <summary>
        /// Tốc độ di chuyển của đạn tính theo đơn vị.
        /// </summary>
        public float speed = 10;

        /// <summary>
        /// Sát thương gây ra cho người chơi bị trúng đạn.
        /// </summary>
        public int damage = 3;

        /// <summary>
        /// Độ trễ cho đến khi tự động hủy (despawn) khi không có gì bị bắn trúng.
        /// </summary>
        public float despawnDelay = 1f;

        /// <summary>
        /// Số lần nảy của đạn vào tường và các chướng ngại vật môi trường khác.
        /// </summary>
        public int bounce = 0;

        /// <summary>
        /// Số lượng người chơi tối đa mà viên đạn này có thể bắn trúng khi phát nổ.
        /// </summary>
        public int maxTargets = 1;

        /// <summary>
        /// Phạm vi trong đó vụ nổ gây sát thương cho các Người chơi khác.
        /// Khu vực này chỉ được kiểm tra nếu maxTargets lớn hơn 1.
        /// </summary>
        public float explosionRange = 1;

        /// <summary>
        /// Clip âm thanh phát ra khi một người chơi bị trúng đạn.
        /// </summary>
        public AudioClip hitClip;

        /// <summary>
        /// Clip âm thanh phát ra khi viên đạn này bị hủy.
        /// </summary>
        public AudioClip explosionClip;

        /// <summary>
        /// Đối tượng được tạo ra khi một người chơi bị trúng đạn.
        /// </summary>
        public GameObject hitFX;

        /// <summary>
        /// Đối tượng được tạo ra khi viên đạn này bị hủy.
        /// </summary>
        public GameObject explosionFX;

        //tham chiếu đến thành phần rigidbody
        private Rigidbody myRigidbody;
        //tham chiếu đến thành phần collider
        private SphereCollider sphereCol;
        //lưu số lượng nảy tối đa để khôi phục
        private int maxBounce;
        //lưu vị trí nảy cuối cùng để tính toán hướng tiếp theo. Thay vì sử dụng
        //vị trí viên đạn hiện tại khi va chạm, việc tính toán độ nảy từ vị trí
        //viên đạn trước đó sẽ cải thiện kết quả cho các viên đạn tốc độ cao vốn có thể bỏ qua các collider
        private Vector3 lastBouncePos;

        /// <summary>
        /// Gameobject người chơi đã bắn ra viên đạn này.
        /// </summary>
        [HideInInspector]
        public GameObject owner;

    
        //lấy tham chiếu các thành phần
        void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
            sphereCol = GetComponent<SphereCollider>();
            maxBounce = bounce;
        }


        //thiết lập vận tốc di chuyển ban đầu
        //Trên Host, thêm coroutine tự động hủy (despawn)
        void OnSpawn()
        {
            //đối với đạn nảy, chỉ lưu vị trí hiện tại ở lần sinh ra đầu tiên (vị trí tháp pháo)
            if (bounce == maxBounce)
                lastBouncePos = transform.position;

            myRigidbody.linearVelocity = speed * transform.forward;
            PoolManager.Despawn(gameObject, despawnDelay);
        }


        ///kiểm tra những gì đã bị trúng khi va chạm. Chỉ thực hiện các công việc không quan trọng của máy khách ở đây,
        //thậm chí không truy cập các biến người chơi hay bất cứ thứ gì tương tự. Phía server được tách riêng bên dưới
        void OnTriggerEnter(Collider col)
        {
            //lưu tạm gameobject tương ứng đã bị bắn trúng
            GameObject obj = col.gameObject;
            //thử lấy thành phần player từ gameobject đã va chạm
            Player player = obj.GetComponent<Player>();

            //chúng ta thực sự đã bắn trúng một người chơi
            //thực hiện các bước kiểm tra tiếp theo
            if (player != null)
            {
                //bỏ qua chính chúng ta & vô hiệu hóa sát thương đồng đội (cùng chỉ số đội)
                if (IsFriendlyFire(owner.GetComponent<Player>(), player)) return;

                //tạo clip âm thanh và hiệu ứng hạt khi trúng đạn
                if (hitFX) PoolManager.Spawn(hitFX, transform.position, Quaternion.identity);
                if (hitClip) AudioManager.Play3D(hitClip, transform.position);
            }
            else if (bounce > 0)
            {
                //người chơi không bị bắn trúng nhưng là thứ khác, và chúng ta vẫn còn một số lần nảy
                //tạo một tia ray chỉ về hướng viên đạn này hiện đang bay tới
                Ray ray = new Ray(lastBouncePos - transform.forward * 0.5f, transform.forward);
                RaycastHit hit;

                //thực hiện spherecast theo hướng bay, trên layer mặc định (default)
                if (Physics.SphereCast(ray, sphereCol.radius, out hit, Mathf.Infinity, 1 << 0))
                {
                    //bỏ qua nhiều va chạm, ví dụ: bên trong các collider
                    if (Vector3.Distance(transform.position, lastBouncePos) < 0.05f)
                    {
                        return;
                    }

                    //lưu điểm va chạm mới nhất
                    lastBouncePos = hit.point;
                    //giảm số lần nảy đi một
                    bounce--;

                    //thứ gì đó đã bị bắn trúng theo hướng viên đạn này đang bay tới
                    //lấy hướng phản xạ mới (nảy ra) của đối tượng va chạm
                    Vector3 dir = Vector3.Reflect(ray.direction, hit.normal);
                    //xoay viên đạn để hướng về hướng mới
                    transform.rotation = Quaternion.LookRotation(dir);
                    //gán lại vận tốc với hướng mới
                    OnSpawn();

                    //phát clip tại vị trí va chạm
                    if (hitClip) AudioManager.Play3D(hitClip, transform.position);
                    //thoát thực thi cho đến lần va chạm tiếp theo
                    return;
                }
            }

            //hủy gameobject
            PoolManager.Despawn(gameObject);

            //mã trước đó hoàn toàn không được đồng bộ với các máy khách, vì tất cả những gì các máy khách cần là
            //vị trí và hướng ban đầu của viên đạn để tính toán hành vi chính xác tương tự như vậy ở phía họ.
            //tại thời điểm này, hãy tiếp tục với các khía cạnh quan trọng của trò chơi chỉ trên server
            if (!PhotonNetwork.IsMasterClient) return;

            //tạo danh sách cho các người chơi bị ảnh hưởng bởi viên đạn này và thêm người chơi va chạm vào ngay lập tức,
            //chúng ta đã thực hiện kiểm tra xác nhận & sát thương đồng đội ở trên rồi
            List<Player> targets = new List<Player>();
            if(player != null) targets.Add(player);

            //trong trường hợp viên đạn này có thể bắn trúng nhiều hơn 1 mục tiêu, hãy thực hiện kiểm tra khu vực vật lý bổ sung
            if (maxTargets > 1)
            {
                //tìm tất cả các collider trong phạm vi đã chỉ định xung quanh viên đạn này, trên lớp Player
                Collider[] others = Physics.OverlapSphere(transform.position, explosionRange, 1 << 8);
                Player ownerPlayer = owner.GetComponent<Player>();

                //lặp qua tất cả các va chạm người chơi tìm thấy
                for (int i = 0; i < others.Length; i++)
                {
                    //lấy thành phần Player từ va chạm đó
                    Player other = others[i].GetComponent<Player>();
                    if (other == null || targets.Contains(other)) continue;

                    //một lần nữa, bỏ qua các viên đạn của chính mình và cả sát thương đồng đội, bây giờ được thực hiện độc quyền trên phía server
                    if (IsFriendlyFire(ownerPlayer, other)) continue;

                    //thêm thành phần Player này vào danh sách
                    //hủy bỏ trong trường hợp chúng ta đạt đến số lượng tối đa ngay bây giờ
                    targets.Add(other);
                    if (targets.Count == maxTargets)
                        break;
                }
            }

            //áp dụng sát thương đạn cho các người chơi bị va chạm
            for(int i = 0; i < targets.Count; i++)
                targets[i].TakeDamage(this);
        }


        //thiết lập hiệu ứng khi đạn biến mất và đặt lại các biến
        void OnDespawn()
        {
            //tạo clip và hiệu ứng hạt khi biến mất (despawn)
            if (explosionFX) PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
            if (explosionClip) AudioManager.Play3D(explosionClip, transform.position);

            //đặt lại các biến đã sửa đổi về trạng thái ban đầu
            myRigidbody.linearVelocity = Vector3.zero;
            myRigidbody.angularVelocity = Vector3.zero;
            bounce = maxBounce;
        }


        //phương thức kiểm tra sát thương đồng đội (cùng chỉ số đội).
        private bool IsFriendlyFire(Player origin, Player target)
        {
            //không kích hoạt sát thương khi va chạm với chính viên đạn của mình
            if (target.gameObject == owner || target.gameObject == null) return true;
            //thực hiện kiểm tra sát thương đồng đội thực tế trên cả hai chỉ số đội và xem chúng có khớp hay không
            else if (!GameManager.GetInstance().friendlyFire && origin.GetView().GetTeam() == target.GetView().GetTeam()) return true;

            //sát thương đồng đội đang tắt, viên đạn này sẽ gây sát thương
            return false;
        }
    }
}
