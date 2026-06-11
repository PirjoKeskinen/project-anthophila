using Godot;

public partial class Main : Control
{

private DialogueLine[] dialogue =
{
	new DialogueLine
	{
		text = "The greenhouse systems are failing."
	},

	new DialogueLine
	{
		text = "We need replacement components."
	},

	new DialogueLine
	{
		text = "You have been chosen for the mission."
	}
};

	private int currentLine = 0;

	private RichTextLabel dialogueLabel;

	private float typingSpeed = 30f;
	private float typingTimer = 0f;

	private bool isTyping = false;

	public override void _Ready()
	{
		dialogueLabel = GetNode<RichTextLabel>(
            "DialoguePanel/RichTextLabel"
		);

		ShowDialogueLine();
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
		dialogueLabel.Text = dialogue[currentLine].text;
		dialogueLabel.VisibleCharacters = 0;

		typingTimer = 0f;
		isTyping = true;
	}
}
