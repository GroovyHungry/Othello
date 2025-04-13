using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
public class MainMenuManage : MonoBehaviour
{
    public static MainMenuManage Instance;
    public GameObject menuPanel;
    public Button pvpButton;
    public Button cpuButton;
    public Animator menuAnimator;
    public CameraMover cameraMover;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        pvpButton.onClick.AddListener(() => {
            OnModeSelected(false);
            cpuButton.interactable = false;
            pvpButton.interactable = false;
        });
        cpuButton.onClick.AddListener(() => {
            OnModeSelected(true);
            pvpButton.interactable = false;
            cpuButton.interactable = false;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private async void OnModeSelected(bool isCPU)
    {
        OthelloManager.Instance.isAIOpponent = isCPU;


        menuAnimator.SetTrigger("Start"); // 例：フェードアウトアニメーション
        await UniTask.Delay(System.TimeSpan.FromSeconds(3.0f));
        menuPanel.SetActive(false);
        await cameraMover.MoveToGameView();

        // 5. オセロゲーム開始
        await OthelloManager.Instance.StartGame(); // ← これをOthelloManagerに追加する
    }
}
