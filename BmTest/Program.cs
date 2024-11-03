using UELib;

var gamePath =
    "/Users/elikramer/Library/Application Support/CrossOver/Bottles/Win10/drive_c/Program Files (x86)/Steam/steamapps/common/Batman Arkham City GOTY/BmGame/CookedPCConsole/";

// Load UPK package
var upk = UnrealLoader.LoadPackage(Path.Combine(gamePath, "unpacked/Playable_Batman_Std_SF.upk"));
upk.TryAddClassType("Material", typeof(UMaterialRS));
upk.TryAddClassType("Texture2D", typeof(UTexture2DRS));
upk.InitializePackage();

// Print UPK contents
Console.WriteLine($"{upk.PackageName} {{");
foreach (var export in upk.Exports.OrderBy(o => o.ClassIndex))
{
    Console.WriteLine($"    {export.Class.ObjectName}'{export.ObjectName}'");
}
Console.WriteLine("}");

DebugPrintTexture2D();
//DebugPrintMaterial();

void DebugPrintMaterial()
{
    // Get Material object
    var mat = upk.FindObject<UMaterialRS>("Batman_Body_Master_MAT");
    mat.BeginDeserializing();

    Console.Write("\n");
    Console.WriteLine(mat.Decompile());
}

void DebugPrintTexture2D()
{
    // Get Texture2D object
    var tex = upk.FindObject<UTexture2DRS>("Batman_V3_Body_D");
    tex.BeginDeserializing();

    Console.Write("\n");
    Console.WriteLine(tex.Decompile());
}
