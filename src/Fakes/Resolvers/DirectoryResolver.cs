﻿using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Resolvers
{
    internal sealed class DirectoryResolver
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        public Func<string, Exception> ErrorNetworkShareNotFound { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryFoundAsFile { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorLastDirectoryFoundAsFile { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryNotFound { get; set; }

        public DirectoryResolver([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;

            ErrorNetworkShareNotFound = _ => ErrorFactory.NetworkPathNotFound();
            ErrorDirectoryFoundAsFile = ErrorFactory.DirectoryNotFound;
            ErrorLastDirectoryFoundAsFile = ErrorFactory.DirectoryNotFound;
            ErrorDirectoryNotFound = ErrorFactory.DirectoryNotFound;
        }

        [NotNull]
        public DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path, [CanBeNull] string incomingPath = null)
        {
            Guard.NotNull(path, nameof(path));

            string completePath = incomingPath ?? path.GetText();

            DirectoryEntry directory = TryResolveDirectory(path, completePath);
            if (directory == null)
            {
                throw ErrorDirectoryNotFound(completePath);
            }

            return directory;
        }

        [CanBeNull]
        public DirectoryEntry TryResolveDirectory([NotNull] AbsolutePath path, [CanBeNull] string incomingPath = null)
        {
            Guard.NotNull(path, nameof(path));

            string completePath = incomingPath ?? path.GetText();

            DirectoryEntry directory = root;

            foreach (AbsolutePathComponent component in path.EnumerateComponents())
            {
                AssertNetworkShareExists(component, completePath);
                AssertIsNotFile(component, directory, completePath);

                if (!directory.Directories.ContainsKey(component.Name))
                {
                    return null;
                }

                directory = directory.Directories[component.Name];
            }

            return directory;
        }

        [AssertionMethod]
        private void AssertNetworkShareExists([NotNull] AbsolutePathComponent component, [NotNull] string incomingPath)
        {
            if (component.IsAtStart && !component.Path.IsOnLocalDrive && !root.Directories.ContainsKey(component.Name))
            {
                throw ErrorNetworkShareNotFound(incomingPath);
            }
        }

        [AssertionMethod]
        private void AssertIsNotFile([NotNull] AbsolutePathComponent component, [NotNull] DirectoryEntry directory,
            [NotNull] string incomingPath)
        {
            if (directory.Files.ContainsKey(component.Name))
            {
                if (component.IsAtEnd)
                {
                    throw ErrorLastDirectoryFoundAsFile(incomingPath);
                }

                throw ErrorDirectoryFoundAsFile(incomingPath);
            }
        }
    }
}
