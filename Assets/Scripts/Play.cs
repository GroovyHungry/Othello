// using UnityEngine;

// public static class PlayerPrefsAutoClear
// {
//     // Play モードでシーン読み込み前に呼ばれる
//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     static void ClearOnPlay()
//     {
//         PlayerPrefs.DeleteAll();
//         PlayerPrefs.Save();
//         Debug.Log("▶ PlayerPrefs を自動クリアしました");
//     }
// }