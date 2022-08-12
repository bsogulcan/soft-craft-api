using TypeScriptCodeGenerator.Modals;

namespace TypeScriptCodeGenerator.Helpers;

public static class EntityHelper
{
    public static List<EntityWrapper> GetRelatedEntities(Entity entity, bool isMainEntity = true,
        bool canBeDuplicate = false)
    {
        var response = new List<EntityWrapper>();

        foreach (var parentEntity in entity.ParentEntities)
        {
            var entityWrapper = new EntityWrapper()
            {
                Entity = parentEntity,
                MainEntity = isMainEntity
            };
            if (canBeDuplicate || !response.Exists(x => x.Entity.Name == entityWrapper.Entity.Name))
            {
                response.Add(entityWrapper);
                var parentEntityRelatedEntities = GetRelatedEntities(parentEntity, false, canBeDuplicate);

                foreach (var relatedEntity in parentEntityRelatedEntities)
                {
                    if (!response.Exists(x => x.Entity.Name == relatedEntity.Entity.Name))
                    {
                        response.Add(relatedEntity);
                    }
                }
            }
        }

        return response;
    }

    public static List<string> GetChildEntity(Entity entity, List<EntityWrapper> relatedEntities)
    {
        var childs = new List<string>();

        foreach (var relatedEntity in relatedEntities)
        {
            if (relatedEntity.Entity.ParentEntities.Any(x => x.Name == entity.Name))
            {
                var x = GetChildEntity(relatedEntity.Entity, relatedEntities);

                childs.Add(relatedEntity.Entity.Name);
                childs.AddRange(x);
            }
        }

        return childs;
    }


    public static List<string> GetParentEntity(Entity entity, List<EntityWrapper> relatedEntities)
    {
        var parent = new List<string>();

        foreach (var relatedEntity in relatedEntities)
        {
            if (relatedEntity.Entity.ParentEntities.Any(x => x.Name == entity.Name))
            {
                var x = GetParentEntity(relatedEntity.Entity, relatedEntities);

                parent.Add(relatedEntity.Entity.Name);
                parent.AddRange(x);
            }
        }

        return parent;
    }

    public static Property GetProperty(ref List<Property> properties, string entityType, bool setUsed = true)
    {
        var property =
            properties.FirstOrDefault(x => x.Type == entityType && (setUsed ? !x.Used : (x.Used || !x.Used)));
        if (property != null)
        {
            if (setUsed)
            {
                property.Used = true;
            }
        }

        return property;
    }
}