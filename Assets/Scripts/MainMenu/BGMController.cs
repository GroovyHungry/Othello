using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

/// <summary>
/// BGM選曲パネルの表示・操作と選択されたBGM番号に応じたRTPC値の更新を管理するクラス
/// </summary>
public class BGMController : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static BGMController Instance;

    /// <summary>
    /// Wwiseサウンドバンク
    /// </summary>
    public Bank othelloBank;

    /// <summary>
    /// BGM選曲パネルのルート GameObject
    /// </summary>
    public GameObject bgmControllerPanel;

    /// <summary>
    /// BGM番号を変更するボタン
    /// </summary>
    public Button selectRightButton; // 次へ
    public Button selectLeftButton; // 前へ

    /// <summary>
    /// 選択中のBGM番号を表示するImageコンポーネント
    /// </summary>
    public Image NumBox;

    /// <summary>
    /// BGM番号ごとの表示用スプライト配列
    /// </summary>
    public Sprite[] numSprites;

    /// <summary>
    /// BGM選曲パネルを閉じるボタン
    /// </summary>
    public Button bgmControllerCloseButton;

    /// <summary>
    /// 選択可能な最大BGM番号
    /// </summary>
    public int maxBGMNum = 1;

    /// <summary>
    /// 選択可能な最小BGM番号
    /// </summary>
    public int minBGMNum = 0;

    /// <summary>
    /// 現在選択中のBGM番号
    /// </summary>
    public static int BGMNum = 0;

    /// <summary>
    /// 選択されたBGM番号を管理するWwise RTPC
    /// </summary>
    public RTPC BGMNumRTPC;

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event SetFilter;
    [SerializeField] private AK.Wwise.Event ResetFilter;
    [SerializeField] private AK.Wwise.Event OnClick;

    /// <summary>
    /// インスタンス設定とUIイベントリスナーの登録を行う
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
        selectLeftButton.onClick.AddListener(SelectRight);
        selectRightButton.onClick.AddListener(SelectLeft);
        bgmControllerCloseButton.onClick.AddListener(CloseBGMController);

    /// <summary>
    /// パネルを非表示にし，Wwiseバンクをロードする
    /// </summary>
    }
    void Start()
    {
        bgmControllerPanel.SetActive(false);
        othelloBank.Load();
    }

    /// <summary>
    /// UIイベントリスナーの解除
    /// </summary>
    private void OnDestroy()
    {
        selectLeftButton.onClick.RemoveListener(SelectRight);
        selectRightButton.onClick.RemoveListener(SelectLeft);
        bgmControllerCloseButton.onClick.RemoveListener(CloseBGMController);
    }

    /// <summary>
    /// BGM選曲パネルを開き，現在のBGM番号を表示更新する
    /// </summary>
    public void OpenBGMControllerPanel()
    {
        SetFilter.Post(gameObject);
        UpdateBGMNum(BGMNum);
        bgmControllerPanel.SetActive(true);
    }

    /// <summary>
    /// 選択中のBGM番号をRTPCに設定する
    /// </summary>
    public void ChangeMusic()
    {
        BGMNumRTPC.SetGlobalValue(BGMNum);
    }

    /// <summary>
    /// BGM番号を増加させ，範囲外なら最小値に戻し，RTPCと表示を更新する
    /// </summary>
    private void SelectRight()
    {
        OnClick.Post(selectRightButton.gameObject);
        if (BGMNum == maxBGMNum)
        {
            BGMNum = 0;
        }
        else
        {
            BGMNum ++;
        }
        ChangeMusic();
        UpdateBGMNum(BGMNum);
    }

    /// <summary>
    /// BGM番号を減少させ，範囲外なら最大値に戻し，RTPCと表示を更新する
    /// </summary>
    private void SelectLeft()
    {
        OnClick.Post(selectLeftButton.gameObject);
        if (BGMNum == minBGMNum)
        {
            BGMNum = maxBGMNum;
        }
        else
        {
            BGMNum --;
        }
        ChangeMusic();
        UpdateBGMNum(BGMNum);
    }

    /// <summary>
    /// 指定されたBGM番号に対応するスプライトをNumBoxに適用する
    /// </summary>
    private void UpdateBGMNum(int BGMNum)
    {
        NumBox.sprite = numSprites[BGMNum];
    }

    /// <summary>
    /// BGM選曲パネルを閉じ、クリック音を再生する
    /// </summary>
    public void CloseBGMController()
    {
        ResetFilter.Post(gameObject);
        OnClick.Post(bgmControllerCloseButton.gameObject);
        bgmControllerPanel.SetActive(false);
    }
}
