using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    [Range(0, 1)]
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
    private TimerMenu m_currentMenu = TimerMenu.Timer;
    private AudioSource m_audioSource;
    private Canvas m_canvas;

    private bool m_paused;
    private int m_loop;
    private float m_nextBeepTime;
    private TimerPlayer m_currentPlayer;
    private List<TurnTime> m_turnTimes;
    private Dictionary<TimerPlayer, TimerPlayer> m_nextTurnMap;
    private Dictionary<TimerPlayer, RectTransform> m_timePlayerClock;

    private List<TimerPlayer> m_timerPlayers;
    private float m_initialTurnTime;
    private float m_numOfBeepsTime;
    private float m_minGap;
    private float m_beepRatio;


    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_canvas = GetComponentInChildren<Canvas>();

        GetComponent<TimerUI>().SwipeEvent += HandleSwipe;

        SetPosition(mainPanel, Vector2.zero);
        SetPosition(gridTimePanel, (Screen.width / m_canvas.scaleFactor) * Vector2.one);

        // get timer settings from game profile
        GameObject persist = GameObject.FindGameObjectWithTag("Settings");
        Settings settings = persist.GetComponent<Settings>();

        m_timerPlayers = settings.timerPlayers;
        m_initialTurnTime = settings.gameProfile.TurnTime;
        m_numOfBeepsTime = Mathf.Max(settings.gameProfile.MaxTime - settings.gameProfile.TurnTime, 5);
        m_minGap = 0.5f;
        m_beepRatio = Mathf.Clamp01(1 - settings.gameProfile.Intensity);

        // build map of which player is next after the current player
        m_nextTurnMap = new Dictionary<TimerPlayer, TimerPlayer>();
        foreach (TimerPlayer timerPlayer in settings.timerPlayers)
        {
            m_nextTurnMap[timerPlayer] = settings.timerPlayers[(settings.timerPlayers.IndexOf(timerPlayer) + 1) % settings.timerPlayers.Count];
        }
        m_currentPlayer = settings.timerPlayers.First();
        
        m_turnTimes = new List<TurnTime>();

        // set the current player
        SwitchToPlayer(m_currentPlayer);
        
        // initilize the player clocks
        m_timePlayerClock = new Dictionary<TimerPlayer, RectTransform>();
        foreach (TimerPlayer timePlayer in settings.timerPlayers)
        {
            RectTransform clock = Instantiate(playerClockPrefab);
            clock.SetParent(playerClockPanel, false);
            clock.GetComponent<Text>().color = timePlayer.Color;
            clock.GetComponentInChildren<Image>().color = timePlayer.Color;
            TimerPlayer tempTimePlayer = timePlayer;
            clock.GetComponent<Button>().onClick.AddListener(() => { SwitchToPlayer(tempTimePlayer); UpdateTimeGrid(); });
            m_timePlayerClock.Add(timePlayer, clock);
        }

        // initialize the time grid
        UpdateTimeGrid();

        // start the timer paused
        Pause();
    }

    public void NextTurn()
    {
        SwitchToPlayer(m_nextTurnMap[m_currentPlayer]);
        UpdateTimeGrid();
    }

    public void Pause()
    {
        m_paused = !m_paused;
        m_turnTimes.Last().m_timeBlocks.Add(new TimeBlock(Time.time, m_paused));
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_currentMenu = m_currentMenu == TimerMenu.Timer ? TimerMenu.Grid : TimerMenu.Timer;
        }

        Vector2 timerMenuTarget = Vector2.zero;
        Vector2 gridMenuTarget = Vector2.zero;

        Vector2 activeMenuPos = Vector2.zero;
        Vector2 inactiveTimerMenuPos = -(Screen.width / m_canvas.scaleFactor) * Vector2.one;
        Vector2 InactiveGridMenuPos = (Screen.width / m_canvas.scaleFactor) * Vector2.one;

        if (m_currentMenu == TimerMenu.Timer)
        {
            timerMenuTarget = activeMenuPos;
            gridMenuTarget = InactiveGridMenuPos;
        }
        else if (m_currentMenu == TimerMenu.Grid)
        {
            timerMenuTarget = inactiveTimerMenuPos;
            gridMenuTarget = activeMenuPos;
        }

        SetPosition(mainPanel, Vector2.Lerp(GetPosition(mainPanel), timerMenuTarget, 8f * Time.deltaTime));
        SetPosition(gridTimePanel, Vector2.Lerp(GetPosition(gridTimePanel), gridMenuTarget, 8f * Time.deltaTime));

        m_turnTimes.Last().m_timeBlocks.Last().m_turnEnd = Time.time;
        
        if (m_nextBeepTime + m_turnTimes.Last().turnDuration(true) < Time.time)
        {
            m_audioSource.time = 0;
            m_audioSource.Play();
            m_loop++;

            float timeToSound = Math.Max(m_numOfBeepsTime * Mathf.Pow(m_beepRatio, m_loop), m_minGap);
            m_nextBeepTime = Time.time + timeToSound;
        }

        timerText.text = formatTime(m_turnTimes.Last().turnDuration(false));
        timerText.color = m_turnTimes.Last().turnDuration(false) < m_initialTurnTime ? normalTimerColor : overTimerColor;

        foreach (TimerPlayer player in m_timerPlayers)
        {
            bool isCurrentPlayer = player == m_currentPlayer;
            RectTransform clock = m_timePlayerClock[player];

            clock.GetComponent<Text>().text = player.Name + "\n" + formatTime(m_turnTimes.Where(e => e.m_player == player).Sum(e => e.turnDuration(false))) + (isCurrentPlayer ? "*" : "");
            clock.GetComponent<Text>().fontSize = isCurrentPlayer ? playerClockActiveFontSize : playerClockFontSize;
            clock.GetComponent<Text>().resizeTextMaxSize = isCurrentPlayer ? playerClockActiveFontSize : playerClockFontSize;
            clock.GetComponentInChildren<Image>().enabled = isCurrentPlayer;
        }

        pauseImage.sprite = (m_paused ? playSprite : pauseSprite);
        timerText.color = new Color(timerText.color.r, timerText.color.g, timerText.color.b, m_paused ? timerPauseAlpha : 1);
    }

    private void UpdateTimeGrid()
    {
        foreach (RectTransform t in timeRowsPanel)
        {
            Destroy(t.gameObject);
        }

        // display player names in the top row
        RectTransform playerRow = Instantiate(timeRowPrefab, timeRowsPanel, true) as RectTransform;
        playerRow.localScale = Vector3.one;
        foreach (TimerPlayer player in m_timerPlayers)
        {
            RectTransform timePanel = Instantiate(timePanelPrefab, playerRow, true) as RectTransform;
            timePanel.localScale = Vector3.one;
            timePanel.GetComponentInChildren<Text>().text = player.Name;
            timePanel.GetComponentInChildren<Text>().color = player.Color;
        }

        // display turn times in the following rows
        foreach (string[] times in makeTurnArray(true))
        {
            RectTransform row = Instantiate(timeRowPrefab, timeRowsPanel, true) as RectTransform;
            row.localScale = Vector3.one;
            for (int i = 0; i < times.Length; i++)
            {
                RectTransform timePanel = Instantiate(timePanelPrefab, row, true) as RectTransform;
                timePanel.localScale = Vector3.one;
                timePanel.GetComponentInChildren<Text>().text = string.IsNullOrEmpty(times[i]) ? "--" : times[i];
                timePanel.GetComponentInChildren<Text>().color = m_timerPlayers[i].Color;
            }
        }
    }

    private void SwitchToPlayer(TimerPlayer newCurrentPlayer)
    {
        m_paused = false;
        m_loop = 0;
        m_currentPlayer = newCurrentPlayer;
        m_nextBeepTime = Time.time + m_initialTurnTime;

        m_turnTimes.Add(new TurnTime(Time.time, m_currentPlayer, m_paused));
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

    private void HandleSwipe(object sender, SwipeArgs e)
    {
        if (m_currentMenu == TimerMenu.Timer && e.SwipDirection == SwipDirection.Left)
        {
            m_currentMenu = TimerMenu.Grid;
        }
        else if (m_currentMenu == TimerMenu.Grid && e.SwipDirection == SwipDirection.Right)
        {
            m_currentMenu = TimerMenu.Timer;
        }
    }

    private string formatTime(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.Hours > 0)
        {
            return string.Format("{0:D1}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else if (timeSpan.Minutes > 0)
        {
            return string.Format("{0:D1}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        else
        {
            return string.Format("{0:D2}", timeSpan.Seconds);
        }
    }

    private List<string[]> makeTurnArray(bool packed)
    {
        List<string[]> turnArray = new List<string[]>();

        string[] row = new string[m_timerPlayers.Count].Fill("");
        turnArray.Add(row);
        foreach (TurnTime turnTime in m_turnTimes)
        {
            if (!packed || row[m_timerPlayers.IndexOf(turnTime.m_player)] != "")
            {
                row = new string[m_timerPlayers.Count].Fill("");
                turnArray.Add(row);
            }
            row[m_timerPlayers.IndexOf(turnTime.m_player)] = formatTime(turnTime.turnDuration(false));
        }
        return turnArray;
    }

    private class TurnTime
    {
        public readonly TimerPlayer m_player;
        public readonly List<TimeBlock> m_timeBlocks;

        public TurnTime(float turnStart, TimerPlayer player, bool paused)
        {
            m_player = player;
            m_timeBlocks = new List<TimeBlock>();
            m_timeBlocks.Add(new TimeBlock(turnStart, paused));
        }
        
        public float turnDuration(bool pausedTime)
        {
            return m_timeBlocks.Where(e => e.m_paused == pausedTime).Sum(e => e.duration());
        }
    }

    private class TimeBlock
    {
        public readonly float m_turnStart;
        public float m_turnEnd;
        public readonly bool m_paused;

        public TimeBlock(float turnStart, bool paused)
        {
            m_turnStart = turnStart;
            m_turnEnd = turnStart;
            m_paused = paused;
        }

        public float duration()
        {
            return m_turnEnd - m_turnStart;
        }
    }
}

public static class ArrayExtensions
{
    public static T[] Fill<T>(this T[] originalArray, T with)
    {
        for (int i = 0; i < originalArray.Length; i++)
        {
            originalArray[i] = with;
        }
        return originalArray;
    }
}
