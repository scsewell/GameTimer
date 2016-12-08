using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameProfile : IProfile
{
    private Dictionary<string, string> profileMap;

    private const string NAME = "name";

    public string Name
    {
        set { profileMap[NAME] = value; }
        get { return profileMap[NAME]; }
    }

    public GameProfile(string name)
    {
        profileMap = new Dictionary<string, string>();
        profileMap.Add(NAME, name);
    }

    public GameProfile(Dictionary<string, string> profileMap)
    {
        this.profileMap = profileMap;
    }

    public Dictionary<string, string> GetProfileMap()
    {
        return profileMap;
    }
}
