using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;
    public GameObject settingPanel;
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider seSlider;
    public Button SettingCloseButton;
    public RTPC masterVolumeRTPC;
    public RTPC bgmVolumeRTPC;
    public RTPC seVolumeRTPC;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも残す
        }
        else
        {
            Destroy(gameObject);
        }

        masterSlider.value = 5;
        bgmSlider.value = 5;
        seSlider.value = 5;
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);
        SettingCloseButton.onClick.AddListener(CloseSetting);
    }
    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        seSlider.onValueChanged.RemoveListener(OnSEVolumeChanged);
        SettingCloseButton.onClick.RemoveListener(CloseSetting);
    }
    private void Start()
    {
    }
    public void OpenSettingPanel()
    {
        settingPanel.SetActive(true);
        // OthelloManager.Waiting = true;
    }
    public void CloseSetting()
    {
        AkSoundEngine.PostEvent("OnClick", SettingCloseButton.gameObject);
        settingPanel.SetActive(false);
        if (MainMenuManager.Instance != null && MainMenuManager.Instance.settingButton != null)
        {
            MainMenuManager.Instance.settingButton.interactable = true;
        }
        if (OthelloManager.Instance != null && OthelloManager.Instance.settingButtonInGame != null)
        {
            OthelloManager.Instance.settingButtonInGame.interactable = true;
        }
        // OthelloManager.Waiting = false;
    }
    private void OnMasterVolumeChanged(float value)
    {
        float masterVolume = value * 10f;
        masterVolumeRTPC.SetGlobalValue(masterVolume);
    }
    private void OnBGMVolumeChanged(float value)
    {
        float bgmVolume = value * 10f;
        bgmVolumeRTPC.SetGlobalValue(bgmVolume);
    }
    private void OnSEVolumeChanged(float value)
    {
        float seVolume = value * 10f;
        seVolumeRTPC.SetGlobalValue(seVolume);
    }
}
