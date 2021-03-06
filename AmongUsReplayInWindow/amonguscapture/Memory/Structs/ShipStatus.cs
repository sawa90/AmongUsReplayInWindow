using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Drawing;

namespace AmongUsCapture
{
    static partial class Struct_2020_12_9s
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct v_ShipStatus
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
            [FieldOffset(0x40)] public UInt32 MapPrefab;
            [FieldOffset(0x44)] public UInt32 ExileCutscenePrefab;
            [FieldOffset(0x48)] public Vector2 InitialSpawnCenter;
            [FieldOffset(0x50)] public Vector2 MeetingSpawnCenter;
            [FieldOffset(0x58)] public Vector2 MeetingSpawnCenter2;
            [FieldOffset(0x60)] public float SpawnRadius;
            [FieldOffset(0x64)] public UInt32 CommonTasks;
            [FieldOffset(0x68)] public UInt32 LongTasks;
            [FieldOffset(0x6c)] public UInt32 NormalTasks;
            [FieldOffset(0x70)] public UInt32 SpecialTasks;
            [FieldOffset(0x74)] public UInt32 DummyLocations;
            [FieldOffset(0x78)] public UInt32 AllCameras;
            [FieldOffset(0x7c)] public UInt32 AllDoors;
            [FieldOffset(0x80)] public UInt32 AllConsoles;
            [FieldOffset(0x84)] public UInt32 Systems;
            [FieldOffset(0x88)] public UInt32 _DJLENIPKKGM_k__BackingField;
            [FieldOffset(0x8c)] public UInt32 _GCGACPGKENC_k__BackingField;
            [FieldOffset(0x90)] public UInt32 _dic_LJFDDJHBOGF_GCGACPGKENC_k__BackingField;
            [FieldOffset(0x94)] public UInt32 _OPPMFCFACJB_k__BackingField;
            [FieldOffset(0x98)] public UInt32 WeaponFires;
            [FieldOffset(0x9c)] public UInt32 WeaponsImage;
            [FieldOffset(0xa0)] public UInt32 VentMoveSounds;
            [FieldOffset(0xa4)] public UInt32 VentEnterSound;
            [FieldOffset(0xa8)] public UInt32 HatchActive;
            [FieldOffset(0xac)] public UInt32 Hatch;
            [FieldOffset(0xb0)] public UInt32 HatchParticles;
            [FieldOffset(0xb4)] public UInt32 ShieldsActive;
            [FieldOffset(0xb8)] public UInt32 ShieldsImages;
            [FieldOffset(0xbc)] public UInt32 ShieldBorder;
            [FieldOffset(0xc0)] public UInt32 ShieldBorderOn;
            [FieldOffset(0xc4)] public UInt32 MedScanner;
            [FieldOffset(0xc8)] public Int32 WeaponFireIdx;
            [FieldOffset(0xcc)] public float Timer;
            [FieldOffset(0xd0)] public float EmergencyCooldown;
            [FieldOffset(0xd4)] public Int32 MapType;
        }
    }

    static partial class Struct_2021_3_5s
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct v_ShipStatus
        {
            // ShipStatus in 2021.3.5s

            [FieldOffset(0x24)] public UInt32 CameraColor;
            [FieldOffset(0x34)] public float MaxLightRadius; // [marker]
            [FieldOffset(0x38)] public float MinLightRadius;
            [FieldOffset(0x3c)] public float MapScale;
            [FieldOffset(0x40)] public UInt32 MapPrefab;
            [FieldOffset(0x44)] public UInt32 ExileCutscenePrefab;
            [FieldOffset(0x48)] public UInt32 EmergencyOverlay;
            [FieldOffset(0x4c)] public UInt32 ReportOverlay;
            [FieldOffset(0x48)] public Vector2 InitialSpawnCenter;
            [FieldOffset(0x58)] public Vector2 MeetingSpawnCenter;
            [FieldOffset(0x60)] public Vector2 MeetingSpawnCenter2;
            [FieldOffset(0x68)] public float SpawnRadius;
            [FieldOffset(0x6c)] public UInt32 CommonTasks;
            [FieldOffset(0x70)] public UInt32 LongTasks;
            [FieldOffset(0x74)] public UInt32 NormalTasks;
            [FieldOffset(0x78)] public UInt32 SpecialTasks;
            [FieldOffset(0x7c)] public UInt32 DummyLocations;
            [FieldOffset(0x80)] public UInt32 AllCameras;
            [FieldOffset(0x84)] public UInt32 AllDoors;
            [FieldOffset(0x88)] public UInt32 AllConsoles;
            [FieldOffset(0x8c)] public UInt32 Systems;
            [FieldOffset(0x90)] public UInt32 SystemNames;
            [FieldOffset(0x94)] public UInt32 _DJLENIPKKGM_k__BackingField;
            [FieldOffset(0x98)] public UInt32 _GCGACPGKENC_k__BackingField;
            [FieldOffset(0x9c)] public UInt32 _dic_LJFDDJHBOGF_GCGACPGKENC_k__BackingField;
            [FieldOffset(0xa0)] public UInt32 _OPPMFCFACJB_k__BackingField;
            [FieldOffset(0xa4)] public UInt32 SabotageSound;
            [FieldOffset(0xa8)] public UInt32 WeaponFires;
            [FieldOffset(0xac)] public UInt32 WeaponsImage;
            [FieldOffset(0xb0)] public UInt32 VentMoveSounds;
            [FieldOffset(0xb4)] public UInt32 VentEnterSound;
            [FieldOffset(0xb8)] public UInt32 HatchActive;
            [FieldOffset(0xbc)] public UInt32 Hatch;
            [FieldOffset(0xc0)] public UInt32 HatchParticles;
            [FieldOffset(0xc4)] public UInt32 ShieldsActive;
            [FieldOffset(0xc8)] public UInt32 ShieldsImages;
            [FieldOffset(0xcc)] public UInt32 ShieldBorder;
            [FieldOffset(0xd0)] public UInt32 ShieldBorderOn;
            [FieldOffset(0xd4)] public UInt32 MedScanner;
            [FieldOffset(0xd8)] public Int32 WeaponFireIdx;
            [FieldOffset(0xdc)] public float Timer;
            [FieldOffset(0xe0)] public float EmergencyCooldown;
            [FieldOffset(0xe4)] public Int32 MapType;
        }
    }

    //EEHJPJEBAGP  in 2020.12.9s
    [StructLayout(LayoutKind.Explicit)]
    public struct Door
    {
        [FieldOffset(0x0c)] public SystemTypes Room;
        [FieldOffset(0x10)] public Int32 Id;
        [FieldOffset(0x14)] public bool Open;
        [FieldOffset(0x18)] public UInt32 myCollider;
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
                foreach(Door door in doors)
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
                for (int i = 0; i< doors.Length;i++)
                {
                    if (!doors[i].Open)
                        doorUint |= bit << i;
                }
                return doorUint;
            }
        }
    }
}
