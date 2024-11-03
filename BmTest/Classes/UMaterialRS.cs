using System.Diagnostics;
using UELib;
using UELib.Engine;

/// <summary>
///     Implements UMaterial/Engine.Material customizations for RSS branch
/// </summary>
class UMaterialRS : UMaterial
{
    public UMaterialRS()
    {
        ShouldDeserializeOnDemand = true;
    }

    protected override void Deserialize()
    {
        // Ensure package build
        Debug.Assert(_Buffer.Package.Build == BuildGeneration.RSS);

        base.Deserialize();
    }
}
