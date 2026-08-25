using System;
using System.Data;
using Godot;

public class InventoryUIController
{
    private VBoxContainer inventoryItems;
    private Inventory inventory;
    private InventoryLoader inventoryLoader;
    private InventoryData inventoryData;
    private Label speakerLabel;
    private Action<string> showText;

    public void Setup(
        VBoxContainer inventoryItems,
        Inventory inventory,
        InventoryLoader inventoryLoader,
        InventoryData inventoryData,
        Label speakerLabel,
        Action<string> showText
    )
    {
        this.inventoryItems = inventoryItems;
        this.inventory = inventory;
        this.inventoryLoader = inventoryLoader;
        this.inventoryData = inventoryData;
        this.speakerLabel = speakerLabel;
        this.showText = showText;
    }

    public void Update()
    {
        foreach (Node child in inventoryItems.GetChildren())
        {
            child.QueueFree();
        }

        foreach (string itemId in inventory.GetItems())
        {
            InventoryItemData item =
                inventoryLoader.GetById(
                    itemId,
                    inventoryData
                );

            if (item == null)
            {
                continue;
            }

            Button button = new Button();

            FontFile font = ResourceLoader.Load<FontFile>(
                "res://Assets/Fonts/ShareTechMono-Regular.ttf"
            );

            button.AddThemeFontOverride("font", font);

            button.Text = item.name;

            button.Pressed += () =>
            {
                speakerLabel.Text = item.name.ToUpper();
                showText(item.description);
            };

            inventoryItems.AddChild(button);
        }
    }
}
