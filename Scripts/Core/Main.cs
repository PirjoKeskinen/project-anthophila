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

	private Button choiceButton1; // TODO: dialogue choices
	private Button choiceButton2; // TODO: dialogue choices

	private Button exitButton1;

	private Button lookAroundButton;

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

	private LocationData GetCurrentLocation()
	{
		return GetLocationById(
			currentLocation,
			locationsData
		);
	}

	private LocationsData LoadLocations()
	{
		string json = FileAccess.GetFileAsString(
			"res://Locations/locations.json"
		);

		return JsonSerializer.Deserialize<LocationsData>(json);
	}

	private void LoadLocationDialogue()
	{
		LocationData location = GetCurrentLocation();

		dialogueData = LoadDialogue(
			"res://Dialogue/Chapters/" + location.dialogue
		);

		dialogue = dialogueData.lines;

		currentLine = 0;
	}

	private void UpdateLocation()
	{
		LocationData location =
			GetCurrentLocation();

		locationLabel.Text = location.name;

		backgroundImage.Texture =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/Backgrounds/" +
				location.background
			);

		UpdateExitButtons();
		UpdateActionButtons();
	}

	private string currentLocation = "bedroom";

	private void UpdateExitButtons()
	{
		LocationData location = GetCurrentLocation();

		if (location.exits.Length > 0)
		{
			LocationData exitLocation =
				GetLocationById(
					location.exits[0],
					locationsData
				);

			exitButton1.Text =
				exitLocation.name;

			exitButton1.Visible = true;
		}
		else
		{
			exitButton1.Visible = false;
		}
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

		exitButton1 = GetNode<Button>(
			"SidePanel/VBoxContainer/ExitButton1"
		);

		lookAroundButton = GetNode<Button>(
			"SidePanel/VBoxContainer/LookAroundButton"
		);

		// choiceButton1.Pressed += OnChoice1Pressed;
		// choiceButton2.Pressed += OnChoice2Pressed;
		exitButton1.Pressed += OnExitButtonPressed;

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;
		lookAroundButton.Visible = false;

		locationsData = LoadLocations();

		LoadLocationDialogue();

		choiceButton1.Text = dialogueData.choices[0].text;
		choiceButton2.Text = dialogueData.choices[1].text;

		UpdateLocation();

		animationPlayer = GetNode<AnimationPlayer>(
			"AnimationPlayer"
		);

		animationPlayer.AnimationFinished += OnAnimationFinished;

		animationPlayer.Play("IntroFadeIn");
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

				LocationData location = GetCurrentLocation();

				if (
					currentLine == dialogue.Length - 1 &&
					!location.dialoguePlayed
				)
				{
					location.dialoguePlayed = true;
					UpdateActionButtons();
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
		// TODO: show dialogue choices when dialogue data supports them

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		dialogueLabel.Text = dialogue[currentLine].text;
		dialogueLabel.VisibleCharacters = 0;

		typingTimer = 0f;
		isTyping = true;
	}

	private void OnChoice1Pressed()
	{
		LocationData location =
			GetCurrentLocation();

		targetLocation =
			location.exits[0];

		animationPlayer.Play("FadeOut");
	}

	private void OnExitButtonPressed()
	{

		GD.Print("Exit button pressed");

		LocationData location =
			GetCurrentLocation();

		targetLocation =
			location.exits[0];

		animationPlayer.Play("FadeOut");
	}

	private void UpdateActionButtons()
	{
		if (GetCurrentLocation().dialoguePlayed)
		{
			lookAroundButton.Visible = true;
			exitButton1.Visible = true;
		}
		else
		{
			lookAroundButton.Visible = false;
			exitButton1.Visible = false;
		}
	}

	private async void OnAnimationFinished(StringName animationName)
	{
		if (animationName == "IntroFadeIn")
		{
			ShowDialogueLine();
			return;
		}

		if (animationName == "FadeOut")
		{
			GD.Print("Changing location to: " + targetLocation);

			await ToSignal(
				GetTree().CreateTimer(0.25f),
				SceneTreeTimer.SignalName.Timeout
			);

			currentLocation = targetLocation;

			UpdateLocation();

			LocationData location = GetCurrentLocation();

			GD.Print(location.id + "dialoguePlayed = " + location.dialoguePlayed);

			if (!location.dialoguePlayed)
			{
				LoadLocationDialogue();
				ShowDialogueLine();
			}
			else
			{
				dialogueLabel.Text = "";
			}

			animationPlayer.Play("FadeIn");
		}
	}
}
