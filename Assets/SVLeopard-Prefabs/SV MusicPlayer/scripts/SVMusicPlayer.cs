
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class SVMusicPlayer : UdonSharpBehaviour
{
    private Transform _barGroup;
    public float _maxBarSize = 1.5f;
    public float barWidth = 1f;
    [Range(0,0.5f)] public float _smooth = 0.25f;
    public bool _dynamicBarColor = false;
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
    private bool isPaused = false;
    private GameObject _pause, _play;
    private Slider _songTime;
    private Text _curTime, _totalTime;
    private float curSongLength;
    private bool isMute = false;
    public AudioClip[] _audioclip;
    private int _curSongIndex;
    private GameObject listrepeat, repeatone, shuffle;
    private int _playmode = 1;
    


    void Start()
    {
        _barGroup = transform.Find("spectrums").Find("bar");
        for(int i = 0; i < 128; i++)
        {
            string name = (i + 1).ToString();
            spectrumbars[i] = _barGroup.transform.Find(name).transform;
        }
        barMaterial = spectrumbars[0].GetComponent<Image>().material;
        highestLogFreq = Mathf.Log(129, 2);
        logFreqMultiplier = 256 / highestLogFreq;
        _songInfo = this.transform.Find("songinfo").gameObject;
        _songinfoText = _songInfo.transform.Find("Text").GetComponent<Text>();
        cover = this.transform.GetComponentInParent<Animator>();
        _scroll = _songInfo.GetComponent<ScrollRect>();
        _image[0] = this.transform.Find("Background").GetComponent<Image>();
        _image[1] = this.transform.Find("cover").transform.Find("Image").GetComponent<Image>();
        _songList = this.transform.Find("songlist").GetComponent<Dropdown>();
        muted = this.transform.Find("muted").gameObject;
        unmute = this.transform.Find("unmute").gameObject;
        _songTime = this.transform.Find("time").GetComponent<Slider>();
        _curTime = _songTime.transform.Find("curtime").GetComponent<Text>();
        _totalTime = _songTime.transform.Find("totaltime").GetComponent<Text>();
        _pause = this.transform.Find("pause").gameObject;
        _play = this.transform.Find("play").gameObject;
        _curSongIndex = UnityEngine.Random.Range(0, _audioclip.Length - 1);
        listrepeat = this.transform.Find("listrepeat").gameObject;
        repeatone = this.transform.Find("repeatone").gameObject;
        shuffle = this.transform.Find("shuffle").gameObject;
        PlaySong(_curSongIndex);
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
        _songTime.value = _audiotime / curSongLength;
        if (_songTime.value > 0.999) ChangeNextSong();


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
        for(int i = 0; i < 50; i++)
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
        _curSongIndex = index;
        AudioClip clip = _audioclip[index];
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
        isMute = !isMute;
        muted.SetActive(isMute);
        unmute.SetActive(!isMute);
        _audioSource.mute = isMute;
    }

    public void SetPause()
    {
        isPaused = true;
        _audioSource.Pause();
        _pause.SetActive(false);
        _play.SetActive(true);
    }

    public void SetPlay()
    {
        isPaused = false;
        _audioSource.Play();
        _pause.SetActive(true);
        _play.SetActive(false);
    }

    void ChangeNextSong()
    {
        switch (_playmode)
        {
            case 1:
                _curSongIndex += 1;
                if (_curSongIndex == _audioclip.Length) _curSongIndex = 0;
                PlaySong(_curSongIndex);
                break;
            case 2:
                PlaySong(_curSongIndex);
                break;
            case 3:
                PlaySong(UnityEngine.Random.Range(0, _audioclip.Length - 1));
                break;
            default:
                break;
        }
    }

    public void NextSong()
    {
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
                PlaySong(UnityEngine.Random.Range(0, _audioclip.Length - 1));
                SetPlay();
                break;
            default:
                break;
        }
    }

    public void LastSong()
    {
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
    }


    public void SetPlayModeToListRepeat()
    {
        listrepeat.SetActive(true);
        repeatone.SetActive(false);
        shuffle.SetActive(false);
        _playmode = 1;
    }
    public void SetPlayModeToRepeatOne()
    {
        listrepeat.SetActive(false);
        repeatone.SetActive(true);
        shuffle.SetActive(false);
        _playmode = 2;
    }
     public void SetPlayModeToShuffle()
    {
        listrepeat.SetActive(false);
        repeatone.SetActive(false);
        shuffle.SetActive(true);
        _playmode = 3;
    }


    public void DropDownChangeSong()
    {
        if (_songList.value < _audioclip.Length) PlaySong(_songList.value);
        _curSongIndex = _songList.value;
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
            if (newXScale >_maxBarSize) newXScale = _maxBarSize;
            spectrumbars[i].localScale = new Vector3(newXScale, barWidth, 1);
        }
    }
}
