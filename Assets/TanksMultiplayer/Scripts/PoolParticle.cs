/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Khi được gắn vào một gameobject chứa ParticleSystems,
    /// xử lý việc tự động despawn sau thời lượng hạt tối đa.
    /// </summary>
    public class PoolParticle : MonoBehaviour 
    {
        /// <summary>
        /// Độ trễ trước khi despawn gameobject này. Được tính toán dựa trên thời lượng của ParticleSystem,
        /// nhưng có thể bị ghi đè bằng cách đặt giá trị lớn hơn 0.
        /// </summary>
        public float delay = 0f;
        
        //tham chiếu đến tất cả các thành phần ParticleSystem
        private ParticleSystem[] pSystems;
        
        
        //khởi tạo các biến
        void Awake()
        {
            pSystems = GetComponentsInChildren<ParticleSystem>();
            
            //không tiếp tục nếu độ trễ đã bị ghi đè
            //ngược lại tìm thời lượng hạt tối đa
            if(delay > 0) return;
            for(int i = 0; i < pSystems.Length; i++)
            {
                var main = pSystems[i].main;
                if(main.duration > delay)
                    delay = main.duration;
            }
        }
        
        
        //phát các hạt
        void OnSpawn()
        {
            //lặp qua các tham chiếu ParticleSystem và phát chúng
            //Unity dường như không tính toán một vòng lặp hạt mới khi
            //hạt được kích hoạt, vì vậy ở đây chúng ta cũng thêm một seed ngẫu nhiên cho nó
            for(int i = 0; i < pSystems.Length; i++)
            {
				pSystems[i].Stop();
                pSystems[i].randomSeed = (uint)Random.Range(0f, uint.MaxValue);
                pSystems[i].Play();
            }

            //thiết lập tự động despawn sau thời lượng phát
            PoolManager.Despawn(gameObject, delay);
        }
    }
}
