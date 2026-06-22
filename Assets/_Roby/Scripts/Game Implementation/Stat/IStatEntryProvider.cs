using System.Collections.Generic;

public interface IStatEntryProvider
{
    public List<StatEntry> StatEntries { get; }
}