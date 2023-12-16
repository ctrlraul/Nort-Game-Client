using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CtrlRaul;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Entity = Nort.Entities.Entity;

using PropAttrDict = System.Collections.Generic.Dictionary<string, (System.Reflection.PropertyInfo, Nort.Hud.InspectAttribute)>;

namespace Nort.Hud;


[AttributeUsage(AttributeTargets.Property)]
public class InspectAttribute : Attribute
{
    public enum InspectionType
    {
        None,
        Options,
    }

    public InspectionType Type { get; } = InspectionType.None;
    public string OptionsPropertyName { get; }
    
    public InspectAttribute() { }
    
    public InspectAttribute(string optionsPropertyName)
    {
        OptionsPropertyName = optionsPropertyName;
        Type = InspectionType.Options;
    }
}


public partial class EntityInspector : PanelContainer
{
    public interface IField
    {
        public event Action<object> ValueChanged;
        public void SetValue(object value);
        public void SetLabel(string value);
        public void Disable();
    }
    
    [Export] private PackedScene booleanFieldScene;
    [Export] private PackedScene optionsFieldScene;

    [Ready] public Label entityLabel;
    [Ready] public Control fieldsListContainer;
    [Ready] public Control fieldsList;

    private readonly List<Entity> selectedEntities = new();

    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        Clear();
    }

    
    public void Clear()
    {
        Visible = false;
        entityLabel.Text = "";
        selectedEntities.Clear();
        fieldsList.QueueFreeChildren();
        fieldsListContainer.Hide();
    }
    
    public void SetEntity(Entity value)
    {
        Clear();

        selectedEntities.Add(value);
        
        Type entityType = value.GetType();
        
        Visible = true;
        entityLabel.Text = entityType.Name;
        
        AddFieldsForProperties(GetInspectableProperties(entityType));
    }
    
    public void SetEntities(List<Entity> entities)
    {
        Clear();
        
        if (!entities.Any())
            return;

        Entity firstEntity = entities.First();

        if (entities.Count == 1)
        {
            SetEntity(firstEntity);
            return;
        }
        
        
        Visible = true;
        entityLabel.Text = "(Multiple Objects)";
        
        selectedEntities.AddRange(entities);

        Type firstEntityType = firstEntity.GetType();

        List<Type> mappedTypes = new() { firstEntityType };
        
        PropAttrDict commonProperties = GetInspectableProperties(firstEntityType);
        
        foreach (Entity entity in entities)
        {
            Type entityType = entity.GetType();

            if (mappedTypes.Contains(entityType))
                continue;
            
            mappedTypes.Add(entityType);
            
            PropAttrDict entityProperties = GetInspectableProperties(entityType);
            
            foreach (string propertyName in entityProperties.Keys)
            {
                if (commonProperties.TryGetValue(propertyName, out var propAttr))
                {
                    if (propAttr.Item1.PropertyType == entityProperties[propertyName].Item1.PropertyType)
                    {
                        continue;
                    }
                }
                    
                commonProperties.Remove(propertyName);
                        
                if (!commonProperties.Any())
                    return; // Rip, nothing compatible
            }
        }
        
        AddFieldsForProperties(commonProperties);
    }
    
    private void AddFieldsForProperties(PropAttrDict propAttrDict)
    {
        foreach ((PropertyInfo, InspectAttribute) propAndAttr in propAttrDict.Values)
        {
            switch (propAndAttr.Item2.Type)
            {
                case InspectAttribute.InspectionType.None:
                    if (propAndAttr.Item1.PropertyType == typeof(bool))
                    {
                        AddBooleanField(propAndAttr.Item1);
                    }
                    else
                    {
                        throw new Exception($"Unhandled InspectAttribute property type: '{propAndAttr.Item1.PropertyType}'");
                    }
                    break;
                
                case InspectAttribute.InspectionType.Options:
                    AddOptionsField(propAndAttr.Item1);
                    break;
                
                default:
                    throw new Exception($"Unhandled InspectAttribute type: '{propAndAttr.Item2.Type}'");
            }
        }
    }
    
    
    private T InstantiateFieldScene<T>(MemberInfo propertyInfo) where T : Control, IField
    {
        string typeName = typeof(T).Name;
        
        PackedScene scene = typeName switch
        {
            nameof(EntityInspectorBooleanField) => booleanFieldScene,
            nameof(EntityInspectorOptionsField) => optionsFieldScene,
            
            _ => throw new Exception($"No inspector field scene configured for type '{typeName}'")
        };
        
        T field = scene.Instantiate<T>();
        fieldsListContainer.Show();
        fieldsList.AddChild(field);
        field.SetLabel(propertyInfo.Name.Capitalize());
        
        return field;
    }

    
    private void AddBooleanField(PropertyInfo propertyInfo)
    {
        EntityInspectorBooleanField field = InstantiateFieldScene<EntityInspectorBooleanField>(propertyInfo);
        
        field.SetValue(HasCommonValue(propertyInfo, out object commonValue) ? commonValue : false);
        
        field.ValueChanged += newValue =>
        {
            foreach (Entity entity in selectedEntities)
            {
                propertyInfo.SetValue(entity, newValue);
            }
        };
    }

    private void AddOptionsField(PropertyInfo propertyInfo)
    {
        EntityInspectorOptionsField field = InstantiateFieldScene<EntityInspectorOptionsField>(propertyInfo);
        
        if (!HasCommonValueOptions(propertyInfo, out List<string> commonValueOptions) || !commonValueOptions.Any())
        {
            field.SetOptions(Array.Empty<string>());
            field.SetValue(-1);
            field.Disable();
            return;
        }
        
        field.SetOptions(commonValueOptions);
        
        if (HasCommonValue(propertyInfo, out object commonValue))
        {
            int index = commonValueOptions.FindIndex(option => option == (string)commonValue);
            field.SetValue(index);
        }
        else
        {
            field.SetValue(-1);
        }

        field.ValueChanged += newValue =>
        {
            foreach (Entity entity in selectedEntities)
            {
                propertyInfo.SetValue(entity, commonValueOptions.ElementAt((int)newValue));
            }
        };
    }

    
    private bool HasCommonValue(PropertyInfo propertyInfo, out object commonValue)
    {
        commonValue = propertyInfo.GetValue(selectedEntities.First());
        
        foreach (Entity entity in selectedEntities)
        {
            Type entityType = entity.GetType();

            if (propertyInfo.ReflectedType != entityType)
                propertyInfo = entityType.GetProperty(propertyInfo.Name)!;
            
            if (propertyInfo.GetValue(entity) != commonValue)
                return false;
        }

        return true;
    }

    private bool HasCommonValueOptions(MemberInfo memberInfo, out List<string> commonValueOptions)
    {
        commonValueOptions = null;

        string optionsPropertyName = CapitalizeFirstLetter(memberInfo.Name) + "Options";

        foreach (Entity entity in selectedEntities)
        {
            Type entityType = entity.GetType();

            PropertyInfo optionsPropertyInfo = entityType.GetProperty(optionsPropertyName);

            if (optionsPropertyInfo == null || optionsPropertyInfo.PropertyType != typeof(IEnumerable<string>))
            {
                Logger.Error(Name, (
                    $"Expected complementary property 'public IEnumerable<string> {optionsPropertyName}' " +
                    $"in class '{entityType.Name}' " +
                    $"for the inspectable field '{memberInfo.Name}'"
                ));

                return false;
            }

            List<string> entityOptions = (optionsPropertyInfo.GetValue(entity) as IEnumerable<string>)?.ToList();

            if (entityOptions == null)
            {
                // Handle the case where entityOptions is null, if needed.
                continue;
            }

            if (commonValueOptions == null)
            {
                commonValueOptions = entityOptions;
                continue;
            }

            commonValueOptions = commonValueOptions.Where(option => entityOptions.Contains(option)).ToList();

            if (!commonValueOptions.Any())
                return false;
        }

        return true;
    }
    
    
    private static string CapitalizeFirstLetter(string input)
    {
        return char.ToUpper(input[0]) + input.Substring(1);
    }
    
    private static PropAttrDict GetInspectableProperties(Type type)
    {
        PropAttrDict inspectableProperties = new();

        foreach (PropertyInfo property in type.GetProperties())
        {
            InspectAttribute attribute = property.GetCustomAttribute<InspectAttribute>();

            if (attribute != null)
                inspectableProperties.Add(property.Name, (property, attribute));
        }
        
        return inspectableProperties;
    }
}
