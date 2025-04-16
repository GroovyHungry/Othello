using UnityEngine;
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
    private OthelloBoard board;
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
        List<GameObject> toDestroy = new List<GameObject>();
        float startDelay = 0.2f;
        float acceleration = 0.01f; 
        for (int i = 0; i < OthelloBoard.gridSize * OthelloBoard.gridSize; i++)
        {
            int x = i % OthelloBoard.gridSize;
            int y = i / OthelloBoard.gridSize;
            GameObject checkedPiece = OthelloBoard.Instance.GetPiece(x, y);
            if (checkedPiece != null)
            {
                float delay = Math.Max(0.05f, startDelay - acceleration * i);
                await checkedPiece.GetComponent<OthelloPiece>().Remove(delay);
                toDestroy.Add(checkedPiece);
            }
        }
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));
        foreach (GameObject piece in toDestroy)
        {
            Destroy(piece);
        }
        OthelloBoard.Instance.ClearBoardState();
    }
    public async UniTask ShowResult()
    {
        int whiteScore = OthelloManager.Instance.CountPieces(true);
        int blackScore = OthelloManager.Instance.CountPieces(false);

        int competitively = Math.Min(whiteScore, blackScore);
        int difference = whiteScore - blackScore;
        bool isWhiteWin = difference > 0;

        await RemoveAllPieces();

        List<Vector2Int> whitePos = new List<Vector2Int>();
        List<Vector2Int> blackPos = new List<Vector2Int>();
        List<Vector2Int> differencesPos = new List<Vector2Int>();
        for (int i = 0; i < OthelloBoard.gridSize * OthelloBoard.gridSize; i++)
        {
            int x = i % OthelloBoard.gridSize;
            int y = i / OthelloBoard.gridSize;
            if (Math.Abs(difference) <= 10)
            {
                if (i < competitively)
                {
                    whitePos.Add(new Vector2Int(x, y));
                    blackPos.Add(new Vector2Int((7 - x), (7 - y)));
                }
                if (isWhiteWin && i < (difference - competitively) && i >= competitively)
                {
                    differencesPos.Add(new Vector2Int(x, y));
                }
                else if (!isWhiteWin && i < -(difference + competitively) && i >= competitively)
                {
                    differencesPos.Add(new Vector2Int((7 - x), (7 - y)));
                }
            }
            else
            {
                if (i < whiteScore)
                {
                    whitePos.Add(new Vector2Int(x, y));
                }
                if (i < blackScore)
                {
                    blackPos.Add(new Vector2Int((7 - x), (7 - y)));
                }
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
    }
    private async UniTask PlaceSequentially(List<Vector2Int> positions, GameObject prefab)
    {
        float interval = 0.08f;

        foreach (var pos in positions)
        {
            Instantiate(prefab, new Vector3(pos.x - 3.5f, pos.y - 3.5f, 0), Quaternion.identity);
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }
}
