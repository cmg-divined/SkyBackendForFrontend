namespace Coflnet.Sky.Commands.Shared;

public class ConfigContainer
{
    public FlipSettings Settings { get; set; }
    public string Name { get; set; }
    public int Version { get; set; }
    public string ChangeNotes { get; set; }
    public string OwnerId { get; set; }
    public int Price { get; set; }
}