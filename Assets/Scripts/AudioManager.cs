using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;
using System.Linq;

/// <summary>
/// BGMの再生・停止および盤面状況に応じたRTPC値の更新を管理するクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static AudioManager Instance;

    /// <summary>
    /// Wwiseサウンドバンク
    /// </summary>
    public Bank othelloBank;

    /// <summary>
    /// 白と黒の駒差によって変化させるWwise RTPC
    /// </summary>
    public RTPC pieceDifferenceRTPC;

    /// <summary>
    /// ゲーム進行度（配置された駒の総数）によって変化させるWwiseRTPC
    /// </summary>
    public RTPC gameProgressRTPC;

    /// <summary>
    /// BGMの再生・停止させるWwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event playBGMEvent;
    [SerializeField] private AK.Wwise.Event stopBGMEvent;

    /// <summary>
    /// シングルトンインスタンスを設定し，シーン切り替え時も破棄されないようにする
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Wwiseバンクのロード，BGMの再生開始，メニューサウンドスイッチの設定を行う
    /// </summary>
    private void Start()
    {
        othelloBank.Load();
        PlayBGM();
        AkSoundEngine.SetSwitch("SceneType", "MainMenu", gameObject);
    }

    /// <summary>
    /// BGMの再生をトリガーするメソッド
    /// </summary>
    public void PlayBGM()
    {
        playBGMEvent.Post(gameObject);
    }

    /// <summary>
    /// 再生中のBGMを停止するメソッド
    /// </summary>
    public void StopBGM()
    {
        stopBGMEvent.Post(gameObject);
    }

    /// <summary>
    /// BGM1 のインタラクティブな変更をRTPCで制御するメソッド
    /// 白黒のコマ差 diff が -10 ~ 10 の範囲で合計７段階のBGMが選ばれる
    /// diff <= -10 : 白完全有利BGM
    /// -9 <= diff <= -7 : 白有利BGM
    /// -6 <= diff <= -4 : 白微有利BGM
    /// -3 <= diff <= 3 : 互角BGM
    /// 4 <= diff <= 6 : 黒微有利BGM
    /// 7 <= diff <= 9 : 黒有利BGM
    /// 10 <= diff : 黒完全有利BGM
    /// </summary>
    public void ChangeBGM_1()
    {
        if (SceneManager.GetActiveScene().name == "OthelloBoard")
        {
            int whiteCount = OthelloBoard.Instance.CountPieces(true);
            int blackCount = OthelloBoard.Instance.CountPieces(false);
            int diff = blackCount - whiteCount;
            float clamped = Mathf.Clamp(diff, -10, 10);
            pieceDifferenceRTPC.SetGlobalValue(clamped);
        }
    }

    /// <summary>
    /// BGM1 のインタラクティブな変更をRTPCで制御するメソッド
    /// ゲームの進行度 totalPieces（配置された駒の総数）で３段階のBGMが選ばれる
    /// 0 <= totalPieces <= 19 : 序盤BGM
    /// 20 <= totalPieces <= 39 : 中盤BGM
    /// 40 <= totalPieces <= 64 : 終盤BGM
    /// </summary>
    public void ChangeBGM_2()
    {
        if (SceneManager.GetActiveScene().name == "OthelloBoard")
        {
            int totalPieces = OthelloBoard.Instance.CountPieces(true) + OthelloBoard.Instance.CountPieces(false);
            gameProgressRTPC.SetGlobalValue(totalPieces);
        }
    }

    /// <summary>
    /// シーンに合わせてBGMを変更するメソッド
    /// </summary>
    /// <param name="trackName">切り替える シーンの名前がついたBGMのトラック名</param>
    public void TransitionBGM(string trackName)
    {
        AkSoundEngine.SetSwitch("SceneType", trackName, gameObject);
        ChangeBGM_1();
        ChangeBGM_2();
    }
}
