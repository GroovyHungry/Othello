using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using AK.Wwise;

/// <summary>
/// コイントス演出を管理し，対CPUとPvPの先手決定を行うクラス
/// </summary>
public class CoinTossManager : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static CoinTossManager Instance;

    /// <summary>
    /// コイントスUIパネルのルートGameObject
    /// </summary>
    public GameObject panel;

    /// <summary>
    /// コインの回転アニメーションを制御するAnimator
    /// </summary>
    public Animator CoinToss;

    /// <summary>
    /// 白を選択するボタン
    /// </summary>
    public Button whiteButton;

    /// <summary>
    /// 黒を選択するボタン
    /// </summary>
    public Button blackButton;

    /// <summary>
    /// ゲーム開始後に表示するUIの親GameObject
    /// </summary>
    public GameObject UI;

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    public AK.Wwise.Event playLoopEvent;
    public AK.Wwise.Event stopLoopEvent;
    [SerializeField] private AK.Wwise.Event OnClick;

    /// <summary>
    /// 再生中のループID
    /// </summary>
    private uint loopPlayingId = AkSoundEngine.AK_INVALID_PLAYING_ID;

    /// <summary>
    /// ユーザーが選択を完了したかどうか
    /// </summary>
    private bool selected = false;

    /// <summary>
    /// ユーザーが選択した色 ("White" or "Black")
    /// </summary>
    private string userChoice;

    /// <summary>
    /// インスタンス設定とUIイベントリスナー登録を行う
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        whiteButton.onClick.AddListener(OnWhiteButtonClicked);
        blackButton.onClick.AddListener(OnBlackButtonClicked);
    }

    /// <summary>
    /// イベントリスナーの解除
    /// </summary>
    private void OnDestroy()
    {
        whiteButton.onClick.RemoveListener(OnWhiteButtonClicked);
        blackButton.onClick.RemoveListener(OnBlackButtonClicked);
    }

    /// <summary>
    /// ユーザーが白を選択したときの処理
    /// </summary>
    private void OnWhiteButtonClicked()
    {
        userChoice = "White";
        if (!selected)
        {
            // クリック音を鳴らす
            OnClick.Post(whiteButton.gameObject);
        }
        selected = true;

        // ボタン無効化
        whiteButton.interactable = false;
        blackButton.interactable = false;
        whiteButton.GetComponent<EventTrigger>().enabled = false;
        blackButton.GetComponent<EventTrigger>().enabled = false;
    }

    /// <summary>
    /// ユーザーが黒を選択したときの処理
    /// クリック音を鳴らし，ボタンを無効化してその選択を保持する
    /// </summary>
    private void OnBlackButtonClicked()
    {
        userChoice = "Black";
        if (!selected)
        {
            // クリック音を鳴らす
            AkSoundEngine.PostEvent("OnClick", blackButton.gameObject);
        }
        selected = true;

        // ボタン無効化
        whiteButton.interactable = false;
        blackButton.interactable = false;
        whiteButton.GetComponent<EventTrigger>().enabled = false;
        blackButton.GetComponent<EventTrigger>().enabled = false;
    }

    /// <summary>
    /// 対CPU用のコイントス演出を実行
    /// 正解判定後コイントス画面を無効化し，ゲーム開始UIを表示する
    /// プレイヤーが正解した場合先行となる
    /// </summary>
    public async UniTask StartCoinTossVsCPU()
    {
        // コイントス開始
        panel.SetActive(true);
        CoinToss.Play("Spinning");
        loopPlayingId = playLoopEvent.Post(gameObject);
        selected = false;

        // ユーザーの選択を待機
        await UniTask.WaitUntil(() => selected);

        // 結果をランダムに決定
        string result = Random.value < 0.5f ? "White" : "Black";
        CoinToss.SetTrigger(result == "White" ? "ShowWhite" : "ShowBlack");
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.8f));

        // ループ音停止
        stopLoopEvent.Post(gameObject);
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.0f));

        // CPUが白を担当するか決定
        bool correct = (userChoice == result);
        OthelloManager.Instance.isAIWhite = correct;

        // コイントス終了
        panel.SetActive(false);
        UI.SetActive(true);
    }

    /// <summary>
    /// PvP用のコイントス演出を実行し、先手を決定してゲームUIを表示する
    /// コイントスの結果の色が先行となる
    /// </summary>
    public async UniTask StartCoinTossPvP()
    {
        // コイントス開始
        panel.SetActive(true);
        whiteButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);
        CoinToss.Play("Spinning");
        loopPlayingId = playLoopEvent.Post(gameObject);

        // 結果をランダムに決定
        string result = Random.value < 0.5f ? "White" : "Black";
        CoinToss.SetTrigger(result == "White" ? "ShowWhite" : "ShowBlack");
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.8f));

        // ループ音停止
        stopLoopEvent.Post(gameObject);
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.0f));

        // 先手を設定
        OthelloManager.Instance.isWhiteTurn = (result == "White");

        // コイントス終了
        panel.SetActive(false);
        UI.SetActive(true);
    }
}
