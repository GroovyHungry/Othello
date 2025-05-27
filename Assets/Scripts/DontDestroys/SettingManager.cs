using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

/// <summary>
/// 音量設定パネルの表示・操作の制御を行うクラス
/// </summary>
public class SettingManager : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static SettingManager Instance;

    /// <summary>
    /// 設定パネルのルートGameObject
    /// </summary>
    public GameObject settingPanel;

    /// <summary>
    /// マスター音量ミュートアイコン
    /// </summary>
    public GameObject muteMaster;

    /// <summary>
    /// BGM音量ミュートアイコン
    /// </summary>
    public GameObject muteBGM;

    /// <summary>
    /// SE音量ミュートアイコン
    /// </summary>
    public GameObject muteSE;

    /// <summary>
    /// マスター音量用スライダー
    /// </summary>
    public Slider masterSlider;

    /// <summary>
    /// BGM音量用スライダー
    /// </summary>
    public Slider bgmSlider;

    /// <summary>
    /// SE音量用スライダー
    /// </summary>
    public Slider seSlider;

    /// <summary>
    /// 設定パネルを閉じるボタン
    /// </summary>
    public Button settingCloseButton;

    /// <summary>
    /// マスター音量RTPC
    /// </summary>
    public RTPC masterVolumeRTPC;

    /// <summary>
    /// BGM 音量RTPC
    /// </summary>
    public RTPC bgmVolumeRTPC;

    /// <summary>
    /// SE音量RTPC
    /// </summary>
    public RTPC seVolumeRTPC;

    /// <summary>
    /// Wwiseイベント
    /// <summary>
    [SerializeField] private AK.Wwise.Event SetFilter;
    [SerializeField] private AK.Wwise.Event ResetFilter;
    [SerializeField] private AK.Wwise.Event OnClick;

    /// <summary>
    /// インスタンス設定およびスライダー初期値設定
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

        // デフォルトスライダー値
        masterSlider.value = 5;
        bgmSlider.value = 5;
        seSlider.value = 5;
    }

    /// <summary>
    /// UIイベントリスナー登録と初期表示設定，RTPC初期値反映
    /// </summary>
    private void Start()
    {
        // 初期非表示
        settingPanel.SetActive(false);

        // 保存値を読み込んでスライダーに反映（通知なし）
        masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MasterVolume", 5));
        bgmSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("BGMVolume", 5));
        seSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SEVolume", 5));

        // スライダー変更時コールバック登録
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);

        // クローズボタン登録
        SettingCloseButton.onClick.AddListener(CloseSetting);

        // 初期RTPC設定
        masterVolumeRTPC.SetGlobalValue(masterSlider.value * 10f);
        bgmVolumeRTPC.SetGlobalValue(bgmSlider.value * 10f);
        seVolumeRTPC.SetGlobalValue(seSlider.value * 10f);
    }

    /// <summary>
    /// UIイベントリスナー解除
    /// </summary>
    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        seSlider.onValueChanged.RemoveListener(OnSEVolumeChanged);
        SettingCloseButton.onClick.RemoveListener(CloseSetting);
    }

    /// <summary>
    /// 設定パネルを開き待機状態にする
    /// </summary>
    public void OpenSettingPanel()
    {
        SetFilter.Post(gameObject);
        settingPanel.SetActive(true);
        OthelloManager.Waiting = true;
    }

    /// <summary>
    /// 設定パネルを閉じる処理
    /// シーンに応じて処理を分岐
    /// </summary>
    public void CloseSetting()
    {
        ResetFilter.Post(gameObject);
        OnClick.Post(SettingCloseButton.gameObject);
        settingPanel.SetActive(false);
        if (MainMenuManager.Instance != null && MainMenuManager.Instance.settingButton != null)
        {
            MainMenuManager.Instance.settingButton.interactable = true;
        }
        if (OthelloManager.Instance != null && OthelloManager.Instance.settingButtonInGame != null)
        {
            OthelloManager.Instance.settingButtonInGame.interactable = true;
        }
        OthelloManager.Waiting = false;
    }

    /// <summary>
    /// マスター音量変更処理
    /// </summary>
    private void OnMasterVolumeChanged(float value)
    {
        float masterVolume = value * 10f;
        masterVolumeRTPC.SetGlobalValue(masterVolume);
        PlayerPrefs.SetFloat("MasterVolume", value);
        if (value == 0)
        {
            muteMaster.SetActive(true);
        }
        else
        {
            muteMaster.SetActive(false);
        }
    }

    /// <summary>
    /// BGM音量変更処理
    /// </summary>
    private void OnBGMVolumeChanged(float value)
    {
        float bgmVolume = value * 10f;
        bgmVolumeRTPC.SetGlobalValue(bgmVolume);
        PlayerPrefs.SetFloat("BGMVolume", value);
        if (value == 0)
        {
            muteBGM.SetActive(true);
        }
        else
        {
            muteBGM.SetActive(false);
        }
    }

    /// <summary>
    /// SE音量変更処理
    /// </summary>
    private void OnSEVolumeChanged(float value)
    {
        float seVolume = value * 10f;
        seVolumeRTPC.SetGlobalValue(seVolume);
        PlayerPrefs.SetFloat("SEVolume", value);
        if (value == 0)
        {
            muteSE.SetActive(true);
        }
        else
        {
            muteSE.SetActive(false);
        }
    }
}
