namespace IceSync.CommonAbstractions;

public class Environment : IEnvironment
{
    public string GetFolderPath(System.Environment.SpecialFolder folder) => System.Environment.GetFolderPath(folder);
}