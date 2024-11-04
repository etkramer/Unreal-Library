using BmTest;

var gamePath =
    "/Users/elikramer/Library/Application Support/CrossOver/Bottles/Win10/drive_c/Program Files (x86)/Steam/steamapps/common/Batman Arkham City GOTY/BMGame/CookedPCConsole/";

// Load game packages
GameInfo.Init(Path.Combine(gamePath, "unpacked/"));
var upk = GameInfo.GetPackage("CV_Batwing");

// Print package imports/exports
Console.WriteLine($"{upk.PackageName} {{");
foreach (var export in upk.Exports.OrderByDescending(o => (int)o))
{
    Console.WriteLine($"  {(int)export}: {export.Class.ObjectName}'{export.ObjectName}'");
}
foreach (var import in upk.Imports)
{
    Console.WriteLine($"  {(int)import}: {import.ClassName}'{import.ObjectName}'");
}
Console.WriteLine("}");

// Test deserializing SkeletalMesh object
DebugPrintMesh();
void DebugPrintMesh()
{
    // Get SkeletalMesh object
    var mesh = upk.FindObject<BmSkeletalMesh>("Batwing_NEW");
    mesh.BeginDeserializing();

    Console.Write("\n");
    Console.WriteLine(mesh.Decompile());
}

// Test deserializing Texture2D object
DebugPrintTexture2D();
void DebugPrintTexture2D()
{
    // Get Texture2D object
    var tex = upk.FindObject<BmTexture2D>("Batwing_NEW_N");
    tex.BeginDeserializing();

    Console.Write("\n");
    Console.WriteLine(tex.Decompile());
}

// Test deserializing Material object
DebugPrintMaterial();
void DebugPrintMaterial()
{
    // Load material UClass
    /*Console.Write("\n");
    var core = UnrealLoader.LoadFullPackage(Path.Combine(gamePath, "unpacked/Core.upk"));
    var matClass = core.FindObject<UClass>("Material");
    Console.WriteLine(matClass.Decompile());*/

    // Load material object
    var mat = upk.FindObject<BmMaterial>("Batwing_NEW_MAT");
    mat.BeginDeserializing();

    Console.Write("\n");
    Console.WriteLine(mat.Decompile());
}
