using System.Diagnostics;
using UELib;
using UELib.Core;
using UELib.Engine;

/// <summary>
///     Implements UTexture2D/Engine.Texture2D customizations for RSS branch
/// </summary>
class BmTexture2D : UTexture2D
{
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

        // Fix property names per https://github.com/gildor2/UEViewer/blob/a0bfb468d42be831b126632fd8a0ae6b3614f981/Unreal/UnObject.cpp#L686
        Properties.Find("self[0x0E0]").Name = new UName("SizeX");
        Properties.Find("self[0x0E4]").Name = new UName("SizeY");
        Properties.Find("self[0x104]").Name = new UName("TextureFileCacheName");
        Properties.Find("self[0x0E8]").Name = new UName("OriginalSizeX");
        Properties.Find("self[0x0EC]").Name = new UName("OriginalSizeY");
    }
}
