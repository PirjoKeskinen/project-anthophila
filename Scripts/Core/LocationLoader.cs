using Godot;
using System.Text.Json;

public class LocationLoader
{
	public LocationsData Load(string filePath)
	{
		string json = FileAccess.GetFileAsString(
			filePath
		);

		return JsonSerializer.Deserialize<LocationsData>(json);
	}

	public LocationData GetById(
	string locationId,
	LocationsData data
	)
	{
		foreach (LocationData location in data.locations)
		{
			if (location.id == locationId)
			{
				return location;
			}
		}

		return null;
	}
}
