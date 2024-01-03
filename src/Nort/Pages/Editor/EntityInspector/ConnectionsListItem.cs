using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Nort;
using Nort.Entities;

public partial class ConnectionsListItem : Control
{
	[Ready] public OptionButton eventOptions;
	[Ready] public OptionButton methodOptions;
	[Ready] public Label targetLabel;

	private Entity entity;
	private EntityConnection connection;
	private List<string> connectableEvents;
	private List<string> connectableMethods;
	
	
	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
	}


	public void SetEntity(Entity value)
	{
		entity = value;
	}

	public void SetConnection(EntityConnection value)
	{
		connection = value;
		
		eventOptions.Clear();
		methodOptions.Clear();
		
		Node target = (
			connection.targetUuid == null
			? Stage.Instance
			: Stage.Instance.GetEntityByUuid(connection.targetUuid)
		);

		Type type = entity.GetType();

		connectableEvents = ConnectableAttribute.GetConnectableEvents(type).Select(info => info.Name).ToList();
		connectableMethods = ConnectableAttribute.GetConnectableMethods(type).Select(info => info.Name).ToList();

		foreach (string eventName in connectableEvents)
			eventOptions.AddItem(eventName.Capitalize());
		
		foreach (string methodName in connectableMethods)
			methodOptions.AddItem(methodName.Capitalize());

		eventOptions.Selected = connectableEvents.IndexOf(connection.eventName);
		methodOptions.Selected = connectableMethods.IndexOf(connection.methodName);

		if (target is Stage)
		{
			targetLabel.Visible = false;
		}
		else
		{
			targetLabel.Text = target.Name;
		}
	}


	private void OnEventOptionsItemSelected(uint index)
	{
		connection.eventName = connectableEvents[(int)index];
	}

	private void OnMethodOptionsItemSelected(uint index)
	{
		connection.methodName = connectableMethods[(int)index];
	}

	private void OnDeleteButtonPressed()
	{
		entity.Connections.Remove(connection);
		QueueFree();
	}
}
