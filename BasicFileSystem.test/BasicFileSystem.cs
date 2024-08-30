namespace BasicFileSystem.test;

using BasicFileSystem;

public class Tests
{
    private Directory TestFileSystem
    {
        get
        {
            // test hiearchy
            // /___a
            // |   |___b
            // |   |   |___1.txt
            // |   |
            // |   |___c
            // |   | 
            // |   |___2.txt
            // |   |___3.txt
            // |
            // |___b
            // |
            // |___1.txt
            var root = Directory.CreateRoot();
            root
                .AddChildDirectory("a")
                    .AddChildDirectory("b")
                        .AddFile("1.txt", string.Empty)
                    .ParentDirectory
                    .AddChildDirectory("c")
                    .ParentDirectory
                    .AddFile("2.txt", string.Empty)
                    .AddFile("3.txt", string.Empty);
            root.AddChildDirectory("b");
            root.AddFile("1.txt", string.Empty);

            return root;
        }
    }

    [Test]
    public void DirectoryPaths()
    {
        var dirPaths = TestFileSystem.Directories().Select(d => d.Path).ToList();
        CollectionAssert.AreEqual(new[] { "/", "/a", "/a/b", "/a/c", "/b" }, dirPaths);
    }

    [Test]
    public void FilePaths()
    {
        var filePaths = TestFileSystem.Files().Select(d => d.Path).ToList();
        CollectionAssert.AreEqual(new[] { "/1.txt", "/a/2.txt", "/a/3.txt", "/a/b/1.txt" }, filePaths);
    }

}