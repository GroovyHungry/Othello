using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
public class CoinTossManager : MonoBehaviour
{
    public static CoinTossManager Instance;
    public GameObject panel;
    public Animator CoinToss;
    public Button whiteButton;
    public Button blackButton;
    private string userChoice;
    public CanvasGroup coinTossGroup;
    public GameObject UI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public async UniTask StartCoinTossVsCPU()
    {
        panel.SetActive(true);

        // CoinToss.Rebind(); // Animatorの状態リセット（オプション）
        CoinToss.Play("Spinning");

        // ユーザー選択待ち
        bool selected = false;
        //ここでCoinTossアニメを再生したい
        whiteButton.onClick.AddListener(() => {
            userChoice = "White";
            selected = true;
            blackButton.interactable = false;
        });

        blackButton.onClick.AddListener(() => {
            userChoice = "Black";
            selected = true;
            whiteButton.interactable = false;
        });

        await UniTask.WaitUntil(() => selected);

        // 結果決定
        string result = Random.value < 0.5f ? "White" : "Black";
        CoinToss.SetTrigger(result == "White" ? "ShowWhite" : "ShowBlack");

        await UniTask.Delay(System.TimeSpan.FromSeconds(2.5f));
        bool correct = (userChoice == result);

        OthelloManager.Instance.isAIWhite = correct;
        // await FadeOutPanel(coinTossGroup, 0.08f);

        panel.SetActive(false);
        UI.SetActive(true);
    }
    public async UniTask StartCoinTossPvP()
    {
        panel.SetActive(true);
        whiteButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);
        CoinToss.Play("Spinning");

        await UniTask.Delay(System.TimeSpan.FromSeconds(2.0f));

        string result = Random.value < 0.5f ? "White" : "Black";
        CoinToss.SetTrigger(result == "White" ? "ShowWhite" : "ShowBlack");

        await UniTask.Delay(System.TimeSpan.FromSeconds(2.0f));

        OthelloManager.Instance.isWhiteTurn = (result == "White" ? true : false);

        panel.SetActive(false);
        UI.SetActive(true);
    }
    // フェードアウト関数
    private async UniTask FadeOutPanel(CanvasGroup group, float stepInterval = 0.1f)
    {
        float[] steps = { 1f, 0.66f, 0.33f, 0f };

        foreach (var alpha in steps)
        {
            group.alpha = alpha;
            await UniTask.Delay(System.TimeSpan.FromSeconds(stepInterval));
        }

        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false); // optional
    }
}
