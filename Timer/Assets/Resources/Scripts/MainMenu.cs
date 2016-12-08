using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    // Main Menu
    public RectTransform mainMenu;
    public Dropdown playerCount;
    public InputField initialTurnTime;
    public InputField averageTurnTime;
    public InputField turnMultiplesToMaxIntensity;
    public InputField minTimeBetweenBeeps;
    public InputField numOfBeepsTime;
    public InputField numBeeps;
    public InputField beepRatio;

    public RectTransform playerOptionsMenu;
    public RectTransform playerOptionsList;
    public RectTransform playerOptionPrefab;
    public Color[] playerColors;

    public RectTransform playerSelectMenu;
    public RectTransform playerSelectPanel;

    public RectTransform colorSelectMenu;
    public RectTransform colorSelectPrefab;

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
    
    // Private Vars
    private ProfileManager profileManager;
    private List<TimerPlayer> chosenPlayers;
    private PlayerProfile editPlayerProfile;
    //private PlayerProfile editGameProfile;

    
    public void Awake()
    {
        profileManager = new ProfileManager();
        chosenPlayers = new List<TimerPlayer>();
    }

    public void Start()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        mainMenu.gameObject.SetActive(true);
        playerProfileMenu.gameObject.SetActive(false);
        playerProfileEditMenu.gameObject.SetActive(false);
        playerOptionsMenu.gameObject.SetActive(true);
        playerSelectMenu.gameObject.SetActive(false);
        colorSelectMenu.gameObject.SetActive(false);

        playerCount.value = PlayerPrefs.GetInt("players") - 1;
        numBeeps.text = "" + PlayerPrefs.GetInt("numBeeps");
        initialTurnTime.text = "" + PlayerPrefs.GetFloat("initialTurnTime");
        averageTurnTime.text = "" + PlayerPrefs.GetFloat("averageTurnTime");
        turnMultiplesToMaxIntensity.text = "" + PlayerPrefs.GetFloat("turnMultiplesToMaxIntensity");
        minTimeBetweenBeeps.text = "" + PlayerPrefs.GetFloat("minTimeBetweenBeeps");
        numOfBeepsTime.text = "" + PlayerPrefs.GetFloat("numOfBeepsTime");
        beepRatio.text = "" + PlayerPrefs.GetFloat("beepRatio");

        PlayerNumChanged();
    }


    //---------- MAIN MENU ------------------------------
    
    public int GetAmountOfPlayers()
    {
        return playerCount.value + 1;
    }

    public void PlayerNumChanged()
    {
        while (chosenPlayers.Count != GetAmountOfPlayers())
        {
            if (chosenPlayers.Count > GetAmountOfPlayers())
            {
                chosenPlayers.RemoveAt(chosenPlayers.Count - 1);
            }
            else
            {
                chosenPlayers.Add(new TimerPlayer(GetUnusedGuestName(), GetUnusedColor()));
            }
        }

        UpdateChoosenPlayerList();
    }

    private void UpdateChoosenPlayerList()
    {
        playerOptionsMenu.gameObject.SetActive(true);
        playerSelectMenu.gameObject.SetActive(false);
        colorSelectMenu.gameObject.SetActive(false);

        foreach (RectTransform t in playerOptionsList)
        {
            Destroy(t.gameObject);
        }

        foreach (TimerPlayer player in chosenPlayers)
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
    }

    private string GetUnusedGuestName(string currentName = null)
    {
        string guestName = null;
        bool unusedNameFound = false;

        for (int guestNumber = 1; !unusedNameFound; guestNumber++)
        {   
            guestName = "Guest " + guestNumber;
            if (!chosenPlayers.Any((player) => { return (player.IsGuest() && player.Name == guestName && guestName != currentName); }))
            {
                unusedNameFound = true;
            }
        }
        return guestName;
    }

    private Color GetUnusedColor()
    {
        List<Color> avaliableColors = playerColors.ToList();

        foreach (TimerPlayer player in chosenPlayers)
        {
            if (avaliableColors.Contains(player.Color))
            {
                avaliableColors.Remove(player.Color);
            }
        }

        if (avaliableColors.Count > 0)
        {
            return avaliableColors[0];
        }
        else
        {
            return playerColors[0];
        }
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
            t.GetComponent<Button>().onClick.AddListener(() => { UpdateTimePlayer(player, tempColor); UpdateChoosenPlayerList(); });
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

        foreach (PlayerProfile playerProfile in profileManager.GetPlayerProfiles())
        {
            PlayerProfile tempPlayerProfile = playerProfile;
            Button button = AddPlayerButton(profilePrefab, tempPlayerProfile.Name, playerSelectPanel);
            button.onClick.AddListener(() => { UpdateTimePlayer(player, tempPlayerProfile); UpdateChoosenPlayerList(); });
        }
        
        Button guestButton = AddPlayerButton(profilePrefab, "Use guest player", playerSelectPanel);
        guestButton.GetComponentInChildren<Image>().color = new Color(109.0f / 255, 75.0f / 255, 49.0f / 255);
        guestButton.onClick.AddListener(() => { UpdateTimePlayer(player, GetUnusedGuestName(player.Name)); UpdateChoosenPlayerList(); });
    }
    
    private void UpdateTimePlayer(TimerPlayer oldPlayer, PlayerProfile profile)
    {
        chosenPlayers[chosenPlayers.IndexOf(oldPlayer)] = new TimerPlayer(profile, oldPlayer.Color);
    }

    private void UpdateTimePlayer(TimerPlayer oldPlayer, string name)
    {
        chosenPlayers[chosenPlayers.IndexOf(oldPlayer)] = new TimerPlayer(name, oldPlayer.Color);
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

        chosenPlayers[chosenPlayers.IndexOf(oldPlayer)] = newPlayer;
    }

    private Button AddPlayerButton(RectTransform prefab, string name, RectTransform parent)
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
        playerProfileEditMenu.gameObject.SetActive(false);

        editPlayerProfile = null;

        UpdatePlayerList();
        UpdateChoosenPlayerList();
    }

    public void ToGameProfileMenu()
    {
        mainMenu.gameObject.SetActive(false);
        gameProfileMenu.gameObject.SetActive(true);
        gameProfileEditMenu.gameObject.SetActive(false);

        editPlayerProfile = null;

        UpdatePlayerList();
        UpdateChoosenPlayerList();
    }

    public void StartTimer()
    {
        PlayerPrefs.SetInt("players", GetAmountOfPlayers());
        PlayerPrefs.SetInt("numBeeps", int.Parse(numBeeps.text));
        PlayerPrefs.SetFloat("initialTurnTime", float.Parse(initialTurnTime.text));
        PlayerPrefs.SetFloat("averageTurnTime", float.Parse(averageTurnTime.text));
        PlayerPrefs.SetFloat("turnMultiplesToMaxIntensity", float.Parse(turnMultiplesToMaxIntensity.text));
        PlayerPrefs.SetFloat("minTimeBetweenBeeps", float.Parse(minTimeBetweenBeeps.text));
        PlayerPrefs.SetFloat("numOfBeepsTime", float.Parse(numOfBeepsTime.text));
        PlayerPrefs.SetFloat("beepRatio", float.Parse(beepRatio.text));

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        GameObject persist = Instantiate(new GameObject());
        Object.DontDestroyOnLoad(persist);
        persist.tag = "Settings";
        Settings settings = persist.AddComponent<Settings>();

        settings.numOfPlayers                   = GetAmountOfPlayers();
        settings.numBeeps                       = int.Parse(numBeeps.text);
        settings.intialTurnTimer                = float.Parse(initialTurnTime.text);
        settings.averageTurnTime                = float.Parse(averageTurnTime.text);
        settings.turnMultiplesToMaxIntensity    = float.Parse(turnMultiplesToMaxIntensity.text);
        settings.minTimeBetweenBeeps            = float.Parse(minTimeBetweenBeeps.text);
        settings.numOfBeepsTime                 = float.Parse(numOfBeepsTime.text);
        settings.beepRatio                      = float.Parse(beepRatio.text);
        settings.timerPlayers                   = chosenPlayers;

        SceneManager.LoadScene(1);
    }


    //---------- EDIT PLAYER PROFILE MENU ------------------------------
    
    public void UpdatePlayerList()
    {
        foreach (RectTransform t in playerProfilesList)
        {
            if (t.GetSiblingIndex() != 0)
            {
                Destroy(t.gameObject);
            }
        }

        foreach (PlayerProfile player in profileManager.GetPlayerProfiles())
        {
            RectTransform t = Instantiate(profilePrefab);
            t.SetParent(playerProfilesList, false);
            t.SetSiblingIndex(1);
            t.GetComponentInChildren<Text>().text = player.Name;

            if (player == editPlayerProfile)
            {
                t.GetComponent<Image>().color = profileEditColor;
            }

            PlayerProfile tempPlayerProfile = player;
            t.GetComponent<Button>().onClick.AddListener(() => { editPlayerProfile = tempPlayerProfile; UpdatePlayerList(); });
        }
        
        if (editPlayerProfile != null)
        {
            playerProfileEditName.text = editPlayerProfile.Name;
            playerProfileEditMenu.gameObject.SetActive(true);
        }
        else
        {
            playerProfileEditMenu.gameObject.SetActive(false);
        }

        profileManager.Save();
    }

    public void AddPlayerProfile()
    {
        string newPlayer = "Unnamed Player";
        editPlayerProfile = profileManager.AddPlayerProfile(newPlayer);
        UpdatePlayerList();
    }

    public void RemovePlayerProfile()
    {
        if (editPlayerProfile != null)
        {
            profileManager.RemovePlayerProfile(editPlayerProfile);
            editPlayerProfile = null;
            UpdatePlayerList();
        }
    }

    public void PlayerSettingsChanged()
    {
        if (editPlayerProfile != null)
        {
            editPlayerProfile.Name = playerProfileEditName.text;
            UpdatePlayerList();
        }
    }

    public void ToMainMenu()
    {
        mainMenu.gameObject.SetActive(true);
        playerProfileMenu.gameObject.SetActive(false);
        playerProfileEditMenu.gameObject.SetActive(false);
    }


    //---------- EDIT GAME PROFILE MENU ------------------------------

}