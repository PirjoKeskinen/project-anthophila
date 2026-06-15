using Godot;
using System;

public partial class Intro : Control
{

	private RichTextLabel introText;

	private float typingSpeed = 30f;
	private float typingTimer = 0f;

	private bool isTyping = true;

	private Button skipButton;
	public override void _Ready()
	{
		skipButton = GetNode<Button>(
			"SkipButton"
		);

		skipButton.Pressed += OnSkipPressed;

		introText = GetNode<RichTextLabel>(
			"RichTextLabel"
		);

		introText.Text =
			"The last bee disappeared long ago.";

		introText.VisibleCharacters = 0;
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
		GetTree().ChangeSceneToFile(
			"res://Scenes/Gameplay/Main.tscn"
		);
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
				GetTree().ChangeSceneToFile(
					"res://Scenes/Gameplay/Main.tscn"
				);
			}
		}
	}

}
