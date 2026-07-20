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

	private InventoryData inventoryData;

	private VBoxContainer inventoryContainer;

	private Button choiceButton1; // TODO: dialogue choices
	private Button choiceButton2; // TODO: dialogue choices

	private Button[] exitButtons;

	private Button moveButton;

	private Button lookAroundButton;

	private Panel menuPanel;
	private Panel inventoryPanel;
	private Button inventoryButton;

	private Button[] inspectButtons;

	private Button backButton;

	private Label locationLabel;

	private Label menuTitle;

	private TextureRect backgroundImage;

	private AudioStreamPlayer normalAnnouncement;
	private AudioStreamPlayer alarmAnnouncement;

	private AnimationPlayer animationPlayer;

	private string targetLocation;

	private Dictionary<string, bool> gameEvents = new();

	private Inventory inventory = new();

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

	private InventoryData LoadInventory()
	{
		string json = FileAccess.GetFileAsString(
			"res://Inventory/inventory.json"
		);

		return JsonSerializer.Deserialize<InventoryData>(json);
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

				if (exitLocation == null)
				{
					GD.PushError($"Location '{location.exits[i]}' not found.");
					continue;
				}

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
			"DialoguePanel/MarginContainer/VBoxContainer/RichTextLabel"
		);

		locationLabel = GetNode<Label>(
			"SidePanel/LocationLabel"
		);

		menuTitle = GetNode<Label>(
			"SidePanel/MenuPanel/MarginContainer/VBoxContainer/MenuTitle"
		);

		backgroundImage = GetNode<TextureRect>(
			"BackgroundImage"
		);

		menuPanel = GetNode<Panel>(
			"SidePanel/MenuPanel"
		);

		inventoryPanel = GetNode<Panel>(
			"SidePanel/InventoryPanel"
		);

		inventoryContainer = GetNode<VBoxContainer>(
			"SidePanel/InventoryPanel/MarginContainer/VBoxContainer"
		);

		choiceButton1 = GetNode<Button>(
			"DialoguePanel/MarginContainer/VBoxContainer/ChoiceContainer/ChoiceButton1"
		);

		choiceButton2 = GetNode<Button>(
			"DialoguePanel/MarginContainer/VBoxContainer/ChoiceContainer/ChoiceButton2"
		);

		exitButtons = new Button[]
		{
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/ExitButton1"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/ExitButton2"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/ExitButton3"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/ExitButton4"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/ExitButton5"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/ExitButton6"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/ExitButton7")
		};

		inspectButtons = new Button[]
		{
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/InspectButton1"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/InspectButton2"),
			GetNode<Button>("SidePanel/MenuPanel/MarginContainer/VBoxContainer/InspectButton3")
		};

		moveButton = GetNode<Button>(
			"SidePanel/MenuPanel/MarginContainer/VBoxContainer/MoveButton"
		);

		lookAroundButton = GetNode<Button>(
			"SidePanel/MenuPanel/MarginContainer/VBoxContainer/LookAroundButton"
		);

		backButton = GetNode<Button>(
			"SidePanel/MenuPanel/MarginContainer/VBoxContainer/BackButton"
		);

		inventoryButton = GetNode<Button>(
			"SidePanel/MenuPanel/MarginContainer/VBoxContainer/InventoryButton"
		);

		moveButton.Pressed += OnMovePressed;
		lookAroundButton.Pressed += OnLookAroundPressed;
		backButton.Pressed += OnBackPressed;
		inventoryButton.Pressed += OnInventoryPressed;

		choiceButton1.Pressed += OnChoice1Pressed;
		choiceButton2.Pressed += OnChoice2Pressed;

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
		inventoryData = LoadInventory();

		LoadLocationDialogue();

		UpdateLocation();

		normalAnnouncement = GetNode<AudioStreamPlayer>(
			"NormalAnnouncement"
		);

		alarmAnnouncement = GetNode<AudioStreamPlayer>(
			"AlarmAnnouncement"
		);

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

				ShowInspectableChoices();

				if (
					currentInspectable != null &&
					currentInspectable.id == "terminal"
				)
				{
					StartTerminalSequence();
				}

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

				ShowInspectableChoices();

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

						ShowText(currentInspectable.text[currentInspectablePage]);
					}
					else
					{
						isReadingInspectable = false;

						if (
							currentInspectable.id == "terminal" &&
							!HasEvent("alarm_triggered")
						)
						{
							dialogueLabel.Text = "\"Everything seems to be in order...\"";
							dialogueLabel.VisibleCharacters = 0;

							typingTimer = 0f;
							isTyping = true;
						}
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

	private void ShowText(string text)
	{
		dialogueLabel.Text = text;
		dialogueLabel.VisibleCharacters = 0;

		typingTimer = 0f;
		isTyping = true;
	}

	private async void ShowNotification(string text)
	{
		ShowText(text);

		await ToSignal(
			GetTree().CreateTimer(1.0f),
			SceneTreeTimer.SignalName.Timeout
		);

		dialogueLabel.Text = "";
	}

	private void ShowDialogueLine()
	{
		// TODO: show dialogue choices when dialogue data supports them

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		ShowText(dialogue[currentLine].text);
	}

	private void SetMenuTitle(string title)
	{
		menuTitle.Text = title;
	}

	private void ShowInspectableChoices()
	{
		if (
			currentInspectable == null ||
			currentInspectable.itemId == null ||
			!isReadingInspectable ||
			currentInspectablePage != currentInspectable.text.Length - 1
		)
		{
			return;
		}

		choiceButton1.Text = "Take";
		choiceButton2.Text = "Leave";

		choiceButton1.Visible = true;
		choiceButton2.Visible = true;
	}

	private void OnChoice1Pressed()
	{
		inventory.AddItem(currentInspectable.itemId);

		InventoryItemData item =
			GetInventoryItemById(currentInspectable.itemId);

		ShowNotification("You took the " + item.name + ".");

		if (currentInspectable.removeAfterPickup)
		{
			LocationData location = GetCurrentLocation();

			List<string> inspectables = new(location.inspectables);

			inspectables.Remove(currentInspectable.id);

			location.inspectables = inspectables.ToArray();

			if (!string.IsNullOrEmpty(location.backgroundAfterPickup))
			{
				location.background = location.backgroundAfterPickup;

				backgroundImage.Texture =
					ResourceLoader.Load<Texture2D>(
						"res://Assets/Backgrounds/" +
						location.background
					);
			}
		}

		UpdateInventoryUI();

		isReadingInspectable = false;
		currentInspectable = null;

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		inventory.PrintItems();
	}

	private void OnChoice2Pressed()
	{
		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		currentInspectable = null;
	}

	private void OnExitButtonPressed(int exitIndex)
	{
		LocationData location =
			GetCurrentLocation();

		targetLocation =
			location.exits[exitIndex];

		animationPlayer.Play("FadeOut");
	}

	private void OnMovePressed()
	{
		SetMenuTitle("Move");

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

		foreach (string inspectableId in location.inspectables)
		{
			GD.Print(" - " + inspectableId);
		}

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

		ShowMainMenu();
	}

	private void OnInventoryPressed()
	{
		UpdateInventoryUI();
		inventoryPanel.Visible = !inventoryPanel.Visible;
	}

	private void OnInspectButtonPressed(int index)
	{
		GetViewport().GuiReleaseFocus();
		LocationData location = GetCurrentLocation();

		string id = location.inspectables[index];

		currentInspectable = GetInspectableById(id);

		if (
			id == "terminal" &&
			HasEvent("alarm_triggered")
		)
		{
			currentInspectable = GetInspectableById("terminal_alarm");

			ShowText(currentInspectable.text[0]);

			return;
		}

		if (id == "terminal")
		{
			normalAnnouncement.Play();
		}

		currentInspectablePage = 0;
		isReadingInspectable = true;

		ShowText(currentInspectable.text[currentInspectablePage]);

		if (id == "terminal")
		{
			normalAnnouncement.Play();
		}
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

	private InventoryItemData GetInventoryItemById(string id)
	{
		foreach (InventoryItemData item in inventoryData.items)
		{
			if (item.id == id)
			{
				return item;
			}
		}

		return null;
	}

	private void UpdateInventoryUI()
	{
		foreach (Node child in inventoryContainer.GetChildren())
		{
			child.QueueFree();
		}

		foreach (string itemId in inventory.GetItems())
		{
			InventoryItemData item = GetInventoryItemById(itemId);

			if (item == null)
			{
				continue;
			}

			Button button = new Button();
			button.Text = item.name;

			inventoryContainer.AddChild(button);
		}
	}

	private void ShowMainMenu()
	{
		SetMenuTitle("Actions");

		bool visible = GetCurrentLocation().dialoguePlayed;

		moveButton.Visible = visible && HasEvent("alarm_triggered");
		lookAroundButton.Visible = visible;

		inventoryButton.Visible = inventory.HasItems();

		backButton.Visible = false;

		foreach (Button button in exitButtons)
		{
			button.Visible = false;
		}

		foreach (Button button in inspectButtons)
		{
			button.Visible = false;
		}
	}

	private void UpdateActionButtons()
	{
		ShowMainMenu();
	}

	private async void StartTerminalSequence()
	{
		currentInspectable = null;

		await ToSignal(
			normalAnnouncement,
			AudioStreamPlayer.SignalName.Finished
		);

		ShowText("Everything seems to be in order...");

		await ToSignal(
			GetTree().CreateTimer(1.0f),
			SceneTreeTimer.SignalName.Timeout
		);

		StartAlarmSequence();
	}
	private async void StartAlarmSequence()
	{
		await ToSignal(
			GetTree().CreateTimer(1.0f),
			SceneTreeTimer.SignalName.Timeout
		);

		backgroundImage.Texture =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/Backgrounds/bedroom-warning.png"
			);

		ShowText(
			"WARNING.\nFire detected in Botanical Sector.\nContainment protocols activated."
		);

		alarmAnnouncement.Play();

		isReadingInspectable = false;
		currentInspectable = null;

		foreach (Button button in inspectButtons)
		{
			button.Visible = false;
		}

		backButton.Visible = false;

		await ToSignal(
			alarmAnnouncement,
			AudioStreamPlayer.SignalName.Finished
		);

		ShowText("Fire...? I have to get to the Botanical Sector!");

		await ToSignal(
			GetTree().CreateTimer(2.0f),
			SceneTreeTimer.SignalName.Timeout
		);

		SetEvent("alarm_triggered");
		UpdateActionButtons();
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
