using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using AK.Wwise;
public class DoubleCheckManager : MonoBehaviour
{
    public static DoubleCheckManager Instance;
    public GameObject doubleCheckPanel;
    public Button yesButton;
    public Button noButton;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }
    private void OnDestroy()
    {
        yesButton.onClick.RemoveListener(OnYesButtonClicked);
        noButton.onClick.RemoveListener(OnNoButtonClicked);
    }
    public void OpenDoubleCheckPanel()
    {
        OthelloManager.Waiting = true;
        doubleCheckPanel.SetActive(true);
    }
    public async void OnYesButtonClicked()
    {
        AkSoundEngine.PostEvent("OnClick", yesButton.gameObject);
        doubleCheckPanel.SetActive(false);
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OthelloBoard")
        {
            OthelloManager.Instance.ExitToMainMenu();
        }
        else if (sceneName == "MainMenu")
        {
            MainMenuManager.Instance.QuietGame();
        }
    }
    public void OnNoButtonClicked()
    {
        AkSoundEngine.PostEvent("OnClick", noButton.gameObject);
        OthelloManager.Waiting = false;
        doubleCheckPanel.SetActive(false);
    }
    void Update()
    {
    }
}
