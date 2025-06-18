using UnityEngine;
using Cysharp.Threading.Tasks;
using AK.Wwise;

/// <summary>
/// オセロの駒一つを管理するクラス
/// </summary>
public class OthelloPiece : MonoBehaviour
{
    /// <summary>
    /// 列インデックス
    /// </summary>
    public int state_X;

    /// <summary>
    /// 行インデックス
    /// </summary>
    public int state_Y;

    /// <summary>
    /// Animatorコンポーネント
    /// </summary>
    private Animator animator;

    /// <summary>
    /// SpriteRendererコンポーネント
    /// </summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// 白駒スプライト
    /// </summary>
    public Sprite whiteSprite;

    /// <summary>
    /// 黒駒スプライト
    /// </summary>
    public Sprite blackSprite;

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event PlacePiece;
    [SerializeField] private AK.Wwise.Event FlipPiece;

    /// <summary>
    /// コンポーネント取得
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 駒の初期位置を設定する
    /// </summary>
    /// <param name="x">列インデックス</param>
    /// <param name="y">行インデックス</param>
    public void InitState(int x, int y)
    {
        state_X = x;
        state_Y = y;
    }

    /// <summary>
    /// 駒を裏返すアニメーションと音声
    /// </summary>
    public async UniTask Flip()
    {
        FlipPiece.Post(gameObject);
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("FlipWhiteToBlackTrigger");
            gameObject.tag = "Black";
            spriteRenderer.sprite = blackSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
            PlacePiece.Post(gameObject);
        }
        else
        {
            animator.SetTrigger("FlipBlackToWhiteTrigger");
            gameObject.tag = "White";
            spriteRenderer.sprite = whiteSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
            PlacePiece.Post(gameObject);
        }
    }
}
