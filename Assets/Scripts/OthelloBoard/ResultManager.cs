using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;

/// <summary>
/// ゲーム終了時の結果表示とその演出を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static ResultManager Instance;

    /// <summary>
    /// 黒勝利表示用GameObject（PvP用）
    /// </summary>
    public GameObject BlackWins;
    /// <summary>
    /// 白勝利表示用GameObject（PvP用）
    /// </summary>
    public GameObject WhiteWins;
    /// <summary>
    /// プレイヤー勝利表示用GameObject（CPU戦用）
    /// </summary>
    public GameObject youWin;
    /// <summary>
    /// プレイヤー敗北表示用GameObject（CPU戦用）
    /// </summary>
    public GameObject youLose;
    /// <summary>
    /// 勝利エフェクトパーツ1
    /// </summary>
    public GameObject WinEffect2;
    /// <summary>
    /// 勝利エフェクトパーツ2
    /// </summary>
    public GameObject WinEffect1;
    /// <summary>
    /// 引き分け表示用GameObject
    /// </summary>
    public GameObject Draw;

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event PlacePiece;
    [SerializeField] private AK.Wwise.Event VinylNoise;
    [SerializeField] private AK.Wwise.Event DrumRolls;

    /// <summary>
    /// インスタンス設定
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 全駒を削除しフェード演出で結果画面に以降する
    /// </summary>
    public async UniTask RemoveAllPieces()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        float speed = 2.0f;
        AudioManager.Instance.StopBGM();
        VinylNoise.Post(gameObject);
        await SceneTransition.Instance.PlayFadeOut(speed);
        OthelloBoard.Instance.ClearBoardState();
        AudioManager.Instance.ChangeBGM_1();
        AudioManager.Instance.ChangeBGM_2();
        await SceneTransition.Instance.PlayFadeIn(speed);
    }

    /// <summary>
    /// ゲーム結果を判定し，演出を表示する
    /// </summary>
    public async UniTask ShowResult()
    {
        int whiteScore = OthelloBoard.Instance.CountPieces(true);
        int blackScore = OthelloBoard.Instance.CountPieces(false);

        int competitively = Math.Min(whiteScore, blackScore);
        int difference = whiteScore - blackScore;
        int diffAbs = Math.Abs(difference);
        bool isWhiteWin = difference > 0;

        await RemoveAllPieces();

        OthelloManager.Waiting = true;

        List<Vector2Int> whitePos = new List<Vector2Int>();
        List<Vector2Int> blackPos = new List<Vector2Int>();
        List<Vector2Int> differencesPos = new List<Vector2Int>();

        for (int i = 0; i < OthelloBoard.gridSize * OthelloBoard.gridSize; i++)
        {
            int x = i % OthelloBoard.gridSize;
            int y = i / OthelloBoard.gridSize;

            ///駒数の差が10以下の時は特殊演出
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

        ///白黒を上下から設置しながら結果を表示
        await UniTask.WhenAll(
            PlaceSequentially(whitePos, OthelloManager.Instance.whitePiecePrefab),
            PlaceSequentially(blackPos, OthelloManager.Instance.blackPiecePrefab)
        );

        ///特殊演出時
        ///一瞬間を置きドラムロール後に結果表示
        if (differencesPos.Count > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            DrumRolls.Post(gameObject);
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            GameObject prefab = isWhiteWin ? OthelloManager.Instance.whitePiecePrefab : OthelloManager.Instance.blackPiecePrefab;
            await PlaceSequentially(differencesPos, prefab);
        }

        ///BGM再生
        await UniTask.Delay(TimeSpan.FromSeconds(2.0f));
        AudioManager.Instance.PlayBGM();

        bool won = false;
        ///AI戦の時
        ///勝敗引の表示
        if (OthelloManager.isAIOpponent)
        {
            if (difference > 0)
            {
                if (OthelloManager.Instance.isAIWhite)
                {
                    youLose.SetActive(true);
                    AudioManager.Instance.TransitionBGM("LoseResult");
                }
                else
                {
                    youWin.SetActive(true);
                    WinEffect1.SetActive(true);
                    WinEffect2.SetActive(true);
                    AudioManager.Instance.TransitionBGM("WinResult");
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
                    AudioManager.Instance.TransitionBGM("WinResult");
                    won = true;
                }
                else
                {
                    youLose.SetActive(true);
                    AudioManager.Instance.TransitionBGM("LoseResult");
                }
            }
            else
            {
                Draw.SetActive(true);
            }
        }
        ///PvPの時
        ///どちらの色の勝ちかを表示
        else
        {
            if (difference > 0)
            {
                WhiteWins.SetActive(true);
                WinEffect1.SetActive(true);
                WinEffect2.SetActive(true);
                AudioManager.Instance.TransitionBGM("WinResult");
            }
            else if (difference < 0)
            {
                BlackWins.SetActive(true);
                WinEffect1.SetActive(true);
                WinEffect2.SetActive(true);
                AudioManager.Instance.TransitionBGM("WinResult");
            }
            else
            {
                Draw.SetActive(true);
            }
        }

        ///AI戦の時，AIに勝利したら次の難易度を解放
        string[] difficultyNames = new string[] { "easy", "normal", "hard", "secret" };
        int unlocked = PlayerPrefs.GetInt("Unlocked", 0);
        if (won)
        {
            if (DifficultySelect.difficulty == difficultyNames[unlocked])
            {
                if (unlocked + 1 < difficultyNames.Length)
                {
                    PlayerPrefs.SetInt("Unlocked", unlocked + 1);
                    PlayerPrefs.Save();
                }
            }
        }

        ///ボタンを無効化し，入力待ち
        OthelloManager.Instance.settingButtonInGame.interactable = false;
        OthelloManager.Instance.settingButtonInGame.GetComponent<EventTrigger>().enabled = false;
        OthelloManager.Instance.exitButton.interactable = false;
        OthelloManager.Instance.exitButton.GetComponent<EventTrigger>().enabled = false;
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
        await SceneTransition.Instance.Transition("MainMenu");
        OthelloManager.Waiting = false;
    }

    /// <summary>
    /// 結果表示のために駒を順次配置する
    /// </summary>
    /// <param name="positions">配置するセル座標のリスト</param>
    /// <param name="prefab">配置する駒のプレハブ</param>
    private async UniTask PlaceSequentially(List<Vector2Int> positions, GameObject prefab)
    {
        float interval = 0.08f;

        foreach (var pos in positions)
        {
            GameObject piece = Instantiate(prefab, new Vector3(pos.x - 3.5f, pos.y - 3.5f, 0), Quaternion.identity);
            PlacePiece.Post(piece);
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }
}
