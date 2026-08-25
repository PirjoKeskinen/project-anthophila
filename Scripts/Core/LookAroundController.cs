using Godot;

public class LookAroundController
{
    private Button[] inspectButtons;
    private LocationLoader locationLoader;
    private LocationsData locationsData;
    private InspectableLoader inspectableLoader;
    private InspectablesData inspectablesData;
    private Label menuTitle;
    private Button[] exitButtons;
    private Button moveButton;
    private Button lookAroundButton;
    private Button inventoryButton;
    private Button backButton;

    public void Setup(
        Button[] inspectButtons,
        LocationLoader locationLoader,
        LocationsData locationsData,
        InspectableLoader inspectableLoader,
        InspectablesData inspectablesData,
        Label menuTitle,
        Button[] exitButtons,
        Button moveButton,
        Button lookAroundButton,
        Button inventoryButton,
        Button backButton
    )
    {
        this.inspectButtons = inspectButtons;
        this.locationLoader = locationLoader;
        this.locationsData = locationsData;
        this.inspectableLoader = inspectableLoader;
        this.inspectablesData = inspectablesData;
        this.menuTitle = menuTitle;
        this.exitButtons = exitButtons;
        this.moveButton = moveButton;
        this.lookAroundButton = lookAroundButton;
        this.inventoryButton = inventoryButton;
        this.backButton = backButton;
    }

    public void Show(string currentLocation)
    {
        LocationData location =
            locationLoader.GetById(
                currentLocation,
                locationsData
            );

        menuTitle.Text = "LOOK AROUND";

        moveButton.Visible = false;
        lookAroundButton.Visible = false;
        inventoryButton.Visible = false;

        foreach (Button button in exitButtons)
        {
            button.Visible = false;
        }

        for (int i = 0; i < inspectButtons.Length; i++)
        {
            if (i < location.inspectables.Length)
            {
                InspectableData inspectable =
                    inspectableLoader.GetById(
                        location.inspectables[i],
                        inspectablesData
                    );

                inspectButtons[i].Text = inspectable.name;
                inspectButtons[i].Visible = true;
            }
            else
            {
                inspectButtons[i].Visible = false;
            }
        }

        backButton.Visible = true;
    }
}
