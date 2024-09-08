using System.Collections.ObjectModel;

namespace BasicFileSystem;

// In-mem FS
// No IO
// Contains folders and files
// Files contain just text/strings for simplicity

// Milestones
// 1. creation of the FS in main
// 2. iterate over the FS

public class Directory
{
    public const string Delimiter = "/";

    Files _files = new();
    Directories _childDirs = new();

    private Directory() => Name = Delimiter;
    public Directory(string name, Directory parentDirectory) => (Name, MaybeParentDirectory) = (name, parentDirectory);

    public static Directory CreateRoot() => new();

    public string Name { get; private set; }
    public Directory? MaybeParentDirectory { get; private set; }
    public Directory ParentDirectory => MaybeParentDirectory ?? throw new NoParentException(Name);
    public bool IsRoot => MaybeParentDirectory is null;
    public string Path => string.Join(Delimiter, Parents().Select(d => d.IsRoot ? string.Empty : d.Name).Reverse().Concat([Name]));

    public IEnumerable<Directory> Parents()
    {
        var currDir = this;
        while (currDir.MaybeParentDirectory is not null)
        {
            yield return currDir.ParentDirectory;
            currDir = currDir.ParentDirectory;
        }
    }

    public IEnumerable<Directory> Directories()
    {
        yield return this;
        foreach (var childDir in _childDirs.SelectMany(dir => dir.Directories()))
        {
            yield return childDir;
        }
    }

    public IEnumerable<File> Files()
    {
        foreach (var file in Directories().SelectMany(dir => dir._files))
        {
            yield return file;
        }
    }

    public Directory AddChildDirectory(string name)
    {
        var newDir = new Directory(name, this);
        if (_childDirs.Contains(newDir))
        {
            throw new DirectoryAlreadyExistsException(name, this);
        }

        _childDirs.Add(newDir);
        return newDir;
    }

    public Directory AddFile(string name, string contents)
    {
        File newFile = new(name, contents, this);
        if (_files.Contains(newFile))
        {
            throw new FileAlreadyExistsException(name, this);
        }

        _files.Add(newFile);
        return this;
    }

    public IEnumerable<FileSystemEntry> Entries()
    {
        foreach (var dir in new[] { this }.Concat(Directories()))
        {
            yield return new(this);
            foreach (var file in dir._files)
            {
                yield return new(file);
            }
        }
    }
}

public record File(string Name, string Contents, Directory ParentDirectory)
{
    public string Path => ParentDirectory.Path + (ParentDirectory.IsRoot ? Name : $"{Directory.Delimiter}{Name}");
}

public record FileSystemEntry
{
    public Directory? Directory { get; private set; }
    public File? File { get; private set; }
    public FileSystemEntry(Directory directory) => Directory = directory;
    public FileSystemEntry(File file) => File = file;
}

class Files : KeyedCollection<string, File>
{
    protected override string GetKeyForItem(File file) => file.Name;
}

public class Directories : KeyedCollection<string, Directory>
{
    protected override string GetKeyForItem(Directory dir) => dir.Name;
}

public abstract class InMemoryFileSystemException : Exception
{
    protected InMemoryFileSystemException(string message) : base(message) { }
}

public class NoParentException : InMemoryFileSystemException
{
    public string DirectoryName { get; private set; }

    public NoParentException(string dirName)
        : base($"Directory '{dirName}' does not have a parent directory")
        => DirectoryName = dirName;
}

public class DirectoryAlreadyExistsException : InMemoryFileSystemException
{
    public Directory ParentDirectory { get; private set; }
    public string DirectoryName { get; private set; }

    public DirectoryAlreadyExistsException(string dirName, Directory parentDirectory)
        : base($"Directory '{dirName}' already exists in '{parentDirectory.Name}'")
        => (DirectoryName, ParentDirectory) = (dirName, parentDirectory);
}

public class FileAlreadyExistsException : InMemoryFileSystemException
{
    public Directory ParentDirectory { get; private set; }
    public string FileName { get; private set; }

    public FileAlreadyExistsException(string fileName, Directory parentDirectory)
        : base($"File '{fileName}' already exists in '{parentDirectory.Name}'")
        => (FileName, ParentDirectory) = (fileName, parentDirectory);
}
