/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TanksMP
{
    /// <summary>
    /// Script này được gắn vào một gameobject được tạo trong lúc chạy trong game scene,
    /// được mang sang intro scene để yêu cầu bắt đầu ngay một trò chơi multiplayer mới.
    /// </summary>
    public class UIRestartButton : MonoBehaviour 
    {
        //lắng nghe các thay đổi scene
        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        
        //cho scene một chút thời gian để khởi tạo
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Invoke("EnterPlay", 0.5f);
        }
        
        
        //gọi nút play ngay lập tức khi load scene
        //tự hủy sau khi sử dụng
        void EnterPlay()
        {
            FindAnyObjectByType<UIMain>().Play();
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }
}
