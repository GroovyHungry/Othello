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
    public Button musicChangerButton;
    public Button quietGameButton;
    public Animator menuAnimator;
    public GameObject SettingPanel;
    private bool selected = false;
    private string inputBuffer = "";
    private float lastCharTime = -1f;
    private const float bufferTimeout = 2f;
    private const string unlockKeyword = "othello";
    [SerializeField] private AK.Wwise.Event OnClick;
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
        musicChangerButton.onClick.AddListener(OnBGMControllerButtonClicked);
        quietGameButton.onClick.AddListener(OnQuietGameButtonClicked);
    }
    private void Update()
    {
        if (inputBuffer.Length > 0 && Time.time - lastCharTime > bufferTimeout)
        {
            inputBuffer = "";
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
                    inputBuffer = "";
                }
            }
        }
    }

    private void OnDestroy()
    {
        pvpButton.onClick.RemoveListener(OnPvpButtonClicked);
        cpuButton.onClick.RemoveListener(OnCpuButtonClicked);
        settingButton.onClick.RemoveListener(OnSettingButtonClicked);
        musicChangerButton.onClick.RemoveListener(OnBGMControllerButtonClicked);
        quietGameButton.onClick.RemoveListener(OnQuietGameButtonClicked);
    }
    public async UniTask QuietGame()
    {
        await SceneTransition.Instance.PlayFadeOut(1.0f);
        Debug.Log("Quiet Game");
        Application.Quit();
    }
    private void OnQuietGameButtonClicked()
    {
        DoubleCheck.Instance.OpenDoubleCheckPanel();
        OnClick.Post(quietGameButton.gameObject);
    }
    private async void OnPvpButtonClicked()
    {
        if (!selected)
        {
            selected = true;
            OnClick.Post(pvpButton.gameObject);
            cpuButton.interactable = false;
            pvpButton.interactable = false;
            settingButton.interactable = false;
            musicChangerButton.interactable = false;
            quietGameButton.interactable = false;
            pvpButton.GetComponent<EventTrigger>().enabled = false;
            cpuButton.GetComponent<EventTrigger>().enabled = false;
            settingButton.GetComponent<EventTrigger>().enabled = false;
            musicChangerButton.GetComponent<EventTrigger>().enabled = false;
            quietGameButton.GetComponent<EventTrigger>().enabled = false;
            await OnModeSelected(false);
        }
    }

    private async void OnCpuButtonClicked()
    {
        if (!selected)
        {
            selected = true;
            OnClick.Post(cpuButton.gameObject);
            pvpButton.interactable = false;
            cpuButton.interactable = false;
            settingButton.interactable = false;
            musicChangerButton.interactable = false;
            quietGameButton.interactable = false;
            cpuButton.GetComponent<EventTrigger>().enabled = false;
            pvpButton.GetComponent<EventTrigger>().enabled = false;
            settingButton.GetComponent<EventTrigger>().enabled = false;
            musicChangerButton.GetComponent<EventTrigger>().enabled = false;
            quietGameButton.GetComponent<EventTrigger>().enabled = false;
            await OnModeSelected(true);
        }
    }

    private void OnSettingButtonClicked()
    {
        SettingManager.Instance.OpenSettingPanel();
        settingButton.interactable = false;
        OnClick.Post(settingButton.gameObject);
    }
    private void OnBGMControllerButtonClicked()
    {
        OnClick.Post(musicChangerButton.gameObject);
        BGMController.Instance.OpenBGMControllerPanel();
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
