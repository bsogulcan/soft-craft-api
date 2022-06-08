using AutoMapper;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.EntityValue.Dtos;
using SoftCraft.AppServices.Enumerate.Dtos;
using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.AppServices.Property.Dtos;
using SoftCraft.Entities;

namespace SoftCraft;

public class SoftCraftApplicationAutoMapperProfile : Profile
{
    public SoftCraftApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        #region EntityToDto

        CreateMap<Project, ProjectFullOutput>();
        CreateMap<Project, ProjectPartOutput>();
        CreateMap<Project, CreateProjectDto>();
        CreateMap<Project, UpdateProjectDto>();

        CreateMap<Entity, EntityFullOutput>();
        CreateMap<Entity, EntityPartOutput>();
        CreateMap<Entity, CreateEntityInput>();
        CreateMap<Entity, UpdateEntityInput>();

        CreateMap<Property, PropertyFullOutput>();
        CreateMap<Property, PropertyPartOutput>();
        CreateMap<Property, CreatePropertyInput>();
        CreateMap<Property, UpdatePropertyInput>();

        CreateMap<Enumerate, EnumerateFullOutput>();
        CreateMap<Enumerate, EnumeratePartOutput>();
        CreateMap<Enumerate, CreateEnumerateInput>();
        CreateMap<Enumerate, UpdateEnumerateInput>();
        
       
        CreateMap<EnumerateValue, EnumerateFullOutput>();
        CreateMap<EnumerateValue, EnumeratePartOutput>();
        CreateMap<EnumerateValue, CreateEnumerateValueInput>();
        CreateMap<EnumerateValue, UpdateEnumerateValueInput>();

        #endregion

        #region DtoToEntity

        CreateMap<ProjectFullOutput, Project>();
        CreateMap<ProjectPartOutput, Project>();
        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();

        CreateMap<EntityFullOutput, Entity>();
        CreateMap<EntityPartOutput, Entity>();
        CreateMap<CreateEntityInput, Entity>();
        CreateMap<UpdateEntityInput, Entity>();

        CreateMap<PropertyFullOutput, Property>();
        CreateMap<PropertyPartOutput, Property>();
        CreateMap<CreatePropertyInput, Property>();
        CreateMap<UpdatePropertyInput, Property>();
        
        
        CreateMap< EnumerateFullOutput, Enumerate>();
        CreateMap<EnumeratePartOutput, Enumerate>();
        CreateMap<CreateEnumerateInput, Enumerate>();
        CreateMap<UpdateEnumerateInput, Enumerate>();
        
        
        CreateMap<EnumerateFullOutput, EnumerateValue>();
        CreateMap<EnumeratePartOutput, EnumerateValue>();
        CreateMap<CreateEnumerateValueInput, EnumerateValue>();
        CreateMap<UpdateEnumerateValueInput, EnumerateValue>();

        #endregion
    }
}