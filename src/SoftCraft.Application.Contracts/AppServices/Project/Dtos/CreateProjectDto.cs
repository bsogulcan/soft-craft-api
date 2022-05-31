namespace SoftCraft.AppServices.Dtos;

public class CreateProjectDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string NormalizedName { get; set; }
    public int Port { get; set; }
}