using Godot;
using System.Text.Json;

public class InventoryLoader
{
	public InventoryData Load(string filePath)
	{
		string json = FileAccess.GetFileAsString(
			filePath
		);

		return JsonSerializer.Deserialize<InventoryData>(json);
	}


	public InventoryItemData GetById(
		string id,
		InventoryData data
	)
	{
		foreach (InventoryItemData item in data.items)
		{
			if (item.id == id)
			{
				return item;
			}
		}

		return null;
	}
}
