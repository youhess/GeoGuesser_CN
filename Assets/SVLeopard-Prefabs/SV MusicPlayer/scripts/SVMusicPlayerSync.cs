
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class SVMusicPlayerSync : UdonSharpBehaviour
{
    [Header("-----------SyncSetting------------")]

    [Tooltip("If true, only owner can control the Music Player")]
    public bool ownerOnly = true;



    [Header("-------Music Player Setting--------")]
    public float _maxBarSize = 1.5f;
    public float barWidth = 1f;
    [Range(0, 0.5f)] public float _smooth = 0.25f;
    public bool _dynamicBarColor = false;
    private Transform _barGroup;
    private Material barMaterial;
    private Transform[] spectrumbars = new Transform[256];
    private float[] _sample = new float[256];
    private float timeDelay = 0.022f;
    private float highestLogFreq;
    private float logFreqMultiplier;
    private Animator cover;
    public bool coverSpin = true;
    private GameObject _songInfo;
    private Text _songinfoText;
    private ScrollRect _scroll;
    private bool _scrollWay = true;
    private float _scrollSpeed = 0.05f;
    private Image[] _image = new Image[2];
    public Sprite[] _songCover;
    private Dropdown _songList;
    public AudioSource _audioSource;
    private GameObject muted, unmute;
    private GameObject _pause, _play;
    private Slider _songTime;
    private Text _curTime, _totalTime;
    private float curSongLength;
    public AudioClip[] _audioclip;
    private GameObject listrepeat, repeatone, shuffle;
    private Slider volumeslider;
    private bool isOwner;
    private string operation = "Initialization";
    private Text ownerInfo;
    private bool isSynced = false;


    [UdonSynced] private int _curSongIndex;
    [UdonSynced] private int _playmode = 1;
    [UdonSynced] private bool isMute = false;
    [UdonSynced] private float musicVolume;
    [UdonSynced] private bool isPaused = false;



    void Start()
    {
        isOwner = Networking.LocalPlayer.IsOwner(gameObject);
        _barGroup = transform.Find("spectrums").Find("bar");
        for (int i = 0; i < 128; i++)
        {
            string name = (i + 1).ToString();
            spectrumbars[i] = _barGroup.transform.Find(name).transform;
        }
        barMaterial = spectrumbars[0].GetComponent<Image>().material;
        highestLogFreq = Mathf.Log(129, 2);
        logFreqMultiplier = 256 / highestLogFreq;
        _songInfo = transform.Find("songinfo").gameObject;
        _songinfoText = _songInfo.transform.Find("Text").GetComponent<Text>();
        cover = transform.GetComponentInParent<Animator>();
        _scroll = _songInfo.GetComponent<ScrollRect>();
        _image[0] = transform.Find("Background").GetComponent<Image>();
        _image[1] = transform.Find("cover").transform.Find("Image").GetComponent<Image>();
        _songList = transform.Find("songlist").GetComponent<Dropdown>();
        muted = transform.Find("muted").gameObject;
        unmute = transform.Find("unmute").gameObject;
        _songTime = transform.Find("time").GetComponent<Slider>();
        _curTime = _songTime.transform.Find("curtime").GetComponent<Text>();
        _totalTime = _songTime.transform.Find("totaltime").GetComponent<Text>();
        volumeslider = transform.Find("volume").GetComponent<Slider>();
        ownerInfo = transform.Find("ownerinfo").GetComponent<Text>();
        ownerInfo.text = string.Format("Control:{0}-Owner:{1}", ownerOnly ? "<color=#FF614A>Owner Only</color>" : "<color=#54E075>Everyone</color>", Networking.GetOwner(gameObject).displayName);
        _pause = transform.Find("pause").gameObject;
        _play = transform.Find("play").gameObject;
        listrepeat = transform.Find("listrepeat").gameObject;
        repeatone = transform.Find("repeatone").gameObject;
        shuffle = transform.Find("shuffle").gameObject;
        if (isOwner)
        {
            Debug.Log("[SVMusicPlayerSync] : Owner Initialization");
            _curSongIndex = UnityEngine.Random.Range(0, _audioclip.Length - 1);
            NextSong();
            musicVolume = volumeslider.value;
        }
    }

    public override void OnDeserialization()
    {
        if (!isSynced)
        {
            SyncState();
        }
    }


    public void SyncEvent()
    {
        isSynced = false;
        SendCustomEventDelayedSeconds("ReqSer", 1.25f);
    }


    public void ReqSer()
    {
        RequestSerialization();
    }


    private void Update()
    {
        if (isPaused)
        {
            cover.SetBool("isplaying", false);
            return;
        }

        _audioSource.GetSpectrumData(_sample, 0, FFTWindow.BlackmanHarris);

        if (timeDelay < 0)
        {
            SetSpectrumData();
            timeDelay = 0.022f;
        }
        else
        {
            timeDelay -= Time.deltaTime;
        }
        if (coverSpin && _audioSource.isPlaying) cover.SetBool("isplaying", true);
        else cover.SetBool("isplaying", false);

        float _audiotime = _audioSource.time;
        _curTime.text = string.Format("{0:00}:{1:00}", Mathf.Floor(_audiotime / 60), Mathf.Floor(_audiotime % 60));
        _songTime.value = _audiotime / _audioclip[_curSongIndex].length;
        if (isOwner)
        {
            if (_songTime.value > 0.999)
            {
                if (_playmode == 2)
                {
                    RepeatSong();
                }
                else
                {
                    NextSong();
                }
            }
        }

        if (_scroll.horizontalNormalizedPosition > 0 && _scroll.horizontalNormalizedPosition < 1)
        {
            if (_scrollWay) _scroll.horizontalNormalizedPosition += _scrollSpeed * Time.deltaTime;
            else _scroll.horizontalNormalizedPosition -= _scrollSpeed * Time.deltaTime;
        }
        else
        {
            _scrollWay = !_scrollWay;
            if (_scroll.horizontalNormalizedPosition >= 1) _scroll.horizontalNormalizedPosition = 0.999f;
            else _scroll.horizontalNormalizedPosition = 0.001f;
        }


    }


    Color GetImageColor(int index)
    {
        float r = 0, g = 0, b = 0;
        int x, y;
        Texture2D _image = _songCover[index].texture;
        x = _image.width;
        y = _image.height;
        for (int i = 0; i < 50; i++)
        {
            int row = UnityEngine.Random.Range(0, x);
            int col = UnityEngine.Random.Range(0, y);
            r += _image.GetPixel(row, col).r;
            g += _image.GetPixel(row, col).g;
            b += _image.GetPixel(row, col).b;
        }
        Color finalcolor = new Color(r / 50, g / 50, b / 50);
        return finalcolor;
    }

    public void PlaySong(int index)
    {
        //if (isOwner)
        //{
        //    _curSongIndex = index;
        //    operation = "SyncPlaySong";
        //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncEvent");
        //}
        AudioClip clip = _audioclip[index];
        Debug.Log("[SVMusicPlayerSync] : Now Playing:" + clip.name);
        _audioSource.clip = clip;
        _audioSource.Stop();
        _songinfoText.text = "   " + clip.name + "   ";
        _image[0].sprite = _songCover[index];
        _image[1].sprite = _songCover[index];
        curSongLength = _audioclip[index].length;
        _totalTime.text = string.Format("{0:0}:{1:00}", Mathf.Floor(curSongLength / 60), Mathf.Floor(curSongLength % 60));
        _audioSource.Play();
        if (_dynamicBarColor) barMaterial.SetColor("_Color", GetImageColor(index));
    }



    public void ToggleMute()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        isMute = !isMute;
        muted.SetActive(isMute);
        unmute.SetActive(!isMute);
        _audioSource.mute = isMute;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncMuteEvent");
    }



    public void SyncMuteEvent()
    {
        operation = "SyncMute";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncEvent");
    }



    public void SetPause()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        isPaused = true;
        _audioSource.Pause();
        _pause.SetActive(false);
        _play.SetActive(true);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncTogglePaus");
    }

    public void SetPlay()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        isPaused = false;
        _audioSource.Play();
        _pause.SetActive(true);
        _play.SetActive(false);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncTogglePaus");
    }



    public void SyncTogglePaus()
    {
        operation = "SyncPause";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncEvent");
    }



    public void RepeatSong()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        PlaySong(_curSongIndex);
        SetPlay();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncPlaySong");
    }

    public void NextSong()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        switch (_playmode)
        {
            case 1:
                _curSongIndex += 1;
                if (_curSongIndex == _audioclip.Length) _curSongIndex = 0;
                PlaySong(_curSongIndex);
                SetPlay();
                break;
            case 2:
                _curSongIndex += 1;
                if (_curSongIndex == _audioclip.Length) _curSongIndex = 0;
                PlaySong(_curSongIndex);
                SetPlay();
                break;
            case 3:
                _curSongIndex = UnityEngine.Random.Range(0, _audioclip.Length - 1);
                PlaySong(_curSongIndex);
                SetPlay();
                break;
            default:
                break;
        }
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncPlaySong");
    }

    public void LastSong()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        switch (_playmode)
        {
            case 1:
                _curSongIndex -= 1;
                if (_curSongIndex == -1) _curSongIndex = _audioclip.Length - 1;
                PlaySong(_curSongIndex);
                SetPlay();
                break;
            case 2:
                _curSongIndex -= 1;
                if (_curSongIndex == -1) _curSongIndex = _audioclip.Length - 1;
                PlaySong(_curSongIndex);
                SetPlay();
                break;
            case 3:
                PlaySong(UnityEngine.Random.Range(0, _audioclip.Length - 1));
                SetPlay();
                break;
            default:
                break;
        }
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncPlaySong");
    }

    public void SyncPlaySong()
    {
        operation = "SyncPlaySong";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncEvent");
    }


    public void SetPlayModeToListRepeat()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        listrepeat.SetActive(true);
        repeatone.SetActive(false);
        shuffle.SetActive(false);
        _playmode = 1;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncPlayMode");
    }
    public void SetPlayModeToRepeatOne()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        listrepeat.SetActive(false);
        repeatone.SetActive(true);
        shuffle.SetActive(false);
        _playmode = 2;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncPlayMode");
    }
    public void SetPlayModeToShuffle()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        listrepeat.SetActive(false);
        repeatone.SetActive(false);
        shuffle.SetActive(true);
        _playmode = 3;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncPlayMode");
    }

    public void SyncPlayMode()
    {
        operation = "SwitchPlayMode";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncEvent");
    }




    public void DropDownChangeSong()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        if (_songList.value < _audioclip.Length) PlaySong(_songList.value);
        _curSongIndex = _songList.value;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncPlaySong");
    }


    public void SetVolume()
    {
        if (ownerOnly && !isOwner)
        {
            return;
        }
        if (!isOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        musicVolume = volumeslider.value;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncSetVolume");
    }

    public void SyncSetVolume()
    {
        operation = "SyncVolume";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncEvent");

    }



    private void SetSpectrumData()
    {
        for (int i = 0; i < 128; i++)
        {
            float value;
            float trueSampleIndex;
            float _multiNum = 1 / _audioSource.volume;
            trueSampleIndex = (highestLogFreq - Mathf.Log(128 - i, 2)) * logFreqMultiplier;
            int sampleIndexFloor = Mathf.FloorToInt(trueSampleIndex);
            sampleIndexFloor = Mathf.Clamp(sampleIndexFloor, 0, _sample.Length - 2);
            float sampleIndexDecimal = trueSampleIndex % 1;
            value = Mathf.SmoothStep(_sample[sampleIndexFloor] * _multiNum, _sample[sampleIndexFloor + 1] * _multiNum, sampleIndexDecimal) * (trueSampleIndex + 1);
            float oldXScale = spectrumbars[i].localScale.x, newXScale;
            newXScale = Mathf.Lerp(oldXScale, Mathf.Max(value * _maxBarSize, 0.01f), 0.5f - _smooth);
            if (newXScale > _maxBarSize) newXScale = _maxBarSize;
            spectrumbars[i].localScale = new Vector3(newXScale, barWidth, 1);
        }
    }


    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isOwner = true;
        }
        else
        {
            isOwner = false;
        }
        ownerInfo.text = string.Format("Control:{0}-Owner:{1}", ownerOnly ? "<color=#FF614A>Owner Only</color>" : "<color=#54E075>Everyone</color>", Networking.GetOwner(gameObject).displayName);
    }


    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            if (!isOwner)
            {
                operation = "Initialization";
                RequestSerialization();
                ownerInfo.text = string.Format("Control:{0}-Owner:{1}", ownerOnly ? "<color=#FF614A>Owner Only</color>" : "<color=#54E075>Everyone</color>", Networking.GetOwner(gameObject).displayName);
            }
        }
    }




    private void SyncState()
    {
        if (isOwner)
        {
            return;
        }

        switch (operation)
        {
            case "Initialization":
                muted.SetActive(isMute);
                unmute.SetActive(!isMute);
                _audioSource.mute = isMute;

                if (isPaused)
                {
                    _audioSource.Pause();
                }
                else
                {
                    _audioSource.Play();
                }
                _pause.SetActive(!isPaused);
                _play.SetActive(isPaused);

                if (_playmode == 1)
                {
                    listrepeat.SetActive(true);
                    repeatone.SetActive(false);
                    shuffle.SetActive(false);
                }
                if (_playmode == 2)
                {
                    listrepeat.SetActive(false);
                    repeatone.SetActive(true);
                    shuffle.SetActive(false);
                }
                if (_playmode == 3)
                {
                    listrepeat.SetActive(false);
                    repeatone.SetActive(false);
                    shuffle.SetActive(true);
                }

                volumeslider.value = musicVolume;

                PlaySong(_curSongIndex);

                Debug.Log("[SVMusicPlayerSync] : Deserialization Operation : Initialization");
                break;

            case "SyncMute":
                muted.SetActive(isMute);
                unmute.SetActive(!isMute);
                _audioSource.mute = isMute;
                Debug.Log("[SVMusicPlayerSync] : Deserialization Operation : SyncMute");
                break;

            case "SyncPause":
                if (isPaused)
                {
                    _audioSource.Pause();
                }
                else
                {
                    _audioSource.Play();
                }
                _pause.SetActive(!isPaused);
                _play.SetActive(isPaused);

                Debug.Log("[SVMusicPlayerSync] : Deserialization Operation : SyncPause");
                break;

            case "SwitchPlayMode":
                if (_playmode == 1)
                {
                    listrepeat.SetActive(true);
                    repeatone.SetActive(false);
                    shuffle.SetActive(false);
                }
                if (_playmode == 2)
                {
                    listrepeat.SetActive(false);
                    repeatone.SetActive(true);
                    shuffle.SetActive(false);
                }
                if (_playmode == 3)
                {
                    listrepeat.SetActive(false);
                    repeatone.SetActive(false);
                    shuffle.SetActive(true);
                }
                Debug.Log("[SVMusicPlayerSync] : Deserialization Operation : SwitchPlayMode");
                break;

            case "SyncPlaySong":
                PlaySong(_curSongIndex);
                Debug.Log("[SVMusicPlayerSync] : Deserialization Operation : SyncPlaySong");
                break;

            case "SyncVolume":
                volumeslider.value = musicVolume;
                Debug.Log("[SVMusicPlayerSync] : Deserialization Operation : SyncVolume");
                break;

            default:
                Debug.Log("[SVMusicPlayerSync] : Unknow Deserialization Operation!");
                break;
        }
        isSynced = true;
    }

}
