using Godot;
using System;

public partial class Diary : Control
{
	private TextureRect diaryImage;
	private RichTextLabel diaryText;
	private AnimationPlayer animationPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		diaryImage = GetNode<TextureRect>("DiaryImage");
		diaryText = GetNode<RichTextLabel>("DiaryText");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
