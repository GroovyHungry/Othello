using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    private bool selected = false;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        selected = false;
        cpuButton.onClick.AddListener(OnCpuButtonClicked);
        pvpButton.onClick.AddListener(OnPvpButtonClicked);
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
        if (!selected)
        {
            selected = true;
            AkSoundEngine.PostEvent("OnClick", pvpButton.gameObject);
            cpuButton.interactable = false;
            pvpButton.interactable = false;
            settingButton.interactable = false;
            pvpButton.GetComponent<EventTrigger>().enabled = false;
            cpuButton.GetComponent<EventTrigger>().enabled = false;
            settingButton.GetComponent<EventTrigger>().enabled = false;
            await OnModeSelected(false);
        }
    }

    private async void OnCpuButtonClicked()
    {
        if (!selected)
        {
            selected = true;
            AkSoundEngine.PostEvent("OnClick", cpuButton.gameObject);
            pvpButton.interactable = false;
            cpuButton.interactable = false;
            settingButton.interactable = false;
            cpuButton.GetComponent<EventTrigger>().enabled = false;
            pvpButton.GetComponent<EventTrigger>().enabled = false;
            settingButton.GetComponent<EventTrigger>().enabled = false;
            AkSoundEngine.PostEvent("OnClick", cpuButton.gameObject);
            await OnModeSelected(true);
        }
    }

    private void OnSettingButtonClicked()
    {
        SettingManager.Instance.OpenSettingPanel();
        settingButton.interactable = false;
        AkSoundEngine.PostEvent("OnClick", settingButton.gameObject);
    }
    private async UniTask OnModeSelected(bool isCPU)
    {
        OthelloManager.isAIOpponent = isCPU;

        menuAnimator.SetTrigger("Start");
        await UniTask.Delay(System.TimeSpan.FromSeconds(3.0f));
        // menuPanel.SetActive(false);

        await SceneTransition.Instance.Transition("OthelloBoard");
        // await OthelloManager.Instance.StartGame();
    }
}
