using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    // Main Menu
    public RectTransform mainMenu;
    public Dropdown playerCount;

    public RectTransform playerOptionsMenu;
    public RectTransform playerOptionsList;
    public RectTransform playerOptionPrefab;
    public Color[] playerColors;

    public RectTransform playerSelectMenu;
    public RectTransform playerSelectPanel;

    public RectTransform colorSelectMenu;
    public RectTransform colorSelectPrefab;
    
    public RectTransform gameProfileActiveMenu;
    public RectTransform gameProfileSelectMenu;
    public RectTransform gameProfileSelectList;
    public Text gameProfileActiveName;
    public Text gameProfileActiveTurnTime;
    public Text gameProfileActiveMaxTime;
    public Text gameProfileActiveIntensity;

    // Profile Edit Menus
    public Color profileEditColor;
    public RectTransform profilePrefab;

    public RectTransform playerProfileMenu;
    public RectTransform playerProfilesList;
    public RectTransform playerProfileEditMenu;
    public InputField playerProfileEditName;

    public RectTransform gameProfileMenu;
    public RectTransform gameProfilesList;
    public RectTransform gameProfileEditMenu;
    public InputField gameProfileEditName;
    public InputField gameProfileEditTurnTime;
    public InputField gameProfileEditMaxTime;
    public Slider gameProfileEditIntensity;
    public Text gameProfileTextIntensity;

    // Private Vars
    private ProfileManager m_profileManager;
    private List<TimerPlayer> m_chosenPlayers;
    private GameProfile m_chosenGameProfile;
    private PlayerProfile m_editPlayerProfile;
    private GameProfile m_editGameProfile;

    
    public void Start()
    {
        m_profileManager = new ProfileManager();
        m_chosenPlayers = new List<TimerPlayer>();

        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        Application.targetFrameRate = 60;

        // if we returned from the timer, use the previous settingss
        // otherwise, use defaults
        GameObject o = GameObject.FindGameObjectWithTag("Settings");
        if (o)
        {
            Settings settings = o.GetComponent<Settings>();
            m_chosenPlayers = settings.timerPlayers;
            m_chosenGameProfile = settings.gameProfile;
            SetAmmountOfPlayers(m_chosenPlayers.Count);
        }
        else
        {
            SetAmmountOfPlayers(4);
        }

        PlayerNumChanged();
    }


    //---------- MAIN MENU ------------------------------
    
    public int GetAmountOfPlayers()
    {
        return playerCount.value + 1;
    }

    public void SetAmmountOfPlayers(int newPlayerCount)
    {
        playerCount.value = newPlayerCount - 1;
    }

    private void PlayerNumChanged()
    {
        while (m_chosenPlayers.Count != GetAmountOfPlayers())
        {
            if (m_chosenPlayers.Count > GetAmountOfPlayers())
            {
                m_chosenPlayers.RemoveAt(m_chosenPlayers.Count - 1);
            }
            else
            {
                m_chosenPlayers.Add(new TimerPlayer(GetUnusedGuestName(), GetUnusedColor()));
            }
        }

        UpdateMainMenu();
    }

    private void UpdateMainMenu()
    {
        // hide unneccessary menus
        mainMenu.gameObject.SetActive(true);
        playerOptionsMenu.gameObject.SetActive(true);
        playerSelectMenu.gameObject.SetActive(false);
        colorSelectMenu.gameObject.SetActive(false);
        gameProfileActiveMenu.gameObject.SetActive(true);
        gameProfileSelectMenu.gameObject.SetActive(false);
        playerProfileMenu.gameObject.SetActive(false);
        playerProfileEditMenu.gameObject.SetActive(false);
        gameProfileMenu.gameObject.SetActive(false);
        gameProfileEditMenu.gameObject.SetActive(false);

        // update player list
        foreach (RectTransform t in playerOptionsList)
        {
            Destroy(t.gameObject);
        }

        foreach (TimerPlayer player in m_chosenPlayers)
        {
            RectTransform t = Instantiate(playerOptionPrefab);
            t.SetParent(playerOptionsList, false);

            Button nameOptions = t.FindChild("PlayerName").GetComponent<Button>();
            Button colorOptions = t.FindChild("PlayerColor").GetComponent<Button>();
            nameOptions.transform.FindChild("Label").GetComponent<Text>().text = player.Name;
            colorOptions.transform.FindChild("Image").GetComponent<Image>().color = player.Color;

            TimerPlayer tempPlayer = player;
            nameOptions.onClick.AddListener(() => { ChoosePlayer(tempPlayer); });
            colorOptions.onClick.AddListener(() => { ChooseColor(tempPlayer); });
        }
        
        // ensure that there is always a game profile active
        if (m_profileManager.GetGameProfiles().Count == 0)
        {
            m_profileManager.AddGameProfile("Default", 30, 60, 0.5f);
        }

        if (m_chosenGameProfile == null)
        {
            m_chosenGameProfile = m_profileManager.GetGameProfiles().First();
        }

        // update selected game profile information
        gameProfileActiveName.text = m_chosenGameProfile.Name;
        TimeSpan turnTime = TimeSpan.FromSeconds(m_chosenGameProfile.TurnTime);
        TimeSpan maxTime = TimeSpan.FromSeconds(m_chosenGameProfile.MaxTime);
        gameProfileActiveTurnTime.text = string.Format("{0:D1}:{1:D2}", turnTime.Minutes, turnTime.Seconds);
        gameProfileActiveMaxTime.text = string.Format("{0:D1}:{1:D2}", maxTime.Minutes, maxTime.Seconds);
        gameProfileActiveIntensity.text = (int)(100 * m_chosenGameProfile.Intensity) + "%";
    }

    private string GetUnusedGuestName(string ignoreName = null)
    {
        int guestNumber = 0;
        while (true)
        {
            string guestName = "Guest " + guestNumber;
            if (!m_chosenPlayers.Any((player) => { return (player.IsGuest() && player.Name == guestName && guestName != ignoreName); }))
            {
                return guestName;
            }
            guestNumber++;
        }
    }

    private List<Color> GetUnusedColors()
    {
        List<Color> avaliableColors = playerColors.ToList();
        foreach (TimerPlayer player in m_chosenPlayers)
        {
            if (avaliableColors.Contains(player.Color))
            {
                avaliableColors.Remove(player.Color);
            }
        }
        return avaliableColors;
    }

    private Color GetUnusedColor()
    {
        List<Color> avaliableColors = GetUnusedColors();
        return avaliableColors.Count > 0 ? avaliableColors[UnityEngine.Random.Range(0, avaliableColors.Count)] : playerColors[0];
    }

    private void ChooseColor(TimerPlayer player)
    {
        playerOptionsMenu.gameObject.SetActive(false);
        colorSelectMenu.gameObject.SetActive(true);

        foreach (RectTransform t in colorSelectMenu)
        {
            Destroy(t.gameObject);
        }

        foreach (Color color in playerColors)
        {
            Color tempColor = color;
            RectTransform t = Instantiate(colorSelectPrefab);
            t.SetParent(colorSelectMenu, false);
            t.GetComponent<Image>().color = tempColor;
            t.GetComponent<Button>().onClick.AddListener(() => { UpdateTimePlayer(player, tempColor); UpdateMainMenu(); });
        }
    }

    private void ChoosePlayer(TimerPlayer player)
    {
        playerOptionsMenu.gameObject.SetActive(false);
        playerSelectMenu.gameObject.SetActive(true);

        foreach (RectTransform t in playerSelectPanel)
        {
            Destroy(t.gameObject);
        }

        foreach (PlayerProfile playerProfile in m_profileManager.GetPlayerProfiles())
        {
            PlayerProfile tempPlayerProfile = playerProfile;
            Button button = AddProfileButton(profilePrefab, tempPlayerProfile.Name, playerSelectPanel);
            button.onClick.AddListener(() => { UpdateTimePlayer(player, tempPlayerProfile); UpdateMainMenu(); });
        }
        
        Button guestButton = AddProfileButton(profilePrefab, "Use Guest", playerSelectPanel);
        guestButton.GetComponentInChildren<Image>().color = new Color(109f / 255, 75f / 255, 49f / 255);
        guestButton.onClick.AddListener(() => { UpdateTimePlayer(player, GetUnusedGuestName(player.Name)); UpdateMainMenu(); });
    }
    
    private void UpdateTimePlayer(TimerPlayer oldPlayer, PlayerProfile profile)
    {
        m_chosenPlayers[m_chosenPlayers.IndexOf(oldPlayer)] = new TimerPlayer(profile, oldPlayer.Color);
    }

    private void UpdateTimePlayer(TimerPlayer oldPlayer, string name)
    {
        m_chosenPlayers[m_chosenPlayers.IndexOf(oldPlayer)] = new TimerPlayer(name, oldPlayer.Color);
    }

    private void UpdateTimePlayer(TimerPlayer oldPlayer, Color color)
    {
        TimerPlayer newPlayer;

        if (oldPlayer.IsGuest())
        {
            newPlayer = new TimerPlayer(oldPlayer.Name, color);
        }
        else
        {
            newPlayer = new TimerPlayer(oldPlayer.Profile, color);
        }

        m_chosenPlayers[m_chosenPlayers.IndexOf(oldPlayer)] = newPlayer;
    }

    public void SelectGameProfile()
    {
        gameProfileActiveMenu.gameObject.SetActive(false);
        gameProfileSelectMenu.gameObject.SetActive(true);

        foreach (RectTransform t in gameProfileSelectList)
        {
            Destroy(t.gameObject);
        }

        foreach (GameProfile gameProfile in m_profileManager.GetGameProfiles())
        {
            GameProfile tempGameProfile = gameProfile;
            Button button = AddProfileButton(profilePrefab, tempGameProfile.Name, gameProfileSelectList);
            button.onClick.AddListener(() => { m_chosenGameProfile = tempGameProfile; UpdateMainMenu(); });
        }
    }

    private Button AddProfileButton(RectTransform prefab, string name, RectTransform parent)
    {
        RectTransform t = Instantiate(prefab);
        t.SetParent(parent, false);
        t.SetSiblingIndex(0);
        t.GetComponentInChildren<Text>().text = name;
        return t.GetComponent<Button>();
    }

    public void ToPlayerProfileMenu()
    {
        mainMenu.gameObject.SetActive(false);
        playerProfileMenu.gameObject.SetActive(true);

        m_editPlayerProfile = null;

        UpdatePlayerProfileList();
    }

    public void ToGameProfileMenu()
    {
        mainMenu.gameObject.SetActive(false);
        gameProfileMenu.gameObject.SetActive(true);

        m_editGameProfile = null;

        UpdateGameProfileList();
    }

    public void StartTimer()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Settings object will exist if returing from timer scene
        GameObject o = GameObject.FindGameObjectWithTag("Settings");
        Settings settings;
        if (!o)
        {
            GameObject persist = Instantiate(new GameObject());
            DontDestroyOnLoad(persist);
            persist.tag = "Settings";
            settings = persist.AddComponent<Settings>();
        }
        else
        {
            settings = o.GetComponent<Settings>();
        }
        
        settings.timerPlayers = m_chosenPlayers;
        settings.gameProfile = m_chosenGameProfile;

        SceneManager.LoadScene(1);
    }


    //---------- EDIT PLAYER PROFILE MENU ------------------------------
    
    public void UpdatePlayerProfileList()
    {
        foreach (RectTransform t in playerProfilesList)
        {
            if (t.GetSiblingIndex() != 0)
            {
                Destroy(t.gameObject);
            }
        }

        foreach (PlayerProfile profile in m_profileManager.GetPlayerProfiles())
        {
            RectTransform t = Instantiate(profilePrefab);
            t.SetParent(playerProfilesList, false);
            t.SetSiblingIndex(1);
            t.GetComponentInChildren<Text>().text = profile.Name;

            if (profile == m_editPlayerProfile)
            {
                t.GetComponent<Image>().color = profileEditColor;
            }

            PlayerProfile tempPlayerProfile = profile;
            t.GetComponent<Button>().onClick.AddListener(() => { m_editPlayerProfile = tempPlayerProfile; UpdatePlayerProfileList(); });
        }
        
        if (m_editPlayerProfile != null)
        {
            playerProfileEditName.text = m_editPlayerProfile.Name;
            playerProfileEditMenu.gameObject.SetActive(true);
        }
        else
        {
            playerProfileEditMenu.gameObject.SetActive(false);
        }
    }

    public void AddPlayerProfile()
    {
        string profileName = "Unnamed Player";
        m_editPlayerProfile = m_profileManager.AddPlayerProfile(profileName);
        UpdatePlayerProfileList();
    }

    public void RemovePlayerProfile()
    {
        if (m_editPlayerProfile != null)
        {
            m_profileManager.RemovePlayerProfile(m_editPlayerProfile);
            foreach (TimerPlayer player in m_chosenPlayers)
            {
                if (player.Profile == m_editPlayerProfile)
                {
                    UpdateTimePlayer(player, GetUnusedGuestName());
                }
            }
            m_editPlayerProfile = null;
            UpdatePlayerProfileList();
        }
    }

    public void PlayerProfileChanged()
    {
        if (m_editPlayerProfile != null)
        {
            m_editPlayerProfile.Name = playerProfileEditName.text;
            m_editPlayerProfile = null;
            UpdatePlayerProfileList();
        }
    }


    //---------- EDIT GAME PROFILE MENU ------------------------------

    public void UpdateGameProfileList()
    {
        foreach (RectTransform t in gameProfilesList)
        {
            if (t.GetSiblingIndex() != 0)
            {
                Destroy(t.gameObject);
            }
        }

        foreach (GameProfile profile in m_profileManager.GetGameProfiles())
        {
            RectTransform t = Instantiate(profilePrefab);
            t.SetParent(gameProfilesList, false);
            t.SetSiblingIndex(1);
            t.GetComponentInChildren<Text>().text = profile.Name;

            if (profile == m_editGameProfile)
            {
                t.GetComponent<Image>().color = profileEditColor;
            }

            GameProfile tempGameProfile = profile;
            t.GetComponent<Button>().onClick.AddListener(() => { m_editGameProfile = tempGameProfile; UpdateGameProfileList(); });
        }

        if (m_editGameProfile != null)
        {
            gameProfileEditName.text = m_editGameProfile.Name;
            gameProfileEditTurnTime.text = m_editGameProfile.TurnTime.ToString();
            gameProfileEditMaxTime.text = m_editGameProfile.MaxTime.ToString();
            gameProfileEditIntensity.value = m_editGameProfile.Intensity;
            gameProfileEditMenu.gameObject.SetActive(true);
        }
        else
        {
            gameProfileEditMenu.gameObject.SetActive(false);
        }
    }

    public void AddGameProfile()
    {
        string profileName = "New Profile";
        m_editGameProfile = m_profileManager.AddGameProfile(profileName, 30, 60, 0.5f);
        UpdateGameProfileList();
    }

    public void RemoveGameProfile()
    {
        if (m_editGameProfile != null)
        {
            m_profileManager.RemoveGameProfile(m_editGameProfile);
            if (m_chosenGameProfile == m_editGameProfile)
            {
                m_chosenGameProfile = null;
            }
            m_editGameProfile = null;
            UpdateGameProfileList();
        }
    }

    public void GameProfileSlidersChanged()
    {
        gameProfileTextIntensity.text = "Overtime Intensity: " + (int)(100 * gameProfileEditIntensity.value) + "%";
    }

    public void GameProfileChanged()
    {
        if (m_editGameProfile != null)
        {
            m_editGameProfile.Name = gameProfileEditName.text;
            if (!string.IsNullOrEmpty(gameProfileEditTurnTime.text))
            {
                m_editGameProfile.TurnTime = int.Parse(gameProfileEditTurnTime.text);
            }
            if (!string.IsNullOrEmpty(gameProfileEditMaxTime.text))
            {
                m_editGameProfile.MaxTime = int.Parse(gameProfileEditMaxTime.text);
            }
            m_editGameProfile.Intensity = gameProfileEditIntensity.value;
            m_editGameProfile = null;
            UpdateGameProfileList();
        }
    }

    public void ToMainMenu()
    {
        m_profileManager.Save();
        PlayerProfileChanged();
        GameProfileChanged();
        UpdateMainMenu();
    }
}