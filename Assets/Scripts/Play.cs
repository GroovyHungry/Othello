using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// プレイのデータをUnity上でのプレイ時削除する
/// ビルド版では使われない
/// </summary>
public static class PlayerPrefsAutoClear
{
    ///エディタでのPlay時にデータ削除
    #if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ClearOnPlayEditorOnly()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("▶ Unity エディタ上の Play 実行前に PlayerPrefs をクリアしました");
    }
    #endif
}
