using System.Diagnostics;
using System.Runtime.InteropServices;
using BmTest.Helpers;
using UELib;
using UELib.Core;

/// <summary>
///     Implements USkeletalMesh/Engine.SkeletalMesh customizations for RSS branch
/// </summary>
class BmSkeletalMesh : UObject
{
    public static BmSkeletalMesh? CurrentlyDeserializing = null;

    public FBoxSphereBounds Bounds;
    public UArray<UObject>? Materials;

    public UVector MeshOrigin;
    public URotator RotOrigin;

    public UArray<FMeshBone>? RefSkeleton;
    public int SkeletalDepth;

    public UArray<FStaticLODModel3>? LODModels;

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
            _Buffer.Read(out float conservativeBounds); // ConservativeBounds
            _Buffer.ReadArray(out UArray<FBoneBounds> perBoneBounds); // PerBoneBounds
        }

        _Buffer.ReadArray(out Materials);

        _Buffer.ReadStruct(out MeshOrigin);
        _Buffer.ReadStruct(out RotOrigin);

        _Buffer.ReadArray(out RefSkeleton);
        _Buffer.Read(out SkeletalDepth);

        _Buffer.ReadArray(out LODModels);
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FStaticLODModel3 : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UArray<FSkelMeshSection3>? Sections;
        public UArray<FSkelMeshChunk3>? Chunks;
        public FSkelIndexBuffer3 IndexBuffer;

        public UArray<short>? UsedBones;

        public int NumVertices;
        public int NumUVSets;

        public UBulkData<ushort> BulkData;
        public UBulkData<int> BulkData2;
        public FSkeletalMeshVertexBuffer3 GPUSkin;

        public UArray<FSkeletalMeshVertexInfluences>? ExtraVertexInfluences;
        public UArray<UColor>? VertexColor;

        public void Deserialize(IUnrealStream stream)
        {
            stream.ReadArray(out Sections);
            stream.ReadStruct(out IndexBuffer);

            stream.ReadArray(out UsedBones);

            if (stream.Version >= 215)
            {
                stream.ReadArray(out Chunks);
                stream.ReadInt32(); // f80
                stream.Read(out NumVertices);
            }

            if (
                stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman4
                && stream.LicenseeVersion >= 159
            )
            {
                stream.ReadArray(out UArray<int> _); // f24_new
                goto bulk;
            }

            if (stream.Version >= 207)
            {
                stream.ReadArray(out UArray<byte> _); // f24
            }

            bulk:
            if (stream.Version >= 221)
            {
                if (stream.Version < 806)
                {
                    stream.Read(out BulkData);
                }
                else
                {
                    stream.Read(out BulkData2);
                }
            }

            // UV sets count
            NumUVSets = 1;

            if (stream.Version >= 709)
            {
                stream.Read(out NumUVSets);
            }

            if (stream.Version >= 333)
            {
                stream.ReadStruct(out GPUSkin);
            }

            if (stream.Version >= 710)
            {
                // https://github.com/gildor2/UEViewer/blob/a0bfb468d42be831b126632fd8a0ae6b3614f981/Unreal/UnrealMesh/UnMesh3.cpp#L1741C5-L1741C39

                // TODO: Don't use global state for this
                var skeletalMesh = BmSkeletalMesh.CurrentlyDeserializing;
                Debug.Assert(skeletalMesh != null);

                // IF we have vertex colors, read them
                var vertexColorsProp = skeletalMesh.Properties.Find("bHasVertexColors");
                if (vertexColorsProp?.BoolValue == true)
                {
                    stream.ReadArray(out VertexColor);
                }
            }

            // Post-UT3
            if (stream.Version >= 534)
            {
                stream.ReadArray(out ExtraVertexInfluences);
            }

            // Adjacency index buffer
            if (stream.Version >= 841)
            {
                stream.ReadArray(out UArray<FSkelIndexBuffer3> _); // unk
            }

            if (
                stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman4
                && stream.LicenseeVersion >= 107
            )
            {
                stream.ReadArray(out UArray<int> _); // unk
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FSkeletalMeshVertexBuffer3
        : IUnrealDeserializableClass,
            IUnrealAtomicStruct
    {
        public int NumUVSets;
        public int bUseFullPrecisionUVs; // 0 = half, 1 = float; copy of corresponding USkeletalMesh field
        public int bUsePackedPosition; // 1 = packed FVector (32-bit), 0 = FVector (96-bit)
        public UVector MeshOrigin;
        public UVector MeshExtension;

        public UArray<FGPUVert3Half>? VertsHalf;
        public UArray<FGPUVert3Float>? VertsFloat;

        public void Deserialize(IUnrealStream stream)
        {
            var isLoading = false; // TODO: Decide on a value for this

            if (isLoading)
            {
                bUsePackedPosition = 0;
            }

            var allowPackedPosition = false;
            NumUVSets = GNumGPUUVSets = 1;

            if (stream.Version >= 709)
            {
                stream.Read(out NumUVSets);
                GNumGPUUVSets = NumUVSets;
            }

            stream.Read(out bUseFullPrecisionUVs);
            if (stream.Version >= 592)
            {
                stream.Read(out bUsePackedPosition);
                stream.ReadStruct(out MeshExtension);
                stream.ReadStruct(out MeshOrigin);
            }

            // UE3 PC version ignored bUsePackedPosition - forced !bUsePackedPosition in FSkeletalMeshVertexBuffer3 serializer.
            // Note: in UDK (newer engine) there is no code to serialize GPU vertex with packed position.
            // Working bUsePackedPosition version was found in all XBox360 games. For PC there is only one game -
            // MOH2010, which uses bUsePackedPosition. PS3 also has bUsePackedPosition support (at least TRON)
            if (!allowPackedPosition)
            {
                bUsePackedPosition = 0; // not used in games (see comment above)
            }

            if (
                stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman4
                && stream.LicenseeVersion >= 192
            )
            {
                // https://github.com/gildor2/UEViewer/blob/a0bfb468d42be831b126632fd8a0ae6b3614f981/Unreal/UnrealMesh/UnMesh3.cpp#L1065
                throw new NotImplementedException();
            }

            if (bUseFullPrecisionUVs == 0)
            {
                stream.Skip(4);
                stream.ReadArray(out VertsHalf);
            }
            else
            {
                stream.Skip(4);
                stream.ReadArray(out VertsFloat);
            }

            if (
                stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman4
                && stream.LicenseeVersion >= 190
            )
            {
                stream.ReadArray(out UArray<UVector> _); // unk1
                stream.ReadInt32(); // unk2
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FSkeletalMeshVertexInfluences
        : IUnrealDeserializableClass,
            IUnrealAtomicStruct
    {
        public UArray<FVertexInfluence> Influences;
        public UArray<FVertexInfluenceMap> VertexInfluenceMapping;
        public UArray<FSkelMeshSection3> Sections;
        public UArray<FSkelMeshChunk3> Chunks;
        public UArray<byte> RequiredBones;
        public byte Usage;

        public void Deserialize(IUnrealStream stream)
        {
            stream.ReadArray(out Influences);
            if (stream.Version >= 609)
            {
                if (stream.Version >= 808)
                {
                    stream.ReadArray(out VertexInfluenceMapping);
                }
                else
                {
                    if (stream.Version >= 806)
                    {
                        stream.ReadByte(); // unk1
                    }

                    stream.ReadArray(out UArray<FVertexInfluenceMapOld> _); // vertexInfluenceMappingOld
                }
            }

            if (stream.Version >= 700)
            {
                stream.ReadArray(out Sections);
                stream.ReadArray(out Chunks);
            }

            if (stream.Version >= 708)
            {
                if (
                    stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman4
                    && stream.LicenseeVersion >= 159
                )
                {
                    stream.ReadArray(out UArray<int> _); // requiredBones32
                }
                else
                {
                    stream.ReadArray(out RequiredBones);
                }
            }

            if (stream.Version >= 715)
            {
                stream.Read(out Usage);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FVertexInfluence : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public int Weights;
        public int Boned;

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out Weights);
            stream.Read(out Boned);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FVertexInfluenceMap : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        // Key
        public int f0;
        public int f4;

        // Value
        public UArray<ushort> f8;

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out f0);
            stream.Read(out f4);
            stream.ReadArray(out f8);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FVertexInfluenceMapOld : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        // Key
        public int f0;
        public int f4;

        // Value
        public UArray<int> f8;

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out f0);
            stream.Read(out f4);
            stream.ReadArray(out f8);
        }
    }

    const int NUM_INFLUENCES_UE3 = 4;
    public static int GNumGPUUVSets = 1;

    private struct FGPUVert3Common
    {
        public FPackedNormal[] Normal;
        public byte[] BoneIndex;
        public byte[] BoneWeight;

        public static FGPUVert3Common Deserialize(IUnrealStream stream)
        {
            FGPUVert3Common vert;

            vert.Normal = new FPackedNormal[3];
            stream.ReadStruct(out vert.Normal[0]);
            stream.ReadStruct(out vert.Normal[2]);

            vert.BoneIndex = new byte[NUM_INFLUENCES_UE3];
            vert.BoneWeight = new byte[NUM_INFLUENCES_UE3];
            for (int i = 0; i < NUM_INFLUENCES_UE3; i++)
            {
                stream.Read(out vert.BoneIndex[i]);
            }
            for (int i = 0; i < NUM_INFLUENCES_UE3; i++)
            {
                stream.Read(out vert.BoneWeight[i]);
            }

            return vert;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FGPUVert3Half : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UVector Pos;
        public FMeshUVHalf[] UV;

        public FPackedNormal[] Normal;
        public byte[] BoneIndex;
        public byte[] BoneWeight;

        public void Deserialize(IUnrealStream stream)
        {
            var common = FGPUVert3Common.Deserialize(stream);
            Normal = common.Normal;
            BoneIndex = common.BoneIndex;
            BoneWeight = common.BoneWeight;

            stream.ReadStruct(out Pos);
            UV = new FMeshUVHalf[GNumGPUUVSets];
            for (int i = 0; i < UV.Length; i++)
            {
                stream.ReadStruct(out UV[i]);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FGPUVert3Float : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UVector Pos;
        public FMeshUVFloat[] UV;

        public FPackedNormal[] Normal;
        public byte[] BoneIndex;
        public byte[] BoneWeight;

        public void Deserialize(IUnrealStream stream)
        {
            var common = FGPUVert3Common.Deserialize(stream);
            Normal = common.Normal;
            BoneIndex = common.BoneIndex;
            BoneWeight = common.BoneWeight;

            stream.ReadStruct(out Pos);
            UV = new FMeshUVFloat[GNumGPUUVSets];
            for (int i = 0; i < UV.Length; i++)
            {
                stream.ReadStruct(out UV[i]);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FSkelMeshChunk3 : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public int FirstVertex;
        public UArray<FRigidVertex3> RigidVerts;
        public UArray<FSoftVertex3> SoftVerts;
        public UArray<short> Bones;
        public int NumRigidVerts;
        public int NumSoftVerts;
        public int MaxInfluences;

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out FirstVertex);
            stream.ReadArray(out RigidVerts);
            stream.ReadArray(out SoftVerts);

            stream.ReadArray(out Bones);
            if (stream.Version >= 333)
            {
                // NOTE: NumRigidVerts and NumSoftVerts may be non-zero while corresponding
                // arrays are empty - that's when GPU skin only left
                stream.Read(out NumRigidVerts);
                stream.Read(out NumSoftVerts);
            }

            if (stream.Version >= 362)
            {
                stream.Read(out MaxInfluences);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FSoftVertex3 : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UVector Pos;
        public FPackedNormal[] Normal;
        public FMeshUVFloat[] UV;

        public byte[] BoneIndex;
        public byte[] BoneWeight;
        public int Color;

        public void Deserialize(IUnrealStream stream)
        {
            stream.ReadStruct(out Pos);

            Normal = new FPackedNormal[3];
            for (int i = 0; i < 3; i++)
            {
                stream.ReadStruct(out Normal[i]);
            }

            int numUVSets = 1;
            if (stream.Version >= 709)
            {
                numUVSets = 4;
            }

            // UV
            UV = new FMeshUVFloat[numUVSets];
            for (int i = 0; i < numUVSets; i++)
            {
                stream.ReadStruct(out UV[i]);
            }

            if (stream.Version >= 710)
            {
                stream.Read(out Color);
            }

            if (stream.Version >= 333)
            {
                for (int i = 0; i < NUM_INFLUENCES_UE3; i++)
                {
                    stream.Read(out BoneIndex[i]);
                }
                for (int i = 0; i < NUM_INFLUENCES_UE3; i++)
                {
                    stream.Read(out BoneWeight[i]);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FRigidVertex3 : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UVector Pos;
        public FPackedNormal[] Normal;
        public FMeshUVFloat[] UV;

        public byte BoneIndex;
        public int Color;

        public void Deserialize(IUnrealStream stream)
        {
            stream.ReadStruct(out Pos);

            Normal = new FPackedNormal[3];
            for (int i = 0; i < 3; i++)
            {
                stream.ReadStruct(out Normal[i]);
            }

            int numUVSets = 1;
            if (stream.Version >= 709)
            {
                numUVSets = 4;
            }

            // UV
            UV = new FMeshUVFloat[numUVSets];
            for (int i = 0; i < numUVSets; i++)
            {
                stream.ReadStruct(out UV[i]);
            }

            if (stream.Version >= 710)
            {
                stream.Read(out Color);
            }

            stream.Read(out BoneIndex);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FMeshUVFloat : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public float U;
        public float V;

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out U);
            stream.Read(out V);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FMeshUVHalf : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public ushort U;
        public ushort V;

        public readonly UVector2D Vector => new() { X = U.HalfToFloat(), Y = V.HalfToFloat() };

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out U);
            stream.Read(out V);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FPackedNormal : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public uint Data;

        public readonly UVector Vector =>
            new()
            {
                // "x / 127.5 - 1" comes from Common.usf, TangentBias() macro
                X = (Data & 0xFF) / 127.5f - 1,
                Y = ((Data >> 8) & 0xFF) / 127.5f - 1,
                Z = ((Data >> 16) & 0xFF) / 127.5f - 1
            };

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out Data);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FSkelIndexBuffer3 : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UArray<ushort>? Indices16;
        public UArray<uint>? Indices32;

        public void Deserialize(IUnrealStream stream)
        {
            byte itemSize = 2;

            if (
                (
                    stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman2
                    || stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman3
                )
                && stream.Package.LicenseeVersion >= 45
            )
            {
                stream.ReadInt32(); // unk34

                // TODO: For some reason, we're still 4 bytes behind. Should test that this second skip
                // doesn't cause issues with other games/versions.
                stream.Skip(4);

                // https://github.com/gildor2/UEViewer/blob/a0bfb468d42be831b126632fd8a0ae6b3614f981/Unreal/UnrealMesh/UnMesh3.cpp#L395C4-L395C26
                goto old_index_buffer;
            }

            if (stream.Version >= 806)
            {
                stream.ReadInt32(); // f0
                stream.Read(out itemSize);
            }

            old_index_buffer:
            if (itemSize == 2)
            {
                stream.ReadArray(out Indices16);
            }
            else
            {
                Debug.Assert(itemSize == 4);
                stream.ReadArray(out Indices32);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FSkelMeshSection3 : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public short MaterialIndex;
        public short ChunkIndex;
        public int FirstIndex;
        public int NumTriangles;

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out MaterialIndex);
            stream.Read(out ChunkIndex);
            stream.Read(out FirstIndex);

            if (
                stream.Package.Build.Name == UnrealPackage.GameBuild.BuildName.Batman3
                || stream.Version < 806
            )
            {
                // NumTriangles is 16-bit here
                NumTriangles = stream.ReadUInt16();
            }
            else if (stream.Version >= 806)
            {
                NumTriangles = stream.ReadInt32();
            }

            if (stream.Version >= 599)
            {
                stream.ReadByte(); // unk2
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FBoxSphereBounds : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UVector Origin;
        public UVector BoxExtent;
        public float SphereRadius;

        public void Deserialize(IUnrealStream stream)
        {
            stream.ReadStruct(out Origin);
            stream.ReadStruct(out BoxExtent);
            stream.Read(out SphereRadius);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FBoneBounds : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public int BoneIndex;
        public UVector Min;
        public UVector Max;

        public void Deserialize(IUnrealStream stream)
        {
            stream.Read(out BoneIndex);
            stream.ReadStruct(out Min);
            stream.ReadStruct(out Max);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct FMeshBone : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UName Name;
        public uint Flags;
        public VJointPos BonePos;
        public int ParentIndex;
        public int NumChildren;

        public void Deserialize(IUnrealStream stream)
        {
            if (
                stream.Package.Build.Generation == BuildGeneration.RSS
                && stream.LicenseeVersion >= 31
            )
            {
                stream.ReadStruct(out BonePos);
                stream.Read(out Name);
                stream.Read(out ParentIndex);
            }

            if (stream.Version >= 515)
            {
                stream.ReadInt32(); // unk44
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct VJointPos : IUnrealDeserializableClass, IUnrealAtomicStruct
    {
        public UQuat Orientation;
        public UVector Position;

        public void Deserialize(IUnrealStream stream)
        {
            if (stream.Package.Version >= 224)
            {
                stream.ReadStruct(out Orientation);
                stream.ReadStruct(out Position);
            }
        }
    }
}
