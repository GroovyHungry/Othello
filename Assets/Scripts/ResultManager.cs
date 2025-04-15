using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Collections;

public class ResultManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static ResultManager Instance;
    private int whiteScore;
    private int blackScore;
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
        whiteScore = OthelloManager.Instance.CountPieces(true);
        blackScore = OthelloManager.Instance.CountPieces(false);

        List<GameObject> toDestroy = new List<GameObject>();

        List<GameObject> placedPieces = new List<GameObject>();
        for (int y = 0; y < OthelloBoard.gridSize; y++)
        {
            for (int x = 0; x < OthelloBoard.gridSize; x++)
            {
                GameObject checkPiece = OthelloBoard.Instance.GetPiece(x, y);
                if (checkPiece != null)
                {
                    await checkPiece.GetComponent<OthelloPiece>().Remove();
                    toDestroy.Add(checkPiece);
                }
            }
        }
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));
        foreach (GameObject piece in toDestroy)
        {
            Object.Destroy(piece);
        }
        OthelloBoard.Instance.ClearBoardState();
    }
}
