using Godot;
using System.Text.Json;

public partial class Main : Control
{

	private DialogueLine[] dialogue;
	private int currentLine = 0;

	private RichTextLabel dialogueLabel;

	private float typingSpeed = 30f;
	private float typingTimer = 0f;

	private bool isTyping = false;

	private DialogueData dialogueData;
	private LocationsData locationsData;

	private Button choiceButton1;
	private Button choiceButton2;

	private Label locationLabel;

	private TextureRect backgroundImage;

	private AnimationPlayer animationPlayer;

	private string targetLocation;

	private DialogueData LoadDialogue(string filePath)
	{
		string json = FileAccess.GetFileAsString(
			filePath
		);

		return JsonSerializer.Deserialize<DialogueData>(json);
	}

	private void ShowChoices()
	{
		choiceButton1.Visible = true;
		choiceButton2.Visible = true;
	}
	private LocationData GetLocationById(
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
	private LocationsData LoadLocations()
	{
		string json = FileAccess.GetFileAsString(
			"res://Locations/locations.json"
		);

		return JsonSerializer.Deserialize<LocationsData>(json);
	}

	private void UpdateLocation()
	{
		LocationData location =
			GetLocationById(
				currentLocation,
				locationsData
			);

		locationLabel.Text = location.name;

		backgroundImage.Texture =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/Backgrounds/" +
				location.background
			);
	}

	private string currentLocation = "bedroom";

	private void ChangeLocation()
	{
		GD.Print("CHANGE LOCATION CALLED");

		currentLocation = targetLocation;
		UpdateLocation();
	}

	public void TestMethod()
	{
		GD.Print("TEST METHOD CALLED");
	}

	public override void _Ready()
	{
		dialogueLabel = GetNode<RichTextLabel>(
			"DialoguePanel/RichTextLabel"
		);

		locationLabel = GetNode<Label>(
			"SidePanel/LocationLabel"
		);

		backgroundImage = GetNode<TextureRect>(
			"BackgroundImage"
		);

		choiceButton1 = GetNode<Button>(
			"DialoguePanel/VBoxContainer/ChoiceButton1"
		);

		choiceButton2 = GetNode<Button>(
			"DialoguePanel/VBoxContainer/ChoiceButton2"
		);

		dialogueData = LoadDialogue(
			"res://Dialogue/Chapters/intro.json"
		);

		dialogue = dialogueData.lines;

		choiceButton1.Text = dialogueData.choices[0].text;
		choiceButton2.Text = dialogueData.choices[1].text;

		choiceButton1.Pressed += OnChoice1Pressed;
		choiceButton2.Pressed += OnChoice2Pressed;

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		ShowDialogueLine();

		locationsData = LoadLocations();

		UpdateLocation();

		animationPlayer = GetNode<AnimationPlayer>(
			"AnimationPlayer"
		);

		animationPlayer.AnimationFinished += OnAnimationFinished;
	}

	public override void _Process(double delta)
	{
		if (!isTyping)
			return;

		typingTimer += (float)delta;

		if (typingTimer >= 1f / typingSpeed)
		{
			typingTimer = 0f;

			dialogueLabel.VisibleCharacters++;

			if (dialogueLabel.VisibleCharacters >= dialogueLabel.Text.Length)
			{
				isTyping = false;

				if (currentLine == dialogue.Length - 1)
				{
					ShowChoices();
				}
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			if (isTyping)
			{
				dialogueLabel.VisibleCharacters =
					dialogueLabel.Text.Length;

				isTyping = false;

				if (currentLine == dialogue.Length - 1)
				{
					ShowChoices();
				}
			}
			else
			{
				if (currentLine < dialogue.Length - 1)
				{
					currentLine++;
					ShowDialogueLine();
				}
			}
		}
	}

	private void ShowDialogueLine()
	{
		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		dialogueLabel.Text = dialogue[currentLine].text;
		dialogueLabel.VisibleCharacters = 0;

		typingTimer = 0f;
		isTyping = true;
	}

	private void OnChoice1Pressed()
	{
		targetLocation = "bedroom";
		animationPlayer.Play("FadeOut");
	}

	private void OnChoice2Pressed()
	{
		targetLocation = "hallway";
		animationPlayer.Play("FadeOut");
	}

	private void OnAnimationFinished(StringName animationName)
	{
		GD.Print("Animation finished: " + animationName);

		if (animationName == "FadeOut")
		{
			GD.Print("Changing location to: " + targetLocation);

			currentLocation = targetLocation;

			UpdateLocation();

			animationPlayer.Play("FadeIn");
		}
	}
}
