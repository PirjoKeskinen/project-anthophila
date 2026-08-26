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
	private InteractionController interactionController = new();
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

		normalAnnouncement = GetNode<AudioStreamPlayer>(
			"NormalAnnouncement"
		);

		interactionController.Setup(
			choiceButton1,
			choiceButton2,
			inspectableLoader,
			inspectablesData,
			dialogueLoader,
			dialogueController,
			gameState,
			inventory,
			speakerLabel,
			normalAnnouncement,
			ShowText,
			ShowDialogueLine
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

		if (dialogueController.IsLastLine())
		{
			interactionController.ShowChoices();
		}

		if (
			interactionController.GetCurrentInspectable() != null &&
			interactionController.GetCurrentInspectable().id == "terminal"
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
					interactionController.GetCurrentInspectable() != null &&
					interactionController.GetCurrentInspectable().id == "terminal"
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
			else
			{
				if (interactionController.IsReadingInspectable())
				{
					interactionController.NextPage();
					return;
				}

				if (dialogueController.HasNextLine())
				{
					dialogueController.NextLine();
				}
				else if (
					interactionController.GetCurrentInspectable() != null &&
					interactionController.GetCurrentInspectable().id == "log")
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

	private void OnChoice1Pressed()
	{
		InspectableData currentInspectable =
			interactionController.GetCurrentInspectable();

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

			interactionController.ClearCurrentInspectable();

			return;
		}

		GD.Print("Before AddItem: " + currentInspectable.itemId);

		inventory.AddItem(currentInspectable.itemId);

		GD.Print("After AddItem");

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

		GD.Print(
			"itemId: " + currentInspectable.itemId +
			" | inventoryData null: " + (inventoryData == null)
		);

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

		choiceButton1.Visible = false;
		choiceButton2.Visible = false;

		interactionController.ClearCurrentInspectable();

		OnLookAroundPressed();

		inventory.PrintItems();
	}

	private void OnChoice2Pressed()
	{
		InspectableData currentInspectable =
			interactionController.GetCurrentInspectable();

		if (
			currentInspectable != null &&
			currentInspectable.id == "elevator_access"
		)
		{
			choiceButton1.Visible = false;
			choiceButton2.Visible = false;

			interactionController.ClearCurrentInspectable();

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

			interactionController.ClearCurrentInspectable();

			OnLookAroundPressed();
		}
		else
		{
			ClearDialogue();

			interactionController.ClearCurrentInspectable();
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

			interactionController.SetCurrentInspectable(
				inspectableLoader.GetById(
					"elevator_access",
					inspectablesData
				)
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

		interactionController.Inspect(index, location);

		if (
			location.inspectables[index] == "terminal" &&
			!gameState.HasEvent("alarm_triggered")
		)
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
