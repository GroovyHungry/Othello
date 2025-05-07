using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class DifficultySelectManager : MonoBehaviour
{
    public static DifficultySelectManager Instance;
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public GameObject errorButton;
    public GameObject DifficultySelectPanel;
    private bool selected = false;
    public static string difficulty = "easy";
    public GameObject NormalBoard;
    public GameObject BrokenBoard;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        int unlocked = PlayerPrefs.GetInt("Unlocked", 0);
        easyButton.gameObject.SetActive(false);
        normalButton.gameObject.SetActive(false);
        hardButton.gameObject.SetActive(false);
        errorButton.SetActive(false);

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
    // Update is called once per frame
    void Update()
    {
    }
    private void OnDestroy()
    {
        easyButton.onClick.RemoveListener(OnEasyButtonClicked);
        normalButton.onClick.RemoveListener(OnNormalButtonClicked);
        hardButton.onClick.RemoveListener(OnHardButtonClicked);
    }
    private void OnEasyButtonClicked()
    {
        AkSoundEngine.PostEvent("OnClick", easyButton.gameObject);
        difficulty = "easy";
        selected = true;
    }
    private void OnNormalButtonClicked()
    {
        AkSoundEngine.PostEvent("OnClick", normalButton.gameObject);
        difficulty = "normal";
        selected = true;
    }
    private void OnHardButtonClicked()
    {
        AkSoundEngine.PostEvent("OnClick", hardButton.gameObject);
        difficulty = "hard";
        selected = true;
    }
    public void OnSecretButtonClicked()
    {
        AkSoundEngine.PostEvent("Noise", gameObject);
        difficulty = "secret";
        NormalBoard.SetActive(false);
        BrokenBoard.SetActive(true);
        selected = true;
    }
    public async UniTask StartDifficultySelect()
    {
        DifficultySelectPanel.SetActive(true);
        selected = false;
        await UniTask.WaitUntil(() => selected);
        OthelloAI.Instance.difficulty = difficulty;
        DifficultySelectPanel.SetActive(false);
    }
}
