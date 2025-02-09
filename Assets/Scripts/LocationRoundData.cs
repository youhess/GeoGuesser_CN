using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

public class LocationRoundData : UdonSharpBehaviour
{
    [SerializeField]
    private TextAsset jsonFile;  // JSON文件引用

    private DataList locationDataList;  // 存储所有位置数据

    void Start()
    {
        LoadLocationData();
    }

    private void LoadLocationData()
    {
        if (jsonFile != null)
        {
            string json = jsonFile.text;
            if (VRCJson.TryDeserializeFromJson(json, out DataToken result))
            {
                if (result.TokenType == TokenType.DataList)
                {
                    locationDataList = result.DataList;
                    Debug.Log($"Loaded {locationDataList.Count} locations");
                }
            }
        }
    }

    // 获取位置信息
    public Vector2 GetLocationLatLong(int index)
    {
        if (locationDataList != null && index < locationDataList.Count)
        {
            var locationData = locationDataList[index].DataDictionary;
            float latitude = 0f, longitude = 0f;

            if (locationData.TryGetValue("latitude", out DataToken latValue))
            {
                latitude = (float)latValue.Double;
            }
            if (locationData.TryGetValue("longitude", out DataToken longValue))
            {
                longitude = (float)longValue.Double;
            }

            return new Vector2(latitude, longitude);
        }
        return Vector2.zero;
    }

    // 获取地点名称
    public string GetLocationName(int index)
    {
        if (locationDataList != null && index < locationDataList.Count)
        {
            var locationData = locationDataList[index].DataDictionary;
            if (locationData.TryGetValue("name", out DataToken nameValue))
            {
                return nameValue.String;
            }
        }
        return "Unknown Location";
    }

    // 获取图片URL
    public string GetImageUrl(int index)
    {
        if (locationDataList != null && index < locationDataList.Count)
        {
            var locationData = locationDataList[index].DataDictionary;
            if (locationData.TryGetValue("image_url", out DataToken urlValue))
            {
                return urlValue.String;
            }
        }
        return "";
    }

    // 获取当前加载的位置数量
    public int GetLocationCount()
    {
        return locationDataList != null ? locationDataList.Count : 0;
    }
}