using Godot;
using System.Text.Json;

public partial class Diary : Control
{
	private TextureRect diaryImage;
	private RichTextLabel diaryText;
	private AnimationPlayer animationPlayer;
	private DiaryData diaryData;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		diaryImage = GetNode<TextureRect>("DiaryImage");
		diaryText = GetNode<RichTextLabel>("DiaryText");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		LoadDiaryData();
	}

	private void LoadDiaryData()
	{
		string json = FileAccess.GetFileAsString(
			"res://Dialogue/Chapters/diary_scene.json"
		);
		diaryData = JsonSerializer.Deserialize<DiaryData>(json);

		if (diaryData == null || diaryData.entries == null)
		{
			GD.PushError("Failed to load diary data.");
			return;
		}

		ShowEntry(0);
	}

	private void ShowEntry(int index)
	{
		if (index < 0 || index >= diaryData.entries.Length)
			return;

		DiaryEntry entry = diaryData.entries[index];

		diaryText.Text = entry.text;

		GD.Print("Diary text: ", diaryText.Text);
		GD.Print("DiaryText visible: ", diaryText.Visible);

		if (string.IsNullOrEmpty(entry.image))
		{
			diaryImage.Texture = null;
		}
		else
		{
			diaryImage.Texture = ResourceLoader.Load<Texture2D>(
				"res://Assets/Diary/" + entry.image
			);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
