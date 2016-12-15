using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Stores, loads, and saves profile data.
/// </summary>
public class ProfileManager
{
    private const string ALL_PROFILES = "allProfiles";
    private const string PLAYER_PROFILES = "playerProfiles";
    private const string GAME_PROFILES = "gameProfiles";

    private Dictionary<string, List<IProfile>> profiles;

    /// <summary>
    /// Constructor that loads any previous profile data from Unity Player Prefs.
    /// </summary>
    public ProfileManager()
    {
        profiles = new Dictionary<string, List<IProfile>>();
        profiles.Add(PLAYER_PROFILES, new List<IProfile>());
        profiles.Add(GAME_PROFILES, new List<IProfile>());

        // load any profiles that were saved
        if (PlayerPrefs.HasKey(ALL_PROFILES))
        {
            string profileMapString = PlayerPrefs.GetString(ALL_PROFILES);

            Dictionary<string, List<Dictionary<string, string>>> profileMaps = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(profileMapString);
            if (profileMaps.ContainsKey(PLAYER_PROFILES))
            {
                foreach (Dictionary<string, string> player in profileMaps[PLAYER_PROFILES])
                {
                    profiles[PLAYER_PROFILES].Add(new PlayerProfile(player));
                }
            }
            if (profileMaps.ContainsKey(GAME_PROFILES))
            {
                foreach (Dictionary<string, string> game in profileMaps[GAME_PROFILES])
                {
                    profiles[GAME_PROFILES].Add(new GameProfile(game));
                }
            }
        }
    }

    /// <summary>
    /// Creates a new player profile.
    /// </summary>
    /// <param name="name">The player's name.</param>
    /// <returns>The newly created profile.</returns>
    public PlayerProfile AddPlayerProfile(string name)
    {
        PlayerProfile player = new PlayerProfile(name);
        profiles[PLAYER_PROFILES].Add(player);
        return player;
    }

    /// <summary>
    /// Removes a player profile.
    /// </summary>
    /// <param name="player">The player profile to remove.</param>
    public void RemovePlayerProfile(PlayerProfile player)
    {
        profiles[PLAYER_PROFILES].Remove(player);
    }

    /// <summary>
    /// Gets all the player profiles as a list.
    /// </summary>
    /// <returns>A newly created list.</returns>
    public List<PlayerProfile> GetPlayerProfiles()
    {
        return profiles[PLAYER_PROFILES].Cast<PlayerProfile>().ToList();
    }

    /// <summary>
    /// Creates a new game profile.
    /// </summary>
    /// <param name="name">The profile's name.</param>
    /// <param name="turnTime">The expected turn time.</param>
    /// <param name="maxTime">When max alarm intensity is reached.</param>
    /// <param name="intensity">How much the intensity of the alarm increases after the turn time is exceeded.</param>
    /// <returns>The newly created profile</returns>
    public GameProfile AddGameProfile(string name, int turnTime, int maxTime, float intensity)
    {
        GameProfile game = new GameProfile(name, turnTime, maxTime, intensity);
        profiles[GAME_PROFILES].Add(game);
        return game;
    }

    /// <summary>
    /// Removes a game profile.
    /// </summary>
    /// <param name="game">The game profile to remove.</param>
    public void RemoveGameProfile(GameProfile game)
    {
        profiles[GAME_PROFILES].Remove(game);
    }

    /// <summary>
    /// Gets all the game profiles as a list.
    /// </summary>
    /// <returns>A newly created list.</returns>
    public List<GameProfile> GetGameProfiles()
    {
        return profiles[GAME_PROFILES].Cast<GameProfile>().ToList();
    }

    /// <summary>
    /// Writes all profile data to a string in the Unity Player Prefs.
    /// </summary>
    public void Save()
    {
        Dictionary<string, List<Dictionary<string, string>>> profileMaps = new Dictionary<string, List<Dictionary<string, string>>>();
        profileMaps.Add(PLAYER_PROFILES, new List<Dictionary<string, string>>());
        profileMaps.Add(GAME_PROFILES, new List<Dictionary<string, string>>());

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