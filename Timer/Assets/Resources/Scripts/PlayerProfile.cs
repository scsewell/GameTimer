using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerProfile : IProfile
{
    private Dictionary<string, string> profileMap;

    private const string NAME = "name";

    public string Name
    {
        set { profileMap[NAME] = value; }
        get { return profileMap[NAME]; }
    }

    public PlayerProfile(string name)
    {
        profileMap = new Dictionary<string, string>();
        profileMap.Add(NAME, name);
    }

    public PlayerProfile(Dictionary<string, string> profileMap)
    {
        this.profileMap = profileMap;
    }

    public Dictionary<string, string> GetProfileMap()
    {
        return profileMap;
    }
}
