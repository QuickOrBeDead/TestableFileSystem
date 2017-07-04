﻿using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Tests.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class FakeDirectoryCurrentSpecs
    {
        [Fact]
        private void When_setting_current_directory_to_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.Directory.SetCurrentDirectory(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_setting_current_directory_to_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Path cannot be the empty string or all whitespace.*");
        }

        [Fact]
        private void When_setting_current_directory_to_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_setting_current_directory_to_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_setting_current_directory_to_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"c:\dir?i");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.");
        }

        [Fact]
        private void When_setting_current_directory_to_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"C:\other\folder");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\other\folder'.");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:\some  ");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some");
        }

        [Fact]
        private void When_setting_current_directory_to_missing_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"E:\");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'E:\'.");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_drive_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\");
        }

        [Fact]
        private void When_setting_current_directory_to_relative_subdirectory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some\");

            // Act
            fileSystem.Directory.SetCurrentDirectory(@".\folder");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some\folder");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\SOME\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"C:\some\FOLDER");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\SOME\folder");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_remote_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\docserver\teams")
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(@"\\docserver\teams");

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(@"The specified path is invalid.");
        }

        [Fact]
        private void When_setting_current_directory_to_existing_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.Directory.SetCurrentDirectory(path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The directory name is invalid.");
        }

        [Fact]
        private void When_setting_current_directory_to_extended_local_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\folder")
                .Build();

            // Act
            fileSystem.Directory.SetCurrentDirectory(@"\\?\C:\some\folder");

            // Assert
            fileSystem.Directory.GetCurrentDirectory().Should().Be(@"C:\some\folder");
        }
    }
}