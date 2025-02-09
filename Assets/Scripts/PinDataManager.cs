using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using System.Collections.Generic;
using TMPro.Examples;
using System.Text;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PinDataManager : UdonSharpBehaviour
{
    [UdonSynced] private string serializedData; // 用于同步的 JSON 字符串
    private DataDictionary playerData; // 存储玩家的经纬度信息

    [UdonSynced] private string playerInfo;
    public TextMeshProUGUI dataListPlayerText;


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

    private void Start()
    {
        Debug.Log("[PinDataManager] 初始化完成");
        //if (playerData == null)
        //{
        //    playerData = new DataDictionary();
        //}
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

    }



}
