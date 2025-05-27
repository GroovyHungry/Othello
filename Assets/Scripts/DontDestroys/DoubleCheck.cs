using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using AK.Wwise;

/// <summary>
/// ゲーム終了確認などのダイアログの表示・操作を管理し，ユーザーの選択に応じてシーン遷移や処理を行うクラス
/// </summary>
public class DoubleCheck : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static DoubleCheck Instance;

    /// <summary>
    /// 確認ダイアログを表示するパネル
    /// </summary>
    public GameObject doubleCheckPanel;

    /// <summary>
    /// 「はい」ボタン
    /// </summary>
    public Button yesButton;

    /// <summary>
    /// 「いいえ」ボタン
    /// </summary>
    public Button noButton;

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event SetFilter;
    [SerializeField] private AK.Wwise.Event ResetFilter;
    [SerializeField] private AK.Wwise.Event OnClick;

    /// <summary>
    /// インスタンスの設定を行う
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
    /// ボタンのイベントリスナーを登録する
    /// </summary>
    private void Start()
    {
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }

    /// <summary>
    /// イベントリスナーを解除する
    /// </summary>
    private void OnDestroy()
    {
        yesButton.onClick.RemoveListener(OnYesButtonClicked);
        noButton.onClick.RemoveListener(OnNoButtonClicked);
    }

    /// <summary>
    /// 確認ダイアログを開き、ゲーム進行を一時停止する
    /// </summary>
    public void OpenDoubleCheckPanel()
    {
        SetFilter.Post(gameObject);
        OthelloManager.Waiting = true;
        doubleCheckPanel.SetActive(true);
    }

    /// <summary>
    /// 「はい」が選択されたときの処理：クリック音再生後、パネルを閉じ、シーンに応じた終了処理を実行する
    /// </summary>
    public async void OnYesButtonClicked()
    {
        ResetFilter.Post(gameObject);
        OnClick.Post(yesButton.gameObject);
        doubleCheckPanel.SetActive(false);

        // 少し待ってから遷移演出などを入れる場合はここで Delay を挿入可能
        await UniTask.Yield();

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OthelloBoard")
        {
            await OthelloManager.Instance.ExitToMainMenu();
        }
        else if (sceneName == "MainMenu")
        {
            await MainMenuManager.Instance.QuietGame();
        }
    }

    /// <summary>
    /// 「いいえ」が選択されたときの処理：クリック音再生後、ダイアログを閉じてゲームを再開する
    /// </summary>
    public void OnNoButtonClicked()
    {
        ResetFilter.Post(gameObject);
        OnClick.Post(noButton.gameObject);
        OthelloManager.Waiting = false;
        doubleCheckPanel.SetActive(false);
    }
}
