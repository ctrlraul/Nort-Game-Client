using System;
using Godot;
using Nort.Popups;
using Nort.UI;

namespace Nort.Listing;

public partial class PlayerListItem : PanelContainer
{
    public event Action Selected;
    
    private DisplayCraft displayCraft;
    private Label nickLabel;
    private Label scoreLabel;

    private Player player;
    public Player Player
    {
        get => player;
        set => SetPlayer(value);
    }
    
    public override void _Ready()
    {
        displayCraft = GetNode<DisplayCraft>("%DisplayCraft");
        nickLabel = GetNode<Label>("%Nick");
        scoreLabel = GetNode<Label>("%Score");
    }

    private void SetPlayer(Player value)
    {
        player = value;
        nickLabel.Text = player.nick;
        scoreLabel.Text = "0";
        displayCraft.Blueprint = player.blueprint;
        displayCraft.Faction = Assets.Instance.PlayerFaction;
        displayCraft.Scale = Vector2.One * 100 / Assets.Instance.GetBlueprintVisualRect(player.blueprint).Size.Length();
    }

    private void Delete()
    {
        Error error = LocalPlayersManager.Instance.Delete(player.id);

        if (error != Error.Ok)
            PopupsManager.Instance.Error($"Failed to delete local player: {error}");
    }
    

    private void OnDeleteButtonPressed()
    {
        DialogPopup popup = PopupsManager.Instance.Warn(
            "All progress will drift into the void. Confirm?",
            "Delete Save?"
        );

        popup.AddButton("Yes", Delete);
        popup.AddButton("Nevermind");
    }

    private void OnSelectButtonPressed()
    {
        Selected?.Invoke();
    }
}