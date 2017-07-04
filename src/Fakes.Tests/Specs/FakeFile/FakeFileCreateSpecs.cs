﻿using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileCreateSpecs
    {
        [Fact]
        private void When_creating_file_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Create(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_creating_file_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Empty path name is not legal.*");
        }

        [Fact]
        private void When_creating_file_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_creating_file_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_creating_file_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create("some?.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        [Fact]
        private void When_creating_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            const string path = @"C:\some\file.txt";

            // Act
            using (var stream = fileSystem.File.Create(path))
            {
                stream.WriteByte(0xFF);
            }

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact]
        private void When_creating_local_file_for_random_access_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.RandomAccess))
            {
                // Assert
                fileSystem.File.Exists(@"c:\doc.txt").Should().BeTrue();
            }
        }

        [Fact]
        private void When_creating_local_file_with_encryption_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.Encrypted);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\doc.txt' is denied.");
        }

        [Fact]
        private void When_creating_local_file_with_delete_on_close_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            using (fileSystem.File.Create(path, 1, FileOptions.DeleteOnClose))
            {
                // Assert
                fileSystem.File.Exists(path).Should().BeTrue();
            }

            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact]
        private void When_creating_existing_local_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path, "existing data")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            fileSystem.File.Exists(@"C:\some\file.txt").Should().BeTrue();
        }

        [Fact]
        private void When_creating_existing_local_file_with_different_casing_it_must_overwrite()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(@"C:\some\FILE.txt", "existing data")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(@"c:\SOME\file.TXT"))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            fileSystem.File.Exists(@"C:\some\FILE.txt").Should().BeTrue();
        }

        [Fact]
        private void When_creating_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            using (fileSystem.File.Create(@"C:\some\file.txt  "))
            {
            }

            // Assert
            fileSystem.File.Exists(@"C:\some\file.txt").Should().BeTrue();
        }

        [Fact]
        private void When_creating_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (fileSystem.File.Create("file.txt"))
            {
            }

            // Assert
            fileSystem.File.Exists(@"C:\some\file.txt").Should().BeTrue();
        }

        [Fact]
        private void When_creating_local_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(path);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\subfolder' is denied.");
        }

        [Fact]
        private void When_creating_local_file_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"C:\some\subfolder");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\subfolder'.");
        }

        [Fact]
        private void When_creating_remote_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            using (fileSystem.File.Create(@"\\server\share\file.txt"))
            {
            }

            // Assert
            fileSystem.File.Exists(@"\\server\share\file.txt").Should().BeTrue();
        }

        [Fact]
        private void When_creating_existing_remote_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"\\server\share\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path, "existing data")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            fileSystem.File.Exists(@"\\server\share\file.txt").Should().BeTrue();
        }

        [Fact]
        private void When_creating_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_creating_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\?\C:\folder")
                .Build();

            // Act
            using (fileSystem.File.Create(@"\\?\C:\folder\file.txt"))
            {
            }

            // Assert
            fileSystem.File.Exists(@"C:\folder\file.txt").Should().BeTrue();
        }

        // TODO: Consider moving this spec to FakeFileStreamSpecs.
        [Fact]
        private void When_creating_local_file_it_must_produce_stream()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            using (var stream = fileSystem.File.Create(path, 2048))
            {
                stream.WriteByte(0xFF);
                stream.Flush(true);
                stream.Seek(0, SeekOrigin.Begin);

                // Assert
                stream.CanRead.Should().BeTrue();
                stream.CanSeek.Should().BeTrue();
                stream.CanWrite.Should().BeTrue();
                stream.Name.Should().Be(path);
                stream.Length.Should().Be(1);
                stream.Position.Should().Be(0);
                stream.IsAsync.Should().BeFalse();
            }
        }
    }
}