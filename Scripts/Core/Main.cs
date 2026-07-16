using Godot;
using System.Collections.Generic;
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
	private InspectablesData inspectablesData;

	private Button choiceButton1; // TODO: dialogue choices
	private Button choiceButton2; // TODO: dialogue choices

	private Button[] exitButtons;

	private Button moveButton;

	private Button lookAroundButton;

	private Button[] inspectButtons;

	private Button backButton;

	private Label locationLabel;

	private TextureRect backgroundImage;

	private AnimationPlayer animationPlayer;

	private string targetLocation;

	private Dictionary<string, bool> gameEvents = new();

	private InspectableData currentInspectable;
	private int currentInspectablePage = 0;

	private bool isReadingInspectable = false;

	private bool HasEvent(string eventId)
	{
		return gameEvents.ContainsKey(eventId);
	}

	private void SetEvent(string eventId)
	{
		gameEvents[eventId] = true;
	}

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

	private InspectablesData LoadInspectables()
	{
		string json = FileAccess.GetFileAsString(
			"res://Dialogue/Inspectables/inspectables.json"
		);

		return JsonSerializer.Deserialize<InspectablesData>(json);
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

		for (int i = 0; i < exitButtons.Length; i++)
		{
			if (i < location.exits.Length)
			{
				LocationData exitLocation =
					GetLocationById(
						location.exits[i],
						locationsData
					);

				exitButtons[i].Text = exitLocation.name;
				exitButtons[i].Visible = true;
			}
			else
			{
				exitButtons[i].Text = "";
				exitButtons[i].Visible = false;
			}
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

		exitButtons = new Button[]
		{
			GetNode<Button>("SidePanel/VBoxContainer/ExitButton1"),
			GetNode<Button>("SidePanel/VBoxContainer/ExitButton2"),
			GetNode<Button>("SidePanel/VBoxContainer/ExitButton3"),
			GetNode<Button>("SidePanel/VBoxContainer/ExitButton4"),
			GetNode<Button>("SidePanel/VBoxContainer/ExitButton5"),
			GetNode<Button>("SidePanel/VBoxContainer/ExitButton6"),
			GetNode<Button>("SidePanel/VBoxContainer/ExitButton7")
		};

		inspectButtons = new Button[]
		{
			GetNode<Button>("SidePanel/VBoxContainer/InspectButton1"),
			GetNode<Button>("SidePanel/VBoxContainer/InspectButton2"),
			GetNode<Button>("SidePanel/VBoxContainer/InspectButton3")
		};

		moveButton = GetNode<Button>(
			"SidePanel/VBoxContainer/MoveButton"
		);

		lookAroundButton = GetNode<Button>(
			"SidePanel/VBoxContainer/LookAroundButton"
		);

		backButton = GetNode<Button>(
			"SidePanel/VBoxContainer/BackButton"
		);

		moveButton.Pressed += OnMovePressed;
		lookAroundButton.Pressed += OnLookAroundPressed;
		backButton.Pressed += OnBackPressed;

		// choiceButton1.Pressed += OnChoice1Pressed;
		// choiceButton2.Pressed += OnChoice2Pressed;

		for (int i = 0; i < exitButtons.Length; i++)
		{
			int index = i;
			exitButtons[i].Pressed += () => OnExitButtonPressed(index);
		}

		for (int i = 0; i < inspectButtons.Length; i++)
		{
			int index = i;
			inspectButtons[i].Pressed += () => OnInspectButtonPressed(index);
		}

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;
		moveButton.Visible = false;
		lookAroundButton.Visible = false;
		backButton.Visible = false;

		foreach (Button button in inspectButtons)
		{
			button.Visible = false;
		}

		locationsData = LoadLocations();
		inspectablesData = LoadInspectables();

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
			else
			{
				if (isReadingInspectable)
				{
					if (currentInspectablePage < currentInspectable.text.Length - 1)
					{
						currentInspectablePage++;

						dialogueLabel.Text = currentInspectable.text[currentInspectablePage];
						dialogueLabel.VisibleCharacters = 0;

						typingTimer = 0f;
						isTyping = true;
					}
					else
					{
						isReadingInspectable = false;
					}

					return;
				}

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

	private void OnExitButtonPressed(int exitIndex)
	{
		GD.Print("Pressed exit " + exitIndex);
		LocationData location =
			GetCurrentLocation();

		targetLocation =
			location.exits[exitIndex];

		animationPlayer.Play("FadeOut");
	}

	private void OnMovePressed()
	{
		lookAroundButton.Visible = false;
		moveButton.Visible = false;

		foreach (Button button in exitButtons)
		{
			button.Visible = button.Text != "";
		}

		backButton.Visible = true;
	}

	private void OnLookAroundPressed()
	{
		moveButton.Visible = false;
		lookAroundButton.Visible = false;

		foreach (Button button in exitButtons)
		{
			button.Visible = false;
		}

		LocationData location = GetCurrentLocation();

		for (int i = 0; i < inspectButtons.Length; i++)
		{
			if (i < location.inspectables.Length)
			{
				InspectableData inspectable =
					GetInspectableById(location.inspectables[i]);

				inspectButtons[i].Text = inspectable.name;
				inspectButtons[i].Visible = true;
			}
			else
			{
				inspectButtons[i].Visible = false;
			}
		}

		backButton.Visible = true;
	}

	private void OnBackPressed()
	{
		isReadingInspectable = false;
		dialogueLabel.Text = "";

		backButton.Visible = false;

		foreach (Button button in inspectButtons)
		{
			button.Visible = false;
		}

		foreach (Button button in exitButtons)
		{
			button.Visible = false;
		}

		bool visible = GetCurrentLocation().dialoguePlayed;

		moveButton.Visible = visible && HasEvent("alarm_triggered");
		lookAroundButton.Visible = visible;
	}

	private void OnInspectButtonPressed(int index)
	{
		GetViewport().GuiReleaseFocus();
		GD.Print("OnInspectButtonPressed");
		LocationData location = GetCurrentLocation();

		string id = location.inspectables[index];

		currentInspectable = GetInspectableById(id);
		currentInspectablePage = 0;
		isReadingInspectable = true;

		dialogueLabel.Text = currentInspectable.text[currentInspectablePage];

		if (currentInspectable.eventId != null)
		{
			SetEvent(currentInspectable.eventId);
		}

		dialogueLabel.VisibleCharacters = 0;

		typingTimer = 0f;
		isTyping = true;
	}

	private InspectableData GetInspectableById(string id)
	{
		foreach (InspectableData inspectable in inspectablesData.inspectables)
		{
			if (inspectable.id == id)
			{
				return inspectable;
			}
		}

		return null;
	}

	private void UpdateActionButtons()
	{
		bool visible = GetCurrentLocation().dialoguePlayed;

		moveButton.Visible = visible && HasEvent("alarm_triggered");
		lookAroundButton.Visible = visible;

		foreach (Button button in exitButtons)
		{
			button.Visible = false;
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
			await ToSignal(
				GetTree().CreateTimer(0.25f),
				SceneTreeTimer.SignalName.Timeout
			);

			currentLocation = targetLocation;

			UpdateLocation();

			LocationData location = GetCurrentLocation();

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
