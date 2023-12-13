using System;
using CtrlRaul.Godot;
using Godot;

namespace Nort.Popups;

public partial class TextInputPopup : GenericPopup
{
    public event Action<string> Submitted;

    [Ready] public Label titleLabel;
    [Ready] public LineEdit lineEdit;

    
    public string Text
    {
        get => lineEdit.Text;
        set => lineEdit.Text = value;
    }

    public string Title
    {
        get => titleLabel.Text;
        set => titleLabel.Text = value;
    }

    public string PlaceholderText
    {
        get => lineEdit.PlaceholderText;
        set => lineEdit.PlaceholderText = value;
    }

    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        Cancellable = true;
        lineEdit.Text = string.Empty;
        lineEdit.PlaceholderText = string.Empty;
        lineEdit.GrabFocus();
    }
    

    private void OnLineEditTextSubmitted(string text)
    {
        Submitted?.Invoke(text);
        Remove();
    }

    private void OnSubmitButtonPressed()
    {
        Submitted?.Invoke(lineEdit.Text);
        Remove();
    }
}