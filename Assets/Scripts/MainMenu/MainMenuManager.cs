using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using AK.Wwise;

/// <summary>
/// メインメニューの表示・操作を管理し，各モード選択やシークレットキーワード入力を処理するクラス
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static MainMenuManager Instance;

    /// <summary>
    /// メニューパネルのルートGameObject
    /// </summary>
    public GameObject menuPanel;

    /// <summary>
    /// PvP対戦選択ボタン
    /// </summary>
    public Button pvpButton;

    /// <summary>
    /// CPU対戦選択ボタン
    /// </summary>
    public Button cpuButton;

    /// <summary>
    /// 設定パネル表示ボタン
    /// </summary>
    public Button settingButton;

    /// <summary>
    /// BGM選曲パネル表示ボタン
    /// </summary>
    public Button musicChangerButton;

    /// <summary>
    /// ゲーム終了ボタン
    /// </summary>
    public Button quietGameButton;

    /// <summary>
    /// メニューアニメーション制御用Animator
    /// </summary>
    public Animator menuAnimator;

    /// <summary>
    /// シークレットキーワード検出用バッファ
    /// </summary>
    private string inputBuffer = string.Empty;

    /// <summary>
    /// 最後にキー入力を受け付けた時間
    /// </summary>
    private float lastCharTime = -1f;

    /// <summary>
    /// キーワード入力の有効時間（秒）
    /// </summary>
    private const float bufferTimeout = 2f;

    /// <summary>
    /// シークレットアンロック用キーワード
    /// </summary>
    private const string unlockKeyword = "othello";

    /// <summary>
    /// クリック音再生用Wwiseイベント
    /// </summary>
    public AK.Wwise.Event onClick;

    /// <summary>
    /// 選択処理中フラグ
    /// </summary>
    private bool selected = false;

    /// <summary>
    /// インスタンス設定と初期リスナー設定を行う
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
    /// ボタンリスナー登録と初期フラグ設定を行う
    /// </summary>
    private void Start()
    {
        selected = false;
        cpuButton.onClick.AddListener(OnCpuButtonClicked);
        pvpButton.onClick.AddListener(OnPvpButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
        musicChangerButton.onClick.AddListener(OnBGMControllerButtonClicked);
        quietGameButton.onClick.AddListener(OnQuietGameButtonClicked);
    }

    /// <summary>
    /// ユーザー入力をチェックし、シークレットキーワードアンロックを処理する
    /// </summary>
    private void Update()
    {
        if (inputBuffer.Length > 0 && Time.time - lastCharTime > bufferTimeout)
        {
            inputBuffer = string.Empty;
        }

        foreach (char c in Input.inputString.ToLower())
        {
            if (char.IsLetter(c))
            {
                lastCharTime = Time.time;
                inputBuffer += c;
                if (inputBuffer.Length > unlockKeyword.Length)
                {
                    inputBuffer = inputBuffer.Substring(inputBuffer.Length - unlockKeyword.Length);
                }
                if (inputBuffer == unlockKeyword)
                {
                    PlayerPrefs.SetInt("Unlocked", 3);
                    PlayerPrefs.Save();
                    Debug.Log("Secret difficulty unlocked by keyword!");
                    inputBuffer = string.Empty;
                }
            }
        }
    }

    /// <summary>
    /// イベントリスナー解除を行う
    /// </summary>
    private void OnDestroy()
    {
        pvpButton.onClick.RemoveListener(OnPvpButtonClicked);
        cpuButton.onClick.RemoveListener(OnCpuButtonClicked);
        settingButton.onClick.RemoveListener(OnSettingButtonClicked);
        musicChangerButton.onClick.RemoveListener(OnBGMControllerButtonClicked);
        quietGameButton.onClick.RemoveListener(OnQuietGameButtonClicked);
    }

    /// <summary>
    /// ゲームをフェードアウトして終了する
    /// </summary>
    public async UniTask QuietGame()
    {
        await SceneTransition.Instance.PlayFadeOut(1.0f);
        Debug.Log("Quiet Game");
        Application.Quit();
    }

    /// <summary>
    /// ゲーム終了ボタンがクリックされたときの処理
    /// </summary>
    private void OnQuietGameButtonClicked()
    {
        DoubleCheck.Instance.OpenDoubleCheckPanel();
        onClick.Post(quietGameButton.gameObject);
    }

    /// <summary>
    /// PvP対戦ボタンがクリックされたときの処理
    /// </summary>
    private async void OnPvpButtonClicked()
    {
        if (!selected)
        {
            selected = true;
            onClick.Post(pvpButton.gameObject);
            DisableAllButtons();
            await OnModeSelected(false);
        }
    }

    /// <summary>
    /// CPU対戦ボタンがクリックされたときの処理
    /// </summary>
    private async void OnCpuButtonClicked()
    {
        if (!selected)
        {
            selected = true;
            onClick.Post(cpuButton.gameObject);
            DisableAllButtons();
            await OnModeSelected(true);
        }
    }

    /// <summary>
    /// 設定ボタンがクリックされたときの処理
    /// </summary>
    private void OnSettingButtonClicked()
    {
        settingButton.interactable = false;
        SettingManager.Instance.OpenSettingPanel();
        onClick.Post(settingButton.gameObject);
    }

    /// <summary>
    /// BGM選曲ボタンがクリックされたときの処理
    /// </summary>
    private void OnBGMControllerButtonClicked()
    {
        onClick.Post(musicChangerButton.gameObject);
        BGMController.Instance.OpenBGMControllerPanel();
    }

    /// <summary>
    /// 全ボタンを無効化するヘルパーメソッド
    /// いらない可能性あり
    /// </summary>
    private void DisableAllButtons()
    {
        pvpButton.interactable = false;
        cpuButton.interactable = false;
        settingButton.interactable = false;
        musicChangerButton.interactable = false;
        quietGameButton.interactable = false;
        pvpButton.GetComponent<EventTrigger>().enabled = false;
        cpuButton.GetComponent<EventTrigger>().enabled = false;
        settingButton.GetComponent<EventTrigger>().enabled = false;
        musicChangerButton.GetComponent<EventTrigger>().enabled = false;
        quietGameButton.GetComponent<EventTrigger>().enabled = false;
    }

    /// <summary>
    /// モード選択後のシーン遷移処理を行う
    /// </summary>
    private async UniTask OnModeSelected(bool isCPU)
    {
        OthelloManager.isAIOpponent = isCPU;
        menuAnimator.SetTrigger("Start");
        await UniTask.Delay(System.TimeSpan.FromSeconds(3.0f));
        await SceneTransition.Instance.Transition("OthelloBoard");
    }
}
