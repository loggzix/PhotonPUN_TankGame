/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System.Collections.Generic;
using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Lớp này cung cấp chức năng pooling đối tượng qua mạng và lưu trữ tất cả các tham chiếu Pool.
    /// Việc tạo (spawning) và hủy (despawning) được xử lý bằng cách gọi các phương thức tương ứng, nhưng cũng có
    /// các phương thức để tạo Pool trong thời gian chạy hoặc hủy hoàn toàn tất cả các gameobject trong một Pool.
    /// </summary>
    public class PoolManager : MonoBehaviour 
    {
        //ánh xạ từ prefab đến Pool container quản lý tất cả các instance của nó
        private static Dictionary<GameObject, Pool> Pools = new Dictionary<GameObject, Pool>();

        
        /// <summary>
        /// Được gọi bởi mỗi Pool, cái này thêm nó vào từ điển (dictionary).
        /// </summary>
        public static void Add(Pool pool) 
        {
            //kiểm tra xem Pool có chứa prefab hay không
            if (pool.prefab == null)
            {
                Debug.LogError("Prefab of pool: " + pool.gameObject.name + " is empty! Can't add pool to Pools Dictionary.");
                return;
            }
            
            //kiểm tra xem Pool đã được thêm vào chưa
            if(Pools.ContainsKey(pool.prefab))    
            {
                Debug.LogError("Pool with prefab " + pool.prefab.name + " has already been added to Pools Dictionary.");
                return;
            }

            //thêm vào từ điển
            Pools.Add(pool.prefab, pool);
        }


        /// <summary>
        /// Tạo một Pool mới trong thời gian chạy. Cái này được gọi cho các prefab chưa được liên kết
        /// với một Pool trong scene trong editor, nhưng vẫn được gọi qua Spawn().
        /// </summary>
        public static void CreatePool(GameObject prefab, int preLoad, bool limit, int maxCount)
        {
            //lỗi debug nếu pool đã được thêm vào trước đó
            if (Pools.ContainsKey(prefab))
            {
                Debug.LogError("Pool Manager already contains Pool for prefab: " + prefab.name);
                return;
            }

            //tạo gameobject mới sẽ giữ thành phần Pool mới
            GameObject newPoolGO = new GameObject("Pool " + prefab.name);
            //thêm thành phần Pool vào gameobject mới trong scene
            Pool newPool = newPoolGO.AddComponent<Pool>();
            //gán các tham số mặc định
            newPool.prefab = prefab;
            newPool.preLoad = preLoad;
            newPool.limit = limit;
            newPool.maxCount = maxCount;
            //để nó tự khởi tạo sau khi gán các biến
            newPool.Awake();
        }
        
        
        /// <summary>
        /// Kích hoạt một instance đã được khởi tạo trước cho prefab được truyền vào, tại vị trí mong muốn.
        /// </summary>
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            //ghi Log trong trường hợp không tìm thấy prefab trong Pool
            //điều này không nghiêm trọng vì sau đó chúng ta sẽ tạo một Pool mới cho nó trong thời gian chạy
            if(!Pools.ContainsKey(prefab))
            {
                Debug.Log("Prefab not found in existing pool: " + prefab.name + " New Pool has been created.");
                CreatePool(prefab, 0, false, 0);
            }
            
            //tạo instance trong Pool tương ứng
            return Pools[prefab].Spawn(position, rotation);
        }
        
        
        /// <summary>
        /// Vô hiệu hóa một instance đã được tạo ra trước đó để sử dụng sau.
        /// Tùy chọn nhận một giá trị thời gian để trì hoãn quy trình despawn.
        /// </summary>
        public static void Despawn(GameObject instance, float time = 0f)
        {
            if(time > 0) GetPool(instance).Despawn(instance, time);
            else GetPool(instance).Despawn(instance);
        }


        /// <summary>
        /// Phương thức tiện ích để tra cứu nhanh một đối tượng được pool.
        /// Trả về thành phần Pool nơi tìm thấy instance.
        /// </summary>
        public static Pool GetPool(GameObject instance)
        {
            //duyệt qua các Pool và tìm instance
            foreach (GameObject prefab in Pools.Keys)
            {
                if(Pools[prefab].active.Contains(instance))
                    return Pools[prefab];
            }
            
            //không thể tìm thấy instance trong bất kỳ Pool nào
            Debug.LogError("PoolManager couldn't find Pool for instance: " + instance.name);
            return null;
        }


        /// <summary>
        /// Vô hiệu hóa tất cả các instance của một Pool, làm cho chúng sẵn sàng để sử dụng sau.
        /// </summary>
        public static void DeactivatePool(GameObject prefab)
        {
            //lỗi debug nếu Pool chưa được thêm vào trước đó
            if (!Pools.ContainsKey(prefab))
            {
                Debug.LogError("PoolManager couldn't find Pool for prefab to deactivate: " + prefab.name);
                return;
            }

            //lưu tạm số lượng đang hoạt động
            int count = Pools[prefab].active.Count;
            //lặp qua từng instance đang hoạt động
            for (int i = count - 1; i > 0; i--)
            {
                Pools[prefab].Despawn(Pools[prefab].active[i]);
            }
        }


        /// <summary>
        /// Hủy tất cả các instance đã despawn của tất cả các Pool để giải phóng bộ nhớ.
        /// Tham số 'limitToPreLoad' quyết định xem chỉ các instance vượt quá giá trị preload
        /// mới nên bị hủy, để giữ lại một lượng tối thiểu các instance bị vô hiệu hóa
        /// </summary>
        public static void DestroyAllInactive(bool limitToPreLoad)
        {
            foreach (GameObject prefab in Pools.Keys)
                Pools[prefab].DestroyUnused(limitToPreLoad);
        }
        
        
        /// <summary>
        /// Hủy Pool cho một prefab cụ thể.
        /// Các instance đang hoạt động hoặc không hoạt động sẽ không còn khả dụng sau khi gọi phương thức này.
        /// </summary>
        public static void DestroyPool(GameObject prefab)
        {
            //debug error if Pool wasn't already added before
            if (!Pools.ContainsKey(prefab))
            {
                Debug.LogError("PoolManager couldn't find Pool for prefab to destroy: " + prefab.name);
                return;
            }

            //hủy pool gameobject bao gồm tất cả các con. Logic game của chúng ta không thay đổi cha của các instance,
            //nhưng nếu bạn làm vậy, bạn nên lặp qua các instance đang hoạt động và không hoạt động một cách thủ công để hủy chúng
            Destroy(Pools[prefab].gameObject);
            //xóa cặp key-value khỏi từ điển
            Pools.Remove(prefab);
        }
        
        
        /// <summary>
        /// Hủy tất cả các Pool được lưu trữ trong từ điển của manager.
        /// Các instance đang hoạt động hoặc không hoạt động sẽ không còn khả dụng sau khi gọi phương thức này.
        /// </summary>
        public static void DestroyAllPools()
        {
            //lặp qua từ điển và hủy mọi pool gameobject
            //xem phương thức DestroyPool để biết thêm các bình luận khác
            foreach (GameObject prefab in Pools.Keys)
                DestroyPool(Pools[prefab].gameObject);
        }
        
        
        //các biến tĩnh luôn giữ giá trị của chúng khi chuyển đổi scene
        //vì vậy chúng ta cần đặt lại chúng khi trò chơi kết thúc hoặc chuyển đổi scene
        void OnDestroy()
        {
            Pools.Clear();
        }
    }
}