namespace Nort.Pages.MissionEditor;

public class ExplorerField
{
    public EditorEntity Entity { get; }
    public string Key { get; }

    public ExplorerField(EditorEntity entity, string key)
    {
        Entity = entity;
        Key = key;
    }
}