using Godot;
using System.Collections.Generic;

public class Inventory
{
    private readonly HashSet<string> items = new();

    public void AddItem(string itemId)
    {
        items.Add(itemId);
    }

    public bool HasItem(string itemId)
    {
        return items.Contains(itemId);
    }

    public void RemoveItem(string itemId)
    {
        items.Remove(itemId);
    }

    public void PrintItems()
    {
        foreach (string item in items)
        {
            GD.Print(item);
        }
    }

    public IEnumerable<string> GetItems()
    {
        return items;
    }

    public bool HasItems()
    {
        return items.Count > 0;
    }
}