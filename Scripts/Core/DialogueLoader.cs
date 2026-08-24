using Godot;
using System.Text.Json;

public class DialogueLoader
{
	public DialogueData Load(string filePath)
	{
		string json = FileAccess.GetFileAsString(
			filePath
		);

		return JsonSerializer.Deserialize<DialogueData>(json);
	}
}
