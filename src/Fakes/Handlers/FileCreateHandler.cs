﻿using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCreateHandler : FakeOperationHandler<FileCreateArguments, IFileStream>
    {
        private const FileAttributes HiddenReadOnlyMask = FileAttributes.Hidden | FileAttributes.ReadOnly;

        public FileCreateHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override IFileStream Handle(FileCreateArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertValidCreationOptions(arguments);

            var resolver = new FileResolver(Root);
            (DirectoryEntry containingDirectory, FileEntry existingFileOrNull, string fileName) =
                resolver.TryResolveFile(arguments.Path);

            AssertIsNotHiddenOrReadOnly(existingFileOrNull, arguments.Path);

            FileEntry file = existingFileOrNull ?? containingDirectory.CreateFile(fileName);

            if ((arguments.Options & FileOptions.DeleteOnClose) != 0)
            {
                file.EnableDeleteOnClose();
            }

            return file.Open(FileMode.Create, FileAccess.ReadWrite, arguments.Path);
        }

        [AssertionMethod]
        private static void AssertValidCreationOptions([NotNull] FileCreateArguments arguments)
        {
            if (arguments.Options.HasFlag(FileOptions.Encrypted))
            {
                throw ErrorFactory.System.UnauthorizedAccess(arguments.Path.GetText());
            }
        }

        [AssertionMethod]
        private void AssertIsNotHiddenOrReadOnly([CanBeNull] FileEntry fileEntry, [NotNull] AbsolutePath absolutePath)
        {
            if (fileEntry != null && (fileEntry.Attributes & HiddenReadOnlyMask) != 0)
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }
    }
}
