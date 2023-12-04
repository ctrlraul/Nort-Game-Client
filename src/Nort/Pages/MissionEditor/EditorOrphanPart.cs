using Nort.Entities;

namespace Nort.Pages.MissionEditor;

public partial class EditorOrphanPart : EditorEntity, IEditorEntity<OrphanPartSetup>
{
    public OrphanPartSetup Setup { get; set; }
}