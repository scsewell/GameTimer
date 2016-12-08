using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ProfileManager
{
    private const string ALL_PROFILES = "allProfiles";
    private const string PLAYER_PROFILES = "playerProfiles";
    private const string GAME_PROFILES = "gameProfiles";
    private const string SENTINEL = "sentinel";

    private Dictionary<string, List<IProfile>> profiles;

    public ProfileManager()
    {
        profiles = new Dictionary<string, List<IProfile>>();
        profiles.Add(PLAYER_PROFILES, new List<IProfile>());
        profiles.Add(GAME_PROFILES, new List<IProfile>());

        string profileMapString = PlayerPrefs.GetString(ALL_PROFILES, SENTINEL);

        if (profileMapString != SENTINEL)
        {
            Dictionary<string, List<Dictionary<string, string>>> profileMaps = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(profileMapString);
            foreach (Dictionary<string, string> player in profileMaps[PLAYER_PROFILES])
            {
                profiles[PLAYER_PROFILES].Add(new PlayerProfile(player));
            }
            /*
            foreach (Dictionary<string, string> game in profileMaps[GAME_PROFILES])
            {
                profiles[GAME_PROFILES].Add(new GameProfile(game));
            }*/
        }
    }

    public PlayerProfile AddPlayerProfile(string name)
    {
        PlayerProfile player = new PlayerProfile(name);
        profiles[PLAYER_PROFILES].Add(player);
        return player;
    }

    public void RemovePlayerProfile(PlayerProfile player)
    {
        profiles[PLAYER_PROFILES].Remove(player);
    }

    public List<PlayerProfile> GetPlayerProfiles()
    {
        return profiles[PLAYER_PROFILES].Cast<PlayerProfile>().ToList<PlayerProfile>();
    }

    public GameProfile AddGameProfile(string name)
    {
        GameProfile game = new GameProfile(name);
        profiles[GAME_PROFILES].Add(game);
        return game;
    }

    public void RemoveGameProfile(GameProfile game)
    {
        profiles[GAME_PROFILES].Remove(game);
    }

    public List<GameProfile> GetGameProfiles()
    {
        return profiles[GAME_PROFILES].Cast<GameProfile>().ToList<GameProfile>();
    }

    public void Save()
    {
        Dictionary<string, List<Dictionary<string, string>>> profileMaps = new Dictionary<string, List<Dictionary<string, string>>>();
        profileMaps.Add(PLAYER_PROFILES, new List<Dictionary<string, string>>());

        foreach (PlayerProfile player in profiles[PLAYER_PROFILES])
        {
            profileMaps[PLAYER_PROFILES].Add(player.GetProfileMap());
        }
        foreach (GameProfile game in profiles[GAME_PROFILES])
        {
            profileMaps[GAME_PROFILES].Add(game.GetProfileMap());
        }

        PlayerPrefs.SetString(ALL_PROFILES, JsonConvert.SerializeObject(profileMaps));
    }
}