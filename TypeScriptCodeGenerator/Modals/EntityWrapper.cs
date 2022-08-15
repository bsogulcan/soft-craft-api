namespace TypeScriptCodeGenerator.Modals;

public class EntityWrapper
{
    public Entity Entity { get; set; }
    public bool MainEntity { get; set; }
    public List<string> Childs { get; set; } = new();
}