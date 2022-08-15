using Humanizer;
using SoftCraft.Entities;
using SoftCraft.Enums;
using SoftCraft.Manager.MicroServiceManager.Helpers.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptCodeGenerator.Modals;

namespace SoftCraft.Manager.MicroServiceManager.Helpers
{
    public static class EntityHelper
    {
        private static List<PropertyWrapper> GetLinkedPropertyTree(Entity entity, PropertyWrapper Child = null)
        {
            List<PropertyWrapper> linkedProperties = new List<PropertyWrapper>();
            var filteredProperties = entity.Properties.Where(x => x.IsRelationalProperty && (x.RelationType == RelationType.OneToZero || x.RelationType == RelationType.OneToOne) && (Child == null || x.RelationalEntity.Name != Child.EntityName));
            foreach (var currentProperty in filteredProperties)
            {
                var newPropertyWrapper = new PropertyWrapper
                {
                    EntityName = currentProperty.RelationalEntity.Name,
                    PropertyName = currentProperty.Name,
                    Child = Child
                };
                newPropertyWrapper.Parents = GetLinkedPropertyTree(currentProperty.RelationalEntity, newPropertyWrapper);
                linkedProperties.Add(newPropertyWrapper);
            }
            return linkedProperties;
        }
        private static List<ComboBoxWrapper> GetComboBoxes(List<PropertyWrapper> properties, List<ComboBoxWrapper> comboBoxes = null, bool isInputProperty = false, string accessString = "", string inputName = "createInput")
        {
            if (comboBoxes == null)
                comboBoxes = new List<ComboBoxWrapper>();
            foreach (var currentProperty in properties)
            {
                comboBoxes.Add(new ComboBoxWrapper
                {
                    EntityName = currentProperty.EntityName,
                    PropertyName = currentProperty.PropertyName,
                    IsInputProperty = isInputProperty,
                    AccessString = $"{inputName}{accessString}.{currentProperty.PropertyName.ToCamelCase()}",
                    NGModel = isInputProperty ? $"{inputName}{accessString}.{currentProperty.PropertyName.ToCamelCase()}Id" : $"selected{currentProperty.EntityName}{currentProperty.PropertyName}Id",
                    DataSource = currentProperty.Parents.Count > 0 ? $"this.{currentProperty.EntityName.Pluralize().ToCamelCase()}By{string.Join("And", currentProperty.Parents.Select(x => x.PropertyName))}" : $"this.{currentProperty.EntityName.Pluralize().ToCamelCase()}",
                    DataSourceGetFunction = currentProperty.Parents.Count > 1 ? $"this.get{currentProperty.EntityName.Pluralize()}Filtered({string.Join(",",currentProperty.Parents.Select(x => $"this.selected{x.EntityName}{x.PropertyName}Id"))})" : currentProperty.Parents.Count > 0 ? $"this.get{currentProperty.EntityName.Pluralize()}By{string.Join(",", currentProperty.Parents.Select(x => $"{x.PropertyName}"))}Id({string.Join(",", currentProperty.Parents.Select(x => $"this.selected{x.EntityName}{x.PropertyName}"))}Id)" : $"this.getAll{currentProperty.EntityName.Pluralize()}()",
                    OnChangeEvent = isInputProperty ? "" : $"on{currentProperty.EntityName}{currentProperty.PropertyName}Changed",
                });
                GetComboBoxes(currentProperty.Parents, comboBoxes: comboBoxes, accessString: $"{accessString}.{currentProperty.PropertyName.ToCamelCase()}", inputName: inputName);
            }
            return comboBoxes;
        }
        private static void GenerateOnChangeTasks(ref List<ComboBoxWrapper> comboBoxes)
        {
            foreach (ComboBoxWrapper currentComboBox in comboBoxes)
            {
                currentComboBox.OnChangeEventTasks.AddRange(GetChangeTasks(comboBoxes,currentComboBox.NGModel));
            }
        }
        private static List<string> GetChangeTasks(List<ComboBoxWrapper> comboBoxes,string NGModel)
        {
            List<string> changeTasks = new List<string>();
            foreach (ComboBoxWrapper currentComboBox in comboBoxes.Where(x => x.DataSourceGetFunction.Contains(NGModel)))
            {
                string setUndefined = $"this.{currentComboBox.NGModel.ToCamelCase()} = undefined;";
                string setSlice = $"{currentComboBox.DataSource}.slice(0);";

                if (!changeTasks.Contains(setUndefined))
                    changeTasks.Add(setUndefined);

                if (!changeTasks.Contains(setSlice))
                    changeTasks.Add(setSlice);

                changeTasks.AddRange(GetChangeTasks(comboBoxes, currentComboBox.NGModel));
            }
            return changeTasks;
        }

        public static List<ComboBoxWrapper> GenerateComboBoxes(Entity entity, string inputName = "createInput")
        {
            var propertyTree = GetLinkedPropertyTree(entity);
            var comboBoxes = GetComboBoxes(propertyTree, isInputProperty: true, inputName: inputName);
            GenerateOnChangeTasks(ref comboBoxes);
            return comboBoxes;
        }
    }
}
