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
    [SerializeField]
    public VRCUrl[] imageUrls;
    [SerializeField]
    private Renderer sphereRenderer;
    [UdonSynced, System.NonSerialized]
    private int currentImageIndex = -1;
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

    [Header("Game Settings")]
    public int minPlayers = 1;
    public int totalRounds = 5;

    public float waitingTime = 1f; //准备阶段（默认10秒）
    public float roundTime = 1f;   //猜测阶段（默认15秒）
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

        currentImageIndex = currentRound;  // 切换到新回合的图片

        Debug.Log($"[GameManager] 开始第 {currentRound} 轮，加载图片 {currentImageIndex}");

        LoadRoundImage(currentImageIndex); // 切换新图片


        countdownPhase = 2;
        countdownTimer = roundTime; //设置猜测时间
        waitingText.text = $"猜测时间！{roundTime} 秒";

        RequestSerialization();
    }

    //private void RevealAnswer()
    //{
    //    Vector2 answerPosition = locationData.GetLocationLatLong(currentRound);
    //    Debug.Log($"Revealing answer for round {currentRound + 1}: {answerPosition}");

    //    if (answerPinPrefab == null || worldMapRectTransform == null || mapTableTransform == null)
    //    {
    //        Debug.LogError("Required references are missing! Please check inspector.");
    //        return;
    //    }

    //    // 获取地图在世界空间中的位置
    //    Vector3[] corners = new Vector3[4];
    //    worldMapRectTransform.GetWorldCorners(corners);

    //    // 计算地图的实际大小和中心点
    //    Vector3 bottomLeft = corners[0];
    //    Vector3 topRight = corners[2];
    //    Vector3 mapSize = topRight - bottomLeft;
    //    Vector3 mapCenter = (bottomLeft + topRight) * 0.5f;

    //    // 实例化答案Pin
    //    if (anwserPinInstance == null)
    //    {
    //        anwserPinInstance = VRCInstantiate(answerPinPrefab);
    //        if (anwserPinInstance == null)
    //        {
    //            Debug.LogError("Answer pin instantiation failed!");
    //            return;
    //        }

    //        // 将Pin放在MapArea下
    //        anwserPinInstance.transform.SetParent(mapTableTransform.parent, false);

    //        // 根据地图大小计算Pin的缩放
    //        float mapDiagonal = mapSize.magnitude;
    //        float scaleFactor = mapDiagonal * 0.01f; // 地图对角线长度的1%
    //        anwserPinInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

    //        // 禁用物理组件
    //        Rigidbody rb = anwserPinInstance.GetComponent<Rigidbody>();
    //        if (rb != null)
    //        {
    //            rb.isKinematic = true;
    //            rb.useGravity = false;
    //        }
    //    }

    //    // 将经纬度转换为UI坐标
    //    Vector2 normalizedPos = latLongMapper.LatLongToUICoords(answerPosition);

    //    // 将UI坐标归一化到0-1范围
    //    normalizedPos.x = (normalizedPos.x + worldMapRectTransform.rect.width * 0.5f) / worldMapRectTransform.rect.width;
    //    normalizedPos.y = (normalizedPos.y + worldMapRectTransform.rect.height * 0.5f) / worldMapRectTransform.rect.height;

    //    // 计算Pin在世界空间中的位置
    //    Vector3 worldPos = bottomLeft + new Vector3(
    //        mapSize.x * normalizedPos.x,
    //        0.4f, // Pin高度，可以调整这个值
    //        mapSize.z * normalizedPos.y
    //    );

    //    // 设置Pin的位置和旋转
    //    anwserPinInstance.transform.position = worldPos;
    //    anwserPinInstance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

    //    // 确保Pin可见
    //    anwserPinInstance.SetActive(true);

    //    Debug.Log($"Pin placed at world position: {worldPos}");
    //    isShowingResults = true;

    //    UpdateResultsUI(answerPosition);

    //    countdownPhase = 3;
    //    countdownTimer = revealTime;
    //    waitingText.text = $"答案揭晓！{revealTime} 秒";

    //    RequestSerialization();
    //}

    private void RevealAnswer()
    {
        if (!Networking.IsOwner(gameObject)) return;

        // 设置倒计时和阶段
        countdownPhase = 3;
        countdownTimer = revealTime;
        RequestSerialization();

        // 触发所有客户端更新Pin
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateAnswerPinAll));
    }

    public void UpdateAnswerPinAll()
{
    // 获取当前回合的正确位置
    Vector2 answerPosition = locationData.GetLocationLatLong(currentRound);
    
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
        //if (!Networking.IsOwner(gameObject)) return; // 只有房主可以开始新回合

        if (currentRound >= totalRounds)
        {
            EndGame();
            return;
        }

        // 进入新回合
        isRoundActive = true;
        currentRound++;  // 轮次增加

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

        //currentImageIndex = currentRound;  // 切换到新回合的图片
        //RequestSerialization();
        //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkLoadPanorama));

        //Debug.Log($"[GameManager] 开始第 {currentRound} 轮，加载图片 {currentImageIndex}");

    }


    private void LoadRoundImage(int roundIndex)
    {
        if (roundIndex >= 0 && roundIndex < imageUrls.Length)
        {
            currentImageIndex = roundIndex;
            RequestSerialization();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkLoadPanorama));
            Debug.Log($"Loading image {currentImageIndex}");
        }
    }

    private void ShowRoundResults(Vector2 targetPos)
    {
        Debug.Log($"Showing results for round {currentRound + 1}");
        isShowingResults = true;

        Vector2 worldPos = latLongMapper.LatLongToUICoords(targetPos);
        answerPinPrefab.transform.localPosition = new Vector3(worldPos.x, 0, worldPos.y);
        answerPinPrefab.SetActive(true); // 显示答案
        UpdateResultsUI(targetPos); //更新结果UI

    }

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



   

    public void StartNextRound()
    {
        isShowingResults = false;

        //for (int i = 0; i < playerPins.Length; i++)
        //{
        //    if (playerPins[i] != null)
        //    {
        //        PinController pinController = playerPins[i].GetComponent<PinController>();
        //        if (pinController != null)
        //        {
        //            //pinController.HideResult(); 
        //        }
        //    }
        //}

        currentRound++;
        StartNewRound();
    }

    private void EndGame()
    {
        gameStarted = false;
        isRoundActive = false;

        if (waitingText != null)
        {
            waitingText.text = "";
        }

        ShowFinalScores();
        RequestSerialization();
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

    private void ShowFinalScores()
    {
        if (scoreText == null) return;

        string scoreBoard = "Game Over!\nFinal Scores:\n";
        float[] sortedScores = new float[MAX_PLAYERS];
        int[] playerIds = new int[MAX_PLAYERS];
        int playerCount = 0;


        scoreText.text = scoreBoard;
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
       

    }
}