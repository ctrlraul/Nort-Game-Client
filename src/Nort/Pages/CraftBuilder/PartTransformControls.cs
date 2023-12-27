using Godot;
using Nort.UI;
using System;
using CtrlRaul.Godot;

namespace Nort.Pages.CraftBuilder;

public partial class PartTransformControls : Control
{
	public event Action<bool> Flip;
	public event Action<float> Rotate;

	private const int Angles = 16;

	[Ready] public Control partOutlineContainer;
	[Ready] public TextureRect partOutline;
	[Ready] public Control buttonsMargin;
	[Ready] public Control rotateIconCenterer;
	[Ready] public Control rotateIconContainer;
	[Ready] public Line2D line2D;
	[Ready] public TextureRect flipIcon;

	private float rotationStartAngle;
	private float lastMouseWheelSpin;

	public bool Flipped
	{
		get => partOutline.FlipH;
		set
		{
			partOutline.FlipH = value;
			flipIcon.FlipH = value;
		}
	}

	public float Angle
	{
		get => partOutlineContainer.RotationDegrees;
		set
		{
			rotateIconContainer.RotationDegrees = value;
			partOutlineContainer.RotationDegrees = value;
		}
	}


	private DisplayCraftPart part;
	public DisplayCraftPart Part
	{
		get => part;
		set
		{
			if (value != null)
			{
				Texture2D texture = Assets.Instance.GetPartTexture(value.partData.partId);

				partOutline.Texture = texture;
				partOutline.Size = texture.GetSize();

				Angle = value.Angle;
				Flipped = value.Flipped;

				Visible = true;
			}
			else
			{
				partOutline.Texture = null;
				Visible = false;
			}

			part = value;
		}
	}


	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
		SetProcessInput(false);
		line2D.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion)
		{
			Vector2 mouse = GetLocalMousePosition();
			float mouseAngle = mouse.Angle();
			float newRotation = Mathf.RadToDeg(mouseAngle - rotationStartAngle);
			const float snap = 360.0f / Angles;

			Angle = Mathf.Floor(newRotation / snap) * snap;

			if (Part != null)
			{
				Part.Angle = Angle;
			}

			line2D.Rotation = mouseAngle;
			line2D.Scale = line2D.Scale with { X = mouse.Length() };

			Rotate?.Invoke(Angle);
		}
	}

	public void Clear()
	{
		Part = null;
		Visible = false;
	}

	public void UpdateTransform(Control control)
	{
		if (Part == null)
		{
			return;
		}

		Position = control.Position + Part.Position * control.Scale;
		partOutlineContainer.Scale = control.Scale;
		buttonsMargin.Position = buttonsMargin.Position with
		{
			Y = partOutline.Texture.GetHeight() * 0.5f * control.Scale.Y + 20
		};
	}

	private void OnRotateButtonButtonDown()
	{
		Vector2 mouse = GetLocalMousePosition();
		float mouseAngle = mouse.Angle();

		rotationStartAngle = mouseAngle - Angle;

		line2D.Visible = true;
		line2D.Rotation = mouseAngle;
		line2D.Scale = line2D.Scale with { X = mouse.Length() };

		SetProcessInput(true);
	}

	private void OnRotateButtonButtonUp()
	{
		SetProcessInput(false);
		line2D.Visible = false;
	}

	private void OnFlipButtonPressed()
	{
		Flipped = !Flipped;
		Part.Flipped = Flipped;
		Flip?.Invoke(Flipped);
	}

	private void OnRotateButtonGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex != MouseButton.WheelUp &&
				mouseButtonEvent.ButtonIndex != MouseButton.WheelDown)
			{
				return;
			}

			// Time stuff to mitigate some mouses sending multiple scroll events at once
			ulong now = Time.GetTicksMsec();

			if (now - lastMouseWheelSpin < 10)
			{
				return;
			}

			int delta = mouseButtonEvent.ButtonIndex == MouseButton.WheelUp ? 1 : -1;

			Angle += 360.0f / Angles * delta;

			Part.Angle = Angle;
			lastMouseWheelSpin = now;
			
			Rotate?.Invoke(Angle);
		}
	}
}


