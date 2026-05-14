/*  File này là một phần của dự án "Tanks Multiplayer" của FLOBUK.
 *  Bạn chỉ được phép sử dụng các tài nguyên này nếu bạn đã mua chúng từ Unity Asset Store.
 * 	Bạn không được cấp phép, cấp phép con, bán, bán lại, chuyển nhượng, chỉ định, phân phối hoặc
 * 	cung cấp Dịch vụ hoặc Nội dung cho bất kỳ bên thứ ba nào. */

using UnityEngine;
using UnityEditor;

namespace TanksMP
{
    // cửa sổ editor giới thiệu/trợ giúp/hỗ trợ của chúng ta
    public class AboutEditor : EditorWindow
    {
        [MenuItem("Window/Tanks Multiplayer/About")]
        static void Init()
        {
            AboutEditor aboutWindow = (AboutEditor)EditorWindow.GetWindowWithRect
                    (typeof(AboutEditor), new Rect(0, 0, 300, 300), false, "About");
            aboutWindow.Show();
        }

        void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(70);
            GUILayout.Label("Tanks Multiplayer", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
    
            GUILayout.BeginHorizontal();
            GUILayout.Space(70);
            GUILayout.Label("by FLOBUK");
            GUILayout.EndHorizontal();        
            GUILayout.Space(20);

            GUILayout.Label("Info", EditorStyles.boldLabel);
            if (GUILayout.Button("Homepage"))
            {
                Help.BrowseURL("https://flobuk.com");
            }
            GUILayout.Space(5);

            GUILayout.Label("Support", EditorStyles.boldLabel);
            if (GUILayout.Button("Online Documentation"))
            {
                Help.BrowseURL("https://flobuk.gitlab.io/assets/docs/unity/tanksmp");
            }
            if (GUILayout.Button("Scripting Reference"))
            {
                Help.BrowseURL("https://flobuk.gitlab.io/assets/docs/unity/tanksmp/api/");
            }
            if (GUILayout.Button("Support Forum"))
            {
                Help.BrowseURL("https://forum.unity3d.com/threads/410465/");
            }
            GUILayout.Space(5);
            
            GUILayout.Label("Support me!", EditorStyles.boldLabel);
            if (GUILayout.Button("Review Asset"))
            {
                Help.BrowseURL("https://assetstore.unity.com/packages/slug/69172?aid=1011lGiF&pubref=editor_tanksmp");
            }
            if (GUILayout.Button("Donate"))
            {
                Help.BrowseURL("https://flobuk.com");
            }
        }
    }
}