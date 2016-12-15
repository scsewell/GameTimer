using System.Collections.Generic;

/// <summary>
/// Stores information for a game profile. Designed to be easily serialized.
/// </summary>
public class GameProfile : IProfile
{
    private const string NAME = "name";
    private const string TURN_TIME = "turnTime";
    private const string MAX_TIME = "maxTime";
    private const string INTENSITY = "intensity";

    // maps profile properties' names to their values
    private Dictionary<string, string> m_profileMap;

    public string Name
    {
        set { m_profileMap[NAME] = value; }
        get { return m_profileMap[NAME]; }
    }

    public int TurnTime
    {
        set { m_profileMap[TURN_TIME] = value.ToString(); }
        get { return int.Parse(m_profileMap[TURN_TIME]); }
    }

    public int MaxTime
    {
        set { m_profileMap[MAX_TIME] = value.ToString(); }
        get { return int.Parse(m_profileMap[MAX_TIME]); }
    }

    public float Intensity
    {
        set { m_profileMap[INTENSITY] = value.ToString(); }
        get { return float.Parse(m_profileMap[INTENSITY]); }
    }

    /// <summary>
    /// Creates a new game profile from scratch.
    /// </summary>
    /// <param name="name">The name of the profile.</param>
    /// <param name="turnTime">The expected turn time.</param>
    /// <param name="maxTime">When max alarm intensity is reached.</param>
    /// <param name="intensity">How much the intensity of the alarm increases after the turn time is exceeded.</param>
    public GameProfile(string name, int turnTime, int maxTime, float intensity)
    {
        m_profileMap = new Dictionary<string, string>();
        m_profileMap.Add(NAME, name);
        m_profileMap.Add(TURN_TIME, turnTime.ToString());
        m_profileMap.Add(MAX_TIME, maxTime.ToString());
        m_profileMap.Add(INTENSITY, intensity.ToString());
    }

    /// <summary>
    /// Creates a new game profile from the property map.
    /// </summary>
    /// <param name="profileMap">A game profile property map.</param>
    public GameProfile(Dictionary<string, string> profileMap)
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
