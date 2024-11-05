using System.Diagnostics;
using UELib;
using UELib.Core;

/// <summary>
///     Implements USkeletalMesh/Engine.SkeletalMesh customizations for RSS branch
/// </summary>
partial class BmSkeletalMesh : UObject
{
    public static BmSkeletalMesh CurrentlyDeserializing = null;

    public FBoxSphereBounds Bounds;
    public UArray<UObject> Materials;

    public UVector MeshOrigin;
    public URotator RotOrigin;

    public UArray<FMeshBone> RefSkeleton;
    public int SkeletalDepth;

    public UArray<FStaticLODModel3> LODModels;

    public BmSkeletalMesh()
    {
        ShouldDeserializeOnDemand = true;
    }

    protected override void Deserialize()
    {
        CurrentlyDeserializing = this;
        base.Deserialize();

        // Ensure package build
        Debug.Assert(_Buffer.Package.Build == BuildGeneration.RSS);

        _Buffer.ReadStruct(out Bounds);

        if (
            (
                Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman1
                || Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman2
                || Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman3
                || Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman4
            )
            && Package.LicenseeVersion >= 15
        )
        {
            _Buffer.ReadFloat(); // conservativeBounds
            _Buffer.ReadArray(out UArray<FBoneBounds> _); // perBoneBounds
        }

        _Buffer.ReadArray(out Materials);

        _Buffer.ReadStruct(out MeshOrigin);
        _Buffer.ReadStruct(out RotOrigin);

        _Buffer.ReadArray(out RefSkeleton);
        _Buffer.Read(out SkeletalDepth);

        _Buffer.ReadArray(out LODModels);
    }
}
