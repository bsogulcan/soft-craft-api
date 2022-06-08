using SoftCraft.Entities;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.Repositories;

public interface IPropertyRepository : IRepository<Property, long>
{
}