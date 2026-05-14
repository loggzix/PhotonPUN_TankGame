/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Sửa đổi màu bắt đầu của hệ thống hạt (particle system) thành màu được truyền vào.
    /// Điều này được sử dụng trên các hạt khi người chơi chết để khớp với màu của đội người chơi.
    /// </summary>
    public class ParticleColor : MonoBehaviour
    {
        /// <summary>
        /// Mảng các hệ thống hạt cần được tô màu.
        /// </summary>
        public ParticleSystem[] particles;

        /// <summary>
        /// Lặp qua tất cả các hạt và gán màu được truyền vào,
        /// nhưng bỏ qua giá trị alpha của màu mới.
        /// </summary>
        public void SetColor(Color color)
        {
            for(int i = 0; i < particles.Length; i++)
            {
                var main = particles[i].main;
                color.a = main.startColor.color.a;
                main.startColor = color;
            }
        }
    }
}