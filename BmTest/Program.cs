using BmTest;

var gamePath =
    "/Users/elikramer/Library/Application Support/CrossOver/Bottles/Win10/drive_c/Program Files (x86)/Steam/steamapps/common/Batman Arkham City GOTY/BMGame/CookedPCConsole/";

// Load game packages
GameInfo.Init(Path.Combine(gamePath, "unpacked/"));
var upk = GameInfo.GetPackage("Playable_Batman_Std_SF");

// Print package exports
Console.WriteLine($"{upk.PackageName} {{");
foreach (var export in upk.Exports)
{
    // Skip a bunch of clutter
    if (export.Class.ObjectName.Name.Contains("MaterialExpression"))
    {
        continue;
    }

    Console.WriteLine($"  {(int)export}: {export.Class.ObjectName}'{export.ObjectName}'");
}
Console.WriteLine("}");

// Test deserializing SkeletalMesh object
DebugPrintMesh();
void DebugPrintMesh()
{
    // Get SkeletalMesh object
    var mesh = upk.FindObject<BmSkeletalMesh>("Batman_Head_Skin");
    mesh.BeginDeserializing();

    // Crash occurs where a PointerProperty follows an ArrayProperty
    // PointerProperties shouldn't exist in the first place
    // But ArrayProperties don't usually cause things to break?

    Console.Write("\n");
    Console.WriteLine(mesh.Decompile());

    foreach (var mat in mesh.Materials)
    {
        mat.BeginDeserializing();
        Console.WriteLine(mat.Decompile());
    }
}

// Test deserializing Material object
/*DebugPrintMaterial();
void DebugPrintMaterial()
{
    // Load material object
    var mat = upk.FindObject<BmMaterial>("Batman_Body_Master_MAT");
    mat.BeginDeserializing();

    Console.Write("\n");
    Console.WriteLine(mat.Decompile());
}*/
