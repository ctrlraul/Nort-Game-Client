using System;
using CtrlRaul.Godot;
using Godot;

namespace Nort.Pages;

public partial class EntityInspectorBooleanField : Control, EntityInspector.IField
{
    #region IEntityInspectorField implementation
    
    public event Action<object> ValueChanged;

    public void SetValue(object value)
    {
        checkBox.ButtonPressed = (bool)value;
    }

    public void SetLabel(string value)
    {
        label.Text = value;
    }

    public void Disable()
    {
        checkBox.Disabled = true;
        Modulate = Modulate with { A = Modulate.A * 0.5f };
    }

    #endregion
    
    
    [Ready] public Label label;
    [Ready] public CheckBox checkBox;

    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
    }


    private void OnCheckBoxToggled(bool value)
    {
        ValueChanged?.Invoke(value);
    }
}