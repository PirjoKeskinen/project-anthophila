using Godot;
using System.Text.Json;

public partial class Intro : Control
{

	private RichTextLabel introText;

	private float typingSpeed = 20f;
	private float typingTimer = 0f;

	private bool isTyping = true;

	private Button skipButton;

	private bool showingTitle = false;

	private IntroData introData;
	private int currentSlide = 0;

	private TextureRect titleLogo;

	private Timer titleTimer;

	private ColorRect fadeRect;

	private TextureRect introImage;

	private string currentImagePath = "";

	private AnimationPlayer animationPlayer;

	private AudioStreamPlayer introMusic;

	private AudioStreamPlayer textSfx;

	private AudioStreamPlayer manImpactSfx;

	private AudioStreamPlayer beeSfx;

	private AudioStreamPlayer beeEchoSfx;

	private bool waitingForFadeOut = false;

	private bool noiseEnabled = true;

	private IntroData LoadIntro(string filePath)
	{
		string json = FileAccess.GetFileAsString(filePath);

		GD.Print(json);

		return JsonSerializer.Deserialize<IntroData>(json);
	}

	public override void _Ready()
	{
		introText = GetNode<RichTextLabel>(
			"RichTextLabel"
		);

		introText.BbcodeEnabled = true;

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

		introImage = GetNode<TextureRect>(
			"IntroImage"
		);

		animationPlayer = GetNode<AnimationPlayer>(
			"AnimationPlayer"
		);

		introMusic =
			GetNode<AudioStreamPlayer>("IntroMusic");

		textSfx = GetNode<AudioStreamPlayer>("TextSFX");

		manImpactSfx =
			GetNode<AudioStreamPlayer>("ManImpactSFX");

		beeSfx = GetNode<AudioStreamPlayer>("BeeSFX");

		beeEchoSfx = GetNode<AudioStreamPlayer>("BeeEchoSFX");

		animationPlayer.AnimationFinished += OnAnimationFinished;

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

				if (currentSlide < introData.slides.Length - 1)
				{
					IntroSlide slide = introData.slides[currentSlide];

					if (slide.fadeOut)
					{
						animationPlayer.Play("TextFadeOut");
						return;
					}

					string previousImage =
						introData.slides[currentSlide].image;

					currentSlide++;

					ShowText();

					string currentImage =
						introData.slides[currentSlide].image;

					bool firstImage =
						string.IsNullOrEmpty(previousImage);

					bool imageChanged =
						previousImage != currentImage;

					if (imageChanged)
					{
						if (firstImage)
						{
							ShowImage();
						}
						else
						{
							waitingForFadeOut = true;
							animationPlayer.Play("ImageFadeOut");
						}
					}
					else
					{
						ShowImage();
					}
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

		ShowText();
		ShowImage();
	}

	private async void FadeNoiseDelayed()
	{
		await ToSignal(
			GetTree().CreateTimer(1.5f),
			SceneTreeTimer.SignalName.Timeout
		);

		FadeOutNoise();
	}

	private async void FadeOutNoise()
	{
		noiseEnabled = false;

		var tween = CreateTween();

		tween.TweenProperty(
			textSfx,
			"volume_db",
			-40,
			1.0
		);

		await ToSignal(
			tween,
			Tween.SignalName.Finished
		);

		textSfx.Stop();
		textSfx.VolumeDb = 0;
	}

	private void ShowText()
	{
		IntroSlide slide =
			introData.slides[currentSlide];

		if (slide.fadeMusic)
		{
			FadeOutMusic();
		}

		if (noiseEnabled && !textSfx.Playing)
		{
			textSfx.Play();
		}

		if (slide.fadeNoise)
		{
			FadeNoiseDelayed();
		}

		if (slide.sfx == "man-impact")
		{
			manImpactSfx.Play();
		}

		if (slide.stopNoise)
		{
			FadeOutNoise();
		}

		introText.Modulate = Colors.White;

		introText.Text = slide.text;

		introText.VisibleCharacters = 0;

		typingTimer = 0f;

		isTyping = true;

		if (
			currentSlide == 9 &&
			!introMusic.Playing
		)
		{
			introMusic.Play();
		}
	}

	private async void ContinueAfterTextFade()
	{
		await ToSignal(
			GetTree().CreateTimer(1.0f),
			SceneTreeTimer.SignalName.Timeout
		);

		currentSlide++;

		ShowText();
		ShowImage();
	}

	private void ShowImage()
	{
		IntroSlide slide =
			introData.slides[currentSlide];

		if (string.IsNullOrEmpty(slide.image))
		{
			introImage.Visible = false;
			currentImagePath = "";
			return;
		}

		if (slide.image == currentImagePath)
		{
			return;
		}

		currentImagePath = slide.image;

		introImage.Texture =
			ResourceLoader.Load<Texture2D>(
				"res://Assets/Intro/" + slide.image
			);

		introImage.Visible = true;

		animationPlayer.Play("ImageShow");
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == "TextFadeOut")
		{
			ContinueAfterTextFade();
			return;
		}

		if (
			animationName == "ImageFadeOut" &&
			waitingForFadeOut
		)
		{
			waitingForFadeOut = false;

			ShowImage();
		}
	}

	private async void FadeOutMusic()
	{
		var tween = CreateTween();

		tween.TweenProperty(
			introMusic,
			"volume_db",
			-40,
			2.0
		);

		await ToSignal(
			tween,
			Tween.SignalName.Finished
		);

		introMusic.Stop();
		introMusic.VolumeDb = 0;
	}

	private void ShowTitle()
	{
		showingTitle = true;

		titleLogo.Visible = true;
		beeSfx.Play();

		introText.Visible = false;

		skipButton.Visible = false;

		titleTimer.Start();
	}

	private async void OnTitleTimeout()
	{
		titleLogo.Visible = false;

		beeSfx.Stop();
		beeEchoSfx.Play();

		fadeRect.Visible = true;

		await ToSignal(
			GetTree().CreateTimer(10f),
			SceneTreeTimer.SignalName.Timeout
		);

		GetTree().ChangeSceneToFile(
			"res://Scenes/Gameplay/Main.tscn"
		);
	}
}
