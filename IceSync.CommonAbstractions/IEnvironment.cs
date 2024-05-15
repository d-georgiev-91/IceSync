namespace IceSync.CommonAbstractions;

public interface IEnvironment
{
    string GetFolderPath(System.Environment.SpecialFolder folder);
}