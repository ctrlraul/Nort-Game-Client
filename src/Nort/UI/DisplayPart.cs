using Godot;

namespace Nort.UI;


public partial class DisplayPart : Control
{
	private TextureRect textureRect;
	
	private TextureRect coreLightTextureRect;
	private TextureRect gimmickTextureRect;
	private TextureRect outlineTextureRect;
	
	private bool _coreLight;
	
	public bool Shiny
	{
		get => textureRect.Material == Assets.SHINY_MATERIAL;
		set => textureRect.Material = value ? Assets.SHINY_MATERIAL : null;
	}

	public Color Color
	{
		get => textureRect.SelfModulate;
		set => textureRect.SelfModulate = value;
	}
	
	public bool Flipped
	{
		get => textureRect.FlipH;
		set => textureRect.FlipH = value;
	}
	
	public float Angle
	{
		get => Rotation;
		set
		{
			Rotation = value;
			if (gimmickTextureRect != null)
				gimmickTextureRect.Rotation = -value;
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

	private Part _part;
	public Part Part
	{
		get => _part;
		set => SetPart(value);
	}
	
	private bool _outline;
	public bool Outline
	{
		get => _outline;
		set
		{
			_outline = value;
			UpdateOutline();
		}
	}

	public PartData PartData
	{
		get => GetPartData();
		set => SetPartData(value);
	}


	private PartData GetPartData()
	{
		return new PartData
		{
			shiny = Shiny,
			Skill = Skill,
			Part = Part,
		};
	}

	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("%TextureRect");
	}

	private void SetPartData(PartData partData)
	{
		Shiny = partData.shiny;
		Skill = partData.Skill;
		Part = partData.Part;
		Flipped = false;
		Angle = 0;
	}

	private void SetPart(Part part)
	{
		_part = part;
		_coreLight = Assets.Instance.IsCore(part);
		
		textureRect.Texture = Assets.Instance.GetPartTexture(part);

		Vector2 textureSize = textureRect.Texture.GetSize();

		textureRect.Size = textureSize;
		textureRect.Position = textureSize * -0.5f;

		UpdateCoreLight();
		UpdateOutline();
	}

	private void UpdateCoreLight()
	{
		if (_coreLight)
		{
			if (coreLightTextureRect != null)
				return;
			
			coreLightTextureRect = new TextureRect();
			coreLightTextureRect.Texture = Assets.CORE_LIGHT_TEXTURE;
			coreLightTextureRect.Position = Assets.CORE_LIGHT_TEXTURE.GetSize() * -0.5f;
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
		if (_outline)
		{
			if (outlineTextureRect == null)
			{
				outlineTextureRect = new TextureRect();
				outlineTextureRect.Rotation = Angle;
				outlineTextureRect.MouseFilter = MouseFilterEnum.Ignore;
				outlineTextureRect.Material = Assets.PART_OUTLINE_MATERIAL;
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
			if (gimmickTextureRect == null)
			{
				gimmickTextureRect = new TextureRect();
				gimmickTextureRect.Rotation = -Angle;
				gimmickTextureRect.MouseFilter = MouseFilterEnum.Ignore;
				gimmickTextureRect.ZIndex = 1;
				AddChild(gimmickTextureRect);
			}

			Texture2D texture = Assets.Instance.GetSkillTexture(skill.id);
			Vector2 textureSize = texture.GetSize();

			gimmickTextureRect.Texture = texture;
			gimmickTextureRect.Position = textureSize * -0.5f;
			gimmickTextureRect.Size = textureSize * 0.5f;
		}
		else if (gimmickTextureRect != null)
		{
			gimmickTextureRect.QueueFree();
			gimmickTextureRect = null;
		}
	}
}