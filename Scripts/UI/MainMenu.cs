using Godot;

public partial class MainMenu : Control
{
	private Button startButton;
	private Button quitButton;

	public override void _Ready()
	{
		startButton = GetNode<Button>(
			"CenterContainer/VBoxContainer/StartButton"
		);

		quitButton = GetNode<Button>(
			"CenterContainer/VBoxContainer/QuitButton"
		);

		startButton.Pressed += OnStartPressed;
		quitButton.Pressed += OnQuitPressed;
	}

	private void OnStartPressed()
	{
		GetTree().ChangeSceneToFile(
			"res://Scenes/UI/Intro.tscn"
		);
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
