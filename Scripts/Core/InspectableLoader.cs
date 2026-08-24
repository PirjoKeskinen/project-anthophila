using Godot;
using System.Text.Json;

public class InspectableLoader
{
	public InspectablesData Load(string filePath)
	{
		string json = FileAccess.GetFileAsString(
			filePath
		);

		return JsonSerializer.Deserialize<InspectablesData>(json);
	}

	public InspectableData GetById(
		string inspectableId,
		InspectablesData data
	)
	{
		foreach (InspectableData inspectable in data.inspectables)
		{
			if (inspectable.id == inspectableId)
			{
				return inspectable;
			}
		}

		return null;
	}
}
