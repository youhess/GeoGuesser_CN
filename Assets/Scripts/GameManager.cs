using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using TMPro;
//using Cyan.PlayerObjectPool;
using VRC.Udon.Common;
using VRC.SDK3.Data;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp;
using System;
using Cyan.PlayerObjectPool;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameManager : UdonSharpBehaviour
{
    [Header("References")]
    public GameObject[] panoramaImages;
    private GameObject currentPanorama;
    public GameObject answerPinPrefab; // 这是 Prefab，不是直接的 GameObject
    private GameObject anwserPinInstance; // 存储实例化的对象
     // 改为使用 ObjectAssigner 而不是 ObjectPool
    public CyanPlayerObjectAssigner objectAssigner;


    //[Header("Pin System")]
    //public CyanPlayerObjectPool objectPool;
    //private GameObject[] playerPins;

    [Header("Panorama Settings")]
    //[SerializeField]
    //public VRCUrl[] imageUrls;
    // Initialize VRCUrl array

    //private VRCUrl[] imageUrls = { 
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/01.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/02.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/03.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/04.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/05.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/06.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/07.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/08.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/09.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/10.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/11.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/12.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/13.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/14.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/15.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/16.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/17.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/18.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/19.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/20.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/21.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/22.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/23.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/24.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/25.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/26.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/27.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/28.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/29.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/30.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/31.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/32.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/33.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/34.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/35.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/36.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/37.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/38.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/39.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/40.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/41.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/42.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/43.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/44.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/45.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/46.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/47.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/48.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/49.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/50.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/51.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/52.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/53.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/54.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/55.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/56.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/57.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/58.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/59.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/60.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/61.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/62.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/63.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/64.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/65.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/66.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/67.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/68.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/69.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/70.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/71.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/72.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/73.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/74.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/75.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/76.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/77.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/78.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/79.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/80.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/81.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/82.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/83.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/84.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/85.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/86.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/87.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/88.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/89.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/90.jpg"),
    //new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/91.jpg")
    //};

    private VRCUrl[] imageUrls = {
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/01.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/02.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/03.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/04.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/05.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/06.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/07.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/08.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/09.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/10.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/11.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/12.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/13.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/14.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/15.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/16.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/17.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/18.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/19.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/20.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/21.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/22.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/23.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/24.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/25.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/26.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/27.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/28.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/29.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/30.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/31.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/32.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/33.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/34.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/35.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/36.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/37.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/38.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/39.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/40.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/41.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/42.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/43.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/44.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/45.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/46.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/47.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/48.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/49.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/50.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/51.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/52.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/53.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/54.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/55.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/56.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/57.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/58.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/59.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/60.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/61.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/62.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/63.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/64.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/65.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/66.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/67.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/68.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/69.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/70.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/71.jpg"),
    new VRCUrl("https://gitee.com/youhess/Geoguesser-China-data/raw/main/72.jpg"),
};


    [SerializeField]
    private Renderer sphereRenderer;
    [UdonSynced]
    private int currentImageIndex = -1; // 当前显示的图片索引
    [UdonSynced]
    private int nextImageIndex = -1; // 下一轮将使用的图片索引

    private VRCImageDownloader imageDownloader;
    private Texture2D[] downloadedTextures;

    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI rulesText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waitingText;
    public GameObject startButton;

    // 添加设置面板UI组件
    public GameObject settingsPanel;          // 设置面板的父物体
    public UnityEngine.UI.Slider waitingTimeSlider;  // 准备阶段时间滑块
    public UnityEngine.UI.Slider roundTimeSlider;    // 猜测阶段时间滑块
    public UnityEngine.UI.Slider revealTimeSlider;   // 揭晓阶段时间滑块
    public UnityEngine.UI.Slider totalRoundsSlider;  // 总回合数滑块
    public TextMeshProUGUI waitingTimeText;   // 显示准备阶段时间的文本
    public TextMeshProUGUI roundTimeText;     // 显示猜测阶段时间的文本
    public TextMeshProUGUI revealTimeText;    // 显示揭晓阶段时间的文本
    public TextMeshProUGUI totalRoundsText;   // 显示总回合数的文本
    public TextMeshProUGUI LocationText;      // 显示地点信息的文本
    public UnityEngine.UI.Button applySettingsButton; // 应用设置按钮
    public UnityEngine.UI.Button toggleSettingsButton; // 切换设置面板按钮

    [Header("Game Data")]
    public LocationRoundData locationData;
    public LatLongMapper latLongMapper;
    public PinDataManager pinDataManager; // 用于同步玩家的经纬度信息

    [Header("Game Settings")]
    public int minPlayers = 1;
    [UdonSynced]
    public int totalRounds = 5;
    [Range(1, 10)]
    public int minRounds = 1;
    [Range(1, 20)]
    public int maxRounds = 10;

    [UdonSynced]
    public float waitingTime = 10f; // 准备阶段（默认10秒）
    [Range(5, 60)]
    public float minWaitingTime = 5f;
    [Range(5, 60)]
    public float maxWaitingTime = 30f;

    [UdonSynced]
    public float roundTime = 15f;   // 猜测阶段（默认15秒）
    [Range(10, 120)]
    public float minRoundTime = 10f;
    [Range(10, 120)]
    public float maxRoundTime = 60f;

    [UdonSynced]
    public float revealTime = 10f;  // 揭晓阶段（默认10秒）
    [Range(5, 60)]
    public float minRevealTime = 5f;
    [Range(5, 60)]
    public float maxRevealTime = 30f;


    [UdonSynced]
    private float countdownTimer = 0f;//倒计时
    [UdonSynced]
    private int countdownPhase = 0; // 0: 未开始, 1: 准备, 2: 猜测, 3: 揭晓


    //[Header("Score System")]
    //public int maxScore = 5000;
    //public float maxDistance = 1000f;

    [Header("Result Visualization")]
    public float resultShowDuration = 5f;
    private bool isShowingResults = false;

    // 同步变量
    [UdonSynced]
    private bool gameStarted = false;
    [UdonSynced]
    private int currentRound = 0;
    //[UdonSynced]
    //private float currentRoundTimeLeft;
    [UdonSynced]
    private bool isRoundActive = false;
    //[UdonSynced]
    //private Vector2[] currentRoundPinPositions;

    [UdonSynced]
    public int[] roundImageIndices; // 存储每个回合使用的图片索引


    // 本地变量
    private VRCPlayerApi localPlayer;
    //private float[] playerScores;
    private int localPlayerId;
    public Transform mapTableTransform;
    public RectTransform worldMapRectTransform;
    private const int MAX_PLAYERS = 16;

    // Initialize the settings UI
    private void InitializeSettingsUI()
    {
        //// Initially hide the settings panel
        //if (settingsPanel != null)
        //{
        //    settingsPanel.SetActive(false);
        //}

        // Setup sliders with current values
        if (waitingTimeSlider != null)
        {
            waitingTimeSlider.minValue = minWaitingTime;
            waitingTimeSlider.maxValue = maxWaitingTime;
            waitingTimeSlider.value = waitingTime;
            UpdateWaitingTimeText();
        }

        if (roundTimeSlider != null)
        {
            roundTimeSlider.minValue = minRoundTime;
            roundTimeSlider.maxValue = maxRoundTime;
            roundTimeSlider.value = roundTime;
            UpdateRoundTimeText();
        }

        if (revealTimeSlider != null)
        {
            revealTimeSlider.minValue = minRevealTime;
            revealTimeSlider.maxValue = maxRevealTime;
            revealTimeSlider.value = revealTime;
            UpdateRevealTimeText();
        }

        if (totalRoundsSlider != null)
        {
            totalRoundsSlider.minValue = minRounds;
            totalRoundsSlider.maxValue = maxRounds;
            totalRoundsSlider.value = totalRounds;
            UpdateTotalRoundsText();
        }
    }

    public void OnWaitingTimeSliderChanged()
{
    if (waitingTimeSlider != null && Networking.IsOwner(gameObject))
    {
        // 直接使用滑块值，不需要手动取整
        waitingTime = waitingTimeSlider.value;
        UpdateWaitingTimeText();
        RequestSerialization(); // 添加这行以立即同步更改
        }
}

public void OnRoundTimeSliderChanged()
{
    if (roundTimeSlider != null && Networking.IsOwner(gameObject))
    {
        // 直接使用滑块值，不需要手动取整
        roundTime = roundTimeSlider.value;
        UpdateRoundTimeText();
        RequestSerialization(); // 添加这行以立即同步更改
        }
}

public void OnRevealTimeSliderChanged()
{
    if (revealTimeSlider != null && Networking.IsOwner(gameObject))
    {
        // 直接使用滑块值，不需要手动取整
        revealTime = revealTimeSlider.value;
        UpdateRevealTimeText();
        RequestSerialization(); // 添加这行以立即同步更改
        }
}

public void OnTotalRoundsSliderChanged()
{
    if (totalRoundsSlider != null && Networking.IsOwner(gameObject))
    {
        // 直接使用滑块值，不需要手动取整
        totalRounds = (int)totalRoundsSlider.value;
        UpdateTotalRoundsText();
        RequestSerialization(); // 添加这行以立即同步更改
        }
}

    // Methods to update UI text
    private void UpdateWaitingTimeText()
    {
        if (waitingTimeText != null)
        {
            waitingTimeText.text = $"{waitingTime}";
        }
    }

    private void UpdateRoundTimeText()
    {
        if (roundTimeText != null)
        {
            roundTimeText.text = $"{roundTime}";
        }
    }

    private void UpdateRevealTimeText()
    {
        if (revealTimeText != null)
        {
            revealTimeText.text = $"{revealTime}";
        }
    }

    private void UpdateTotalRoundsText()
    {
        if (totalRoundsText != null)
        {
            totalRoundsText.text = $"{totalRounds}";
        }
    }

    // 目前没有用到，后期用来切换设置面板的显示状态
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    // 应用设置
    public void ApplySettings()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Debug.Log("只有房主可以应用设置");
            return;
        }

        // Update the synced variables
        waitingTime = waitingTimeSlider.value;
        roundTime = roundTimeSlider.value;
        revealTime = revealTimeSlider.value;
        totalRounds = Mathf.RoundToInt(totalRoundsSlider.value);

        // Sync to all clients
        RequestSerialization();

        Debug.Log($"设置已应用 - 准备时间: {waitingTime}秒, 猜测时间: {roundTime}秒, 揭晓时间: {revealTime}秒, 总回合数: {totalRounds}");

        // Hide settings panel after applying
        // 没必要隐藏设置面板，之后再添加
        //settingsPanel.SetActive(false);
    }


    void Start()
    {
        Debug.Log("[GameManager] 初始化完成");

        localPlayer = Networking.LocalPlayer; // 获取本地玩家

        if (localPlayer != null)
        {
            localPlayerId = localPlayer.playerId;
        }

        imageDownloader = new VRCImageDownloader();
        downloadedTextures = new Texture2D[imageUrls.Length];

        // Initialize settings UI
        InitializeSettingsUI();

        UpdateStartButtonState();


        if (waitingText != null)
        {
            waitingText.text = "";
        }
    }

    private void Update()
    {
        if (!gameStarted) return; // 游戏未开始

        if (countdownTimer > 0)
        {
            countdownTimer -= Time.deltaTime;
            waitingText.text = GetCountdownText();
        }
        else
        {
            HandleCountdownEnd();
        }

    }

    public void StartGame()
    {
        if (!Networking.IsOwner(gameObject)) return; // 只有房主才能开始游戏

        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);


        if (players.Length < minPlayers) return; // 玩家数量不足

        // 初始化 PinDataManager
        if (pinDataManager != null)
        {
            pinDataManager.InitializeRounds(totalRounds);
        }

        // 初始化回合图片索引数组
        roundImageIndices = new int[totalRounds];
        for (int i = 0; i < totalRounds; i++)
        {
            roundImageIndices[i] = -1; // 使用-1表示尚未分配
        }

        // 重置分数 对局重新开始
        gameStarted = true; 
        currentRound = 0;
        // 进入准备阶段
        countdownPhase = 1; // 进入准备阶段
        countdownTimer = waitingTime; // 设置倒计时
        ResetScores(); 

        if (waitingText != null)
        {
            waitingText.text = "";
        }

      
        waitingText.text = "Preparing！";

        //RequestSerialization();

        // 需要在StartGame阶段设置currentImageIndex，可以优化图片的加载
        //currentImageIndex = UnityEngine.Random.Range(0, imageUrls.Length);
        //Debug.Log($"[GameManager] 开始第 {currentRound} 轮，加载图片 {currentImageIndex}");

        RequestSerialization();

        //// 然后触发所有客户端加载图片
        //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkLoadPanorama));

        // 同时预加载第一轮的图片
        PreloadNextImage();
    }

    private string GetCountdownText()
    {
        switch (countdownPhase)
        {
            case 1: return $"Preparing: {Mathf.CeilToInt(countdownTimer)}";
            case 2: return $"Guessing Time: {Mathf.CeilToInt(countdownTimer)}";
            case 3: return $"Answer Time: {Mathf.CeilToInt(countdownTimer)}";
            default: return "";
        }
    }

    private void HandleCountdownEnd()
    {
        if (!Networking.IsOwner(gameObject)) return;

        switch (countdownPhase)
        {
            case 1:
                StartGuessingPhase(); // 进入猜测阶段
                break;
            case 2:
                RevealAnswer(); // 揭晓答案
                break;
            case 3:
                StartNewRound(); // 进入新一轮
                break;
        }
    }

    private void StartGuessingPhase()
    {
        if (!Networking.IsOwner(gameObject)) return;

        // 使用预加载的图片索引
        if (nextImageIndex >= 0)
        {
            currentImageIndex = nextImageIndex;
            nextImageIndex = -1; // 重置预加载索引
        }
        else
        {
            // 如果没有预加载，随机选择一个
            currentImageIndex = UnityEngine.Random.Range(0, imageUrls.Length);
        }

        // 记录当前回合的图片索引
        roundImageIndices[currentRound] = currentImageIndex;

        RequestSerialization();
        // 显示图片
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkLoadPanorama));
     

        countdownPhase = 2;
        countdownTimer = roundTime; //设置猜测时间
        RequestSerialization();

        
        waitingText.text = $"猜测时间！{roundTime} 秒";

        
    }

    private void RevealAnswer()
    {
      
        if (!Networking.IsOwner(gameObject)) return;

        // 在显示答案之前，保存当前回合的所有玩家答案，然后计算所有玩家的得分
        pinDataManager.SaveRoundAnswers(currentRound);
        // 计算当前回合的得分并显示
        pinDataManager.CalculateRoundScores(this, currentRound);

       

        // 设置倒计时和阶段
        countdownPhase = 3;
        countdownTimer = revealTime;
        RequestSerialization();

        //// 触发所有客户端更新Pin
        //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateAnswerPinAll));


        //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(pinDataManager.SetShowAllPins));
        ////pinDataManager.SetShowAllPins(true);
        //// 添加新的网络事件调用，用于显示所有Pin的连线
        //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ShowAllPinLines));

        //// 显示地点信息
        //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateLocationInfo));

        // 4. 发送单个网络事件来更新所有客户端的UI和状态
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateAnswerPinAndLineAndPhaseUI));

        // 在这里预加载下一轮的图片
        PreloadNextImage();

    }

    // 新增方法，处理揭晓阶段的所有UI更新
    public void UpdateAnswerPinAndLineAndPhaseUI()
    {
        // 更新答案Pin
        UpdateAnswerPinAll();

        // 显示所有玩家的Pin
        pinDataManager.SetShowAllPins();

        // 显示Pin之间的连线
        ShowAllPinLines();

        // 更新地点信息
        UpdateLocationInfo();
    }

    private void PreloadNextImage()
    {
        // 选择下一轮的图片
        nextImageIndex = UnityEngine.Random.Range(0, imageUrls.Length);

        // 确保同步 nextImageIndex 到所有客户端
        RequestSerialization();

        // 在后台开始加载图片
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PreloadImageForNextRound));
    }

    public void PreloadImageForNextRound()
    {
        if (nextImageIndex >= 0 && nextImageIndex < imageUrls.Length)
        {
            Debug.Log($"[GameManager] 预加载下一轮图片索引: {nextImageIndex}");

            // 检查是否已缓存该图片
            if (downloadedTextures[nextImageIndex] != null)
            {
                Debug.Log($"[GameManager] 图片 {nextImageIndex} 已缓存，无需预加载");
                return;
            }

            // 使用与 NetworkLoadPanorama 相同的方式，但不直接应用到主渲染器
            var textureInfo = new TextureInfo();
            textureInfo.GenerateMipMaps = true;

            // 下载图片但不直接应用到渲染器
            imageDownloader.DownloadImage(
                imageUrls[nextImageIndex],
                null, // 不指定材质，仅下载
                (IUdonEventReceiver)this,
                textureInfo
            );
        }
        else
        {
            Debug.LogError($"[GameManager] 无效的预加载图片索引: {nextImageIndex}");
        }
    }

    // 添加显示所有Pin连线的方法
    public void ShowAllPinLines()
    {
        // 等待一小段时间确保 answerPinInstance 已经创建
        SendCustomEventDelayedSeconds(nameof(DelayedShowPinLines), 0.2f);
    }

    // 延迟执行的方法，设置所有Pin的连线
    public void DelayedShowPinLines()
    {
        if (anwserPinInstance == null)
        {
            Debug.LogError("[GameManager] 答案Pin不存在，无法创建连线");
            return;
        }

        // 通过ObjectAssigner找到所有活动的Pin
        Component[] activePoolObjects = objectAssigner._GetActivePoolObjects();
        if (activePoolObjects == null || activePoolObjects.Length == 0)
        {
            Debug.LogWarning("[GameManager] 没有活动的Pin对象");
            return;
        }

        foreach (Component poolObject in activePoolObjects)
        {
            if (poolObject == null) continue;

            // 获取PinController组件
            PinController pinController = poolObject.GetComponent<PinController>();
            if (pinController != null)
            {
                // 设置答案Pin并显示连线
                pinController.SetAnswerPin(anwserPinInstance);
                // 设置答案Pin并显示连线
                pinController.ShowLineToAnswer(true);
            }
        }
    }

    // 隐藏所有Pin连线的方法
    public void HideAllPinLines()
    {
        Component[] activePoolObjects = objectAssigner._GetActivePoolObjects();
        if (activePoolObjects == null) return;

        foreach (Component poolObject in activePoolObjects)
        {
            if (poolObject == null) continue;

            PinController pinController = poolObject.GetComponent<PinController>();
            if (pinController != null)
            {
                pinController.ShowLineToAnswer(false);
            }
        }
    }

    //更新地点信息
    public void UpdateLocationInfo()
    {
       string cnname = locationData.GetLocationCnName(currentImageIndex);
       string enname = locationData.GetLocationEnName(currentImageIndex);
        LocationText.text = $"{cnname} \n ({enname})";
    }

    // 添加更新答案Pin的方法
    public void UpdateAnswerPinAll()
{

    Vector2 answerPosition = locationData.GetLocationLatLong(currentImageIndex); // currentImageIndex是当前回合的图片索引

        // 计算世界空间位置
    Vector3[] corners = new Vector3[4];
    worldMapRectTransform.GetWorldCorners(corners);
    Vector3 bottomLeft = corners[0];
    Vector3 topRight = corners[2];
    Vector3 mapSize = topRight - bottomLeft;
    
    // 计算Pin位置
    Vector2 normalizedPos = latLongMapper.LatLongToUICoords(answerPosition);
    normalizedPos.x = (normalizedPos.x + worldMapRectTransform.rect.width * 0.5f) / worldMapRectTransform.rect.width;
    normalizedPos.y = (normalizedPos.y + worldMapRectTransform.rect.height * 0.5f) / worldMapRectTransform.rect.height;
    
    Vector3 worldPos = bottomLeft + new Vector3(
        mapSize.x * normalizedPos.x,
        0.4f,
        mapSize.z * normalizedPos.y
    );

    // 确保Pin实例存在
    if (anwserPinInstance == null)
    {
        anwserPinInstance = VRCInstantiate(answerPinPrefab);
        anwserPinInstance.transform.SetParent(mapTableTransform.parent, false);
        
        float mapDiagonal = (corners[2] - corners[0]).magnitude;
        float scaleFactor = mapDiagonal * 0.01f;
        anwserPinInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        
        Rigidbody rb = anwserPinInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

        // 验证实例化是否成功
        if (anwserPinInstance == null)
        {
            Debug.LogError("[GameManager] 答案Pin实例化失败!");
            return;
        }

        // 更新Pin位置和显示
        anwserPinInstance.transform.position = worldPos;
    anwserPinInstance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    anwserPinInstance.SetActive(true);
    
    // 更新UI
    waitingText.text = $"答案揭晓！{revealTime} 秒";
    UpdateResultsUI(answerPosition);
}

    private void StartNewRound()
    {
        if (!Networking.IsOwner(gameObject)) return; // 只有房主可以开始新回合

        if (currentRound >= totalRounds - 1)
        {
            EndGame();
            return;                 
        }

        // 隐藏除了自己的pin的所有pin
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(pinDataManager.SetHideOtherPins));
        // 隐藏所有Pin连线
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HideAllPinLines));

        // 进入新回合
        isRoundActive = true;
        currentRound++; // 更新回合数

        if (currentRound == 0) // 只有第一轮有准备时间
        {
            // 隐藏答案Pin
            if (anwserPinInstance != null)
            {
                anwserPinInstance.SetActive(false);
            }

            countdownPhase = 1;
            countdownTimer = waitingTime;
            waitingText.text = $"准备中... {Mathf.CeilToInt(waitingTime)} 秒后开始！";
        }
        else // 直接进入猜测阶段，不再等待
        {
            // 隐藏答案Pin
            if (anwserPinInstance != null)
            {
                anwserPinInstance.SetActive(false);
            }

            countdownPhase = 2;
            countdownTimer = roundTime;
            waitingText.text = $"猜测时间！{Mathf.CeilToInt(roundTime)} 秒";
        }

        //// 为新回合加载新图片
        //currentImageIndex = UnityEngine.Random.Range(0, imageUrls.Length);
        //Debug.Log($"[GameManager] 开始第 {currentRound} 轮，加载图片 {currentImageIndex}");
        RequestSerialization();

        // 使用预加载的图片索引
        if (nextImageIndex >= 0)
        {
            currentImageIndex = nextImageIndex;
            nextImageIndex = -1; // 重置预加载索引
        }
        else
        {
            // 如果没有预加载，随机选择一个
            currentImageIndex = UnityEngine.Random.Range(0, imageUrls.Length);
        }
        // 记录新回合的图片索引
        roundImageIndices[currentRound] = currentImageIndex;

        RequestSerialization();
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkLoadPanorama));
    }


 

    //private void ShowRoundResults(Vector2 targetPos)
    //{
    //    Debug.Log($"Showing results for round {currentRound + 1}");
    //    isShowingResults = true;

    //    Vector2 worldPos = latLongMapper.LatLongToUICoords(targetPos);
    //    answerPinPrefab.transform.localPosition = new Vector3(worldPos.x, 0, worldPos.y);
    //    answerPinPrefab.SetActive(true); // 显示答案
    //    UpdateResultsUI(targetPos); //更新结果UI

    //}

    private void UpdateResultsUI(Vector2 targetPos)
    {
        if (rulesText != null)
        {
            string resultsText = $"Round {currentRound + 1} Results\n";
            resultsText += $"Correct Location: Lat {targetPos.x:F2}, Long {targetPos.y:F2}\n\n";
            rulesText.text = resultsText;
        }
    }

    public void NetworkLoadPanorama()
    {
        Debug.Log($"NetworkLoadPanorama - Player: {localPlayer.displayName}, Round: {currentRound}, Index: {currentImageIndex}");

        if (currentImageIndex >= 0 && currentImageIndex < imageUrls.Length && sphereRenderer != null)
        {
            var textureInfo = new TextureInfo();
            textureInfo.GenerateMipMaps = true;
            imageDownloader.DownloadImage(
                imageUrls[currentImageIndex],
                sphereRenderer.material,
                (IUdonEventReceiver)this,
                textureInfo
            );
            Debug.Log($"Started downloading image {currentImageIndex}");
        }
        else
        {
            Debug.LogError($"Invalid state - ImageIndex: {currentImageIndex}, Renderer: {sphereRenderer != null}");
        }
    }


    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        string resultString = result.Result.ToString();
        Debug.Log($"Image load success: {result.SizeInMemoryBytes} bytes: {resultString}");

        // 找出这是哪个URL对应的图片
        int imageIndex = -1;

        // 从结果字符串中提取URL部分
        if (resultString.Contains("ImageFrom:"))
        {
            string resultUrl = resultString.Split(new string[] { "ImageFrom:" }, StringSplitOptions.None)[1];
            resultUrl = resultUrl.Split(' ')[0]; // 移除后面的 (UnityEngine.Texture2D) 部分

            // 匹配我们的URL数组
            for (int i = 0; i < imageUrls.Length; i++)
            {
                if (resultUrl == imageUrls[i].ToString())
                {
                    imageIndex = i;
                    Debug.Log($"匹配到图片索引: {i} 对应URL: {resultUrl}");
                    break;
                }
            }
        }

        // 如果找到了对应的索引
        if (imageIndex >= 0)
        {
            // 无论是当前图片还是预加载的图片，都保存到缓存中
            downloadedTextures[imageIndex] = result.Result;

            // 如果是当前正在显示的图片，应用到渲染器
            if (imageIndex == currentImageIndex && sphereRenderer != null)
            {
                sphereRenderer.material.SetTexture("_MainTex", result.Result);
                Debug.Log($"Applied current texture (index {imageIndex}) for player {localPlayer.displayName}");
            }
            else
            {
                Debug.Log($"Cached preloaded texture for index {imageIndex}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not determine index for downloaded image: {resultString}");
        }
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.LogError($"Failed to load image: {result.Error}");
    }

    private void EndGame()
    {
        gameStarted = false;
        isRoundActive = false;
        RequestSerialization(); 
        //if (waitingText != null)
        //{
        //    waitingText.text = "Game Over!";
        //}

        // Send network event to all clients to show game over
     

        


        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ShowGameOverAndFinalScores));

    }

    public void ShowGameOverAndFinalScores()
    {
        Debug.Log("Game Over!");
        if (waitingText != null)
        {
            waitingText.text = "Game Over!";
        }

        // 计算并显示最终得分
        pinDataManager.CalculateFinalScores(this);

    }

    private void ResetScores()
    {
        // TODO: Reset scores for all players  把PinDataManager里的ResetScores方法拷贝过来
        //for (int i = 0; i < MAX_PLAYERS; i++)
        //{
        //    playerScores[i] = 0;
        //}
    }

    private void UpdateGameUI()
    {
        if (rulesText != null)
        {
            string status = isRoundActive ?
                $"Round {currentRound + 1}/{totalRounds}\nTime Left: {Mathf.CeilToInt(countdownTimer)}s" :
                "Waiting for next round...";
            rulesText.text = status;
        }
    }


    public void UpdateStartButtonState()
    {
        if (startButton == null) return;

        if (gameStarted)
        {
            startButton.SetActive(false);
            return;
        }

        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        bool canStart = players.Length >= minPlayers;
        bool isOwner = Networking.IsOwner(gameObject);

        startButton.SetActive(true);
        startButton.GetComponent<UnityEngine.UI.Button>().interactable = isOwner && canStart;

        TextMeshProUGUI buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText)
        {
            buttonText.text = !isOwner ? "Owner Only" :
                            !canStart ? $"Need {minPlayers} players" :
                            "Start Game";
        }

        // 更新应用设置按钮
        if (applySettingsButton != null)
        {
            applySettingsButton.interactable = isOwner;

            // 可选：添加说明文本
            TextMeshProUGUI settingsButtonText = applySettingsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (settingsButtonText && !isOwner)
            {
                settingsButtonText.text = "Owner Only";
            }
            else if (settingsButtonText)
            {
                settingsButtonText.text = "Apply Settings";
            }
        }

    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        UpdateStartButtonState();

        if (player == localPlayer && gameStarted)
        {
            if (waitingText != null)
            {
                waitingText.text = $"Game in progress.\nPlease wait for next round.\n\n" +
                                 $"Current Round: {currentRound + 1}/{totalRounds}\n" +
                                 $"Time Left: {Mathf.CeilToInt(countdownTimer)}s";
            }
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        UpdateStartButtonState();
        Debug.Log($"Player {player.displayName} left");
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        UpdateStartButtonState();
        Debug.Log($"Ownership transferred to {player.displayName}");
    }

    private void OnDestroy()
    {
        if (imageDownloader != null)
        {
            imageDownloader.Dispose();
        }
    }


    public override void OnDeserialization() 
    {
        Debug.Log($"[GameManager] OnDeserialization - currentImageIndex: {currentImageIndex}, Phase: {countdownPhase}");

        // 如果图片索引有效且发生了变化，重新加载图片
        if (currentImageIndex >= 0 && currentImageIndex < imageUrls.Length)
        {
            NetworkLoadPanorama();
        }

        // 如果 nextImageIndex 有效，开始预加载
        if (nextImageIndex >= 0 && nextImageIndex < imageUrls.Length)
        {
            PreloadImageForNextRound();
        }

        // Update the UI to reflect any changes in settings
        UpdateSettingsUI();

    }

    // Add this method to update the UI based on synced values
    private void UpdateSettingsUI()
    {
        // Update sliders to match synced values
        if (waitingTimeSlider != null)
        {
            waitingTimeSlider.value = waitingTime;
            UpdateWaitingTimeText();
        }

        if (roundTimeSlider != null)
        {
            roundTimeSlider.value = roundTime;
            UpdateRoundTimeText();
        }

        if (revealTimeSlider != null)
        {
            revealTimeSlider.value = revealTime;
            UpdateRevealTimeText();
        }

        if (totalRoundsSlider != null)
        {
            totalRoundsSlider.value = totalRounds;
            UpdateTotalRoundsText();
        }
    }

    public Vector2 GetRoundAnswer(int roundIndex)
    {
        if (roundIndex >= 0 && roundIndex < roundImageIndices.Length && roundImageIndices[roundIndex] >= 0)
        {
            // 使用该回合实际使用的图片索引
            int imageIndex = roundImageIndices[roundIndex];
            return locationData.GetLocationLatLong(imageIndex);
        }

        // 回退到当前图片索引（如果回合索引无效）
        return locationData.GetLocationLatLong(currentImageIndex);
    }

    // 在 GameManager 类中添加
    public int[] GetRoundImageIndices()
    {
        return roundImageIndices;
    }

    public int GetImageUrlsLength()
    {
        return imageUrls.Length;
    }

    public int GetCurrentImageIndex()
    {
        return currentImageIndex;
    }
}