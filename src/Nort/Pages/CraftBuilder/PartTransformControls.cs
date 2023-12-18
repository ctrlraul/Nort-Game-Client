using Godot;
using Nort.UI;
using System;

namespace Nort.Pages.CraftBuilder;

public partial class PartTransformControls : Control
{
	public event Action<bool> Flip;
	public event Action<float> Rotate;

	private const int Angles = 16;

	private Control outlineSpriteContainer;
	private TextureRect outlineSprite;
	private Control buttonsMargin;
	private Control rotateIconCenterer;
	private Control rotateIconContainer;
	private Line2D line;
	private TextureRect flipIcon;

	private float rotationStartAngle;
	private float lastMouseWheelSpin;

	private float currentAngle;

	public bool Flipped
	{
		get => outlineSprite.FlipH;
		set
		{
			outlineSprite.FlipH = value;
			flipIcon.FlipH = value;
		}
	}

	public float Angle
	{
		get => currentAngle;
		set
		{
			currentAngle = value;
			rotateIconContainer.RotationDegrees = value;
			outlineSpriteContainer.RotationDegrees = value;
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
				Vector2 textureSize = texture.GetSize();

				outlineSprite.Texture = texture;
				outlineSprite.Size = textureSize;
				outlineSprite.PivotOffset = textureSize * 0.5f;
				outlineSprite.Position = -textureSize * 0.5f;

				Angle = value.Angle;
				Flipped = value.Flipped;

				Visible = true;
			}
			else
			{
				outlineSprite.Texture = null;
				Visible = false;
			}

			part = value;
		}
	}


	public override void _Ready()
	{
		base._Ready();

		outlineSpriteContainer = GetNode<Control>("%OutlineSpriteContainer");
		outlineSprite = GetNode<TextureRect>("%OutlineSprite");
		buttonsMargin = GetNode<Control>("%ButtonsMargin");
		rotateIconCenterer = GetNode<Control>("%RotateIconCenterer");
		rotateIconContainer = GetNode<Control>("%RotateIconContainer");
		line = GetNode<Line2D>("%Line2D");
		flipIcon = GetNode<TextureRect>("%FlipIcon");

		SetProcessInput(false);
		line.Visible = false;
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

			line.Rotation = mouseAngle;
			line.Scale = line.Scale with { X = mouse.Length() };

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
		outlineSpriteContainer.Scale = control.Scale;
		buttonsMargin.Position = buttonsMargin.Position with
		{
			Y = outlineSprite.Texture.GetHeight() * 0.5f * control.Scale.Y + 20
		};
	}

	private void OnRotateButtonButtonDown()
	{
		Vector2 mouse = GetLocalMousePosition();
		float mouseAngle = mouse.Angle();

		rotationStartAngle = mouseAngle - currentAngle;

		line.Visible = true;
		line.Rotation = mouseAngle;
		line.Scale = line.Scale with { X = mouse.Length() };

		SetProcessInput(true);
	}

	private void OnRotateButtonButtonUp()
	{
		SetProcessInput(false);
		line.Visible = false;
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

			currentAngle += Mathf.DegToRad(360.0f / Angles * delta);

			Part.Angle = currentAngle;
			lastMouseWheelSpin = now;
		}
	}
}


