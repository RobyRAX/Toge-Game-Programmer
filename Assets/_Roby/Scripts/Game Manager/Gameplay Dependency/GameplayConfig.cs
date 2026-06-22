using System.Collections.Generic;
using RAXY.Utility;
using UnityEngine;

public class GameplayConfig : Singleton<GameplayConfig>, IStatEntryProvider
{
    public GameplayConfigSO ConfigSO;

    public List<StatEntry> StatEntries => ConfigSO.StatEntries;
}
