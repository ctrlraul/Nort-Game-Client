using System;
using System.Collections.Generic;
using CtrlRaul.Godot;
using Godot;

namespace Nort.Hud;


public partial class EntityInspectorOptionsField : Control, EntityInspector.IField
{
    #region IEntityInspectorField implementation
    
    public event Action<object> ValueChanged;

    public void SetValue(object value)
    {
        optionButton.Selected = (int)value;
    }

    public void SetLabel(string value)
    {
        label.Text = value;
    }

    public void Disable()
    {
        optionButton.Disabled = true;
        Modulate = Modulate with { A = Modulate.A * 0.5f };
    }

    #endregion
    
    
    [Ready] public Label label;
    [Ready] public OptionButton optionButton;

    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
    }
    
    public void SetOptions(IEnumerable<string> value)
    {
        optionButton.Clear();
        
        foreach (string option in value)
            optionButton.AddItem(option.Capitalize());
    }

    private void OnOptionButtonItemSelected(ulong index)
    {
        ValueChanged?.Invoke((int)index);
    }
}