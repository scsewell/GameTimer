using System.Collections.Generic;

/// <summary>
/// Stores information for a single user. Designed to be easily serialized.
/// </summary>
public class PlayerProfile : IProfile
{
    private const string NAME = "name";

    // maps profile properties' names to their values
    private Dictionary<string, string> m_profileMap;

    public string Name
    {
        set { m_profileMap[NAME] = value; }
        get { return m_profileMap[NAME]; }
    }

    /// <summary>
    /// Creates a new player profile.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    public PlayerProfile(string name)
    {
        m_profileMap = new Dictionary<string, string>();
        m_profileMap.Add(NAME, name);
    }

    /// <summary>
    /// Creates a new player profile from the property map.
    /// </summary>
    /// <param name="profileMap">A player profile property map.</param>
    public PlayerProfile(Dictionary<string, string> profileMap)
    {
        m_profileMap = profileMap;
    }

    /// <summary>
    /// Gets the profile map.
    /// </summary>
    /// <returns>A reference to the profiles property map.</returns>
    public Dictionary<string, string> GetProfileMap()
    {
        return m_profileMap;
    }
}
