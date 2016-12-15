using UnityEngine;

/// <summary>
/// Stores player data for an active game.
/// </summary>
public class TimerPlayer
{
    private readonly PlayerProfile m_profile;
    public PlayerProfile Profile
    {
        get { return m_profile; }
    }

    private readonly string m_name;
    public string Name
    {
        get { return m_name; }
    }

    private readonly Color m_color;
    public Color Color
    {
        get { return m_color; }
    }

    public TimerPlayer(string name, Color color)
        : this(null, name, color)
    {
    }

    public TimerPlayer(PlayerProfile profile, Color color)
        : this(profile, profile.Name, color)
    {
    }

    private TimerPlayer(PlayerProfile profile, string name, Color color)
    {
        m_profile = profile;
        m_name = name;
        m_color = color;
    }

    public bool IsGuest()
    {
        return m_profile == null;
    }
}
