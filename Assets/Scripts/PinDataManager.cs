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
    private int totalRounds = 5; // 与 GameManager 保持一致

    // 显示最终得分
    public TextMeshProUGUI finalScoresText;

    [UdonSynced]
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


        // 添加检查
        if (objectAssigner == null)
        {
            Debug.LogError("[PinDataManager] CyanPlayerObjectAssigner 未设置！");
            return;
        }
    }

    // 由GameManager调用的初始化方法
    //public void InitializeRounds(int rounds)
    //{
    //    totalRounds = rounds;
    //    roundAnswers = new DataList[totalRounds];
    //    serializedRoundAnswers = new string[totalRounds];
    //    Debug.Log($"[PinDataManager] 初始化 {totalRounds} 回合的数据存储");
    //}

    // 新增：设置所有Pin可见性的方法
    public void SetShowAllPins(bool show)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (showAllPins != show)  // 只在值真正改变时才更新
        {
            Debug.Log($"[PinDataManager] 设置所有Pin可见性: {show}");
            showAllPins = show;
            // 立即在本地执行可见性更新
            RequestSerialization();  // 会触发其他客户端的 OnDeserialization
            UpdatePinVisibility();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdatePinVisibility));
        }
    }

    public void InitializeRounds(int rounds)
    {
        if (rounds <= 0)
        {
            Debug.LogError($"[PinDataManager] Invalid rounds count: {rounds}");
            return;
        }

        totalRounds = rounds;
        roundAnswers = new DataList[totalRounds];
        serializedRoundAnswers = new string[totalRounds];

        // 初始化每个元素
        for (int i = 0; i < totalRounds; i++)
        {
            roundAnswers[i] = new DataList();
            serializedRoundAnswers[i] = "";
        }

        Debug.Log($"[PinDataManager] 初始化 {totalRounds} 回合的数据存储");
    }

    //public void UpdateDataListPlayerText()
    //{
    //    dataListPlayerText.text = playerInfo;
    //    RequestSerialization(); // 请求同步数据
    //}



    // 更新玩家的 Pin 经纬度数据 
    public void UpdatePlayerPinData(int playerId, Vector2 pinCoordinates)
    {
        Debug.Log($"UpdatePlayerPinData: {playerId}, {pinCoordinates}"); // 打印玩家 ID 和经纬度

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
                    dataPoint.SetValue("longitude", pinCoordinates.x);
                    dataPoint.SetValue("latitude", pinCoordinates.y);
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
            newDataPoint.SetValue("longitude", pinCoordinates.x);
            newDataPoint.SetValue("latitude", pinCoordinates.y);
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
        if (!Networking.IsOwner(gameObject)) return;

        // 保存当前的 dataList 到对应回合
        roundAnswers[roundIndex] = dataList;

        // 序列化当前回合数据
        if (VRCJson.TrySerializeToJson(dataList, JsonExportType.Minify, out DataToken jsonToken))
        {
            serializedRoundAnswers[roundIndex] = jsonToken.String;
        }

        // 更新 UI 显示
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
            // 获取该轮的正确答案
            Vector2 correctAnswer = gameManager.GetRoundAnswer(round);

            // 获取该轮的所有玩家答案
            if (roundAnswers[round] != null)
            {
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

                            // 计算得分
                            float score = CalculateScore(correctAnswer, playerGuess);

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
        finalScoresText.text = finalScores.ToString();
    }

    private float CalculateScore(Vector2 correctAnswer, Vector2 playerGuess)
    {
        // 计算距离（可以使用简单的欧几里得距离或更复杂的地球表面距离）
        float distance = Vector2.Distance(correctAnswer, playerGuess);
        
        // 基于距离计算分数（示例：满分5000，距离越远分数越低）
        float maxScore = 5000f;
        float maxDistance = 1000f; // 最大距离，超过这个距离得0分
        
        if (distance >= maxDistance) return 0;
        
        return maxScore * (1 - distance / maxDistance);
    }

    private void UpdatePinVisibility()
    {
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
        Debug.Log("反序列化前调用");
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

        // 反序列化每轮的数据
        for (int i = 0; i < totalRounds; i++)
        {
            if (!string.IsNullOrEmpty(serializedRoundAnswers[i]))
            {
                if (VRCJson.TryDeserializeFromJson(serializedRoundAnswers[i], out DataToken dataToken) &&
                    dataToken.TokenType == TokenType.DataList)
                {
                    roundAnswers[i] = dataToken.DataList;
                }
            }
        }

        // 使用相同的代码更新存储显示
        StringBuilder storageText = new StringBuilder("Game Round Data:\n");
        for (int i = 0; i < totalRounds; i++)
        {
            storageText.AppendLine($"Round {i + 1}:");
            if (roundAnswers[i] != null && roundAnswers[i].Count > 0)
            {
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

        //Debug.Log("UpdatePinVisibility被调用啦");
        //// 更新Pin可见性
        //UpdatePinVisibility();
    }



}
