using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Drawing;
using AUOffsetManager;

namespace AmongUsCapture
{
    public class ShipStatus
    {
        public IntPtr AllDoors;
        public float EmergencyCooldown;
        public IntPtr GapPlatform;
        public ShipStatus(IntPtr baseAddr, ProcessMemory MemInstance, GameOffsets CurrentOffsets)
        {
            var shipStatusPtr = MemInstance.Read<IntPtr>(baseAddr, CurrentOffsets.ShipStatusOffsets.ShipStatusPtr);
            AllDoors = MemInstance.Read<IntPtr>(shipStatusPtr, CurrentOffsets.ShipStatusOffsets.AllDoors);
            EmergencyCooldown = MemInstance.Read<float>(shipStatusPtr, CurrentOffsets.ShipStatusOffsets.EmergencyCooldown);
            GapPlatform = MemInstance.Read<IntPtr>(shipStatusPtr, CurrentOffsets.ShipStatusOffsets.GapPlatform);
        }
    }
    /*
    public interface ShipStatus
    {
        public abstract IntPtr AllDoors { get; }
        public abstract IntPtr GapPlatform { get; }
    }
    static partial class Struct_2020_12_9s
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct v_ShipStatus : ShipStatus
        {
            // FMDGHKGPBPP in 2020.10.8i
            // DAFPFFMKPJJ in 2020.10.22s
            // OLEKJGCEKAM in 2020.11.4s
            // EIEMHBCHLNI in 2020.11.17s
            // HLBNNHFCNAJ in 2020.12.9s

            [FieldOffset(0x24)] public UInt32 CameraColor;
            [FieldOffset(0x34)] public float MaxLightRadius; // [marker]
            [FieldOffset(0x38)] public float MinLightRadius;
            [FieldOffset(0x3c)] public float MapScale;
            [FieldOffset(0x40)] public IntPtr MapPrefab;
            [FieldOffset(0x44)] public IntPtr ExileCutscenePrefab;
            [FieldOffset(0x48)] public Vector2 InitialSpawnCenter;
            [FieldOffset(0x50)] public Vector2 MeetingSpawnCenter;
            [FieldOffset(0x58)] public Vector2 MeetingSpawnCenter2;
            [FieldOffset(0x60)] public float SpawnRadius;
            [FieldOffset(0x64)] public IntPtr CommonTasks;
            [FieldOffset(0x68)] public IntPtr LongTasks;
            [FieldOffset(0x6c)] public IntPtr NormalTasks;
            [FieldOffset(0x70)] public IntPtr SpecialTasks;
            [FieldOffset(0x74)] public IntPtr DummyLocations;
            [FieldOffset(0x78)] public IntPtr AllCameras;
            [FieldOffset(0x7c)] public IntPtr AllDoors;
            [FieldOffset(0x80)] public IntPtr AllConsoles;
            [FieldOffset(0x84)] public IntPtr Systems;
            [FieldOffset(0x88)] public IntPtr _DJLENIPKKGM_k__BackingField;
            [FieldOffset(0x8c)] public IntPtr _GCGACPGKENC_k__BackingField;
            [FieldOffset(0x90)] public IntPtr _dic_LJFDDJHBOGF_GCGACPGKENC_k__BackingField;
            [FieldOffset(0x94)] public IntPtr _OPPMFCFACJB_k__BackingField;
            [FieldOffset(0x98)] public IntPtr WeaponFires;
            [FieldOffset(0x9c)] public IntPtr WeaponsImage;
            [FieldOffset(0xa0)] public IntPtr VentMoveSounds;
            [FieldOffset(0xa4)] public IntPtr VentEnterSound;
            [FieldOffset(0xa8)] public IntPtr HatchActive;
            [FieldOffset(0xac)] public IntPtr Hatch;
            [FieldOffset(0xb0)] public IntPtr HatchParticles;
            [FieldOffset(0xb4)] public IntPtr ShieldsActive;
            [FieldOffset(0xb8)] public IntPtr ShieldsImages;
            [FieldOffset(0xbc)] public IntPtr ShieldBorder;
            [FieldOffset(0xc0)] public IntPtr ShieldBorderOn;
            [FieldOffset(0xc4)] public IntPtr MedScanner;
            [FieldOffset(0xc8)] public Int32 WeaponFireIdx;
            [FieldOffset(0xcc)] public float Timer;
            [FieldOffset(0xd0)] public float EmergencyCooldown;
            [FieldOffset(0xd4)] public Int32 MapType;

            IntPtr ShipStatus.AllDoors => AllDoors;
            IntPtr ShipStatus.GapPlatform => IntPtr.Zero;
        }
    }

    static partial class Struct_2021_3_5s
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct v_ShipStatus : ShipStatus
        {
            // ShipStatus in 2021.3.5s

            [FieldOffset(0x24)] public UInt32 CameraColor;
            [FieldOffset(0x34)] public float MaxLightRadius; // [marker]
            [FieldOffset(0x38)] public float MinLightRadius;
            [FieldOffset(0x3c)] public float MapScale;
            [FieldOffset(0x40)] public IntPtr MapPrefab;
            [FieldOffset(0x44)] public IntPtr ExileCutscenePrefab;
            [FieldOffset(0x48)] public IntPtr EmergencyOverlay;
            [FieldOffset(0x4c)] public IntPtr ReportOverlay;
            [FieldOffset(0x48)] public Vector2 InitialSpawnCenter;
            [FieldOffset(0x58)] public Vector2 MeetingSpawnCenter;
            [FieldOffset(0x60)] public Vector2 MeetingSpawnCenter2;
            [FieldOffset(0x68)] public float SpawnRadius;
            [FieldOffset(0x6c)] public IntPtr CommonTasks;
            [FieldOffset(0x70)] public IntPtr LongTasks;
            [FieldOffset(0x74)] public IntPtr NormalTasks;
            [FieldOffset(0x78)] public IntPtr SpecialTasks;
            [FieldOffset(0x7c)] public IntPtr DummyLocations;
            [FieldOffset(0x80)] public IntPtr AllCameras;
            [FieldOffset(0x84)] public IntPtr AllDoors;
            [FieldOffset(0x88)] public IntPtr AllConsoles;
            [FieldOffset(0x8c)] public IntPtr Systems;
            [FieldOffset(0x90)] public IntPtr SystemNames;
            [FieldOffset(0x94)] public IntPtr _DJLENIPKKGM_k__BackingField;
            [FieldOffset(0x98)] public IntPtr _GCGACPGKENC_k__BackingField;
            [FieldOffset(0x9c)] public IntPtr _dic_LJFDDJHBOGF_GCGACPGKENC_k__BackingField;
            [FieldOffset(0xa0)] public IntPtr _OPPMFCFACJB_k__BackingField;
            [FieldOffset(0xa4)] public IntPtr SabotageSound;
            [FieldOffset(0xa8)] public IntPtr WeaponFires;
            [FieldOffset(0xac)] public IntPtr WeaponsImage;
            [FieldOffset(0xb0)] public IntPtr VentMoveSounds;
            [FieldOffset(0xb4)] public IntPtr VentEnterSound;
            [FieldOffset(0xb8)] public IntPtr HatchActive;
            [FieldOffset(0xbc)] public IntPtr Hatch;
            [FieldOffset(0xc0)] public IntPtr HatchParticles;
            [FieldOffset(0xc4)] public IntPtr ShieldsActive;
            [FieldOffset(0xc8)] public IntPtr ShieldsImages;
            [FieldOffset(0xcc)] public IntPtr ShieldBorder;
            [FieldOffset(0xd0)] public IntPtr ShieldBorderOn;
            [FieldOffset(0xd4)] public IntPtr MedScanner;
            [FieldOffset(0xd8)] public Int32 WeaponFireIdx;
            [FieldOffset(0xdc)] public float Timer;
            [FieldOffset(0xe0)] public float EmergencyCooldown;
            [FieldOffset(0xe4)] public Int32 MapType;
            [FieldOffset(0xf8)] public IntPtr GapPlatform;

            IntPtr ShipStatus.AllDoors => AllDoors;
            IntPtr ShipStatus.GapPlatform => GapPlatform;
        }
    }

    static partial class Struct_2021_6_15s
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct v_ShipStatus : ShipStatus
        {
            // ShipStatus in 2021.3.5s

            [FieldOffset(0x24)] public UInt32 CameraColor;
            [FieldOffset(0x34)] public float MaxLightRadius; // [marker]
            [FieldOffset(0x38)] public float MinLightRadius;
            [FieldOffset(0x3c)] public float MapScale;
            [FieldOffset(0x40)] public IntPtr MapPrefab;
            [FieldOffset(0x44)] public IntPtr ExileCutscenePrefab;
            [FieldOffset(0x48)] public IntPtr EmergencyOverlay;
            [FieldOffset(0x4c)] public IntPtr ReportOverlay;
            [FieldOffset(0x48)] public Vector2 InitialSpawnCenter;
            [FieldOffset(0x58)] public Vector2 MeetingSpawnCenter;
            [FieldOffset(0x60)] public Vector2 MeetingSpawnCenter2;
            [FieldOffset(0x68)] public float SpawnRadius;
            [FieldOffset(0x6c)] public IntPtr CommonTasks;
            [FieldOffset(0x70)] public IntPtr LongTasks;
            [FieldOffset(0x74)] public IntPtr NormalTasks;
            [FieldOffset(0x78)] public IntPtr SpecialTasks;
            [FieldOffset(0x7c)] public IntPtr DummyLocations;
            [FieldOffset(0x80)] public IntPtr AllCameras;
            [FieldOffset(0x84)] public IntPtr AllDoors;
            [FieldOffset(0x88)] public IntPtr AllConsoles;
            [FieldOffset(0x8c)] public IntPtr Systems;
            [FieldOffset(0x90)] public IntPtr SystemNames;
            [FieldOffset(0x94)] public IntPtr _DJLENIPKKGM_k__BackingField;
            [FieldOffset(0x98)] public IntPtr _GCGACPGKENC_k__BackingField;
            [FieldOffset(0x9c)] public IntPtr _dic_LJFDDJHBOGF_GCGACPGKENC_k__BackingField;
            [FieldOffset(0xa0)] public IntPtr _OPPMFCFACJB_k__BackingField;
            [FieldOffset(0xa4)] public IntPtr SabotageSound;
            [FieldOffset(0xa8)] public IntPtr WeaponFires;
            [FieldOffset(0xac)] public IntPtr WeaponsImage;
            [FieldOffset(0xb0)] public IntPtr VentMoveSounds;
            [FieldOffset(0xb4)] public IntPtr VentEnterSound;
            [FieldOffset(0xb8)] public IntPtr HatchActive;
            [FieldOffset(0xbc)] public IntPtr Hatch;
            [FieldOffset(0xc0)] public IntPtr HatchParticles;
            [FieldOffset(0xc4)] public IntPtr ShieldsActive;
            [FieldOffset(0xc8)] public IntPtr ShieldsImages;
            [FieldOffset(0xcc)] public IntPtr ShieldBorder;
            [FieldOffset(0xd0)] public IntPtr ShieldBorderOn;
            [FieldOffset(0xd4)] public IntPtr MedScanner;
            [FieldOffset(0xd8)] public Int32 WeaponFireIdx;
            [FieldOffset(0xdc)] public float Timer;
            [FieldOffset(0xe0)] public float EmergencyCooldown;
            [FieldOffset(0xe4)] public Int32 MapType;
            [FieldOffset(0xf8)] public IntPtr GapPlatform;

            IntPtr ShipStatus.AllDoors => AllDoors;
            IntPtr ShipStatus.GapPlatform => GapPlatform;
        }
    }
    */

