using AutoMapper;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.Navigations.Dtos;
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

        CreateMap<Navigation, NavigationFullOutput>();
        CreateMap<Navigation, NavigationPartOutput>();
        CreateMap<Navigation, CreateNavigationInput>();
        CreateMap<Navigation, UpdateNavigationInput>();

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

        CreateMap<NavigationFullOutput, Navigation>();
        CreateMap<NavigationPartOutput, Navigation>();
        CreateMap<CreateNavigationInput, Navigation>();
        CreateMap<UpdateNavigationInput, Navigation>();

        #endregion
    }
}