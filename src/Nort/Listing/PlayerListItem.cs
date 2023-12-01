using System;
using Godot;
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
        displayCraft.Color = Assets.Instance.PlayerFaction.Color;
        displayCraft.Scale = Vector2.One * Mathf.Clamp(1f / (player.parts.Count * 0.225f) * 0.5f, 0.25f, 0.5f);
    }

    private void OnDeleteButtonPressed()
    {
        Error error = LocalPlayersManager.Instance.Delete(player.id);
        if (error != Error.Ok)
            PopupsManager.Instance.Error($"Failed to delete local player: {error}");
    }

    private void OnSelectButtonPressed()
    {
        Selected?.Invoke();
    }
}