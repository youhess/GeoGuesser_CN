using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using System.Collections.Generic;
using TMPro.Examples;
using System.Text;
using TMPro;
using Cyan.PlayerObjectPool;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PinDataManager : UdonSharpBehaviour
{

    // 存储所有玩家的 Pin 经纬度数据
    [UdonSynced] private string playerInfo;
    public TextMeshProUGUI dataListPlayerText;

    // 存储每轮的答案
    private DataList[] roundAnswers;
    [UdonSynced] private string[] serializedRoundAnswers; 
    public TextMeshProUGUI gameDataStoreageText;

    [UdonSynced]
    private int totalRounds = 5; // 与 GameManager 保持一致

    // 显示最终得分
    public TextMeshProUGUI roundScoresText;

    // 添加一个用于存储当前轮次得分的字符串
    [UdonSynced] 
    private string currentRoundScoresText = "";

    //[UdonSynced]
    private bool showAllPins = false;  // 控制是否显示所有Pin的状态

    // 改为使用 ObjectAssigner 而不是 ObjectPool
    public CyanPlayerObjectAssigner objectAssigner;

    private DataList dataList = new DataList()
    {
        //new DataDictionary()
        //{
        //    { "id", 1 },
        //    { "longitude", 120.123456 },
        //    { "latitude", 30.123456 }
        //},
        // 可以根据需要继续添加更多的数据点
    };
    [UdonSynced] private string serializedData; // 用于同步的 JSON 字符串
    //private DataDictionary playerData; // 存储玩家的经纬度信息


    private void Start()
    {
        Debug.Log("[PinDataManager] 初始化完成");


        //if (objectAssigner == null)
        //{
        //    Debug.Log($"[PinDataManager] 开始查找 CyanPlayerObjectAssigner");

        //    // 在子级层次结构中查找 CyanPlayerObjectAssigner
        //    objectAssigner = GetComponentInChildren<CyanPlayerObjectAssigner>();

        //    if (objectAssigner != null)
        //    {
        //        Debug.Log($"[PinDataManager] 成功找到 objectAssigner: {objectAssigner.gameObject.name}");
        //    }
        //    else
        //    {
        //        Debug.LogError($"[PinDataManager] 无法找到 CyanPlayerObjectAssigner，请检查子级层次结构！");
        //        return;
        //    }
        //}

        Debug.Log("[PinDataManager] 初始化完成");
    }


    // 新增：设置所有Pin可见性的方法
    public void SetShowAllPins()
    {
            //if (!Networking.IsOwner(gameObject)) return;
  
            Debug.Log($"[PinDataManager] 设置所有Pin可见");
            showAllPins = true;
        // 立即在本地执行可见性更新
        //RequestSerialization();  // 会触发其他客户端的 OnDeserialization
        // 应该每一个客户端都调用一次
        UpdatePinVisibility();
            //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdatePinVisibility)); // 有可能要在所有客户端执行，onDeserialization不一定会触发
        
    }
    public void SetHideOtherPins()
    {
        //if (!Networking.IsOwner(gameObject)) return;
        Debug.Log($"[PinDataManager] 设置所有Pin不可见");
        showAllPins = false;
        // 立即在本地执行可见性更新
        //RequestSerialization();  // 会触发其他客户端的 OnDeserialization
        // 应该每一个客户端都调用一次
        UpdatePinVisibility();
        //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdatePinVisibility)); // 有可能要在所有客户端执行，onDeserialization不一定会触发
    }

    public void InitializeRounds(int rounds)
    {
        Debug.Log($"[PinDataManager] 开始初始化回合数: {rounds}");

        totalRounds = rounds;
        roundAnswers = new DataList[totalRounds];
        serializedRoundAnswers = new string[totalRounds];

        // 确保每个元素都被初始化
        for (int i = 0; i < totalRounds; i++)
        {
            roundAnswers[i] = new DataList();
            serializedRoundAnswers[i] = "";
            Debug.Log($"[PinDataManager] 初始化回合 {i}");
        }

        // 同步修改，确保其他客户端也收到更新
        //RequestSerialization();

    }

    //public void UpdateDataListPlayerText()
    //{
    //    dataListPlayerText.text = playerInfo;
    //    RequestSerialization(); // 请求同步数据
    //}



    // 更新玩家的 Pin 经纬度数据 
    public void UpdatePlayerPinData(int playerId, Vector2 pinCoordinates, bool isPlacedOnMap)
    {
        Debug.Log($"UpdatePlayerPinData: {playerId}, {pinCoordinates}, placed: {isPlacedOnMap}");

        bool playerFound = false;

        // 遍历 dataList 查找是否存在相同的 playerId
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList.TryGetValue(i, TokenType.DataDictionary, out DataToken dataToken))
            {
                DataDictionary dataPoint = dataToken.DataDictionary;

                // 检查当前 DataDictionary 是否包含相同的 playerId
                if (dataPoint.TryGetValue("id", TokenType.Int, out DataToken idToken) && idToken.Int == playerId)
                {
                    // 更新经纬度
                    dataPoint.SetValue("latitude", pinCoordinates.x);
                    dataPoint.SetValue("longitude", pinCoordinates.y);
                    dataPoint.SetValue("isPlaced", isPlacedOnMap);
                    playerFound = true;
                    break;
                }
            }
        }

        // 如果未找到相同的 playerId，则添加新的 DataDictionary
        if (!playerFound)
        {
            DataDictionary newDataPoint = new DataDictionary();
            newDataPoint.SetValue("id", playerId);
            newDataPoint.SetValue("latitude", pinCoordinates.x);
            newDataPoint.SetValue("longitude", pinCoordinates.y);
            newDataPoint.SetValue("isPlaced", isPlacedOnMap);
            dataList.Add(newDataPoint);
        }

        // 序列化 DataList 为 JSON
        if (VRCJson.TrySerializeToJson(dataList, JsonExportType.Minify, out DataToken jsonToken))
        {
            serializedData = jsonToken.String;
            //RequestSerialization(); // 请求同步
        }
        else
        {
            Debug.LogError("Failed to serialize dataList to JSON.");
        }

        // 更新同步变量 playerInfo
        playerInfo = LogDataListContents();
        dataListPlayerText.text = playerInfo;
        RequestSerialization(); // 请求同步

    }


    //// 获取玩家的 Pin 经纬度数据
    //public Vector2 GetPlayerPinData(string playerId)
    //{
    //    if (playerData != null && playerData.ContainsKey(playerId))
    //    {
    //        return (Vector2)playerData[playerId];
    //    }
    //    return Vector2.zero; // 如果找不到数据，返回默认值
    //}

    public string LogDataListContents()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList.TryGetValue(i, TokenType.DataDictionary, out DataToken dataToken))
            {
                DataDictionary dataDict = dataToken.DataDictionary;
                sb.AppendLine($"Index {i}: DataDictionary with {dataDict.Count} entries");

                // 获取所有键
                DataList keys = dataDict.GetKeys();
                for (int j = 0; j < keys.Count; j++)
                {
                    DataToken keyToken = keys[j];
                    string key = keyToken.String;

                    // 获取对应的值
                    if (dataDict.TryGetValue(keyToken, out DataToken valueToken))
                    {
                        string value = valueToken.ToString();
                        sb.AppendLine($"    Key: {key}, Value: {value}");
                    }
                    else
                    {
                        sb.AppendLine($"    Key: {key}, Value: <Unable to retrieve>");
                    }
                }
            }
            else
            {
                sb.AppendLine($"Index {i}: Not a DataDictionary or retrieval failed.");
            }
        }
        // 将收集的字符串分配给 UI 文本
        return sb.ToString();
    }

    // 保存当前回合的所有玩家答案
    public void SaveRoundAnswers(int roundIndex)
    {
        // 只有所有者才能保存数据
        if (!Networking.IsOwner(gameObject)) return;

        // 保存当前的 dataList 到对应回合
        roundAnswers[roundIndex] = dataList;

        // 序列化当前回合数据
        if (VRCJson.TrySerializeToJson(dataList, JsonExportType.Minify, out DataToken jsonToken))
        {
            serializedRoundAnswers[roundIndex] = jsonToken.String;
        }

        // 更新 UI 显示， 这个没有必要在所有客户端都更新，因为只有所有者才能保存数据
        StringBuilder storageText = new StringBuilder("Game Round Data:\n");
        for (int i = 0; i < totalRounds; i++)
        {
            storageText.AppendLine($"Round {i + 1}:");
            if (roundAnswers[i] != null && roundAnswers[i].Count > 0)  // 检查是否有数据
            {
                // 遍历该轮次的所有玩家答案
                for (int j = 0; j < roundAnswers[i].Count; j++)
                {
                    if (roundAnswers[i].TryGetValue(j, TokenType.DataDictionary, out DataToken dataToken))
                    {
                        DataDictionary playerAnswer = dataToken.DataDictionary;
                        if (playerAnswer.TryGetValue("id", TokenType.Int, out DataToken idToken) &&
                            playerAnswer.TryGetValue("longitude", TokenType.Float, out DataToken longToken) &&
                            playerAnswer.TryGetValue("latitude", TokenType.Float, out DataToken latToken))
                        {
                            storageText.AppendLine($"Player {idToken.Int}: Lat {latToken.Float:F2}, Long {longToken.Float:F2}");
                        }
                    }
                }
            }
            else
            {
                storageText.AppendLine("No data");
            }
            storageText.AppendLine();
        }
        gameDataStoreageText.text = storageText.ToString();

        // 清空当前回合的数据，准备下一轮
        // 其实不需要清空，因为每轮的数据都是独立的
        //dataList = new DataList();

        // 同步
        RequestSerialization();
    }

    // PinDataManager.cs

    public void CalculateFinalScores(GameManager gameManager)
    {
        StringBuilder finalScores = new StringBuilder("Final Scores:\n");

        // 用 DataDictionary 存储玩家分数
        DataDictionary playerTotalScores = new DataDictionary();

        // 遍历每一轮
        for (int round = 0; round < totalRounds; round++)
        {
            // 检查这个回合是否已经完成（有没有存储玩家答案）
            if (roundAnswers[round] == null || roundAnswers[round].Count == 0)
            {
                Debug.LogWarning($"[PinDataManager] 回合 {round} 没有有效的玩家答案数据，跳过计分");
                continue;
            }

            // 获取该轮使用的图片索引
            int imageIndex = round < gameManager.roundImageIndices.Length ?
                             gameManager.roundImageIndices[round] :
                             gameManager.GetCurrentImageIndex();

            // 如果该回合没有记录图片索引或索引无效，使用默认索引(0)或跳过
            if (imageIndex < 0 || imageIndex >= gameManager.GetImageUrlsLength())
            {
                Debug.LogWarning($"[PinDataManager] 回合 {round} 的图片索引 {imageIndex} 无效，使用默认索引0");
                imageIndex = 0; // 使用第一张图片作为默认
            }

            // 获取对应图片的正确答案位置
            Vector2 correctAnswer = gameManager.locationData.GetLocationLatLong(imageIndex);
            Debug.Log($"[PinDataManager] 计算回合 {round} 的最终得分, 图片索引: {imageIndex}, 正确答案: {correctAnswer}");

            // 计算每个玩家在这轮的得分
            for (int i = 0; i < roundAnswers[round].Count; i++)
            {
                if (roundAnswers[round].TryGetValue(i, TokenType.DataDictionary, out DataToken dataToken))
                {
                    DataDictionary playerAnswer = dataToken.DataDictionary;
                    if (playerAnswer.TryGetValue("id", TokenType.Int, out DataToken idToken) &&
                        playerAnswer.TryGetValue("longitude", TokenType.Float, out DataToken longToken) &&
                        playerAnswer.TryGetValue("latitude", TokenType.Float, out DataToken latToken))
                    {
                        int playerId = idToken.Int;
                        Vector2 playerGuess = new Vector2(latToken.Float, longToken.Float);

                        // 获取放置状态，如果不存在则默认为 false
                        bool isPlaced = false;
                        if (playerAnswer.TryGetValue("isPlaced", TokenType.Boolean, out DataToken placedToken))
                        {
                            isPlaced = placedToken.Boolean;
                        }

                        // 记录详细的调试信息
                        Debug.Log($"[PinDataManager] 回合 {round}, 玩家 {playerId} 猜测: {playerGuess}, 正确答案: {correctAnswer}, 已放置: {isPlaced}");

                        // 计算得分，考虑 pin 是否被放置
                        float score = CalculateScore(correctAnswer, playerGuess, isPlaced);

                        // 累加到玩家总分
                        float currentScore = 0;
                        string playerKey = $"player_{playerId}";

                        if (playerTotalScores.TryGetValue(playerKey, TokenType.Float, out DataToken currentScoreToken))
                        {
                            currentScore = currentScoreToken.Float;
                        }

                        playerTotalScores.SetValue(playerKey, currentScore + score);
                    }
                }
            }
        }

        // 构建最终得分字符串
        DataList playerKeys = playerTotalScores.GetKeys();
        for (int i = 0; i < playerKeys.Count; i++)
        {
            string playerKey = playerKeys[i].String;
            if (playerTotalScores.TryGetValue(playerKey, TokenType.Float, out DataToken scoreToken))
            {
                int playerId = int.Parse(playerKey.Split('_')[1]);
                finalScores.AppendLine($"Player {playerId}: {scoreToken.Float:F0}");
            }
        }

        // 更新UI显示
        roundScoresText.text = finalScores.ToString();
    }

    //public void CalculateFinalScores(GameManager gameManager)
    //{
    //    StringBuilder finalScores = new StringBuilder("Final Scores:\n");

    //    // 用 DataDictionary 存储玩家分数
    //    DataDictionary playerTotalScores = new DataDictionary();

    //    // 遍历每一轮
    //    for (int round = 0; round < totalRounds; round++)
    //    {
    //        // 获取该轮的正确答案
    //        Vector2 correctAnswer = gameManager.GetRoundAnswer(round);

    //        // 获取该轮的所有玩家答案
    //        if (roundAnswers[round] != null)
    //        {
    //            // 计算每个玩家在这轮的得分
    //            for (int i = 0; i < roundAnswers[round].Count; i++)
    //            {
    //                if (roundAnswers[round].TryGetValue(i, TokenType.DataDictionary, out DataToken dataToken))
    //                {
    //                    DataDictionary playerAnswer = dataToken.DataDictionary;
    //                    if (playerAnswer.TryGetValue("id", TokenType.Int, out DataToken idToken) &&
    //                        playerAnswer.TryGetValue("longitude", TokenType.Float, out DataToken longToken) &&
    //                        playerAnswer.TryGetValue("latitude", TokenType.Float, out DataToken latToken))
    //                    {
    //                        int playerId = idToken.Int;
    //                        Vector2 playerGuess = new Vector2(latToken.Float, longToken.Float);
    //                        //Vector2 playerGuess = new Vector2(longToken.Float, latToken.Float);

    //                        // 获取放置状态，如果不存在则默认为 false
    //                        bool isPlaced = false;
    //                        if (playerAnswer.TryGetValue("isPlaced", TokenType.Boolean, out DataToken placedToken))
    //                        {
    //                            isPlaced = placedToken.Boolean;
    //                        }

    //                        //Debug.Log($"[PinDataManager] Player {playerId} guess: {playerGuess},correctAnswer: {correctAnswer}, isPlaced: {isPlaced}");

    //                        // 计算得分，考虑 pin 是否被放置
    //                        float score = CalculateScore(correctAnswer, playerGuess, isPlaced);

    //                        // 累加到玩家总分
    //                        float currentScore = 0;
    //                        string playerKey = $"player_{playerId}";

    //                        if (playerTotalScores.TryGetValue(playerKey, TokenType.Float, out DataToken currentScoreToken))
    //                        {
    //                            currentScore = currentScoreToken.Float;
    //                        }

    //                        playerTotalScores.SetValue(playerKey, currentScore + score);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    // 构建最终得分字符串
    //    DataList playerKeys = playerTotalScores.GetKeys();
    //    for (int i = 0; i < playerKeys.Count; i++)
    //    {
    //        string playerKey = playerKeys[i].String;
    //        if (playerTotalScores.TryGetValue(playerKey, TokenType.Float, out DataToken scoreToken))
    //        {
    //            int playerId = int.Parse(playerKey.Split('_')[1]);
    //            finalScores.AppendLine($"Player {playerId}: {scoreToken.Float:F0}");
    //        }
    //    }

    //    // 更新UI显示
    //    roundScoresText.text = finalScores.ToString();
    //}

    // 计算每一轮所有玩家的得分
    public void CalculateRoundScores(GameManager gameManager, int roundIndex)
    {
        StringBuilder roundScores = new StringBuilder($"Round {roundIndex + 1} Scores:\n");

        // 获取图片索引（用于调试）
        int imageIndex = gameManager.roundImageIndices[roundIndex];

        // 获取该轮的正确答案
        Vector2 correctAnswer = gameManager.GetRoundAnswer(roundIndex);
        // 获取该轮的所有玩家答案
        if (roundAnswers[roundIndex] != null)
        {
            // 计算每个玩家在这轮的得分
            for (int i = 0; i < roundAnswers[roundIndex].Count; i++)
            {
                if (roundAnswers[roundIndex].TryGetValue(i, TokenType.DataDictionary, out DataToken dataToken))
                {
                    DataDictionary playerAnswer = dataToken.DataDictionary;
                    if (playerAnswer.TryGetValue("id", TokenType.Int, out DataToken idToken) &&
                        playerAnswer.TryGetValue("longitude", TokenType.Float, out DataToken longToken) &&
                        playerAnswer.TryGetValue("latitude", TokenType.Float, out DataToken latToken))
                    {
                        int playerId = idToken.Int;
                        Vector2 playerGuess = new Vector2(latToken.Float, longToken.Float);
                        //Vector2 playerGuess = new Vector2(longToken.Float, latToken.Float);
                        // 获取放置状态，如果不存在则默认为 false
                        bool isPlaced = false;
                        if (playerAnswer.TryGetValue("isPlaced", TokenType.Boolean, out DataToken placedToken))
                        {
                            isPlaced = placedToken.Boolean;
                        }
                   

                        // 计算得分时考虑放置状态
                        float score = CalculateScore(correctAnswer, playerGuess, isPlaced);
                        Debug.Log($"[PinDataManager] Player {playerId} guess: {playerGuess},correctAnswer: {correctAnswer}, isPlaced: {isPlaced}, score: {score}");
                        // 添加到字符串
                        roundScores.AppendLine($"Player {playerId}: {score:F0}{(!isPlaced ? " (Not placed)" : "")}");
                    }
                }
            }
        }

        // 保存计算结果到同步变量
        currentRoundScoresText = roundScores.ToString();

        // 立即更新本地UI
        UpdateRoundScoresUI();

        // 请求同步，这会触发其他客户端的 OnDeserialization
        RequestSerialization();

        // 发送网络事件，确保所有客户端都更新 UI
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateRoundScoresUI));
    }

    // 添加一个 UI 更新方法，可以被网络事件调用
    public void UpdateRoundScoresUI()
    {
        if (roundScoresText != null)
        {
            roundScoresText.text = currentRoundScoresText;
        }
    }

    private float CalculateScore(Vector2 correctAnswer, Vector2 playerGuess, bool isPlacedOnMap)
    {
        // If Pin isn't placed on the map, return 0 points
        if (!isPlacedOnMap)
        {
            return 0;
        }

        // Calculate distance (using Euclidean distance)
        float distance = Vector2.Distance(correctAnswer, playerGuess);

        Debug.Log($"[PinDataManager] Distance: {distance}");

        // More sensitive score calculation
        float maxScore = 100f;
        float maxDistance = 15f; // Reduced from 1000 to make scoring more sensitive to distance

        // Optional: Add a minimum score threshold for very close guesses
        float minDistance = 1f; // Within 5 units is considered "very close"

        if (distance <= minDistance)
        {
            // Very close guesses get max or near-max points
            return maxScore * 0.95f + (minDistance - distance) / minDistance * (maxScore * 0.05f);
        }
        else if (distance >= maxDistance)
        {
            // Too far gets 0 points
            return 0;
        }
        else
        {
            // Use a non-linear curve to make score drop more quickly with distance
            // This creates more meaningful differentiation between close and far pins
            float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);
            return maxScore * 0.95f * (1 - Mathf.Pow(normalizedDistance, 2));
        }
    }

    private void UpdatePinVisibility()
    {

        if (!Networking.IsOwner(objectAssigner.gameObject))
        {
            Debug.LogWarning("[PinDataManager] 当前玩家不是 objectAssigner 的所有者，可能无法正确获取对象");
        }
        else
        {
            Debug.Log("[PinDataManager] 当前玩家是 objectAssigner 的所有者");
        }



        // 获取所有活动的池对象
        Component[] activePoolObjects = objectAssigner._GetActivePoolObjects();
        // 显示Pool中的所有对象
        // 添加日志显示长度
        Debug.Log($"[PinDataManager] 活动池对象数量: {activePoolObjects.Length}");

        if (activePoolObjects == null)
        {
            Debug.LogError("[PinDataManager] 无法获取活动的池对象！");
            return;
        }

        foreach (Component poolObject in activePoolObjects)
        {
            if (poolObject == null) continue;

            GameObject pinObject = poolObject.gameObject;
            VRCPlayerApi owner = Networking.GetOwner(pinObject);
            if (owner == null || !owner.IsValid()) continue;

            int pinOwnerId = owner.playerId;
            bool isOwner = (Networking.LocalPlayer.playerId == pinOwnerId);

            // 更新渲染器可见性
            Renderer[] renderers = pinObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    Color color = mat.color;
                    color.a = (showAllPins || isOwner) ? 1.0f : 0.0f;
                    mat.color = color;

                    mat.SetFloat("_Mode", (showAllPins || isOwner) ? 0 : 3);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", (showAllPins || isOwner) ? 1 : 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (showAllPins || isOwner) ? -1 : 3000;
                }
            }

            // 更新UI元素可见性
            CanvasGroup[] canvasGroups = pinObject.GetComponentsInChildren<CanvasGroup>(true);
            foreach (CanvasGroup canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = (showAllPins || isOwner) ? 1.0f : 0.0f;
                canvasGroup.interactable = (showAllPins || isOwner);
                canvasGroup.blocksRaycasts = (showAllPins || isOwner);
            }
        }

        Debug.Log($"[PinDataManager] 更新了 {activePoolObjects.Length} 个Pin的可见性, showAllPins: {showAllPins}");
    }

    // 在数据反序列化前调用
    public override void OnDeserialization()
    {

        Debug.Log("OnDeserialization called");

        // 更新轮次得分 UI
        UpdateRoundScoresUI();

        // 如果数组还没有初始化，则进行初始化
        if (roundAnswers == null || serializedRoundAnswers == null ||
        roundAnswers.Length != totalRounds || serializedRoundAnswers.Length != totalRounds)
        {
            Debug.Log("Initializing roundAnswers and serializedRoundAnswers arrays");
            roundAnswers = new DataList[totalRounds];
            serializedRoundAnswers = new string[totalRounds];
            for (int i = 0; i < totalRounds; i++)
            {
                roundAnswers[i] = new DataList();
                serializedRoundAnswers[i] = "";
            }
        }

        if (!string.IsNullOrEmpty(serializedData))
        {
            if (VRCJson.TryDeserializeFromJson(serializedData, out DataToken dataToken) && dataToken.TokenType == TokenType.DataList)
            {
                dataList = dataToken.DataList;
            }
            else
            {
                Debug.LogError("Failed to deserialize JSON to DataList.");
            }
        }

        // 这里更新 UI 确保所有玩家都能看到最新的信息
        dataListPlayerText.text = playerInfo;

        //// 反序列化每轮的数据
        //for (int i = 0; i < totalRounds; i++)
        //{
        //    if (!string.IsNullOrEmpty(serializedRoundAnswers[i]))
        //    {
        //        if (VRCJson.TryDeserializeFromJson(serializedRoundAnswers[i], out DataToken dataToken) &&
        //            dataToken.TokenType == TokenType.DataList)
        //        {
        //            roundAnswers[i] = dataToken.DataList;
        //        }
        //    }
        //}

        // 反序列化每一回合的数据
        for (int i = 0; i < totalRounds; i++)
        {
            Debug.Log($"[PinDataManager] OnDeserialization - 访问索引 {i}，数组长度: {serializedRoundAnswers.Length}, totalRounds: {totalRounds}");

            if (i < serializedRoundAnswers.Length && !string.IsNullOrEmpty(serializedRoundAnswers[i]))
            {
                if (VRCJson.TryDeserializeFromJson(serializedRoundAnswers[i], out DataToken dataToken))
                {
                    if (dataToken.TokenType == TokenType.DataList)
                    {
                        roundAnswers[i] = dataToken.DataList;
                        Debug.Log($"[PinDataManager] Successfully deserialized round {i} data");
                    }
                    else
                    {
                        Debug.LogError($"[PinDataManager] Round {i} data type error: {dataToken.TokenType}");
                    }
                }
                else
                {
                    Debug.LogError($"[PinDataManager] Failed to deserialize round {i} data");
                }
            }
        }
        //// 更新Pin可见性
        //UpdatePinVisibility();
    }



}
