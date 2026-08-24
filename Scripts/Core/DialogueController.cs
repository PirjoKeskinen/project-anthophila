using Godot;

public class DialogueController
{
	private RichTextLabel dialogueLabel;
	private Label speakerLabel;

	private DialogueLine[] dialogue;
	private int currentLine = 0;

	private float typingSpeed = 30f;
	private float typingTimer = 0f;

	private bool isTyping = false;

	public void Setup(
		RichTextLabel dialogueLabel,
		Label speakerLabel
	)
	{
		this.dialogueLabel = dialogueLabel;
		this.speakerLabel = speakerLabel;
	}

	public void SetDialogue(DialogueLine[] lines)
	{
		dialogue = lines;
		currentLine = 0;
	}

	public void ShowText(string text)
	{
		dialogueLabel.Text = text;
		dialogueLabel.VisibleCharacters = 0;

		typingTimer = 0f;
		isTyping = true;
	}

	public void ShowCurrentLine()
	{
		if (dialogue == null || dialogue.Length == 0)
		{
			GD.PushError("DialogueController: No dialogue has been set.");
			return;
		}

		if (currentLine < 0 || currentLine >= dialogue.Length)
		{
			GD.PushError(
				$"DialogueController: currentLine {currentLine} is out of range."
			);
			return;
		}

		speakerLabel.Text = dialogue[currentLine].speaker;

		ShowText(dialogue[currentLine].text);
	}

	public bool IsTyping()
	{
		return isTyping;
	}

	public void FinishTyping()
	{
		dialogueLabel.VisibleCharacters =
			dialogueLabel.Text.Length;

		isTyping = false;
	}

	public void ProcessTyping(double delta)
	{
		if (!isTyping)
			return;

		typingTimer += (float)delta;

		if (typingTimer >= 1f / typingSpeed)
		{
			typingTimer = 0f;

			dialogueLabel.VisibleCharacters++;

			if (
				dialogueLabel.VisibleCharacters >=
				dialogueLabel.Text.Length
			)
			{
				isTyping = false;
			}
		}
	}

	public bool HasNextLine()
	{
		return currentLine < dialogue.Length - 1;
	}

	public void NextLine()
	{
		currentLine++;
		ShowCurrentLine();
	}

	public bool IsLastLine()
	{
		return currentLine == dialogue.Length - 1;
	}
}
