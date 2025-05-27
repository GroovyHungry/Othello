using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;
using AK.Wwise;

/// <summary>
/// シーン遷移（フェードアウト→シーンロード→フェードイン）を管理し，BGMの変更をするクラス
/// </summary>
public class SceneTransition : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static SceneTransition Instance;

    /// <summary>
    /// フェードエフェクト用のイメージオブジェクト
    /// </summary>
    public GameObject EffectImage;

    /// <summary>
    /// フェードアニメーションを制御するAnimator
    /// </summary>
    public Animator EffectAnimator;

    /// <summary>
    /// 全てのSEを停止するWwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event StopAllSE;

    /// <summary>
    /// インスタンスの設定を行う
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定したシーンへフェードアウト→ロード→フェードインで遷移する
    /// </summary>
    /// <param name="nextSceneName">遷移先のシーン名</param>
    public async UniTask Transition(string nextSceneName)
    {
        float speed = 1.0f;
        AudioManager.Instance.TransitionBGM(nextSceneName);
        await PlayFadeOut(speed);
        StopAllSE.Post(gameObject);
        await SceneManager.LoadSceneAsync(nextSceneName);
        await PlayFadeIn(speed);
    }

    /// <summary>
    /// フェードアウトアニメーションを指定の速度に合わせて再生する
    /// </summary>
    /// <param name="speed">フェードアウトの速度</param>
    public async UniTask PlayFadeOut(float speed)
    {
        EffectImage.SetActive(true);
        EffectAnimator.speed = speed;
        EffectAnimator.SetTrigger("FadeOut");
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f/speed));
    }

    /// <summary>
    /// フェードインアニメーションを指定の速度に合わせて再生する
    /// </summary>
    /// <param name="speed">フェードインの速度</param>
    public async UniTask PlayFadeIn(float speed)
    {
        EffectAnimator.speed = speed;
        EffectAnimator.SetTrigger("FadeIn");
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f/speed));
        EffectImage.SetActive(false);
    }
}
