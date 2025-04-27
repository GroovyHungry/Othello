using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using AK.Wwise;
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;
    public GameObject menuPanel;
    public Button pvpButton;
    public Button cpuButton;
    public Button settingButton;
    public Animator menuAnimator;
    public GameObject SettingPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        pvpButton.onClick.AddListener(OnPvpButtonClicked);
        cpuButton.onClick.AddListener(OnCpuButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
    }
    private void OnDestroy()
    {
        pvpButton.onClick.RemoveListener(OnPvpButtonClicked);
        cpuButton.onClick.RemoveListener(OnCpuButtonClicked);
        settingButton.onClick.RemoveListener(OnSettingButtonClicked);
    }
    private async void OnPvpButtonClicked()
    {
        OnModeSelected(false);
        cpuButton.interactable = false;
        AkSoundEngine.PostEvent("OnClick", pvpButton.gameObject);
    }

    private async void OnCpuButtonClicked()
    {
        OnModeSelected(true);
        pvpButton.interactable = false;
        AkSoundEngine.PostEvent("OnClick", cpuButton.gameObject);
    }

    private void OnSettingButtonClicked()
    {
        SettingPanel.SetActive(true);
        settingButton.interactable = false;
        AkSoundEngine.PostEvent("OnClick", settingButton.gameObject);
    }
    private async void OnModeSelected(bool isCPU)
    {
        OthelloManager.isAIOpponent = isCPU;

        menuAnimator.SetTrigger("Start");
        await UniTask.Delay(System.TimeSpan.FromSeconds(3.0f));
        menuPanel.SetActive(false);

        await SceneTransition.Instance.Transition("OthelloBoard");
        // await OthelloManager.Instance.StartGame();
    }
}
