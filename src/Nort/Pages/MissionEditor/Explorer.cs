using Godot;
using System;
using System.Collections.Generic;
using CtrlRaul;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Nort.Entities;

namespace Nort.Pages.MissionEditor;

public partial class Explorer : PanelContainer
{
    [Export] private PackedScene explorerOptionsFieldItemScene;

    [Ready] public Label entityLabel;
    [Ready] public VBoxContainer fieldsList;

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        Clear();
    }

    public void SetEntity(EditorEntity entity)
    {
        Clear();

        Visible = true;
        entityLabel.Text = entity.Name;

        foreach (ExplorerField field in entity.explorerFields)
        {
            Node item = field switch
            {
                ExplorerOptionsField => explorerOptionsFieldItemScene.Instantiate(),
                _ => throw new NotImplementedException(),
            };

            fieldsList.AddChild(item);
            
            ((ExplorerOptionsFieldItem)item).SetField(field as ExplorerOptionsField);
        }
    }

    public void SetEntities(List<EditorEntity> entities)
    {
        /*Clear();

        const string entrySeparator = "---";

        Visible = true;

        Dictionary<string, int> mutualFields = new();

        foreach (EditorEntity entity in entities)
        {
            foreach (ExplorerField field in entity.explorerFields)
            {
                string entry = $"{field.GetScript().ResourcePath}{entrySeparator}{field.Key}";

                if (mutualFields.ContainsKey(entry))
                {
                    mutualFields[entry]++;
                }
                else
                {
                    mutualFields[entry] = 1;
                }
            }
        }

        int entitiesCount = entities.Count;

        foreach (var entry in mutualFields)
        {
            if (mutualFields[entry.Key] < entitiesCount)
            {
                continue;
            }

            string[] parts = entry.Key.Split(entrySeparator);
            string fieldScriptPath = parts[0];
            string fieldKey = parts[1];

            var script = GD.Load<Script>(fieldScriptPath).New();
            Control item = null;

            GD.Print($"{fieldScriptPath}: {script is ExplorerOptionsField}");

            if (script is ExplorerOptionsField)
            {
                item = (Control)explorerOptionsFieldItemScene.Instance();
            }

            GD.Assert(item != null);

            fieldsList.AddChild(item);
            ((ExplorerOptionsFieldItem)item).SetField(script);
        }*/
    }

    public void Clear()
    {
        Visible = false;
        entityLabel.Text = "";
        fieldsList.QueueFreeChildren(); // Assumes you want to free the child nodes
    }
}
