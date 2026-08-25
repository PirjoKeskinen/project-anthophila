using Godot;
using System.Collections.Generic;

public partial class Main : Control
{
	private RichTextLabel dialogueLabel;
	private DialogueLoader dialogueLoader = new();
	private LocationLoader locationLoader = new();
	private InspectableLoader inspectableLoader = new();
	private InventoryLoader inventoryLoader = new();
	private GameState gameState = new();
	private DialogueController dialogueController = new();
	private InventoryUIController inventoryUIController = new();
	private LookAroundController lookAroundController = new();
	private DialogueData dialogueData;
	private LocationsData locationsData;
	private InspectablesData inspectablesData;
	private InventoryData inventoryData;
	private Button choiceButton1;
	private Button choiceButton2;
	private Button[] exitButtons;
	private Button moveButton;
	private Button lookAroundButton;
	private VBoxContainer inventoryItems;
	private Button inventoryButton;
	private Button[] inspectButtons;
	private Button backButton;
	private Label locationLabel;
	private Label menuTitle;
	private Label speakerLabel;
	private TextureRect backgroundImage;
	private AudioStreamPlayer normalAnnouncement;
	private AudioStreamPlayer alarmAnnouncement;
	private AudioStreamPlayer doorSFX;
	private AudioStreamPlayer keycardSFX;
	private AudioStreamPlayer appleSFX;
	private AudioStreamPlayer protectiveSuitSFX;
	private AudioStreamPlayer applePickupSFX;
	private AudioStreamPlayer oxygenBottleSFX;
	private AudioStreamPlayer elevatorSFX;
	private AnimationPlayer animationPlayer;
	private string targetLocation;
	private Inventory inventory = new();
	private InspectableData currentInspectable;
	private int currentInspectablePage = 0;
	private bool isReadingInspectable = false;

	private LocationData GetCurrentLocation()
	{
		return locationLoader.GetById(
			currentLocation,
			locationsData
		);
	}