    //EEHJPJEBAGP  in 2020.12.9s
    [StructLayout(LayoutKind.Explicit)]
    public struct Door
    {
        [FieldOffset(0x0c)] public SystemTypes Room;
        [FieldOffset(0x10)] public Int32 Id;
        [FieldOffset(0x14)] public bool Open;
        [FieldOffset(0x18)] public IntPtr myCollider;
    }

    public class Doors
    {
        public static class skeld
        {
            public static float[][] Doors = new float[][]
            {
                new float[]{ -6.7f,   2.2f,  -6.7f,    0.4f},
                new float[]{ -5.6f,  -13.5f, -5.6f,  -15.2f},
                new float[]{ -15f,    2.2f,  -15f,    0.4f },
                new float[]{ -1.7f,  -4.6f,  -0.2f,  -4.6f },
                new float[]{ -15f,   -10.6f,   -15f,   -12.4f},
                new float[]{ -18.0f, -1.7f,  -16.6f, -1.7f },
                new float[]{ -15f,   -4.4f,  -15f,   -6.0f},
                new float[]{  0.9f,  -11.2f,  0.9f,  -13f },
                new float[]{  4.8f,   2.2f,   4.8f,    0.4f  },
                new float[]{ -10.5f, -13.5f, -9.1f,  -13.5f},
                new float[]{ -10.1f, -0.7f,  -8.7f,  -0.7f },
                new float[]{ -18.0f, -9.2f,  -16.6f, -9.2f },
                new float[]{ -1.7f,  -8.8f,  -0.2f,  -8.8f }
            };

