using System.Diagnostics;
using UELib;
using UELib.Core;
using UELib.Engine;

/// <summary>
///     Implements UMaterial/Engine.Material customizations for RSS branch
/// </summary>
class BmMaterial : UMaterial
{
    public BmMaterial()
    {
        ShouldDeserializeOnDemand = true;
    }

    protected override void Deserialize()
    {
        // Ensure package build
        Debug.Assert(_Buffer.Package.Build == BuildGeneration.RSS);

        if (DeserializationState.HasFlag(ObjectState.Deserialied))
        {
            return;
        }

        // Deserialize UObject props
        base.Deserialize();

        if (_Buffer.Version >= 858)
        {
            _Buffer.ReadInt32(); // unkMask
        }

        if (_Buffer.Version >= 656)
        {
            _Buffer.ReadArray(out UArray<string> _); // f10

            // TODO: We have more fields after this (https://github.com/gildor2/UEViewer/blob/a0bfb468d42be831b126632fd8a0ae6b3614f981/Unreal/UnrealMaterial/UnTexture3.cpp#L1204C3-L1204C25)
            // Checking the array now helps us know if we've missed any properties.
        }
    }
}
