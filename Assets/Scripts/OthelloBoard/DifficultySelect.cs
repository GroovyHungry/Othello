using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// 難易度選択パネルの表示・操作を行い，選択結果をCPUの強さに反映するクラス
/// </summary>
public class DifficultySelect : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static DifficultySelect Instance;

    /// <summary>
    /// 簡単モード選択ボタン
    /// </summary>
    public Button easyButton;

    /// <summary>
    /// 普通モード選択ボタン
    /// </summary>
    public Button normalButton;

    /// <summary>
    /// 難しいモード選択ボタン
    /// </summary>
    public Button hardButton;

    /// <summary>
    /// 隠しモードボタン
    /// </summary>
    public GameObject errorButton;

    /// <summary>
    /// 難易度選択パネルのルートGameObject
    /// </summary>
    public GameObject difficultySelectPanel;

    /// <summary>
    /// 通常モード用GameObject
    /// </summary>
    public GameObject normalBoard;

    /// <summary>
    /// 隠しモード用GameObject
    /// </summary>
    public GameObject brokenBoard;

    /// <summary>
    /// 選択完了フラグ
    /// </summary>
    private bool selected = false;

    /// <summary>
    /// 選択された難易度 ("easy", "normal", "hard", "secret")
    /// </summary>
    public static string difficulty = "easy";

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event OnClick;
    [SerializeField] private AK.Wwise.Event Noise;

    /// <summary>
    /// インスタンス設定を行う
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 初期化：アンロック状態に応じたボタン表示とリスナー登録
    /// </summary>
    private void Start()
    {
        int unlocked = PlayerPrefs.GetInt("Unlocked", 0);
        PlayerPrefs.Save();

        // 全ボタンを一旦非表示
        easyButton.gameObject.SetActive(false);
        normalButton.gameObject.SetActive(false);
        hardButton.gameObject.SetActive(false);
        errorButton.SetActive(false);

        // アンロック状況に応じて表示・リスナー設定
        if (unlocked >= 0)
        {
            easyButton.gameObject.SetActive(true);
            easyButton.onClick.AddListener(OnEasyButtonClicked);
        }
        if (unlocked >= 1)
        {
            normalButton.gameObject.SetActive(true);
            normalButton.onClick.AddListener(OnNormalButtonClicked);
        }
        if (unlocked >= 2)
        {
            hardButton.gameObject.SetActive(true);
            hardButton.onClick.AddListener(OnHardButtonClicked);
        }
        if (unlocked >= 3)
        {
            errorButton.SetActive(true);
        }
    }

    /// <summary>
    /// イベントリスナーの解除
    /// </summary>
    private void OnDestroy()
    {
        easyButton.onClick.RemoveListener(OnEasyButtonClicked);
        normalButton.onClick.RemoveListener(OnNormalButtonClicked);
        hardButton.onClick.RemoveListener(OnHardButtonClicked);
    }

    /// <summary>
    /// 簡単モード選択時
    /// </summary>
    private void OnEasyButtonClicked()
    {
        OnClick.Post(easyButton.gameObject);
        difficulty = "easy";
        selected = true;
    }

    /// <summary>
    /// 普通モード選択時のコールバック
    /// </summary>
    private void OnNormalButtonClicked()
    {
        OnClick.Post(normalButton.gameObject);
        difficulty = "normal";
        selected = true;
    }

    /// <summary>
    /// 難しいモード選択時のコールバック
    /// </summary>
    private void OnHardButtonClicked()
    {
        OnClick.Post(hardButton.gameObject);
        difficulty = "hard";
        selected = true;
    }

    /// <summary>
    /// 隠しモード選択時のコールバック
    /// </summary>
    public void OnSecretButtonClicked()
    {
        Noise.Post(gameObject);
        difficulty = "secret";
        normalBoard.SetActive(false);
        brokenBoard.SetActive(true);
        selected = true;
    }

    /// <summary>
    /// 難易度選択パネルを表示し，ユーザーが選択するまで待機する
    /// </summary>
    public async UniTask StartDifficultySelect()
    {
        difficultySelectPanel.SetActive(true);
        selected = false;
        await UniTask.WaitUntil(() => selected);
        OthelloAI.Instance.difficulty = difficulty;
        difficultySelectPanel.SetActive(false);
    }
}
