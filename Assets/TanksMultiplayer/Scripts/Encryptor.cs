/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Mã hóa các chuỗi được truyền vào bằng cách sử dụng một khóa làm mờ (obfuscation key) đã thiết lập cũng như
    /// định danh duy nhất của thiết bị đang chạy. Được sử dụng để lưu trữ các giao dịch mua trong ứng dụng (IAP).
    /// </summary>
    public class Encryptor : MonoBehaviour
    {
        //tham chiếu đến instance của script này
        private static Encryptor instance;

        /// <summary>
        /// Liệu có nên bật mã hóa hay không.
        /// </summary>
        public bool encrypt = false;

        /// <summary>
        /// Khóa 56+8 bit để mã hóa chuỗi: 8 ký tự, không sử dụng các ký tự
        /// đặc biệt trong code (=.,? v.v.) và hãy chơi thử để đảm bảo khóa của bạn thực sự hoạt động!
        /// Trên Windows Phone, khóa này phải dài đúng 16 ký tự (128 bit).
        /// HÃY LƯU KHÓA NÀY Ở ĐÂU ĐÓ PHÍA BẠN, ĐỂ NÓ KHÔNG BỊ MẤT KHI CẬP NHẬT.
        /// </summary>
        public string secret = "abcd1234";


        //thiết lập tham chiếu
		void Awake()
		{
			instance = this;
		}


        /// <summary>
        /// Trả về tham chiếu đến instance của script này.
        /// </summary>
        public static Encryptor GetInstance()
        {
			return instance;
        }


        /// <summary>
        /// Mã hóa chuỗi dựa trên khóa bí mật + định danh thiết bị.
        /// </summary>
        public static string Encrypt(string toEncrypt)
        {
            //nếu mã hóa không được bật, chỉ cần trả lại chuỗi ban đầu
            if (!instance.encrypt) return toEncrypt;
            //đính kèm định danh thiết bị vào chuỗi mã hóa
            toEncrypt += SystemInfo.deviceUniqueIdentifier;
			
            #pragma warning disable 0219
            //chuyển đổi khóa bí mật và chuỗi đầu vào sang mảng byte
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(instance.secret);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            byte[] resultArray = null;
            #pragma warning restore 0219

            #if UNITY_WP8
                //DES không khả dụng trên windows phone, chúng ta sử dụng AesManaged thay thế
                AesManaged aes = new AesManaged();
                aes.Key = keyArray;
                ICryptoTransform cTransform = aes.CreateEncryptor();
                //hack 16 ký tự đầu tiên và đưa chúng xuống cuối để tránh đầu vào IV bị lỗi định dạng 75:                 Array.Resize(ref toEncryptArray, toEncryptArray.Length + 16);
                Array.Copy(toEncryptArray, 0, toEncryptArray, toEncryptArray.Length - 16, 16);
                resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            #elif (UNITY_ANDROID || UNITY_IPHONE)
                //tạo dịch vụ DES mới và thiết lập tất cả các thuộc tính cần thiết
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Key = keyArray;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;
                //tạo trình mã hóa DES
                ICryptoTransform cTransform = des.CreateEncryptor();
                //mã hóa mảng đầu vào, sau đó chuyển đổi lại thành chuỗi
                //và trả về chuỗi mã hóa cuối cùng (không thể đọc được)
                resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            #else
                keyArray = null;
                resultArray = toEncryptArray;
            #endif

            //trả về chuỗi đã mã hóa
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

		
        /// <summary>
        /// Giải mã chuỗi dựa trên khóa bí mật + định danh thiết bị.
        /// </summary>
        public static string Decrypt(string toDecrypt)
        {
            //nếu mã hóa không được bật, chỉ cần trả lại chuỗi ban đầu
            if (!instance.encrypt) return toDecrypt;
           
            #pragma warning disable 0219
            //chuyển đổi khóa bí mật và chuỗi đầu vào sang mảng byte
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(instance.secret);
            byte[] toDecryptArray = Convert.FromBase64String(toDecrypt);
            byte[] resultArray = null;
            #pragma warning restore 0219

            #if UNITY_WP8
                AesManaged aes = new AesManaged();
                aes.Key = keyArray;
                ICryptoTransform cTransform = aes.CreateDecryptor();
                resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                //hack mảng một lần nữa và đưa khối cuối cùng trở lại vị trí bắt đầu
                Array.Copy(resultArray, resultArray.Length - 16, resultArray, 0, 16);
                Array.Resize(ref resultArray, resultArray.Length - 16);
            #elif (UNITY_ANDROID || UNITY_IPHONE)
                //tạo dịch vụ DES mới và thiết lập tất cả các thuộc tính cần thiết
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Key = keyArray;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;
                //tạo trình giải mã DES
                ICryptoTransform cTransform = des.CreateDecryptor();
                //giải mã mảng đầu vào, sau đó chuyển đổi lại thành chuỗi
                //và trả về chuỗi giải mã cuối cùng (chuỗi gốc)
                resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
            #else
                keyArray = null;
                resultArray = toDecryptArray;
            #endif

            //loại bỏ định danh thiết bị và trả về chuỗi đã giải mã
            return (UTF8Encoding.UTF8.GetString(resultArray, 0, resultArray.Length))
				   .Replace(SystemInfo.deviceUniqueIdentifier, String.Empty);
        }
    }
}