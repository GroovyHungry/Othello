using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using AK.Wwise;

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
    public AK.Wwise.Event playLoopEvent;
    public AK.Wwise.Event stopLoopEvent;
    private uint loopPlayingId = AkSoundEngine.AK_INVALID_PLAYING_ID;
    private bool selected = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        whiteButton.onClick.AddListener(OnWhiteButtonClicked);
        blackButton.onClick.AddListener(OnBlackButtonClicked);
    }

    private void OnDestroy()
    {
        whiteButton.onClick.RemoveListener(OnWhiteButtonClicked);
        blackButton.onClick.RemoveListener(OnBlackButtonClicked);
    }

    private void OnWhiteButtonClicked()
    {
        userChoice = "White";
        selected = true;
        blackButton.interactable = false;
        AkSoundEngine.PostEvent("OnClick", whiteButton.gameObject);
    }

    private void OnBlackButtonClicked()
    {
        userChoice = "Black";
        selected = true;
        whiteButton.interactable = false;
        AkSoundEngine.PostEvent("OnClick", blackButton.gameObject);
    }

    public async UniTask StartCoinTossVsCPU()
    {
        panel.SetActive(true);

        CoinToss.Play("Spinning");
        loopPlayingId = playLoopEvent.Post(gameObject);

        selected = false;

        await UniTask.WaitUntil(() => selected);

        string result = Random.value < 0.5f ? "White" : "Black";
        CoinToss.SetTrigger(result == "White" ? "ShowWhite" : "ShowBlack");
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.8f));

        stopLoopEvent.Post(gameObject);
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.0f));

        bool correct = (userChoice == result);
        OthelloManager.Instance.isAIWhite = correct;

        panel.SetActive(false);
        UI.SetActive(true);
    }

    public async UniTask StartCoinTossPvP()
    {
        panel.SetActive(true);
        whiteButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);

        CoinToss.Play("Spinning");
        loopPlayingId = playLoopEvent.Post(gameObject);

        string result = Random.value < 0.5f ? "White" : "Black";
        CoinToss.SetTrigger(result == "White" ? "ShowWhite" : "ShowBlack");
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.8f));

        stopLoopEvent.Post(gameObject);
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.0f));

        OthelloManager.Instance.isWhiteTurn = (result == "White");

        panel.SetActive(false);
        UI.SetActive(true);
    }

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
        group.gameObject.SetActive(false);
    }
}
