using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class OthelloAI : MonoBehaviour
{
    public static OthelloAI Instance;
    private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public async UniTask PlayAITurn()
    {
        OthelloManager.isAIPlaying = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.4f));
        string aiTag = OthelloManager.Instance.IsAIWhite() ? "White" :"Black";

        OthelloManager.Instance.GetValidAndInvalidCells(out List<OthelloCell> validCells, out _);

        List<Vector2Int> validMoves = new List<Vector2Int>();
        foreach (var cell in validCells)
        {
            validMoves.Add(new Vector2Int(cell.x, cell.y));
        }
        if (validMoves.Count == 0)
        {
            OthelloManager.isAIPlaying = false;
            return;
        }
        Vector2Int selected = validMoves[Random.Range(0, validMoves.Count)];
        Vector3 pos = new Vector3(selected.x - 3.5f, selected.y - 3.5f, 0);

        await OthelloManager.Instance.PlacePieces(selected.x, selected.y, aiTag, pos);

        OthelloManager.isAIPlaying = false;
    }
}
