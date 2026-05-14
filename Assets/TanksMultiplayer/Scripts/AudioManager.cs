/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TanksMP
{
    /// <summary>
    /// Xử lý việc phát nhạc nền, các clip 2D và 3D phát một lần (one-shot) trong trò chơi.
    /// Sử dụng PoolManager để kích hoạt các AudioSource 3D tại các vị trí mong muốn trong thế giới.
    /// </summary>
	public class AudioManager : MonoBehaviour
	{
        //tham chiếu đến instance của script này
		private static AudioManager instance;

        /// <summary>
        /// AudioSource để phát lại các đoạn nhạc dài.
        /// </summary>
		public AudioSource musicSource;

        /// <summary>
        /// AudioSource để phát lại các clip 2D phát một lần.
        /// </summary>
		public AudioSource audioSource;

        /// <summary>
        /// Prefab được khởi tạo để phát lại các clip 3D phát một lần.
        /// </summary>
        public GameObject oneShotPrefab;

        /// <summary>
        /// Mảng để lưu trữ các clip nhạc nền, để chúng có thể được
        /// tham chiếu trong PlayMusic() bằng cách truyền vào giá trị chỉ số của chúng.
        /// </summary>
        public AudioClip[] musicClips;
        

        // Thiết lập tham chiếu instance, nếu chưa được thiết lập,
        // và tiếp tục lắng nghe các thay đổi scene.
		void Awake()
		{
            if (instance != null)
                return;

            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
		}


        /// <summary>
        /// Trả về tham chiếu đến instance của script này.
        /// </summary>
		public static AudioManager GetInstance()
		{
			return instance;
		}


        // Dừng phát nhạc sau khi chuyển đổi scene. Để tiếp tục phát
        // nhạc trong scene mới, yêu cầu gọi lại PlayMusic().
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            musicSource.Stop();
        }


        /// <summary>
        /// Phát sound clip ở chế độ 2D trên nguồn âm thanh nền.
        /// Chỉ có thể có một clip nhạc phát cùng một lúc.
        /// Chỉ phát nhạc nếu người chơi đã bật nó trong cài đặt.
        /// </summary>
        public static void PlayMusic(int index)
        {
            instance.musicSource.clip = instance.musicClips[index];

            //cài đặt người dùng có thể đã vô hiệu hóa nguồn âm thanh
            if (instance.musicSource.enabled)
                instance.musicSource.Play();
        }


        /// <summary>
        /// Phát sound clip được truyền vào trong không gian 2D.
        /// </summary>
        public static void Play2D(AudioClip clip)
        {
            instance.audioSource.PlayOneShot(clip);
        }


        /// <summary>
        /// Phát sound clip được truyền vào trong không gian 3D, với pitch ngẫu nhiên tùy chọn (phạm vi 0-1).
        /// Tự động tạo một audio source để phát lại bằng PoolManager của chúng ta.
        /// </summary>
        public static void Play3D(AudioClip clip, Vector3 position, float pitch = 0f)
        {
            //hủy thực thi nếu clip không được thiết lập
            if (clip == null) return;
            //tính toán pitch ngẫu nhiên trong phạm vi xung quanh 1, lên hoặc xuống
            pitch = UnityEngine.Random.Range(1 - pitch, 1 + pitch);

            //kích hoạt audio gameobject mới từ pool
            GameObject audioObj = PoolManager.Spawn(instance.oneShotPrefab, position, Quaternion.identity);
            //lấy audio source để sử dụng sau này
            AudioSource source = audioObj.GetComponent<AudioSource>();
            
            //gán các thuộc tính, phát clip
            source.clip = clip;
            source.pitch = pitch;
            source.Play();
            
            //vô hiệu hóa audio gameobject khi clip dừng phát
            PoolManager.Despawn(audioObj, clip.length);
        }
    }
}

