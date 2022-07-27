# About soft-craft-api!

SoftCraft is fastest way to create new web application based on Abp Boilerplate framework, we call it Generator! This is the backend service of the **soft-craft** application.

# What are the benefits?
SoftCraft creates a backend using (.Net Core 6) and frontend (Angular) application based on the 'Project', 'Entity', 'Enums' and 'Navigation Items' data defined by the user from the web interface.

# What Does It Create?
 DotNetCodeGenerator creates Entities, Entity Configurations, Repositories, Enums, Dtos, AppServices and Permissions.
 TypeScriptCodeGenerator creates Dtos, Services, Routes, Enums, List-Create-Edit Components and Navigation Items.
 ProjectManager creates AbpBoilerplate application according to the generated codes.

# Architecture
![Architecture](https://imgkub.com/images/2022/07/28/soft-craft-api.png)

# Example Entity
![Entity details from Ui](https://imgkub.com/images/2022/07/28/teacher-entity.png)

## C# Code Results
#### Entity
    using System;
	using Abp.Domain.Entities.Auditing;
	using System.Collections.Generic;
	using Abp.Domain.Entities;

	namespace EFProject.Domain.Entities
	{
		public class Teacher : FullAuditedEntity<int>
		{
			public Teacher()
			{
				Courses = new HashSet<Course>();
			}
			public string TeacherName { get; set; }
			public virtual ICollection<Course>Courses { get; set; }
			public int StandartId { get; set; }
			public virtual Standart Standart { get; set; }
		}
	}

#### Entity Configuration (Entity Framework)
    using EFProject.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    
    namespace EFProject.Domain.Configurations
    {
    	public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    	{
    		public void Configure(EntityTypeBuilder<Teacher> builder)
    		{
    			builder.ToTable("Teachers");
    			builder.HasKey(x => x.Id);
    			builder.Property(x => x.TeacherName).HasMaxLength(50);
    
    			builder.HasMany(x => x.Courses)
    				.WithOne(y => y.Teacher)
    				.HasForeignKey(y => y.TeacherId)
    				.OnDelete(DeleteBehavior.ClientSetNull);
    		}
    	}
    }

#### Application Service
    using Abp.Application.Services;
    using Abp.Authorization;
    using EFProject.Authorization;
    using EFProject.Domain.Teacher.Dtos;
    using EFProject.Domain.Teacher;
    using EFProject.Domain.Entities;
    using EFProject.EntityFrameworkCore.Repositories;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    
    namespace EFProject.Domain.Teacher
    {
    	[AbpAuthorize(PermissionNames.Teacher)]
    	public class TeacherAppService : AsyncCrudAppService<Entities.Teacher, TeacherFullOutput, int, GetTeacherInput, CreateTeacherInput, UpdateTeacherInput, GetTeacherInput, DeleteTeacherInput>, ITeacherAppService
    	{
    		public TeacherAppService(ITeacherRepository teacherRepository) : base(teacherRepository)
    		{
    			CreatePermissionName = PermissionNames.Teacher_Create;
    			UpdatePermissionName = PermissionNames.Teacher_Update;
    			DeletePermissionName = PermissionNames.Teacher_Delete;
    			GetPermissionName = PermissionNames.Teacher_Get;
    			GetAllPermissionName = PermissionNames.Teacher_GetList;
    		}
    
    		[AbpAuthorize(PermissionNames.Teacher_GetList)]
    		public async Task<PagedResultDto<TeacherFullOutput>> GetTeachersByStandartId(int standartId)
    		{
    			var items = await Repository.GetAllListAsync(x => x.StandartId == standartId);
    			return new PagedResultDto<TeacherFullOutput>
    			{
    				TotalCount = items.Count,
    				Items = ObjectMapper.Map<List<TeacherFullOutput>>(items)
    			};
    		}
    
    	}
    }



