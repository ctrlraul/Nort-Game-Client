using System;
using CtrlRaul.Godot.Linq;
using Godot;

namespace Nort.Popups;

public partial class DialogPopup : GenericPopup
{
    private const float ButtonWidth = 100;

    public Label titleLabel;
    public TextureRect spinnerTexture;
    public PanelContainer messageContainer;
    public RichTextLabel messageLabel;
    public HBoxContainer buttonsContainer;


    public string Title
    {
        set
        {
            if (!IsInsideTree())
                throw new Exception("Add to tree first");
            titleLabel.Text = value;
            titleLabel.Visible = value != string.Empty;
        }
    }


    public string Message
    {
        set
        {
            if (!IsInsideTree())
                throw new Exception("Add to tree first");
            messageLabel.Text = value;
            messageContainer.Visible = value != string.Empty;
        }
    }


    public bool Spinner
    {
        set
        {
            if (!IsInsideTree())
                throw new Exception("Add to tree first");
            spinnerTexture.Visible = value;
        }
    }


    public override void _Ready()
    {
        base._Ready();

        titleLabel = GetNode<Label>("%TitleLabel");
        // spinnerTexture = GetNode<TextureRect>("%Spinner");
        messageContainer = GetNode<PanelContainer>("%MessageContainer");
        messageLabel = GetNode<RichTextLabel>("%MessageLabel");
        buttonsContainer = GetNode<HBoxContainer>("%ButtonsContainer");

        titleLabel.Visible = false;
        messageContainer.Visible = false;
        buttonsContainer.Visible = false;

        buttonsContainer.QueueFreeChildren();
    }

    public void AddButton(string text, Action OnPressed = null)
    {
        Button button = new();

        if (buttonsContainer.GetChildCount() > 0)
        {
            buttonsContainer.AddSpacer(false);
        }

        buttonsContainer.Visible = true;
        buttonsContainer.AddChild(button);

        button.Text = text;
        button.CustomMinimumSize = button.CustomMinimumSize with { X = ButtonWidth };
        
        if (OnPressed == null)
        {
            button.Pressed += Remove;
        }
        else
        {
            button.Pressed += () =>
            {
                OnPressed.Invoke();
                Remove();
            };
        }
    }
}