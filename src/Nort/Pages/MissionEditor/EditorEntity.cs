using System;
using System.Collections.Generic;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities;

namespace Nort.Pages.MissionEditor;

public partial class EditorEntity : Control
{
    public event Action Pressed;
    public event Action DragStart;
    public event Action DragStop;

    [Ready] public Button hitBox;
    [Ready] public Panel selectionIndicator;

    public List<ExplorerField> explorerFields;

    private bool selected;
    public bool Selected
    {
        get => selected;
        set
        {
            selectionIndicator.Visible = value;
            selected = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        Selected = false;
        explorerFields = InitExplorerFields();
    }

    protected virtual List<ExplorerField> InitExplorerFields()
    {
        return new List<ExplorerField>();
    }

    private void OnHitBoxPressed()
    {
        Pressed?.Invoke();
    }

    private void OnHitBoxButtonDown()
    {
        DragStart?.Invoke();
    }

    private void OnHitBoxButtonUp()
    {
        DragStop?.Invoke();
    }
}