	private void LoadLocationDialogue()
	{
		LocationData location = GetCurrentLocation();

		dialogueData = dialogueLoader.Load(
			"res://Dialogue/Chapters/" + location.dialogue
		);

		dialogueController.SetDialogue(dialogueData.lines);
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

		bool hallwayUnlocked =
			currentLocation != "hallway" ||
			gameState.HasEvent("outside_mission_unlocked");

		for (int i = 0; i < exitButtons.Length; i++)
		{
			if (i < location.exits.Length)
			{
				LocationData exitLocation =
					locationLoader.GetById(
						location.exits[i],
						locationsData
					);

				if (exitLocation == null)
				{
					GD.PushError($"Location '{location.exits[i]}' not found.");
					continue;
				}

				if (
					currentLocation == "hallway" &&
					!hallwayUnlocked &&
					location.exits[i] != "greenhouse"
				)
				{
					exitButtons[i].Text = "";
					exitButtons[i].Visible = false;
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
			"DialoguePanel/MarginContainer/VBoxContainer/MarginContainer/RichTextLabel"
		);

		locationLabel = GetNode<Label>(
			"SidePanel/LocationLabel"
		);

		speakerLabel = GetNode<Label>(
			"DialoguePanel/MarginContainer/VBoxContainer/SpeakerLabel"
		);

		dialogueController.Setup(
			dialogueLabel,
			speakerLabel
		);

		menuTitle = GetNode<Label>(
			"SidePanel/MenuPanel/MarginContainer/VBoxContainer/MenuTitle"
		);

		backgroundImage = GetNode<TextureRect>(
			"BackgroundImage"
		);

		inventoryItems = GetNode<VBoxContainer>(
			"SidePanel/MenuPanel/MarginContainer/VBoxContainer/InventoryItems"
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
		lookAroundButton.Pressed += () =>
		{
			ClearDialogue();
			OnLookAroundPressed();
		};
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
		menuTitle.Visible = false;

		foreach (Button button in inspectButtons)
		{
			button.Visible = false;
		}

		locationsData = locationLoader.Load(
			"res://Locations/locations.json"
		);

		inspectablesData = inspectableLoader.Load(
			"res://Dialogue/Inspectables/inspectables.json"
		);

		lookAroundController.Setup(
			inspectButtons,
			locationLoader,
			locationsData,
			inspectableLoader,
			inspectablesData,
			menuTitle,
			exitButtons,
			moveButton,
			lookAroundButton,
			inventoryButton,
			backButton
		);

		inventoryData = inventoryLoader.Load(
			"res://Inventory/inventory.json"
		);

		inventoryUIController.Setup(
			inventoryItems,
			inventory,
			inventoryLoader,
			inventoryData,
			speakerLabel,
			ShowText
		);

		LoadLocationDialogue();

		UpdateLocation();

		normalAnnouncement = GetNode<AudioStreamPlayer>(
			"NormalAnnouncement"
		);

		alarmAnnouncement = GetNode<AudioStreamPlayer>(
			"AlarmAnnouncement"
		);

		doorSFX = GetNode<AudioStreamPlayer>(
			"DoorSFX"
		);

		keycardSFX = GetNode<AudioStreamPlayer>(
			"KeycardSFX"
		);

		animationPlayer = GetNode<AnimationPlayer>(
			"AnimationPlayer"
		);

		appleSFX = GetNode<AudioStreamPlayer>(
			"AppleSFX"
		);

		protectiveSuitSFX = GetNode<AudioStreamPlayer>(
			"ProtectiveSuitSFX"
		);

		applePickupSFX = GetNode<AudioStreamPlayer>(
			"ApplePickupSFX"
		);

		oxygenBottleSFX = GetNode<AudioStreamPlayer>(
			"OxygenBottleSFX"
		);

		elevatorSFX = GetNode<AudioStreamPlayer>(
			"ElevatorSFX"
		);

		animationPlayer.AnimationFinished += OnAnimationFinished;

		animationPlayer.Play("IntroFadeIn");
	}

	public override void _Process(double delta)
	{
		bool wasTyping = dialogueController.IsTyping();

		dialogueController.ProcessTyping(delta);

		if (!wasTyping || dialogueController.IsTyping())
			return;

		if (
			currentInspectable != null &&
			currentInspectable.id == "elevator_access" &&
			dialogueController.IsLastLine()
		)
		{
			choiceButton1.Text = "Go to the surface";
			choiceButton2.Text = "Stay in the bunker";

			choiceButton1.Visible = true;
			choiceButton2.Visible = true;
		}
		else
		{
			ShowInspectableChoices();
		}

		if (
			currentInspectable != null &&
			currentInspectable.id == "terminal"
		)
		{
			StartTerminalSequence();
		}

		LocationData location = GetCurrentLocation();

		if (
			dialogueController.IsLastLine() &&
			!location.dialoguePlayed
		)
		{
			location.dialoguePlayed = true;
			UpdateActionButtons();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			if (dialogueController.IsTyping())
			{
				dialogueController.FinishTyping();

				if (
					currentInspectable != null &&
					currentInspectable.id == "terminal"
				)
				{
					StartTerminalSequence();
				}

				ShowInspectableChoices();

				LocationData location = GetCurrentLocation();

				if (
					dialogueController.IsLastLine() &&
					!location.dialoguePlayed
				)
				{
					location.dialoguePlayed = true;
					UpdateActionButtons();
				}
			}
			else
			{
				if (isReadingInspectable && currentInspectable != null)
				{
					if (currentInspectablePage < currentInspectable.text.Length - 1)
					{
						currentInspectablePage++;

						ShowText(currentInspectable.text[currentInspectablePage]);
					}
					else
					{
						isReadingInspectable = false;
					}

					return;
				}

				if (dialogueController.HasNextLine())
				{
					dialogueController.NextLine();
				}
				else if (currentInspectable != null &&
						 currentInspectable.id == "log")
				{
					gameState.SetEvent("greenhouse_log_read");
					gameState.SetEvent("outside_mission_unlocked");

					UpdateActionButtons();
				}
			}
		}
	}

	private void ShowText(string text)
	{
		dialogueController.ShowText(text);
	}

	private async void ShowNotification(string text)
	{
		ShowText(text);

		await ToSignal(
			GetTree().CreateTimer(3.0f),
			SceneTreeTimer.SignalName.Timeout
		);

		ClearDialogue();
	}

	private void ShowDialogueLine()
	{
		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		dialogueController.ShowCurrentLine();
	}

	private void ClearDialogue()
	{
		dialogueLabel.Text = "";
		speakerLabel.Text = "";
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

		if (currentInspectable.id == "elevator_access")
		{
			choiceButton1.Text = "Go to the surface";
			choiceButton2.Text = "Stay in the bunker";
		}
		else if (currentInspectable.id == "apple")
		{
			choiceButton1.Text = "Take";
			choiceButton2.Text = "Eat";
		}
		else if (
			currentInspectable.itemId == "protective_suit" ||
			currentInspectable.itemId == "oxygen_bottle"
		)
		{
			choiceButton1.Text = "Equip";
			choiceButton2.Text = "Leave";
		}
		else
		{
			choiceButton1.Text = "Take";
			choiceButton2.Text = "Leave";
		}

		choiceButton1.Visible = true;
		choiceButton2.Visible = true;
	}

	private void OnChoice1Pressed()
	{
		if (
			currentInspectable != null &&
			currentInspectable.id == "elevator_access"
		)
		{
			ClearDialogue();

			targetLocation = "surface";

			elevatorSFX.Play();
			animationPlayer.Play("FadeOut");

			choiceButton1.Visible = false;
			choiceButton2.Visible = false;

			currentInspectable = null;

			return;
		}

		inventory.AddItem(currentInspectable.itemId);

		if (currentInspectable.itemId == "keycard")
		{
			keycardSFX.Play();
		}

		if (currentInspectable.itemId == "protective_suit")
		{
			protectiveSuitSFX.Play();
		}

		if (currentInspectable.itemId == "apple")
		{
			applePickupSFX.Play();
		}

		if (currentInspectable.itemId == "oxygen_bottle")
		{
			oxygenBottleSFX.Play();
		}

		InventoryItemData item =
			inventoryLoader.GetById(
				currentInspectable.itemId,
				inventoryData
			);

		if (
			currentInspectable.itemId == "protective_suit" ||
			currentInspectable.itemId == "oxygen_bottle"
		)
		{
			ShowNotification("You equip the " + item.name + ".");
		}
		else
		{
			ShowNotification("You took the " + item.name + ".");
		}

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

		inventoryUIController.Update();

		if (currentLocation == "bedroom")
		{
			UpdateBedroomBackground();
		}

		OnLookAroundPressed();

		isReadingInspectable = false;
		currentInspectable = null;

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		inventory.PrintItems();
	}

	private void OnChoice2Pressed()
	{
		if (
			currentInspectable != null &&
			currentInspectable.id == "elevator_access"
		)
		{
			choiceButton1.Visible = false;
			choiceButton2.Visible = false;

			currentInspectable = null;

			ClearDialogue();

			ShowMainMenu();

			return;
		}

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		if (currentInspectable != null &&
			currentInspectable.id == "apple")
		{
			appleSFX.Play();

			ClearDialogue();

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

			speakerLabel.Text = "SCIENTIST";
			ShowText("Yummy!");

			currentInspectable = null;

			OnLookAroundPressed();
		}
		else
		{
			ClearDialogue();

			currentInspectable = null;
		}
	}

	private void OnExitButtonPressed(int exitIndex)
	{
		GetViewport().GuiReleaseFocus();

		LocationData location = GetCurrentLocation();

		if (
			currentLocation == "bedroom" &&
			location.exits[exitIndex] == "hallway" &&
			!inventory.HasItem("keycard")
		)
		{
			speakerLabel.Text = "SCIENTIST";
			ShowText("Oh wait, I need to take my keycard...");
			return;
		}

		if (
			currentLocation == "elevator" &&
			location.exits[exitIndex] == "surface"
		)
		{
			if (
				!inventory.HasItem("keycard") ||
				!inventory.HasItem("protective_suit") ||
				!inventory.HasItem("oxygen_bottle")
			)
			{
				speakerLabel.Text = "SYSTEM";
				ShowText("ACCESS DENIED.\nSAFETY GEAR MISSING.");
				return;
			}

			dialogueData = dialogueLoader.Load(
				"res://Dialogue/Chapters/elevator-access.json"
			);

			dialogueController.SetDialogue(dialogueData.lines);

			currentInspectable = inspectableLoader.GetById(
				"elevator_access",
				inspectablesData
			);

			ShowDialogueLine();

			return;
		}

		targetLocation =
			location.exits[exitIndex];

		doorSFX.Play();

		animationPlayer.Play("FadeOut");
	}

	private void OnMovePressed()
	{
		ClearDialogue();

		SetMenuTitle("MOVE");

		lookAroundButton.Visible = false;
		moveButton.Visible = false;
		inventoryButton.Visible = false;

		foreach (Button button in exitButtons)
		{
			button.Visible = button.Text != "";
		}

		backButton.Visible = true;
	}

	private void OnLookAroundPressed()
	{
		lookAroundController.Show(currentLocation);
	}

	private void OnBackPressed()
	{
		isReadingInspectable = false;

		ClearDialogue();

		ShowMainMenu();
	}

	private void OnInventoryPressed()
	{
		ClearDialogue();

		SetMenuTitle("INVENTORY");

		moveButton.Visible = false;
		lookAroundButton.Visible = false;
		inventoryButton.Visible = false;

		foreach (Button button in exitButtons)
		{
			button.Visible = false;
		}

		foreach (Button button in inspectButtons)
		{
			button.Visible = false;
		}

		inventoryItems.Visible = true;

		inventoryUIController.Update();

		backButton.Visible = true;
	}

	private void OnInspectButtonPressed(int index)
	{
		GetViewport().GuiReleaseFocus();
		LocationData location = GetCurrentLocation();

		string id = location.inspectables[index];

		currentInspectable = inspectableLoader.GetById(
			id,
			inspectablesData
		);

		if (id == "elevator_access")
		{
			if (
				!inventory.HasItem("keycard") ||
				!inventory.HasItem("protective_suit") ||
				!inventory.HasItem("oxygen_bottle")
			)
			{
				speakerLabel.Text = "SYSTEM";
				ShowText("ACCESS DENIED. SAFETY GEAR MISSING.");
				return;
			}
		}

		if (id == "card_reader")
		{
			if (!inventory.HasItem("keycard"))
			{
				speakerLabel.Text = "CARD READER";
				ShowText("Keycard goes here to access the elevator.");
				return;
			}
		}

		if (
			id == "terminal" &&
			gameState.HasEvent("alarm_triggered")
		)
		{
			currentInspectable = inspectableLoader.GetById(
				"terminal_alarm",
				inspectablesData
			);

			speakerLabel.Text = currentInspectable.name.ToUpper();

			ShowText(currentInspectable.text[0]);

			return;
		}

		if (id == "terminal")
		{
			normalAnnouncement.Play();
		}

		currentInspectablePage = 0;

		speakerLabel.Text = currentInspectable.name.ToUpper();

		if (
			!string.IsNullOrEmpty(currentInspectable.dialogue) &&
			!(
				currentInspectable.id == "log" &&
				gameState.HasEvent("greenhouse_log_read")
			)
		)
		{
			isReadingInspectable = false;

			dialogueData = dialogueLoader.Load(
				"res://Dialogue/Chapters/" +
				currentInspectable.dialogue
			);

			dialogueController.SetDialogue(dialogueData.lines);

			ShowDialogueLine();
		}
		else
		{
			isReadingInspectable = true;

			ShowText(currentInspectable.text[currentInspectablePage]);
		}

		if (id == "terminal")
		{
			normalAnnouncement.Play();
		}
	}

	private void ShowMainMenu()
	{
		bool visible = GetCurrentLocation().dialoguePlayed;

		menuTitle.Visible = visible;
		SetMenuTitle("ACTIONS");

		inventoryItems.Visible = false;

		moveButton.Visible = visible && gameState.HasEvent("alarm_triggered");
		lookAroundButton.Visible = visible;

		inventoryButton.Visible = visible && inventory.HasItems();

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

	private void UpdateBedroomBackground()
	{
		bool hasKeycard = inventory.HasItem("keycard");
		bool alarmTriggered =
			gameState.HasEvent("alarm_triggered");

		string background;

		if (alarmTriggered && hasKeycard)
		{
			background = "bedroom-warning-nocard.png";
		}
		else if (alarmTriggered && !hasKeycard)
		{
			background = "bedroom-warning.png";
		}
		else if (!alarmTriggered && hasKeycard)
		{
			background = "bedroom-nocard.png";
		}
		else
		{
			background = "bedroom.png";
		}

		backgroundImage.Texture =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/Backgrounds/" + background
			);
	}

	private async void StartTerminalSequence()
	{
		currentInspectable = null;

		await ToSignal(
			normalAnnouncement,
			AudioStreamPlayer.SignalName.Finished
		);

		speakerLabel.Text = "SCIENTIST";
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

		gameState.SetEvent("alarm_triggered");
		UpdateBedroomBackground();

		InspectableData alarmTerminal =
			inspectableLoader.GetById(
				"terminal_alarm",
				inspectablesData
			);

		speakerLabel.Text = alarmTerminal.name.ToUpper();

		ShowText(alarmTerminal.text[0]);

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

		speakerLabel.Text = "SCIENTIST";

		ShowText("Fire...? I have to get to the Botanical Sector!");

		await ToSignal(
			GetTree().CreateTimer(2.0f),
			SceneTreeTimer.SignalName.Timeout
		);

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

			if (
				currentLocation == "elevator" &&
				targetLocation == "surface"
			)
			{
				await ToSignal(
					elevatorSFX,
					AudioStreamPlayer.SignalName.Finished
				);
			}

			currentLocation = targetLocation;

			UpdateLocation();

			animationPlayer.Play("FadeIn");

			return;
		}

		GD.Print("Animation finished: " + animationName);

		if (animationName == "FadeIn")
		{
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
		}
	}
}
