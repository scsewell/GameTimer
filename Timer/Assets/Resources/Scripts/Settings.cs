using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Settings : MonoBehaviour
{
    public int numOfPlayers;
    public int numBeeps;

    public float intialTurnTimer;
    public float averageTurnTime;
    public float turnMultiplesToMaxIntensity;
    public float minTimeBetweenBeeps;
    public float numOfBeepsTime;
    public float beepRatio;

    public List<TimerPlayer> timerPlayers;
}

public class TimerPlayer
{
    private readonly PlayerProfile profile;
    public PlayerProfile Profile
    {
        get { return profile; }
    }

    private readonly string name;
    public string Name
    {
        get { return name; }
    }

    private readonly Color color;
    public Color Color
    {
        get { return color; }
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
        this.profile = profile;
        this.name = name;
        this.color = color;
    }

    public bool IsGuest()
    {
        return profile == null;
    }
}
