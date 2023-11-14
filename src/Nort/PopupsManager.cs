using Godot;
using Nort.Popups;

namespace Nort;

public partial class PopupsManager : Node
{
    public static PopupsManager Instance { get; private set; }

    private PackedScene dialogPopupScene = GD.Load<PackedScene>("res://Scenes/Popups/DialogPopup.tscn");

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
        popup.Cancelable = true;
        return popup;
    }

    public DialogPopup Error(string message, string title = "Error")
    {
        DialogPopup popup = dialogPopupScene.Instantiate<DialogPopup>();
        AddChild(popup);
        popup.Title = title;
        popup.Message = message;
        popup.Cancelable = true;
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
        popup.Cancelable = true;
        popup.SetWarn();
        return popup;
    }

    // public SettingsPopup Settings()
    // {
    //     SettingsPopup popup = settingsPopupScene.Instantiate<SettingsPopup>();
    //     AddChild(popup);
    //     return popup;
    // }

    public T Custom<T>(PackedScene customPopupScene) where T : Node
    {
        T popup = customPopupScene.Instantiate<T>();
        AddChild(popup);
        return popup;
    }
}