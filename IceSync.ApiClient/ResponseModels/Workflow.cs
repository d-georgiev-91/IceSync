namespace IceSync.ApiClient.ResponseModels;

public class Workflow
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; }

    public bool IsRunning { get; set; }

    public required string MultiExecBehavior { get; set; }
}