using Godot;
using System;
using CtrlRaul.Godot;

namespace Nort.Entities;

public enum InspectHint
{
	Options,
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InspectAttribute : Attribute
{
	public InspectAttribute() { }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InspectOptionsAttribute : Attribute
{
	public string OptionsMethodName { get; }

	public InspectOptionsAttribute(string optionsMethodName)
	{
		OptionsMethodName = optionsMethodName;
	}
}

public partial class EditorCarrierCraft : Craft
{
	[InspectOptions(nameof(GetBlueprints))] public string blueprint;
	[InspectOptions(nameof(GetFactions))] public string faction;

	public void GetBlueprints()
	{
		// ...
	}

	public void GetFactions()
	{
		// ...
	}
}
