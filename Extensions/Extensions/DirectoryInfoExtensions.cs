namespace Extensions;

public static class DirectoryInfoExtensions
{
    public static IEnumerable<FileSystemInfo> AllFilesAndFolders(this DirectoryInfo dir)
    {
        foreach (var f in dir.GetFiles())
            yield return f;
        foreach (var d in dir.GetDirectories())
        {
            yield return d;
            foreach (var o in AllFilesAndFolders(d))
                yield return o;
        }
    }
}