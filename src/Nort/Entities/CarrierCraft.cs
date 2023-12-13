using System.Collections.Generic;
using System.Linq;
using Nort.Hud;
using Nort.Pages;

namespace Nort.Entities;

public partial class CarrierCraft : Craft
{
    #region EntityInspector compatibility

    [Savable]
    [Inspect(nameof(BlueprintIdOptions))]
    public string BlueprintId
    {
        get => Blueprint.id;
        set => Blueprint = Assets.Instance.GetBlueprint(value);
    }

    public IEnumerable<string> BlueprintIdOptions => Config.CarrierBlueprints;

    #endregion
    

    public CarrierCraft() : base()
    {
        blueprint = Assets.Instance.GetBlueprint(Config.CarrierBlueprints.First());
    }
}