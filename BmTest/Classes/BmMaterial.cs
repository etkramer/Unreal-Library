using System.Diagnostics;
using UELib;
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
        base.Deserialize();

        // Ensure package build
        Debug.Assert(_Buffer.Package.Build == BuildGeneration.RSS);

        if (DeserializationState.HasFlag(ObjectState.Deserialied))
        {
            return;
        }

        /*var inner = Package.Exports.Where(o => o.Outer == ExportTable);
        foreach (var e in inner)
        {
            Console.WriteLine(e.GetReferencePath());
        }*/

        /*if (_Buffer.Version > 656)
        {
            // Starting with version 656 UE3 has deprecated ReferencedTextures array.
            // This array is serialized inside FMaterialResource which is not needed
            // for us in other case.
            // FMaterialResource serialization is below

            // Current theory:
            //   Tag name is actually struct name
            //     * These actually are simple properties, not named properties
            //     * This is how we can have i.e. multiple SpecularPower2 entries
            //     * Confirmed a bit by BoolProperty offsets being constant and StructProperty offsets varying
            //   Struct data is stored at the tag's offset

            int[] offsets =  [ 4, 25, 46, 67, 88, 109, 139, 169, 199, 229, 259, 289, 319, 349 ];
            for (int i = 0; i < offsets.Length; i++)
            {
                if (i == 5)
                {
                    Console.Write("\n");
                }

                var pos = offsets[i];
                var nextPos = (offsets.Length > i + 1) ? offsets[i + 1] : offsets[i];

                _Buffer.Position = pos;

                var type = (PropertyType)_Buffer.ReadInt16();
                var offset = _Buffer.ReadUInt16();

                // What is offset relative to? End of header?

                Console.WriteLine($"{pos}:");
                Console.WriteLine($"  tag type: {type}");

                bool isSimpleType = type == PropertyType.StructProperty;
                if (isSimpleType)
                {
                    var name = new UName($"self[0x{offset:X3}]");
                    Console.WriteLine($"  tag offset: {offset}");
                    Console.WriteLine($"  tag name: {name}");
                }

                if (type == PropertyType.StructProperty)
                {
                    var structName = _Buffer.ReadNameReference();
                    Console.WriteLine($"  struct name: {structName}");

                    // Where is the SpecularColor2 struct defined?

                    var unk = _Buffer.ReadInt32();
                    Console.WriteLine($"  unk: {unk}"); // size?

                    // NOTE: This is definitely incorrect.
                    // We have a property that starts at 199, but another property has an offset of 200.
                    var ptr = ExportTable.SerialOffset + offset;
                    _Buffer.AbsolutePosition = ptr;
                    Console.WriteLine($"  data at 0x{ptr:X3}");

                    Console.Write("    ");
                    for (int j = 0; j < 12; j++)
                    {
                        Console.Write($" {_Buffer.ReadByte():X2}");
                    }
                    Console.Write("\n");
                }
                else
                {
                    var name = _Buffer.ReadNameReference();
                    var size = _Buffer.ReadInt32();
                    var arrayIndex = _Buffer.ReadInt32();

                    Console.WriteLine($"  tag name: {name}");
                    Console.WriteLine($"  tag size: {size:0b}");
                    Console.WriteLine(
                        $"  actual size: {(nextPos == pos ? "unknown" : (nextPos - pos).ToString("0b"))}"
                    );
                }
            }

            // From 0, for Batman_Body_Master_MAT:
            // 8: bSpecularBlinnPhong (21b)
            // 29: bSpecularConserveEnergy (21b)
            // 50: bSpecularMaskByShading (21b)
            // 71: bUsedWithStaticMesh (21b)
            // 92: bUsedWithSkeletalMesh (21b)
            // 113: DiffuseColor (30b)
            //     Did we miss an offset/unnamed property?
            //     Or texture references just bigger
            // 143: SpecularColor (30b)
            // 173: SpecularPower (30b)
            // 203: Normal (30b)
            // 233: FresnelMin (30b)
            // 263: FresnelExponent (30b)
            // 293: SpecularColor2 (30b)
            // 323: SpecularPower2 (30b)
            // 353: TwoSidedLightingMask
        }

        Console.WriteLine($"\nDeserializing {Name}...");*/
    }
}
