using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public interface IFileSystem
    {
        [NotNull]
        IFile File { get; }

        [NotNull]
        IDirectory Directory { get; }

        [NotNull]
        IFileInfo ConstructFileInfo([NotNull] string fileName);

        [NotNull]
        IDirectoryInfo ConstructDirectoryInfo([NotNull] string path);
    }
}
