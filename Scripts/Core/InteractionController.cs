using System.Threading;
using Godot;

public class InteractionController
{
    private Button choiceButton1;
    private Button choiceButton2;
    private InspectableLoader inspectableLoader;
    private InspectablesData inspectablesData;
    private DialogueLoader dialogueLoader;
    private DialogueController dialogueController;
    private GameState gameState;
    private Inventory inventory;
    private Label speakerLabel;
    private AudioStreamPlayer normalAnnouncement;
    private System.Action<string> showText;
    private System.Action showDialogueLine;
    private InspectableData currentInspectable;
    private int currentInspectablePage = 0;
    private bool isReadingInspectable = false;

    public void Setup(
        Button choiceButton1,
        Button choiceButton2,
        InspectableLoader inspectableLoader,
        InspectablesData inspectablesData,
        DialogueLoader dialogueLoader,
        DialogueController dialogueController,
        GameState gameState,
        Inventory inventory,
        Label speakerLabel,
        AudioStreamPlayer normalAnnouncement,
        System.Action<string> showText,
        System.Action showDialogueLine
    )
    {
        this.choiceButton1 = choiceButton1;
        this.choiceButton2 = choiceButton2;
        this.inspectableLoader = inspectableLoader;
        this.inspectablesData = inspectablesData;
        this.dialogueLoader = dialogueLoader;
        this.dialogueController = dialogueController;
        this.gameState = gameState;
        this.inventory = inventory;
        this.speakerLabel = speakerLabel;
        this.normalAnnouncement = normalAnnouncement;
        this.showText = showText;
        this.showDialogueLine = showDialogueLine;
    }

    public void Inspect(int index, LocationData location)
    {
        string id = location.inspectables[index];

        currentInspectable = inspectableLoader.GetById(
            id,
            inspectablesData
        );

        if (id == "elevator_access")
        {
            if (
                !inventory.HasItem("keycard") ||
                !inventory.HasItem("protective_suit") ||
                !inventory.HasItem("oxygen_bottle")
            )
            {
                speakerLabel.Text = "SYSTEM";
                showText("ACCESS DENIED. SAFETY GEAR MISSING.");
                return;
            }
        }

        if (
            id == "terminal" &&
            gameState.HasEvent("alarm_triggered")
        )
        {
            currentInspectable =
                inspectableLoader.GetById(
                    "terminal_alarm",
                    inspectablesData
                );

            speakerLabel.Text =
                currentInspectable.name.ToUpper();

            showText(currentInspectable.text[0]);

            return;
        }


        currentInspectablePage = 0;

        speakerLabel.Text =
            currentInspectable.name.ToUpper();

        if (
            !string.IsNullOrEmpty(
                currentInspectable.dialogue
            ) &&
            !(
                currentInspectable.id == "log" &&
                gameState.HasEvent("greenhouse_log_read")
            )
        )
        {
            isReadingInspectable = false;

            DialogueData dialogueData =
                dialogueLoader.Load(
                    "res://Dialogue/Chapters/" +
                    currentInspectable.dialogue
                );

            dialogueController.SetDialogue(
                dialogueData.lines
            );

            showDialogueLine();
        }
        else
        {
            isReadingInspectable = true;

            showText(
                currentInspectable.text[

                currentInspectablePage
                ]
            );
        }
    }

    public void ShowChoices()
    {
        if (currentInspectable == null)
        {
            return;
        }

        if (currentInspectable.id == "elevator_access")
        {
            choiceButton1.Text = "Go to the surface";
            choiceButton2.Text = "Stay in the bunker";

            choiceButton1.Visible = true;
            choiceButton2.Visible = true;

            return;
        }

        if (
            currentInspectable.itemId == null ||
            !isReadingInspectable ||
            currentInspectablePage !=
                currentInspectable.text.Length - 1
        )
        {
            return;
        }

        if (currentInspectable.id == "apple")
        {
            choiceButton1.Text = "Take";
            choiceButton2.Text = "Eat";
        }
        else if (
            currentInspectable.itemId == "protective_suit" ||
            currentInspectable.itemId == "oxygen_bottle"
        )
        {
            choiceButton1.Text = "Equip";
            choiceButton2.Text = "Leave";
        }
        else
        {
            choiceButton1.Text = "Take";
            choiceButton2.Text = "Leave";
        }

        choiceButton1.Visible = true;
        choiceButton2.Visible = true;
    }

    public void NextPage()
    {
        if (
            currentInspectablePage <
            currentInspectable.text.Length - 1
        )
        {
            currentInspectablePage++;

            showText(
                currentInspectable.text[
                    currentInspectablePage
                ]
            );
        }
        else
        {
            isReadingInspectable = false;
        }
    }

    public bool IsReadingInspectable()
    {
        return isReadingInspectable;
    }

    public InspectableData GetCurrentInspectable()
    {
        return currentInspectable;
    }

    public void ClearCurrentInspectable()
    {
        currentInspectable = null;
        isReadingInspectable = false;
        currentInspectablePage = 0;
    }

    public void SetCurrentInspectable(InspectableData inspectable)
    {
        currentInspectable = inspectable;
    }
}
