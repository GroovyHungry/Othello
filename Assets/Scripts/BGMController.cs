using UnityEngine;
using System.Linq;
using AK.Wwise;
using UnityEngine.SceneManagement;
public class BGMController : MonoBehaviour
{
    public static BGMController Instance;
    public Bank othelloBank;
    public RTPC pieceDifferenceRTPC;
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
    public void ChangeBGM()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OthelloBoard")
        {
            Debug.Log("Changed BGM");
            int whiteCount = OthelloManager.Instance.CountPieces(true);
            int blackCount = OthelloManager.Instance.CountPieces(false);
            int diff = blackCount - whiteCount;
            float clamped = Mathf.Clamp(diff, -10, 10);

            pieceDifferenceRTPC.SetGlobalValue(clamped);
        }
    }
    public void TransitionBGM(string trackName)
    {
        AkSoundEngine.SetSwitch("SceneType", trackName, gameObject);
    }

}
