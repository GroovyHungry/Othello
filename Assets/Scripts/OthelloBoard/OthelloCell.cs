using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using AK.Wwise;

/// <summary>
/// マウス操作によるセルのハイライトとクリック処理を管理するクラス
/// </summary>
public class OthelloCell : MonoBehaviour
{
    /// <summary>
    /// ホバー時に表示するフレームのSpriteRenderer
    /// </summary>
    public SpriteRenderer hoverFrame;

    /// <summary>
    /// セルの列インデックス (0-7)
    /// </summary>
    public int x;

    /// <summary>
    /// セルの行インデックス (0-7)
    /// </summary>
    public int y;

    /// <summary>
    /// ホバー判定
    /// </summary>
    private bool isHovering = false;

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event OnSelect;
    [SerializeField] private AK.Wwise.Event InvalidMove;

    /// <summary>
    /// 毎フレーム実行：UIロック状態を確認し，マウスオーバー時のフレーム表示を制御する
    /// </summary>
    private void Update()
    {
        // 初期化中，待機中，AI実行中はホバー処理無効化
        if (OthelloManager.initializing || OthelloManager.Waiting || OthelloManager.isAIPlaying)
        {
            hoverFrame.enabled = false;
            isHovering = false;
            return;
        }

        if (IsMouseOver())
        {
            string currentTag = OthelloManager.isWhiteTurn ? "White" : "Black";

            if (DifficultySelect.difficulty == "secret")
            {
                // シークレットモードは空セル全てホバー可
                if (OthelloBoard.Instance.IsCellEmpty(x, y))
                    TryEnableHover();
                else
                    DisableHover();
            }
            else
            {
                // 通常モードは空セルかつ合法手のみホバー可
                if (OthelloBoard.Instance.IsCellEmpty(x, y)
                    && OthelloBoard.Instance.IsValidMove(x, y, currentTag))
                    TryEnableHover();
                else
                    DisableHover();
            }
        }
        else
        {
            DisableHover();
        }
    }

    /// <summary>
    /// クリック位置がこのセル上かを判定する
    /// </summary>
    /// <returns>セル内ならtrue</returns>
    private bool IsMouseOver()
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D col = GetComponent<Collider2D>();
        return col != null && col.OverlapPoint(new Vector2(world.x, world.y));
    }

    /// <summary>
    /// マウスダウン時の処理：セルの配置・警告を実行する
    /// </summary>
    private void OnMouseDown()
    {
        // UIロック時は無視
        if (OthelloManager.initializing || OthelloManager.Waiting || OthelloManager.isAIPlaying)
            return;

        string currentTag = OthelloManager.isWhiteTurn ? "White" : "Black";

        // 合法手なら駒を配置
        if (OthelloBoard.Instance.IsCellEmpty(x, y) && OthelloBoard.Instance.IsValidMove(x, y, currentTag))
        {
            OthelloManager.Instance.placedFlag = true;
            OnSelect.Post(gameObject);
            Vector3 pos = transform.position;
            _ = OthelloManager.Instance.PlacePiece(x, y, currentTag, pos);
        }
        else
        {
            // シークレットモードは空セルクリックで警告Wwiseサウンド
            if (OthelloBoard.Instance.IsCellEmpty(x, y) && DifficultySelect.difficulty == "secret")
            {
                InvalidMove.Post(gameObject);
            }
        }
    }

    /// <summary>
    /// ホバー開始処理：フレーム表示とWwiseイベント発火
    /// </summary>
    private void TryEnableHover()
    {
        if (!isHovering)
        {
            hoverFrame.enabled = true;
            OnSelect.Post(gameObject);
            isHovering = true;
        }
    }

    /// <summary>
    /// ホバー終了処理：フレーム非表示とリセット
    /// </summary>
    private void DisableHover()
    {
        hoverFrame.enabled = false;
        isHovering = false;
    }
}