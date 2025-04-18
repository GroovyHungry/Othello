using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
public class MainMenuManage : MonoBehaviour
{
    public static MainMenuManage Instance;
    public GameObject menuPanel;
    public Button pvpButton;
    public Button cpuButton;
    public Button settingButton;
    public Animator menuAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}
    private void Start()
    {
        Debug.Log("Start");
        pvpButton.onClick.AddListener(() => {
            OnModeSelected(false);
            cpuButton.interactable = false;
            // pvpButton.interactable = false;
        });
        cpuButton.onClick.AddListener(() => {
            OnModeSelected(true);
            pvpButton.interactable = false;
            // cpuButton.interactable = false;
        });
        settingButton.onClick.AddListener(() => {
        });
    }

    // Update is called once per frame
    void Update()
    {
        
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