            static Dictionary<SystemTypes, UInt32> room2Uint = new Dictionary<SystemTypes, UInt32>()
            {
                { SystemTypes.Cafeteria,    0b0000100001001 },
                { SystemTypes.MedBay,       0b0010000000000 },
                { SystemTypes.UpperEngine,  0b0000000100100 },
                { SystemTypes.Security,     0b0000001000000 },
                { SystemTypes.LowerEngine,  0b0100000010000 },
                { SystemTypes.Electrical,   0b0001000000000 },
                { SystemTypes.Storage,      0b1000010000010}
            };

            public static UInt32 Doors2Uint(Door[] doors)
            {
                UInt32 doorUint = 0;
                foreach (Door door in doors)
                {
                    if (!door.Open)
                        doorUint |= room2Uint.GetValueOrDefault(door.Room);
                }
                return doorUint;
            }
        }

        public static class polus
        {
            public static float[][] Doors = new float[][]
                {
                new float[]{ 11.1f, -8.3f,  11.1f, -10.3f},
                new float[]{  6.8f, -11.1f,  8.0f, -11.1f},
                new float[]{  4.8f, -13.6f,  5.8f, -13.6f},
                new float[]{  4.8f, -18.4f,  5.8f, -18.4f},
                new float[]{  5.2f, -22.6f,  6.3f, -22.6f},
                new float[]{ 12.4f, -20.9f, 13.4f, -20.9f},
                new float[]{ 10.2f, -19.4f, 11.3f, -19.4f},
                new float[]{ 28.6f, -16.2f, 28.6f, -17.7f},
                new float[]{ 17.2f, -20.9f, 17.2f, -22.4f},
                new float[]{ 25.9f, -8.9f,  26.9f, -8.9f},
                new float[]{ 24.7f, -8.7f,  24.7f, -10.2f},
                new float[]{ 17.2f, -10.0f, 17.2f, -11.5f},
                new float[]{ 25.3f, -23.7f, 25.3f, -25.3f},
                new float[]{ 23.1f, -23.8f, 24.3f, -23.8f},
                new float[]{ 37.8f, -8.9f,  37.8f, -10.4f},
                new float[]{ 38.2f, -11.5f, 39.5f, -11.5f},
                };

