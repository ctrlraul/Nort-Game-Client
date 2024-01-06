using Godot;

namespace Nort.UI;


public partial class DisplayPart : Control
{
	private TextureRect textureRect;
	
	private TextureRect coreLightTextureRect;
	private TextureRect skillTextureRect;
	private TextureRect outlineTextureRect;
	
	private bool coreLight;
	private Faction faction;
	
	public bool Shiny
	{
		get => ((ShaderMaterial)textureRect.Material).GetShaderParameter("shiny").AsBool();
		set => ((ShaderMaterial)textureRect.Material).SetShaderParameter("shiny", value);
	}

	public Faction Faction
	{
		get => faction;
		set
		{
			faction = value;
			textureRect.SelfModulate = Assets.Instance.GetFactionColor(value, Shiny);
		}
	}
	
	public bool Flipped
	{
		get => textureRect.FlipH;
		set => textureRect.FlipH = value;
	}
	
	public float Angle
	{
		get => RotationDegrees;
		set
		{
			RotationDegrees = value;
			if (skillTextureRect != null)
				skillTextureRect.RotationDegrees = -value;
		}
	}

	private Skill skill;
	public Skill Skill
	{
		get => skill;
		set
		{
			skill = value;
			UpdateSkill();
		}
	}

	private Part part;
	public Part Part
	{
		get => part;
		set
		{
			part = value;
			coreLight = Assets.IsCore(part);
		
			textureRect.Texture = Assets.Instance.GetPartTexture(part);

			Vector2 textureSize = textureRect.Texture.GetSize();

			textureRect.Size = textureSize;
			textureRect.Position = textureSize * -0.5f;

			UpdateCoreLight();
			UpdateOutline();
		}
	}
	
	private bool outline;
	public bool Outline
	{
		get => outline;
		set
		{
			outline = value;
			UpdateOutline();
		}
	}

	public PartData PartData
	{
		get
		{
			return new PartData
			{
				shiny = Shiny,
				Skill = Skill,
				Part = Part,
			};
		}
		set
		{
			Shiny = value.shiny;
			Skill = value.Skill;
			Part = value.Part;
		}
	}

	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("%TextureRect");
	}

	private void UpdateCoreLight()
	{
		if (coreLight)
		{
			if (coreLightTextureRect != null)
				return;
			
			coreLightTextureRect = new TextureRect();
			coreLightTextureRect.Texture = Assets.CoreLightTexture;
			coreLightTextureRect.Position = Assets.CoreLightTexture.GetSize() * -0.5f;
			coreLightTextureRect.MouseFilter = MouseFilterEnum.Ignore;
			
			AddChild(coreLightTextureRect);
		}
		else if (coreLightTextureRect != null)
		{
			coreLightTextureRect.QueueFree();
			coreLightTextureRect = null;
		}
	}

	private void UpdateOutline()
	{
		if (outline)
		{
			if (outlineTextureRect == null)
			{
				outlineTextureRect = new TextureRect();
				outlineTextureRect.RotationDegrees = Angle;
				outlineTextureRect.MouseFilter = MouseFilterEnum.Ignore;
				outlineTextureRect.Material = Assets.PartOutlineMaterial;
				AddChild(outlineTextureRect);
			}

			outlineTextureRect.Texture = textureRect.Texture;
			outlineTextureRect.Position = textureRect.Position;
			outlineTextureRect.Size = textureRect.Size;
		}
		else if (outlineTextureRect != null)
		{
			outlineTextureRect.QueueFree();
			outlineTextureRect = null;
		}
	}

	private void UpdateSkill()
	{
		if (skill != null)
		{
			if (skillTextureRect == null)
			{
				skillTextureRect = new TextureRect();
				skillTextureRect.RotationDegrees = -Angle;
				skillTextureRect.MouseFilter = MouseFilterEnum.Ignore;
				skillTextureRect.ZIndex = 1;
				AddChild(skillTextureRect);
			}

			Texture2D texture = Assets.Instance.GetSkillTexture(skill.id);
			Vector2 halfSize = texture.GetSize() * 0.5f;

			skillTextureRect.Texture = texture;
			skillTextureRect.Position = -halfSize;
			skillTextureRect.Size = halfSize;
			skillTextureRect.PivotOffset = halfSize;
		}
		else if (skillTextureRect != null)
		{
			skillTextureRect.QueueFree();
			skillTextureRect = null;
		}
	}
}