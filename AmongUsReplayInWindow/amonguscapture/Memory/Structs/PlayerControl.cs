using System;
using System.Runtime.InteropServices;
using System.Numerics;



namespace AmongUsCapture
{
    //FFGALNAPKCD in 2020.12.9s dissect mono
    //後ろに_がついている変数はNOGGNBJBALNのような変数名をamongus_replay_modのものに対応させた
    [StructLayout(LayoutKind.Explicit)]
    public struct PlayerControl
    {
        [FieldOffset(0x00)] public UInt32 LocalPlayer;
        [FieldOffset(0x0C)] public uint SpawnId;
        [FieldOffset(0x10)] public uint NetId;
        [FieldOffset(0x14)] public uint DirtyBits;
        [FieldOffset(0x18)] public byte SpawnFlags;
        [FieldOffset(0x19)] public uint sendMode;
        [FieldOffset(0x1C)] public uint OwnerId;
        [FieldOffset(0x20)] public byte NOGGNBJBALN;
        [FieldOffset(0x24)] public int MKMDLEOKDIN;
        [FieldOffset(0x28)] public byte PlayerId;
        [FieldOffset(0x2C)] public float MaxReportDistance;
        [FieldOffset(0x30)] public bool moveable;
        [FieldOffset(0x31)] public bool inVent;
        [FieldOffset(0x34)] public UInt32 _cachedData_;
        [FieldOffset(0x38)] public UInt32 FootSteps;
        [FieldOffset(0x3C)] public UInt32 KillSfx;
        [FieldOffset(0x40)] public UInt32 KillAnimations;
        [FieldOffset(0x44)] public float killTimer;
        [FieldOffset(0x48)] public int RemainingEmergencies;
        [FieldOffset(0x4C)] public UInt32 nameText;
        [FieldOffset(0x50)] public UInt32 LightPrefab;
        [FieldOffset(0x54)] public UInt32 myLight_;
        [FieldOffset(0x58)] public UInt32 Collider;
        [FieldOffset(0x5C)] public UInt32 MyPhysics;
        [FieldOffset(0x60)] public UInt32 NetTransform;
        [FieldOffset(0x64)] public UInt32 CurrentPet;
        [FieldOffset(0x68)] public UInt32 HatRenderer;
        [FieldOffset(0x6C)] public UInt32 myRend_;
        [FieldOffset(0x70)] public UInt32 hitBuffer_; 
        [FieldOffset(0x74)] public UInt32 myTasks; 
        [FieldOffset(0x78)] public UInt32 ScannerAnims;
        [FieldOffset(0x7C)] public UInt32 ScannersImages;
        [FieldOffset(0x80)] public UInt32 closest_;
        [FieldOffset(0x84)] public byte isNew_;
        [FieldOffset(0x88)] public UInt32 cache_;
        [FieldOffset(0x8C)] public UInt32 itemsInRange_;
        [FieldOffset(0x90)] public UInt32 newItemsInRange_;
        [FieldOffset(0x94)] public byte scannerCount_;
        [FieldOffset(0x95)] public bool infectedSet;
    }

    //
    [StructLayout(LayoutKind.Explicit)]
    struct CustomNetworkTransform
    {
        [FieldOffset(0x3c)] public Vector2 targetSyncPosition_;
        [FieldOffset(0x44)] public Vector2 targetSyncVelocity_;
        [FieldOffset(0x50)] public Vector2 prevPosSent_;
        [FieldOffset(0x58)] public Vector2 prevVelSent_;
    }

}
