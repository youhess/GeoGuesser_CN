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

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameManager : UdonSharpBehaviour
{
    [Header("References")]
    public GameObject[] panoramaImages;
    private GameObject currentPanorama;
    public GameObject answerPinPrefab; // 这是 Prefab，不是直接的 GameObject
    private GameObject anwserPinInstance; // 存储实例化的对象


    //[Header("Pin System")]
    //public CyanPlayerObjectPool objectPool;
    //private GameObject[] playerPins;

    [Header("Panorama Settings")]
    //[SerializeField]
    //public VRCUrl[] imageUrls;
    // Initialize VRCUrl array
    private VRCUrl[] imageUrls = { 
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/01.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/02.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/03.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/04.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/05.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/06.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/07.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/08.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/09.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/10.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/11.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/12.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/13.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/14.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/15.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/16.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/17.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/18.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/19.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/20.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/21.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/22.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/23.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/24.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/25.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/26.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/27.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/28.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/29.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/30.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/31.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/32.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/33.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/34.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/35.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/36.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/37.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/38.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/39.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/40.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/41.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/42.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/43.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/44.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/45.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/46.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/47.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/48.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/49.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/50.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/51.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/52.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/53.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/54.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/55.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/56.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/57.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/58.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/59.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/60.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/61.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/62.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/63.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/64.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/65.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/66.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/67.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/68.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/69.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/70.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/71.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/72.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/73.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/74.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/75.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/76.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/77.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/78.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/79.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/80.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/81.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/82.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/83.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/84.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/85.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/86.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/87.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/88.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/89.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/90.jpg"),
    new VRCUrl("https://raw.githubusercontent.com/youhess/Geoguesser-China-data/main/91.jpg")
    };

    [SerializeField]
    private Renderer sphereRenderer;
    [UdonSynced]
    private int currentImageIndex = -1; // 当前显示的图片索引
    private VRCImageDownloader imageDownloader;
    private Texture2D[] downloadedTextures;

    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI rulesText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waitingText;
    public GameObject startButton;

    [Header("Game Data")]
    public LocationRoundData locationData;
    public LatLongMapper latLongMapper;
    public PinDataManager pinDataManager; // 用于同步玩家的经纬度信息

    [Header("Game Settings")]
    public int minPlayers = 1;
    public int totalRounds = 5;

    public float waitingTime = 2f; //准备阶段（默认10秒）
    public float roundTime = 2f;   //猜测阶段（默认15秒）
    public float revealTime = 10f;  //揭晓阶段（默认10秒）

    [UdonSynced]
    private float countdownTimer = 0f;//倒计时
    [UdonSynced]
    private int countdownPhase = 0; // 0: 未开始, 1: 准备, 2: 猜测, 3: 揭晓


    [Header("Score System")]
    public int maxScore = 5000;
    public float maxDistance = 1000f;

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

    // 本地变量
    private VRCPlayerApi localPlayer;
    //private float[] playerScores;
    private int localPlayerId;
    public Transform mapTableTransform;
    public RectTransform worldMapRectTransform;
    private const int MAX_PLAYERS = 16;

    void Start()
    {
        Debug.Log("[GameManager] 初始化完成");

        localPlayer = Networking.LocalPlayer;
        if (localPlayer != null)
        {
            localPlayerId = localPlayer.playerId;
        }

        imageDownloader = new VRCImageDownloader();
        downloadedTextures = new Texture2D[imageUrls.Length];

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

        // 重置分数 对局重新开始
        gameStarted = true; 
        currentRound = 0;
        ResetScores(); 

        if (waitingText != null)
        {
            waitingText.text = "";
        }

        countdownPhase = 1; // 进入准备阶段
        countdownTimer = waitingTime; // 设置倒计时
        waitingText.text = "准备中... 10 秒后开始！";

        RequestSerialization();
    }

    private string GetCountdownText()
    {
        switch (countdownPhase)
        {
            case 1: return $"准备中... {Mathf.CeilToInt(countdownTimer)} 秒";
            case 2: return $"猜测时间！{Mathf.CeilToInt(countdownTimer)} 秒";
            case 3: return $"答案揭晓！{Mathf.CeilToInt(countdownTimer)} 秒";
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

        // TODO: 之后可以用更加先进的算法来选择图片
        currentImageIndex = UnityEngine.Random.Range(0, imageUrls.Length);
        Debug.Log($"[GameManager] 开始第 {currentRound} 轮，加载图片 {currentImageIndex}");
        RequestSerialization();


        //LoadRoundImage(currentImageIndex); // 切换新图片

        //// 然后触发所有客户端加载图片
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkLoadPanorama));

        // 等待一小段时间后再触发图片加载
        //SendCustomEventDelayedSeconds(nameof(NetworkLoadPanorama), 0.5f);

        countdownPhase = 2;
        countdownTimer = roundTime; //设置猜测时间
        waitingText.text = $"猜测时间！{roundTime} 秒";

        
    }

    private void RevealAnswer()
    {
      
        if (!Networking.IsOwner(gameObject)) return;

        // 在显示答案之前，保存当前回合的所有玩家答案
        pinDataManager.SaveRoundAnswers(currentRound);

       


        // 设置倒计时和阶段
        countdownPhase = 3;
        countdownTimer = revealTime;
        RequestSerialization();

        // 触发所有客户端更新Pin
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateAnswerPinAll));


        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(pinDataManager.SetShowAllPins));
        //pinDataManager.SetShowAllPins(true);
    }

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

        // 为新回合加载新图片
        currentImageIndex = UnityEngine.Random.Range(0, imageUrls.Length);
        Debug.Log($"[GameManager] 开始第 {currentRound} 轮，加载图片 {currentImageIndex}");
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
        Debug.Log($"Image load success: {result.SizeInMemoryBytes} bytes");

        if (sphereRenderer != null)
        {
            sphereRenderer.material.SetTexture("_MainTex", result.Result);
            downloadedTextures[currentImageIndex] = result.Result;
            Debug.Log($"Applied texture for player {localPlayer.displayName}");
        }
        else
        {
            Debug.LogError("SphereRenderer is null!");
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

        //if (waitingText != null)
        //{
        //    waitingText.text = "Game Over!";
        //}

        // Send network event to all clients to show game over
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ShowGameOver));

        // 计算并显示最终得分
        pinDataManager.CalculateFinalScores(this);

        RequestSerialization();
    }

    public void ShowGameOver()
    {
        Debug.Log("Game Over!");
        if (waitingText != null)
        {
            waitingText.text = "Game Over!";
        }
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
            buttonText.text = !isOwner ? "Waiting for host" :
                            !canStart ? $"Need {minPlayers} players" :
                            "Start Game";
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

    }

    // 提供给 PinDataManager 用于获取每轮正确答案的方法
    public Vector2 GetRoundAnswer(int roundIndex)
    {
        return locationData.GetLocationLatLong(roundIndex);
    }
}