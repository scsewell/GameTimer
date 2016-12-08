using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Timer : MonoBehaviour
{
    public RectTransform mainPanel;
    public RectTransform playerClockPrefab;
    public RectTransform playerClockPanel;
    public Text timerText;
    public Image pauseImage;
    public Sprite pauseSprite;
    public Sprite playSprite;
    public Color normalTimerColor = Color.white;
    public Color overTimerColor = Color.red;
    [Range(0,1)]
    public float timerPauseAlpha = 0.45f;
    [Range(10, 40)]
    public int playerClockFontSize = 24;
    [Range(10, 40)]
    public int playerClockActiveFontSize = 34;
    
    public RectTransform gridTimePanel;
    public RectTransform timeRowsPanel;
    public RectTransform timeRowPrefab;
    public RectTransform timePanelPrefab;
    
    private enum TimerMenu { Timer, Grid }
    private TimerMenu currentMenu = TimerMenu.Timer;
    private AudioSource source;
    private Canvas canvas;

    private bool paused;
    private int loop;
    private float timeToSound;
    private TimerPlayer currentPlayer;
    private List<TurnTime> turnTimes;
    private Dictionary<TimerPlayer, TimerPlayer> nextTurnMap;
    private Dictionary<TimerPlayer, RectTransform> timePlayerClock;

    private int players;
    private float initialTurnTime;
    private float desiredAverageTurnTime;
    private float desiredAverageTurnsToIntensity;
    private float minGap;
    private float numOfBeepsTime;
    private int numOfBeeps;
    private float beepRatio;
    private List<TimerPlayer> timerPlayers;


    void Start ()
    {
        source = GetComponent<AudioSource>();
        canvas = GetComponentInChildren<Canvas>();

        GetComponent<TimerUI>().SwipeEvent += HandleSwipe;

        SetPosition(mainPanel, new Vector2(0, 0));
        SetPosition(gridTimePanel, new Vector2(Screen.width / canvas.scaleFactor, Screen.width / canvas.scaleFactor));

        GameObject persist = GameObject.FindGameObjectWithTag("Settings");
        Settings settings = persist.GetComponent<Settings>();

        players                         = settings.numOfPlayers;
        numOfBeeps                      = settings.numBeeps;
        initialTurnTime                 = settings.intialTurnTimer;
        desiredAverageTurnTime          = settings.averageTurnTime;
        desiredAverageTurnsToIntensity  = settings.turnMultiplesToMaxIntensity;
        minGap                          = settings.minTimeBetweenBeeps;
        numOfBeepsTime                  = settings.numOfBeepsTime;
        beepRatio                       = settings.beepRatio;
        timerPlayers                    = settings.timerPlayers;
        
        Destroy(persist);

        nextTurnMap = new Dictionary<TimerPlayer, TimerPlayer>();
        foreach (TimerPlayer timerPlayer in settings.timerPlayers)
        {
            nextTurnMap[timerPlayer] = settings.timerPlayers[(settings.timerPlayers.IndexOf(timerPlayer) + 1) % settings.timerPlayers.Count];
        }
        currentPlayer = settings.timerPlayers.First();

        turnTimes = new List<TurnTime>();

        SwitchToPlayer(currentPlayer);

        Pause();
        
        timePlayerClock = new Dictionary<TimerPlayer, RectTransform>();

        foreach (TimerPlayer timePlayer in settings.timerPlayers)
        {
            RectTransform clock = Instantiate(playerClockPrefab);
            clock.SetParent(playerClockPanel, false);
            clock.GetComponent<Text>().color = timePlayer.Color;
            clock.GetComponentInChildren<Image>().color = timePlayer.Color;
            TimerPlayer tempTimePlayer = timePlayer;
            clock.GetComponent<Button>().onClick.AddListener(() => { SwitchToPlayer(tempTimePlayer);});
            timePlayerClock.Add(timePlayer, clock);
        }
    }
    

    void Update ()
    {
        Vector2 timerMenuTarget = Vector2.zero;
        Vector2 gridMenuTarget = Vector2.zero;

        Vector2 activeMenuPos = new Vector2(0, 0);
        Vector2 inactiveTimerMenuPos = new Vector2(-Screen.width / canvas.scaleFactor, -Screen.width / canvas.scaleFactor);
        Vector2 InactiveGridMenuPos = new Vector2(Screen.width / canvas.scaleFactor, Screen.width / canvas.scaleFactor);

        if (currentMenu == TimerMenu.Timer)
        {
            timerMenuTarget = activeMenuPos;
            gridMenuTarget = InactiveGridMenuPos;
        }
        else if (currentMenu == TimerMenu.Grid)
        {
            timerMenuTarget = inactiveTimerMenuPos;
            gridMenuTarget = activeMenuPos;
        }

        SetPosition(mainPanel, Vector2.Lerp(GetPosition(mainPanel), timerMenuTarget, 2f * Time.deltaTime));
        SetPosition(gridTimePanel, Vector2.Lerp(GetPosition(gridTimePanel), gridMenuTarget, 2.0f * Time.deltaTime));

        turnTimes.Last().pauseTimes.Last().turnEnd = Time.time;
        
        if (timeToSound + turnTimes.Last().turnDuration(true) < Time.time)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
            source.Play();
            loop++;

            timeToSound = Time.time + GetNextTime(loop, numOfBeepsTime, minGap, numOfBeeps, beepRatio);
        }
        
        timerText.text = formatTime(turnTimes.Last().turnDuration(false));
        if (turnTimes.Last().turnDuration(false) > desiredAverageTurnTime)
        {
            timerText.color = overTimerColor;
        }
        else
        {
            timerText.color = normalTimerColor;
        }

        foreach (TimerPlayer player in timerPlayers)
        {
            timePlayerClock[player].GetComponent<Text>().text = player.Name + "\n" + formatTime(turnTimes.Where(e => e.player == player).Sum(e => e.turnDuration(false))) + (player == currentPlayer ? "*" : "");
            if (player == currentPlayer)
            {
                timePlayerClock[player].GetComponent<Text>().fontSize = playerClockActiveFontSize;
                timePlayerClock[player].GetComponentInChildren<Image>().enabled = true;
            }
            else
            {
                timePlayerClock[player].GetComponent<Text>().fontSize = playerClockFontSize;
                timePlayerClock[player].GetComponentInChildren<Image>().enabled = false;
            }
        }

        pauseImage.sprite = (paused ? playSprite : pauseSprite);
        timerText.color = new Color(timerText.color.r, timerText.color.g, timerText.color.b, paused ? timerPauseAlpha : 1);
    }

    private void UpdateTimeGrid()
    {
        foreach (RectTransform t in gridTimePanel)
        {
            Destroy(t.gameObject);
        }
    }

    private float GetNextTime(int n, float averateTurnTime, float minGap, int numOfBeeps, float beepRatio)
    {
        float firstBeep = averateTurnTime / GeometricSeriesSum(numOfBeeps, beepRatio);
        return Mathf.Max(minGap, firstBeep * Mathf.Pow(beepRatio, n));
    }

    private float GeometricSeriesSum(int n, float r)
    {
        return (1 - Mathf.Pow(r, n + 1)) / (1 - r);
    }

    public float GetIntensity()
    {
        return Mathf.Clamp01(((turnTimes.Last().turnDuration(false)) - initialTurnTime) / (desiredAverageTurnTime * desiredAverageTurnsToIntensity));
    }

    public void NextTurn()
    {
        SwitchToPlayer(nextTurnMap[currentPlayer]);
        UpdateTimeGrid();
    }

    public void SwitchToPlayer(TimerPlayer newCurrentPlayer)
    {
        paused = false;
        currentPlayer = newCurrentPlayer;
        loop = 0;
        timeToSound = Time.time + initialTurnTime;

        turnTimes.Add(new TurnTime(Time.time, currentPlayer, paused));
    }

    public void Pause()
    {   
        paused = !paused;
        turnTimes.Last().pauseTimes.Add(new PauseTime(Time.time, paused));
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    private void SetPosition(RectTransform t, Vector2 pos)
    {
        t.offsetMin = new Vector2(pos.x, t.offsetMin.y);
        t.offsetMax = new Vector2(pos.y, t.offsetMax.y);
    }
    
    private Vector2 GetPosition(RectTransform t)
    {
        return new Vector2(t.offsetMin.x, t.offsetMax.x);
    }

    public void HandleSwipe(object sender, SwipeArgs e)
    {
        if (currentMenu == TimerMenu.Timer && e.SwipDirection == SwipDirection.Left)
        {
            currentMenu = TimerMenu.Grid;
        }
        else if (currentMenu == TimerMenu.Grid && e.SwipDirection == SwipDirection.Right)
        {
            currentMenu = TimerMenu.Timer;
        }
    }

    private string formatTime(float seconds)
    {
        string formatted;
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.Hours > 0)
        {
            formatted = string.Format("{0:D1}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else if (timeSpan.Minutes > 0)
        {
            formatted = string.Format("{0:D1}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        else
        {
            formatted = string.Format("{0:D2}", timeSpan.Seconds);
        }
        return formatted;
    }

    private List<string[]> makeTurnArray(bool packed)
    {
        List<string[]> turnArray = new List<string[]>();

        string[] row = new string[players];
        row.Fill("");
        turnArray.Add(row);
        foreach (TurnTime turnTime in turnTimes)
        {
            if (!packed || row[timerPlayers.IndexOf(turnTime.player)] != "")
            {
                row = new string[players];
                row.Fill("");
                turnArray.Add(row);
            }
            row[timerPlayers.IndexOf(turnTime.player)] = formatTime(turnTimes.Last().turnDuration(false));
        }
        return turnArray;
    }

    private class TurnTime
    {
        public readonly TimerPlayer player;
        public readonly List<PauseTime> pauseTimes;

        public TurnTime(float turnStart, TimerPlayer player, bool paused)
        {
            this.player = player;
            pauseTimes = new List<PauseTime>();
            pauseTimes.Add(new PauseTime(turnStart, paused));
        }

        public float turnDuration(bool pauseStatusToMatch)
        {
            return pauseTimes.Where(e => e.paused == pauseStatusToMatch).Sum(e => e.duration());
        }
    }

    private class PauseTime
    {
        public readonly float turnStart;
        public float turnEnd;
        public readonly bool paused;

        public PauseTime(float turnStart, bool paused)
        {
            this.turnStart = turnStart;
            this.turnEnd = turnStart;
            this.paused = paused;
        }

        public float duration()
        {
            return turnEnd - turnStart;
        }
    }
}

public static class ArrayExtensions
{
    public static void Fill<T>(this T[] originalArray, T with)
    {
        for (int i = 0; i < originalArray.Length; i++)
        {
            originalArray[i] = with;
        }
    }

    public static IEnumerable<T> Cycle<T>(this IEnumerable<T> iterable)
    {
        while (true)
        {
            foreach (T t in iterable)
            {
                yield return t;
            }
        }
    }
}
