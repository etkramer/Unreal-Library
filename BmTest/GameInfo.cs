using UELib;

namespace BmTest;

public static class GameInfo
{
    static readonly Dictionary<string, UnrealPackage> s_packages =  [ ];

    public static void Init(string gamePath)
    {
        // Use 2-space independation
        UnrealConfig.Indention = "  ";

        // Load in all upk files
        foreach (var filePath in Directory.GetFiles(gamePath, "*.upk"))
        {
            // Disable stdout (temporary)
            var stdout = Console.Out;
            Console.SetOut(TextWriter.Null);

            // Open upk, but don't do any deserialization yet
            var pkg = UnrealLoader.LoadPackage(filePath);

            // Reenable stdout
            Console.SetOut(stdout);

            // Skip compressed packages
            if (pkg.Summary.CompressionFlags != 0)
            {
                Console.WriteLine($"Skipping compressed package {pkg.PackageName}.upk");
                pkg.Dispose();

                continue;
            }

            s_packages.Add(pkg.PackageName, pkg);
        }
    }

    public static UnrealPackage GetPackage(string name)
    {
        var pkg = s_packages[name];

        // Is package initialized?
        if (pkg.Objects.Count == 0)
        {
            // Perform init if needed
            pkg.TryAddClassType("Material", typeof(BmMaterial));
            pkg.TryAddClassType("Texture2D", typeof(BmTexture2D));
            pkg.TryAddClassType("SkeletalMesh", typeof(BmSkeletalMesh));
            pkg.InitializePackage();
        }

        return pkg;
    }
}
