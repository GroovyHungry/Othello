using UnityEngine;
using System.Linq;
using AK.Wwise;
using UnityEngine.SceneManagement;
public class BGMController : MonoBehaviour
{
    public static BGMController Instance;
    public Bank othelloBank;
    public RTPC pieceDifferenceRTPC;
    public RTPC gameProgressRTPC;
    [SerializeField] AK.Wwise.Event playBGMEvent;
    [SerializeField] AK.Wwise.Event stopBGMEvent;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも残す
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        othelloBank.Load();
        PlayBGM();
        AkSoundEngine.SetSwitch("SceneType", "MainMenu", gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayBGM()
    {
        playBGMEvent.Post(gameObject);
    }
    public void StopBGM()
    {
        stopBGMEvent.Post(gameObject);
    }
    public void ChangeBGM_1()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OthelloBoard")
        {
            int whiteCount = OthelloBoard.Instance.CountPieces(true);
            int blackCount = OthelloBoard.Instance.CountPieces(false);
            int diff = blackCount - whiteCount;
            float clamped = Mathf.Clamp(diff, -10, 10);

            pieceDifferenceRTPC.SetGlobalValue(clamped);
        }
    }
    public void ChangeBGM_2()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OthelloBoard")
        {
            int whiteCount = OthelloBoard.Instance.CountPieces(true);
            int blackCount = OthelloBoard.Instance.CountPieces(false);
            int progress = blackCount + whiteCount;

            gameProgressRTPC.SetGlobalValue(progress);
        }
    }
    public void TransitionBGM(string trackName)
    {
        AkSoundEngine.SetSwitch("SceneType", trackName, gameObject);
        ChangeBGM_1();
        ChangeBGM_2();
    }
}
