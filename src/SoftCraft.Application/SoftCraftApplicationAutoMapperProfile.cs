using AutoMapper;
using SoftCraft.AppServices.Dtos;
using SoftCraft.Entities;

namespace SoftCraft;

public class SoftCraftApplicationAutoMapperProfile : Profile
{
    public SoftCraftApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<Project, ProjectDto>();
        CreateMap<Project, CreateProjectDto>();
        CreateMap<Project, UpdateProjectDto>();

        CreateMap<ProjectDto, Project>();
        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();
    }
}