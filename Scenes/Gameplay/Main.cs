using Godot;

public partial class Main : Control
{
	private string[] dialogue =
	{
		"The greenhouse systems are failing.",
		"We need replacement components.",
        "You have been chosen for the mission."
	};

	private int currentLine = 0;

	private RichTextLabel dialogueLabel;

	public override void _Ready()
	{
		dialogueLabel = GetNode<RichTextLabel>(
            "DialoguePanel/RichTextLabel"
		);

		dialogueLabel.Text = dialogue[currentLine];
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			if (currentLine < dialogue.Length - 1)
			{
				currentLine++;
				dialogueLabel.Text = dialogue[currentLine];
			}
		}
	}
}
