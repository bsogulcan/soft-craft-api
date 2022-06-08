using SoftCraft.AppServices.Project.Dtos;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Enumerate.Dtos;

public class EnumerateFullOutput:EntityDto<long>
{
   public long ProjectId { get; set; }
   
   public ProjectPartOutput Project { get; set; }

   public string Name { get; set; }

   public string DisplayName { get; set; }
   
   // public ICollection<EnumerateValueFullOutput> Type { get; set; }
}