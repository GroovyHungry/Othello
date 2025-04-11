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
    public async UniTask StartCoinToss()
    {
        panel.SetActive(true);

        // CoinToss.Rebind(); // Animatorの状態リセット（オプション）
        CoinToss.Play("spinning_piece_Clip");

        // ユーザー選択待ち
        bool selected = false;
        //ここでCoinTossアニメを再生したい
        whiteButton.onClick.AddListener(() => { userChoice = "White"; selected = true; });
        blackButton.onClick.AddListener(() => { userChoice = "Black"; selected = true; });

        await UniTask.WaitUntil(() => selected);

        // アニメーション再生
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.5f)); // 2秒回転

        // 結果決定
        string result = Random.value < 0.5f ? "White" : "Black";
        CoinToss.SetTrigger(result == "White" ? "Selected White" : "Selected Black");

        await UniTask.Delay(System.TimeSpan.FromSeconds(2.5f));
        bool correct = (userChoice == result);

        OthelloManager.Instance.isAIWhite = correct;

        panel.SetActive(false);
    }
}
