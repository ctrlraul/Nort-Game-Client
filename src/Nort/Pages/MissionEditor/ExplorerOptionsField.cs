using System;
using System.Collections.Generic;

namespace Nort.Pages.MissionEditor;

public class ExplorerOptionsField : ExplorerField
{
    public IEnumerable<object> Options { get; }
    public IEnumerable<string> OptionNames { get; }

    public ExplorerOptionsField(EditorEntity entity, string key, IEnumerable<object> options, IEnumerable<string> optionNames) : base(entity, key)
    {
        Options = options;
        OptionNames = optionNames;
    }
}