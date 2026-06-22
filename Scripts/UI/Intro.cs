using Godot;
using System.Text.Json;

public partial class Intro : Control
{

	private RichTextLabel introText;

	private float typingSpeed = 30f;
	private float typingTimer = 0f;

	private bool isTyping = true;

	private Button skipButton;

	private bool showingTitle = false;

	private IntroData introData;
	private int currentSlide = 0;

	private TextureRect titleLogo;

	private Timer titleTimer;

	private ColorRect fadeRect;

	private IntroData LoadIntro(string filePath)
	{
		string json = FileAccess.GetFileAsString(filePath);

		return JsonSerializer.Deserialize<IntroData>(json);
	}

	public override void _Ready()
	{
		introText = GetNode<RichTextLabel>(
			"RichTextLabel"
		);

		titleLogo = GetNode<TextureRect>(
			"TitleLogo"
		);

		titleLogo.Visible = false;

		titleTimer = GetNode<Timer>(
			"TitleTimer"
		);

		titleTimer.Timeout += OnTitleTimeout;

		fadeRect = GetNode<ColorRect>(
			"FadeRect"
		);

		fadeRect.Visible = false;

		skipButton = GetNode<Button>(
			"SkipButton"
		);

		skipButton.Pressed += OnSkipPressed;

		introData = LoadIntro(
			"res://Dialogue/Intro/intro.json"
		);

		ShowSlide();
	}

	public override void _Process(double delta)
	{
		if (!isTyping)
			return;

		typingTimer += (float)delta;

		if (typingTimer >= 1f / typingSpeed)
		{
			typingTimer = 0f;

			introText.VisibleCharacters++;

			if (introText.VisibleCharacters >= introText.Text.Length)
			{
				isTyping = false;
			}
		}
	}

	private void OnSkipPressed()
	{
		ShowTitle();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			if (isTyping)
			{
				introText.VisibleCharacters =
					introText.Text.Length;

				isTyping = false;
			}
			else
			{
				if (showingTitle)
				{
					GetTree().ChangeSceneToFile(
						"res://Scenes/Gameplay/Main.tscn"
					);

					return;
				}

				currentSlide++;

				if (currentSlide < introData.slides.Length)
				{
					ShowSlide();
				}
				else
				{
					ShowTitle();
				}
			}
		}
	}

	private void ShowSlide()
	{
		titleLogo.Visible = false;
		introText.Visible = true;

		IntroSlide slide =
			introData.slides[currentSlide];

		introText.Text = slide.text;

		introText.VisibleCharacters = 0;

		typingTimer = 0f;

		isTyping = true;
	}

	private void ShowTitle()
	{
		showingTitle = true;

		titleLogo.Visible = true;
		introText.Visible = false;

		skipButton.Visible = false;

		titleTimer.Start();
	}

	private async void OnTitleTimeout()
	{
		titleLogo.Visible = false;

		fadeRect.Visible = true;

		await ToSignal(
			GetTree().CreateTimer(3f),
			SceneTreeTimer.SignalName.Timeout
		);

		GetTree().ChangeSceneToFile(
			"res://Scenes/Gameplay/Main.tscn"
		);
	}
}
