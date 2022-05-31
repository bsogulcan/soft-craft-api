using AutoMapper;
using SoftCraft.AppServices.Dtos;
using SoftCraft.AppServices.Entity.Dtos;
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

        CreateMap<Project, ProjectDto>();
        CreateMap<Project, CreateProjectDto>();
        CreateMap<Project, UpdateProjectDto>();

        CreateMap<Entity, EntityDto>();
        CreateMap<Entity, CreateEntityInput>();
        CreateMap<Entity, UpdateEntityInput>();

        CreateMap<Property, PropertyDto>();
        CreateMap<Property, CreatePropertyInput>();
        CreateMap<Property, UpdatePropertyInput>();

        #endregion

        #region DtoToEntity

        CreateMap<ProjectDto, Project>();
        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();

        CreateMap<EntityDto, Entity>();
        CreateMap<CreateEntityInput, Entity>();
        CreateMap<UpdateEntityInput, Entity>();

        CreateMap<PropertyDto, Property>();
        CreateMap<CreatePropertyInput, Property>();
        CreateMap<UpdatePropertyInput, Property>();
        #endregion
    }
}