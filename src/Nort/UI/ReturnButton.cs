using Godot;

namespace Nort.UI;

public partial class ReturnButton : TextureButton
{
    private void OnPressed()
    {
        if (GetSignalConnectionList(BaseButton.SignalName.Pressed).Count > 1)
        {
            return;
        }
        
        PagesNavigator.Instance.GoBack();
    }
}