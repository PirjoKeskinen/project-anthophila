using Godot;

public partial class MainMenu : Control
{
	private Button startButton;
	private Button quitButton;

	private bool isTransitioning = false;

	private TextureRect startDecoration;

	private TextureRect quitDecoration;

	private Texture2D hexNormal;
	private Texture2D hexHover;
	private Texture2D hexPressed;

	private AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		startButton = GetNode<Button>(
			"CenterContainer/VBoxContainer/StartButton"
		);

		startDecoration =
			GetNode<TextureRect>(
				"CenterContainer/VBoxContainer/StartButton/TextureRect"
			);

		quitButton =
			GetNode<Button>(
				"CenterContainer/VBoxContainer/QuitButton"
			);

		quitDecoration =
			GetNode<TextureRect>(
				"CenterContainer/VBoxContainer/QuitButton/TextureRect"
			);

		startButton.Pressed += OnStartPressed;
		quitButton.Pressed += OnQuitPressed;

		hexNormal =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/UI/button-normal.png"
			);

		hexHover =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/UI/button-hover.png"
			);

		hexPressed =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/UI/button-pressed.png"
			);

		SetupButtonDecoration(
			startButton,
			startDecoration
		);

		SetupButtonDecoration(
			quitButton,
			quitDecoration
		);

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	private async void OnStartPressed()
	{

		isTransitioning = true;

		startButton.Disabled = true;
		quitButton.Disabled = true;

		startDecoration.Texture = hexPressed;

		animationPlayer.Play("FadeOut");

		await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

		await ToSignal(GetTree().CreateTimer(2.00), SceneTreeTimer.SignalName.Timeout);

		GetTree().ChangeSceneToFile("res://Scenes/UI/Intro.tscn");
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}

	private void SetupButtonDecoration(
		Button button,
		TextureRect decoration
	)

	{
		decoration.Texture = hexNormal;

		button.MouseEntered += () =>
		{
			if (isTransitioning)
				return;

			decoration.Texture = hexHover;
		};

		button.MouseExited += () =>
		{
			if (isTransitioning)
				return;

			decoration.Texture = hexNormal;
		};

		button.ButtonDown += () =>
		{
			if (isTransitioning)
				return;

			decoration.Texture = hexPressed;
		};

		button.ButtonUp += () =>
		{
			if (isTransitioning)
				return;

			decoration.Texture = hexHover;
		};
	}
}
