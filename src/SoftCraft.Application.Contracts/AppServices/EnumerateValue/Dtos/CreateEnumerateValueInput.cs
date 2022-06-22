namespace SoftCraft.AppServices.EnumerateValue.Dtos;

public class CreateEnumerateValueInput
{
    public long EnumerateId { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public int Value { get; set; }
}