using System.Collections.Generic;

public class GameState
{
    private Dictionary<string, bool> gameEvents = new();

    public bool HasEvent(string eventId)
    {
        return gameEvents.ContainsKey(eventId);
    }

    public void SetEvent(string eventId)
    {
        gameEvents[eventId] = true;
    }
}
