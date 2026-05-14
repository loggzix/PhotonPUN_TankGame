/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Lớp con tương tác và được quản lý bởi PoolManager.
    /// Xử lý tất cả việc tạo/hủy nội bộ các instance đang hoạt động/không hoạt động.
    /// </summary>
    public class Pool : MonoBehaviour
    {
        /// <summary>
        /// Prefab để khởi tạo cho việc pooling.
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// Số lượng instance cần tạo khi bắt đầu trò chơi.
        /// </summary>
        public int preLoad = 0;

        /// <summary>
        /// Liệu việc tạo các instance mới có nên bị giới hạn trong thời gian chạy hay không.
        /// </summary>
        public bool limit = false;

        /// <summary>
        /// Số lượng instance tối đa được tạo, nếu tính năng giới hạn được bật.
        /// </summary>
        public int maxCount;

        /// <summary>
        /// Danh sách các prefab instance đang hoạt động cho pool này.
        /// </summary>  
        [HideInInspector]
        public List<GameObject> active = new List<GameObject>();

        /// <summary>
        /// Danh sách các prefab instance không hoạt động cho pool này.
        /// </summary>
        [HideInInspector]
        public List<GameObject> inactive = new List<GameObject>();


        /// <summary>
        /// Khởi tạo được gọi bởi PoolManager trên các pool được tạo trong thời gian chạy.
        /// </summary>
        public void Awake()
        {
            //không thể khởi tạo nếu không có prefab
            if (prefab == null) return;

            //thêm pool này vào từ điển của PoolManager
            PoolManager.Add(this);

            PreLoad();
        }


        /// <summary>
        /// Tải một lượng đối tượng nhất định trước khi bắt đầu chơi.
        /// </summary>
        public void PreLoad()
        {
            if (prefab == null)
            {
                Debug.LogWarning("Prefab trong pool bị trống! Không có Preload nào xảy ra. Vui lòng kiểm tra lại tham chiếu.");
                return;
            }

            //khởi tạo số lượng tải trước đã định nghĩa nhưng không vượt quá số lượng đối tượng tối đa
            for (int i = totalCount; i < preLoad; i++)
            {
                //khởi tạo instance mới của prefab
                GameObject obj = (GameObject)Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                //đặt instance mới làm con của transform này
                obj.transform.SetParent(transform);

                //đổi tên nó thành một tiêu đề duy nhất để dễ quản lý hơn trong editor
                Rename(obj.transform);
                //vô hiệu hóa đối tượng bao gồm cả các đối tượng con
                obj.SetActive(false);
                //thêm đối tượng vào danh sách các instance không hoạt động
                inactive.Add(obj);
            }
        }


        /// <summary>
        /// Kích hoạt (hoặc khởi tạo) một instance mới cho pool này.
        /// </summary>
        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            //khởi tạo các biến
            GameObject obj;
            Transform trans;

            //có các đối tượng không hoạt động sẵn sàng để kích hoạt
            if (inactive.Count > 0)
            {
                //lấy đối tượng không hoạt động đầu tiên trong danh sách
                obj = inactive[0];
                //chúng ta muốn kích hoạt nó, xóa nó khỏi danh sách không hoạt động
                inactive.RemoveAt(0);

                //lấy transform của instance
                trans = obj.transform;
            }
            else
            {
                //chúng ta không có bất kỳ đối tượng không hoạt động nào sẵn dùng,
                //chúng ta phải khởi tạo một cái mới
                //kiểm tra xem số lượng giới hạn có cho phép khởi tạo mới hay không
                //nếu không, không trả về gì cả
                if (limit && active.Count >= maxCount)
                    return null;

                //có thể khởi tạo, khởi tạo instance mới của prefab
                obj = (GameObject)Object.Instantiate(prefab);
                //lấy transform của instance
                trans = obj.transform;
                //đổi tên nó thành một tiêu đề duy nhất để dễ quản lý hơn trong editor
                Rename(trans);
            }

            //thiết lập vị trí và vòng xoay được truyền vào
            trans.position = position;
            trans.rotation = rotation;
            //trong trường hợp nó không phải là con của transform này, hãy đặt nó làm con ngay bây giờ
            if (trans.parent != transform)
                trans.parent = transform;

            //thêm đối tượng vào danh sách các instance đang hoạt động
            active.Add(obj);
            //kích hoạt đối tượng bao gồm cả các đối tượng con
            obj.SetActive(true);
            //gọi phương thức OnSpawn() trên mọi thành phần và đối tượng con của đối tượng này
            obj.BroadcastMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);

            //gửi đi instance
            return obj;
        }


        /// <summary>
        /// Vô hiệu hóa một instance của pool này để sử dụng sau.
        /// </summary>
        public void Despawn(GameObject instance)
        {
            //tìm kiếm instance này trong các instance đang hoạt động
            if (!active.Contains(instance))
            {
                Debug.LogWarning("Không thể despawn - Không tìm thấy instance: " + instance.name + " trong Pool " + this.name);
                return;
            }

            //trong trường hợp nó đã bị tách ra khỏi cha trong thời gian chạy, hãy đặt lại cha ngay bây giờ
            if (instance.transform.parent != transform)
                instance.transform.parent = transform;

            //chúng ta muốn vô hiệu hóa nó, xóa nó khỏi danh sách hoạt động
            active.Remove(instance);
            //thêm đối tượng vào danh sách các instance không hoạt động thay thế
            inactive.Add(instance);
            //gọi phương thức OnDespawn() trên mọi thành phần và đối tượng con của đối tượng này
            instance.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
            //vô hiệu hóa đối tượng bao gồm cả các đối tượng con
            instance.SetActive(false);
        }


        /// <summary>
        /// Vô hiệu hóa có thời gian một instance của pool này để sử dụng sau.
        /// </summary>
        public void Despawn(GameObject instance, float time)
        {
            //tạo lớp PoolTimeObject mới để theo dõi instance
            PoolTimeObject timeObject = new PoolTimeObject();
            //gán biến thời gian và instance của lớp này
            timeObject.instance = instance;
            timeObject.time = time;

            //bắt đầu vô hiệu hóa có thời gian bằng cách sử dụng các thuộc tính đã tạo
            StartCoroutine(DespawnInTime(timeObject));
        }


        //coroutine đợi 'time' giây trước khi vô hiệu hóa instance
        IEnumerator DespawnInTime(PoolTimeObject timeObject)
        {
            //lưu tạm instance cần vô hiệu hóa
            GameObject instance = timeObject.instance;

            //đợi số giây đã định nghĩa
            float timer = Time.time + timeObject.time;
            while (instance.activeInHierarchy && Time.time < timer)
                yield return null;

            //instance đã bị vô hiệu hóa trong lúc chờ rồi
            if (!instance.activeInHierarchy) yield break;
            //hủy nó ngay bây giờ (despawn)
            Despawn(instance);
        }


        /// <summary>
        /// Hủy tất cả các instance không hoạt động của pool này (nặng đối với bộ thu gom rác - garbage collector).
        /// Tham số xác định xem chỉ các instance vượt quá giá trị preLoad mới nên bị hủy.
        /// </summary>
        public void DestroyUnused(bool limitToPreLoad)
        {
            //chỉ hủy các instance vượt quá lượng giới hạn
            if (limitToPreLoad)
            {
                //bắt đầu từ instance không hoạt động cuối cùng và đếm ngược
                //cho đến khi chỉ số đạt đến lượng giới hạn
                for (int i = inactive.Count - 1; i >= preLoad; i--)
                {
                    //hủy đối tượng tại 'i'
                    Object.Destroy(inactive[i]);
                }
                //xóa phạm vi các đối tượng đã bị hủy (bây giờ là null) khỏi danh sách
                if (inactive.Count > preLoad)
                    inactive.RemoveRange(preLoad, inactive.Count - preLoad);
            }
            else
            {
                //limitToPreLoad là false, hủy tất cả các instance không hoạt động
                for (int i = 0; i < inactive.Count; i++)
                {
                    Object.Destroy(inactive[i]);
                }
                //đặt lại danh sách
                inactive.Clear();
            }
        }


        /// <summary>
        /// Hủy một số lượng cụ thể các instance không hoạt động (nặng đối với bộ thu gom rác).
        /// </summary>
        public void DestroyCount(int count)
        {
            //số lượng được truyền vào vượt quá số lượng instance không hoạt động
            if (count > inactive.Count)
            {
                Debug.LogWarning("Destroy Count value: " + count + " is greater than inactive Count: " +
                                inactive.Count + ". Destroying all available inactive objects of type: " +
                                prefab.name + ". Use DestroyUnused(false) instead to achieve the same.");
                DestroyUnused(false);
                return;
            }

            //bắt đầu từ cuối, đếm ngược chỉ số và hủy từng instance không hoạt động
            //cho đến khi chúng ta đã hủy đủ số lượng được truyền vào
            for (int i = inactive.Count - 1; i >= inactive.Count - count; i--)
            {
                Object.Destroy(inactive[i]);
            }
            //xóa phạm vi các đối tượng đã bị hủy (bây giờ là null) khỏi danh sách
            inactive.RemoveRange(inactive.Count - count, count);
        }


        //tạo một cái tên duy nhất cho mỗi instance khi khởi tạo
        //để phân biệt chúng với nhau trong editor
        private void Rename(Transform instance)
        {
            //đếm tổng số instance và gán số tự do tiếp theo
            //chuyển đổi nó trong phạm vi hàng trăm:
            //không nên có hàng nghìn instance cùng một lúc
            //vị dụ: TestEnemy(Clone)001
            instance.name += (totalCount + 1).ToString("#000");
        }


        //đếm tất cả các instance của tùy chọn pool này
        private int totalCount
        {
            get
            {
                //khởi tạo giá trị đếm
                int count = 0;
                //cộng số lượng đang hoạt động và không hoạt động
                count += active.Count;
                count += inactive.Count;
                //trả về kết quả đếm cuối cùng
                return count;
            }
        }


        //khi pool này bị hủy,
        //xóa sạch danh sách các instance
        void OnDestroy()
        {
            active.Clear();
            inactive.Clear();
        }
    }


    /// <summary>
    /// Lưu trữ các thuộc tính được sử dụng khi vô hiệu hóa có thời gian các instance.
    /// </summary>
    [System.Serializable]
    public class PoolTimeObject
    {
        /// <summary>
        /// Instance cần vô hiệu hóa.
        /// </summary>
        public GameObject instance;

        /// <summary>
        /// Độ trễ cho đến khi vô hiệu hóa.
        /// </summary>
        public float time;
    }
}