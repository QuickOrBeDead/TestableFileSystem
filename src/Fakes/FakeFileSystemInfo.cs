using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public abstract class FakeFileSystemInfo : IFileSystemInfo
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        internal FakeFileSystem Owner { get; }

        [NotNull]
        internal AbsolutePath AbsolutePath { get; private set; }

        [CanBeNull]
        private EntryProperties propertiesSnapshot;

        [NotNull]
        internal EntryProperties Properties => propertiesSnapshot ?? (propertiesSnapshot = RetrieveEntryProperties());

        public abstract string Name { get; }

        public string Extension { get; private set; }

        public string FullName { get; private set; }

        public FileAttributes Attributes
        {
            get
            {
                Properties.AssertNoError();
                return Properties.Attributes;
            }
            set
            {
                Owner.File.SetAttributes(FullName, value);
                Invalidate();
            }
        }

        public DateTime CreationTime
        {
            get => CreationTimeUtc.ToLocalTime();
            set => CreationTimeUtc = value.ToUniversalTime();
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                Properties.AssertNoError();
                return Properties.CreationTimeUtc;
            }
            set
            {
                Owner.File.SetCreationTimeUtc(FullName, value);
                Invalidate();
            }
        }

        public DateTime LastAccessTime
        {
            get => LastAccessTimeUtc.ToLocalTime();
            set => LastAccessTimeUtc = value.ToUniversalTime();
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                Properties.AssertNoError();
                return Properties.LastAccessTimeUtc;
            }
            set
            {
                Owner.File.SetLastAccessTimeUtc(FullName, value);
                Invalidate();
            }
        }

        public DateTime LastWriteTime
        {
            get => LastWriteTimeUtc.ToLocalTime();
            set => LastWriteTimeUtc = value.ToUniversalTime();
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                Properties.AssertNoError();
                return Properties.LastWriteTimeUtc;
            }
            set
            {
                Owner.File.SetLastWriteTimeUtc(FullName, value);
                Invalidate();
            }
        }

        public abstract bool Exists { get; }

        internal FakeFileSystemInfo([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner, [NotNull] AbsolutePath path)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(path, nameof(path));

            this.root = root;
            Owner = owner;

            AbsolutePath = path;
            FullName = path.GetText();
            Extension = Path.GetExtension(FullName);
        }

        public void Refresh()
        {
            propertiesSnapshot = RetrieveEntryProperties();
        }

        [NotNull]
        private EntryProperties RetrieveEntryProperties()
        {
            var handler = new GetEntryPropertiesHandler(root);
            var arguments = new GetEntryPropertiesArguments(AbsolutePath);

            lock (Owner.TreeLock)
            {
                return handler.Handle(arguments);
            }
        }

        private void Invalidate()
        {
            propertiesSnapshot = null;
        }

        public void Delete()
        {
            Owner.File.Delete(FullName);
        }

        internal void ChangePath([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath = path;
            FullName = path.GetText();
            Extension = Path.GetExtension(FullName);

            Invalidate();
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
