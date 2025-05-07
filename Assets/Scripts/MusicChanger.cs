using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

public class MusicChanger : MonoBehaviour
{
    public static MusicChanger Instance;
    public Bank othelloBank;
    public GameObject musicChangerPanel;
    public Button selectRightButton;
    public Button selectLeftButton;
    public Image NumBox;
    public Sprite[] numSprites;
    public Button musicChangerCloseButton;
    public int maxBGMNum = 1;
    public int minBGMNum = 0;
    public static int BGMNum = 0;
    public RTPC BGMNumRTPC;
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
        selectLeftButton.onClick.AddListener(SelectRight);
        selectRightButton.onClick.AddListener(SelectLeft);
        musicChangerCloseButton.onClick.AddListener(CloseMusicChanger);
    }
    void Start()
    {
        musicChangerPanel.SetActive(false);
        othelloBank.Load();
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void OnDestroy()
    {
        selectLeftButton.onClick.RemoveListener(SelectRight);
        selectRightButton.onClick.RemoveListener(SelectLeft);
        musicChangerCloseButton.onClick.RemoveListener(CloseMusicChanger);
    }
    public void OpenMusicChangerPanel()
    {
        UpdateBGMNum(BGMNum);
        musicChangerPanel.SetActive(true);
    }
    public void ChangeMusic()
    {
        BGMNumRTPC.SetGlobalValue(BGMNum);
    }
    private void SelectRight()
    {
        AkSoundEngine.PostEvent("OnClick", selectRightButton.gameObject);
        if (BGMNum == maxBGMNum)
        {
            BGMNum = 0;
        }
        else
        {
            BGMNum ++;
        }
        ChangeMusic();
        UpdateBGMNum(BGMNum);
    }
    private void SelectLeft()
    {
        AkSoundEngine.PostEvent("OnClick", selectLeftButton.gameObject);
        if (BGMNum == minBGMNum)
        {
            BGMNum = maxBGMNum;
        }
        else
        {
            BGMNum --;
        }
        ChangeMusic();
        UpdateBGMNum(BGMNum);
    }
    private void UpdateBGMNum(int BGMNum)
    {
        NumBox.sprite = numSprites[BGMNum];
    }
    public void CloseMusicChanger()
    {
        AkSoundEngine.PostEvent("OnClick", musicChangerCloseButton.gameObject);
        musicChangerPanel.SetActive(false);
    }
}
