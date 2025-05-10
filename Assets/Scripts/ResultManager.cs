using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Collections;
using System;

public class ResultManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static ResultManager Instance;
    // private int whiteScore;
    // private int blackScore;
    // private int competitively;
    // private int difference;
    public GameObject BlackWins;
    public GameObject WhiteWins;
    public GameObject youWin;
    public GameObject youLose;
    public GameObject WinEffect2;
    public GameObject WinEffect1;
    public GameObject Draw;
    void Awake()
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
    public async UniTask RemoveAllPieces()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        // List<GameObject> toDestroy = new List<GameObject>();
        // float startDelay = 0.2f;
        // float acceleration = 0.01f;
        float speed = 2.0f;
        BGMController.Instance.StopBGM();
        await SceneTransition.Instance.PlayFadeOut(speed);
        OthelloBoard.Instance.ClearBoardState();
        BGMController.Instance.ChangeBGM_1();
        BGMController.Instance.ChangeBGM_2();
        await SceneTransition.Instance.PlayFadeIn(speed);
    }
    public async UniTask ShowResult()
    {
        int whiteScore = OthelloBoard.Instance.CountPieces(true);
        int blackScore = OthelloBoard.Instance.CountPieces(false);

        int competitively = Math.Min(whiteScore, blackScore);
        int difference = whiteScore - blackScore;
        int diffAbs = Math.Abs(difference);
        bool isWhiteWin = difference > 0;

        await RemoveAllPieces();

        List<Vector2Int> whitePos = new List<Vector2Int>();
        List<Vector2Int> blackPos = new List<Vector2Int>();
        List<Vector2Int> differencesPos = new List<Vector2Int>();

        for (int i = 0; i < OthelloBoard.gridSize * OthelloBoard.gridSize; i++)
        {
            int x = i % OthelloBoard.gridSize;
            int y = i / OthelloBoard.gridSize;

            if (diffAbs <= 10)
            {
                if (i < competitively)
                {
                    whitePos.Add(new Vector2Int(x, y));
                    blackPos.Add(new Vector2Int(7 - x, 7 - y));
                }
                else if (i < competitively + diffAbs)
                {
                    if (isWhiteWin)
                        differencesPos.Add(new Vector2Int(x, y));
                    else
                        differencesPos.Add(new Vector2Int(7 - x, 7 - y));
                }
            }
            else
            {
                if (i < whiteScore) whitePos.Add(new Vector2Int(x, y));
                if (i < blackScore) blackPos.Add(new Vector2Int(7 - x, 7 - y));
            }
        }

        await UniTask.WhenAll(
            PlaceSequentially(whitePos, OthelloManager.Instance.whitePiecePrefab),
            PlaceSequentially(blackPos, OthelloManager.Instance.blackPiecePrefab)
        );

        if (differencesPos.Count > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
            GameObject prefab = isWhiteWin ? OthelloManager.Instance.whitePiecePrefab : OthelloManager.Instance.blackPiecePrefab;
            await PlaceSequentially(differencesPos, prefab);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        BGMController.Instance.PlayBGM();

        bool won = false;
        if (OthelloManager.isAIOpponent)
        {
            if (difference > 0)
            {
                if (OthelloManager.Instance.isAIWhite)
                {
                    youLose.SetActive(true);
                    BGMController.Instance.TransitionBGM("LoseResult");
                }
                else
                {
                    youWin.SetActive(true);
                    WinEffect1.SetActive(true);
                    WinEffect2.SetActive(true);
                    BGMController.Instance.TransitionBGM("WinResult");
                    won = true;
                }
            }
            else if (difference < 0)
            {
                if (OthelloManager.Instance.isAIWhite)
                {
                    youWin.SetActive(true);
                    WinEffect1.SetActive(true);
                    WinEffect2.SetActive(true);
                    BGMController.Instance.TransitionBGM("WinResult");
                    won = true;
                }
                else
                {
                    youLose.SetActive(true);
                    BGMController.Instance.TransitionBGM("LoseResult");
                }
            }
            else if (difference == 0)
            {
                Draw.SetActive(true);
            }
        }
        else
        {
            if (difference > 0)
            {
                WhiteWins.SetActive(true);
                WinEffect1.SetActive(true);
                WinEffect2.SetActive(true);
                BGMController.Instance.TransitionBGM("WinResult");
            }
            else if (difference < 0)
            {
                BlackWins.SetActive(true);
                WinEffect1.SetActive(true);
                WinEffect2.SetActive(true);
                BGMController.Instance.TransitionBGM("WinResult");
            }
            else if (difference == 0)
            {
                Draw.SetActive(true);
            }
        }
        string[] difficultyNames = new string[] { "easy", "normal", "hard", "secret" };
        int unlocked = PlayerPrefs.GetInt("Unlocked", 0);
        if (won)
        {
            if (DifficultySelectManager.difficulty == difficultyNames[unlocked])
            {
                if (unlocked + 1 < difficultyNames.Length)
                {
                    PlayerPrefs.SetInt("Unlocked", unlocked + 1);
                    PlayerPrefs.Save();
                }
            }
        }
        OthelloManager.Instance.settingButtonInGame.interactable = false;
        OthelloManager.Instance.settingButtonInGame.GetComponent<EventTrigger>().enabled = false;
        OthelloManager.Instance.exitButton.interactable = false;
        OthelloManager.Instance.exitButton.GetComponent<EventTrigger>().enabled = false;
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
        await SceneTransition.Instance.Transition("MainMenu");
    }

    private async UniTask PlaceSequentially(List<Vector2Int> positions, GameObject prefab)
    {
        float interval = 0.08f;

        foreach (var pos in positions)
        {
            GameObject piece = Instantiate(prefab, new Vector3(pos.x - 3.5f, pos.y - 3.5f, 0), Quaternion.identity);
            AkSoundEngine.PostEvent("PlacePiece", piece);
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }
}
