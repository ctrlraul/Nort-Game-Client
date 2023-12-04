using System.Linq;
using System.Reflection;
using CtrlRaul.Godot;
using Godot;

namespace Nort.Pages.MissionEditor;

public partial class ExplorerOptionsFieldItem : Control
{
    [Ready] public Label label;
    [Ready] public OptionButton optionButton;

    private ExplorerOptionsField field;
    private PropertyInfo propertyInfo;

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        optionButton.Clear();
    }

    public void SetField(ExplorerOptionsField value)
    {
        field = value;
        propertyInfo = field.Entity.GetType().GetProperty(field.Key);
        label.Text = field.Key.Capitalize();

        foreach (string optionName in field.OptionNames)
            optionButton.AddItem(optionName);

        optionButton.Selected = 0;
    }

    private void OnOptionButtonItemSelected(ulong index)
    {
        propertyInfo.SetValue(field.Entity, field.Options.ElementAt((int)index));
    }
}