            public static UInt32 Doors2Uint(Door[] doors)
            {
                UInt32 doorUint = 0;
                UInt32 bit = 1;
                for (int i = 0; i < doors.Length; i++)
                {
                    if (!doors[i].Open)
                        doorUint |= bit << i;
                }
                return doorUint;
            }
        }

        public static class airship
        {
            public static float[][] Doors = new float[][]
                {
                new float[]{ -16.4f, -2f,  -16.4f,  0f},
                new float[]{  -8.6f, -2f,   -8.6f,  0f},
                new float[]{ -14f,   -0.3f,-12.8f, -0.3f},
                new float[]{ -11.7f, -2.5f,-10.3f, -2.5f},
                new float[]{  -1.6f,  7f,   -0.3f,  7f},
                new float[]{  -3.8f,  9.5f, -3.8f,  8f},
                new float[]{   2.6f,  9.5f,  2.6f,  8f},
                new float[]{  -8.9f, -8.2f, -8.9f, -6.7f},
                new float[]{  -8.9f, -13f,  -8.9f,-11.5f},
                new float[]{  -1.6f, -13f,  -1.6f,-11.5f},
                new float[]{   4.1f,  0.5f,   4.1f,-1f},
                new float[]{  17.5f,  0.5f,  17.5f, -1f},
                new float[]{  16.5f,  8f,    16.5f,  9.5f},
                new float[]{  23.8f,  8.5f,  23.8f, 10f},
                new float[]{  19f,    5.2f,  20.3f,  5.2f},
                new float[]{ 28.7f,   6.3f,  29.6f,  6.3f},
                new float[]{ 30.3f,   6.3f,  31.2f,  6.3f},
                new float[]{ 31.8f,   6.3f,  32.7f,  6.3f},
                new float[]{ 33.2f,   6.3f,  34.1f,  6.3f},
                new float[]{ 21.1f,  -7.9f,  21.1f, -9.3f},
                new float[]{ 31.8f,  -5f,    33.1f, -5f},
                new float[]{  6f,     8.5f,   7f,    8.5f},
                new float[]{  8.5f,   8.5f,    9.5f,   8.5f},
                };

            public static UInt32 Doors2Uint(Door[] doors, bool IsLeft)
            {
                UInt32 doorUint = 0;
                UInt32 bit = 1;
                for (int i = 0; i < doors.Length; i++)
                {
                    if (!doors[i].Open)
                        doorUint |= bit << i;
                }
                if (IsLeft)
                    doorUint |= bit << doors.Length;
                else
                    doorUint |= bit << (doors.Length + 1);
                return doorUint;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct MovingPlatformBehaviour
            {
                [FieldOffset(0x0c)] public Vector3 LeftPositon;
                [FieldOffset(0x18)] public Vector3 RightPositon;
                [FieldOffset(0x40)] public bool IsLeft;
            }
        }

    }
}
