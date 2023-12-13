using Godot;
using Nort.Popups;

namespace Nort;

public partial class PopupsManager : Node
{
    public static PopupsManager Instance { get; private set; }

    private PackedScene dialogPopupScene = GD.Load<PackedScene>("res://Scenes/Popups/DialogPopup.tscn");
    private PackedScene textInputPopupScene = GD.Load<PackedScene>("res://Scenes/Popups/TextInputPopup.tscn");

    // private PackedScene settingsPopupScene =
    //     GD.Load<PackedScene>("res://common/interface/SettingsPopup/SettingsPopup.tscn");

    private PopupsManager()
    {
        Instance = this;
    }

    public DialogPopup Info(string message, string title = default)
    {
        DialogPopup popup = dialogPopupScene.Instantiate<DialogPopup>();
        AddChild(popup);
        popup.Title = title;
        popup.Message = message;
        popup.Cancellable = true;
        return popup;
    }

    public DialogPopup Error(string message, string title = "Error")
    {
        DialogPopup popup = dialogPopupScene.Instantiate<DialogPopup>();
        AddChild(popup);
        popup.Title = title;
        popup.Message = message;
        popup.Cancellable = true;
        popup.SetError();
        popup.AddButton("Ok");
        return popup;
    }

    public DialogPopup Warn(string message, string title = "Warning")
    {
        DialogPopup popup = dialogPopupScene.Instantiate<DialogPopup>();
        AddChild(popup);
        popup.Title = title;
        popup.Message = message;
        popup.Cancellable = true;
        popup.SetWarn();
        return popup;
    }

    // public SettingsPopup Settings()
    // {
    //     SettingsPopup popup = settingsPopupScene.Instantiate<SettingsPopup>();
    //     AddChild(popup);
    //     return popup;
    // }

    public TextInputPopup TextInput(string title, string text = default, string placeholderText = default)
    {
        TextInputPopup popup = textInputPopupScene.Instantiate<TextInputPopup>();
        AddChild(popup);
        popup.Title = title;
        popup.Text = text;
        popup.PlaceholderText = placeholderText;
        popup.Cancellable = true;
        return popup;
    }

    public T Custom<T>(PackedScene customPopupScene) where T : Node
    {
        T popup = customPopupScene.Instantiate<T>();
        AddChild(popup);
        return popup;
    }
}