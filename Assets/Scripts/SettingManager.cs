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
        if (Instance == null) Instance = this;
		else Destroy(gameObject);

        masterSlider.value = 5;
        bgmSlider.value = 5;
        seSlider.value = 5;
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);
    }
    private void Start()
    {
        SettingCloseButton.onClick.AddListener(() => {
            CloseSetting();
        });
    }
    public void CloseSetting()
    {
        AkSoundEngine.PostEvent("OnClick", SettingCloseButton.gameObject);
        settingPanel.SetActive(false);
        MainMenuManage.Instance.settingButton.interactable = true;
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
