using System;
using System.Runtime.InteropServices;
using System.Numerics;
using AUOffsetManager;


namespace AmongUsCapture
{
    public class PlayerControl
    {
        public uint NetId;
        public byte PlayerId;
        public bool inVent;
        public bool protectedByGuardian;
        public int RemainingEmergencies;
        public IntPtr nameText;
        public UInt32 myLight_;
        public IntPtr NetTransform;
        public IntPtr myTasks;

        public PlayerControl(IntPtr _objectPtr, ProcessMemory MemInstance, GameOffsets CurrentOffsets)
        {
            PlayerControlOffsets offsets = CurrentOffsets.PlayerControlOffsets;
            var intPtrSize = MemInstance.is64Bit ? 8 : 4;
            int size = ((int)Math.Ceiling((decimal)((intPtrSize + offsets.myTasks) / 8.0))) * 8; //Find the nearest multiple of 8
            byte[] buffer = MemInstance.ReadByteArray(_objectPtr, size);
            if (buffer != null)
            {
                NetId = BitConverter.ToUInt32(buffer, offsets.NetId);
                PlayerId = buffer[offsets.PlayerId];
                inVent = BitConverter.ToBoolean(buffer, offsets.inVent);
                protectedByGuardian = BitConverter.ToBoolean(buffer, offsets.protectedByGuardian);
                RemainingEmergencies = BitConverter.ToInt32(buffer, offsets.RemainingEmergencies);
                myLight_ = BitConverter.ToUInt32(buffer, offsets.myLight_);
                if (MemInstance.is64Bit)
                {
                    nameText = (IntPtr)BitConverter.ToInt64(buffer, offsets.nameText);
                    NetTransform = (IntPtr)BitConverter.ToInt64(buffer, offsets.NetTransform);
                    myTasks = (IntPtr)BitConverter.ToInt64(buffer, offsets.myTasks);
                }
                else
                {
                    nameText = (IntPtr)BitConverter.ToInt32(buffer, offsets.nameText);
                    NetTransform = (IntPtr)BitConverter.ToInt32(buffer, offsets.NetTransform);
                    myTasks = (IntPtr)BitConverter.ToInt32(buffer, offsets.myTasks);
                }
            }
        }
    }
    /*
    public interface PlayerControl
    {
        public abstract uint NetId { get; }
        public abstract uint sendMode { get; }
        public abstract uint OwnerId { get; }
        public abstract byte PlayerId { get; }
        public abstract float MaxReportDistance { get; }
        public abstract bool moveable { get; }
        public abstract bool inVent { get; }
        public abstract float killTimer { get; }
        public abstract int RemainingEmergencies { get; }
        public abstract IntPtr nameText { get; }
        public abstract IntPtr LightPrefab { get; }
        public abstract UInt32 myLight_ { get; }
        public abstract IntPtr Collider { get; }
        public abstract IntPtr MyPhysics { get; }
        public abstract IntPtr NetTransform { get; }
        public abstract IntPtr CurrentPet { get; }
        public abstract IntPtr myTasks { get; }
        public abstract IntPtr closest_ { get; }
        public abstract byte isNew_ { get; }
        public abstract IntPtr cache_ { get; }
        public abstract IntPtr itemsInRange_ { get; }
        public abstract IntPtr newItemsInRange_ { get; }
        public abstract byte scannerCount_ { get; }
        public abstract bool infectedSet { get; }
    }

    static partial class Struct_2020_12_9s
    {
        //FFGALNAPKCD in 2020.12.9s dissect mono
        //後ろに_がついている変数はNOGGNBJBALNのような変数名をamongus_replay_modのものに対応させた
        [StructLayout(LayoutKind.Explicit)]
        public struct v_PlayerControl : PlayerControl
        {
            [FieldOffset(0x00)] public IntPtr LocalPlayer;
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
            [FieldOffset(0x34)] public IntPtr _cachedData_;
            [FieldOffset(0x38)] public IntPtr FootSteps;
            [FieldOffset(0x3C)] public IntPtr KillSfx;
            [FieldOffset(0x40)] public IntPtr KillAnimations;
            [FieldOffset(0x44)] public float killTimer;
            [FieldOffset(0x48)] public int RemainingEmergencies;
            [FieldOffset(0x4C)] public IntPtr nameText;
            [FieldOffset(0x50)] public IntPtr LightPrefab;
            [FieldOffset(0x54)] public UInt32 myLight_;
            [FieldOffset(0x58)] public IntPtr Collider;
            [FieldOffset(0x5C)] public IntPtr MyPhysics;
            [FieldOffset(0x60)] public IntPtr NetTransform;
            [FieldOffset(0x64)] public IntPtr CurrentPet;
            [FieldOffset(0x68)] public IntPtr HatRenderer;
            [FieldOffset(0x6C)] public IntPtr myRend_;
            [FieldOffset(0x70)] public IntPtr hitBuffer_;
            [FieldOffset(0x74)] public IntPtr myTasks;
            [FieldOffset(0x78)] public IntPtr ScannerAnims;
            [FieldOffset(0x7C)] public IntPtr ScannersImages;
            [FieldOffset(0x80)] public IntPtr closest_;
            [FieldOffset(0x84)] public byte isNew_;
            [FieldOffset(0x88)] public IntPtr cache_;
            [FieldOffset(0x8C)] public IntPtr itemsInRange_;
            [FieldOffset(0x90)] public IntPtr newItemsInRange_;
            [FieldOffset(0x94)] public byte scannerCount_;
            [FieldOffset(0x95)] public bool infectedSet;

            uint PlayerControl.NetId => NetId;
            uint PlayerControl.sendMode => sendMode;
            uint PlayerControl.OwnerId => OwnerId;
            byte PlayerControl.PlayerId => PlayerId;
            float PlayerControl.MaxReportDistance => MaxReportDistance;
            bool PlayerControl.moveable => moveable;
            bool PlayerControl.inVent => inVent;
            float PlayerControl.killTimer => killTimer;
            int PlayerControl.RemainingEmergencies => RemainingEmergencies;
            IntPtr PlayerControl.nameText => nameText;
            IntPtr PlayerControl.LightPrefab => LightPrefab;
            UInt32 PlayerControl.myLight_ => myLight_;
            IntPtr PlayerControl.Collider => Collider;
            IntPtr PlayerControl.MyPhysics => MyPhysics;
            IntPtr PlayerControl.NetTransform => NetTransform;
            IntPtr PlayerControl.CurrentPet => CurrentPet;
            IntPtr PlayerControl.myTasks => myTasks;
            IntPtr PlayerControl.closest_ => closest_;
            byte PlayerControl.isNew_ => isNew_;
            IntPtr PlayerControl.cache_ => cache_;
            IntPtr PlayerControl.itemsInRange_ => itemsInRange_;
            IntPtr PlayerControl.newItemsInRange_ => newItemsInRange_;
            byte PlayerControl.scannerCount_ => scannerCount_;
            bool PlayerControl.infectedSet => infectedSet;
        }
    }


    static partial class Struct_2021_3_5s
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct v_PlayerControl : PlayerControl
        {
            [FieldOffset(0x00)] public IntPtr LocalPlayer;
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
            [FieldOffset(0x34)] public IntPtr _cachedData_;
            [FieldOffset(0x38)] public IntPtr FootSteps;
            [FieldOffset(0x3C)] public IntPtr KillSfx;
            [FieldOffset(0x40)] public IntPtr KillAnimations;
            [FieldOffset(0x44)] public float killTimer;
            [FieldOffset(0x48)] public int RemainingEmergencies;
            [FieldOffset(0x4C)] public IntPtr nameText;
            [FieldOffset(0x50)] public IntPtr LightPrefab;
            [FieldOffset(0x54)] public UInt32 myLight_;
            [FieldOffset(0x58)] public IntPtr textTranslator;
            [FieldOffset(0x5C)] public IntPtr Collider;
            [FieldOffset(0x60)] public IntPtr MyPhysics;
            [FieldOffset(0x64)] public IntPtr NetTransform;
            [FieldOffset(0x68)] public IntPtr CurrentPet;
            [FieldOffset(0x6C)] public IntPtr HatRenderer;
            [FieldOffset(0x70)] public IntPtr myRend_;
            [FieldOffset(0x74)] public IntPtr hitBuffer_;
            [FieldOffset(0x78)] public IntPtr myTasks;
            [FieldOffset(0x7C)] public IntPtr ScannerAnims;
            [FieldOffset(0x80)] public IntPtr ScannersImages;
            [FieldOffset(0x84)] public IntPtr closest_;
            [FieldOffset(0x88)] public byte isNew_;
            [FieldOffset(0x8C)] public IntPtr cache_;
            [FieldOffset(0x90)] public IntPtr itemsInRange_;
            [FieldOffset(0x94)] public IntPtr newItemsInRange_;
            [FieldOffset(0x98)] public byte scannerCount_;
            [FieldOffset(0x99)] public bool infectedSet;

            uint PlayerControl.NetId => NetId;
            uint PlayerControl.sendMode => sendMode;
            uint PlayerControl.OwnerId => OwnerId;
            byte PlayerControl.PlayerId => PlayerId;
            float PlayerControl.MaxReportDistance => MaxReportDistance;
            bool PlayerControl.moveable => moveable;
            bool PlayerControl.inVent => inVent;
            float PlayerControl.killTimer => killTimer;
            int PlayerControl.RemainingEmergencies => RemainingEmergencies;
            IntPtr PlayerControl.nameText => nameText;
            IntPtr PlayerControl.LightPrefab => LightPrefab;
            UInt32 PlayerControl.myLight_ => myLight_;
            IntPtr PlayerControl.Collider => Collider;
            IntPtr PlayerControl.MyPhysics => MyPhysics;
            IntPtr PlayerControl.NetTransform => NetTransform;
            IntPtr PlayerControl.CurrentPet => CurrentPet;
            IntPtr PlayerControl.myTasks => myTasks;
            IntPtr PlayerControl.closest_ => closest_;
            byte PlayerControl.isNew_ => isNew_;
            IntPtr PlayerControl.cache_ => cache_;
            IntPtr PlayerControl.itemsInRange_ => itemsInRange_;
            IntPtr PlayerControl.newItemsInRange_ => newItemsInRange_;
            byte PlayerControl.scannerCount_ => scannerCount_;
            bool PlayerControl.infectedSet => infectedSet;
        }
    }

    static partial class Struct_2021_6_15s
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct v_PlayerControl : PlayerControl
        {
            [FieldOffset(0x00)] public IntPtr LocalPlayer;
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
            [FieldOffset(0x34)] public IntPtr _cachedData_;
            [FieldOffset(0x38)] public IntPtr FootSteps;
            [FieldOffset(0x3C)] public IntPtr KillSfx;
            [FieldOffset(0x40)] public IntPtr KillAnimations;
            [FieldOffset(0x44)] public float killTimer;
            [FieldOffset(0x48)] public int RemainingEmergencies;
            [FieldOffset(0x4C)] public IntPtr nameText;
            [FieldOffset(0x50)] public IntPtr LightPrefab;
            [FieldOffset(0x54)] public UInt32 myLight_;
            [FieldOffset(0x58)] public IntPtr Collider;
            [FieldOffset(0x5C)] public IntPtr MyPhysics;
            [FieldOffset(0x60)] public IntPtr NetTransform;
            [FieldOffset(0x64)] public IntPtr CurrentPet;
            [FieldOffset(0x68)] public IntPtr HatRenderer;
            [FieldOffset(0x6C)] public IntPtr myRend_;
            [FieldOffset(0x70)] public IntPtr MyAnim;
            [FieldOffset(0x74)] public IntPtr hitBuffer_;
            [FieldOffset(0x78)] public IntPtr myTasks;
            [FieldOffset(0x7C)] public IntPtr ScannerAnims;
            [FieldOffset(0x80)] public IntPtr ScannersImages;
            [FieldOffset(0x84)] public IntPtr closest_;
            [FieldOffset(0x88)] public byte isNew_;
            [FieldOffset(0x8C)] public IntPtr cache_;
            [FieldOffset(0x90)] public IntPtr itemsInRange_;
            [FieldOffset(0x94)] public IntPtr newItemsInRange_;
            [FieldOffset(0x98)] public byte scannerCount_;
            [FieldOffset(0x99)] public bool infectedSet;

            uint PlayerControl.NetId => NetId;
            uint PlayerControl.sendMode => sendMode;
            uint PlayerControl.OwnerId => OwnerId;
            byte PlayerControl.PlayerId => PlayerId;
            float PlayerControl.MaxReportDistance => MaxReportDistance;
            bool PlayerControl.moveable => moveable;
            bool PlayerControl.inVent => inVent;
            float PlayerControl.killTimer => killTimer;
            int PlayerControl.RemainingEmergencies => RemainingEmergencies;
            IntPtr PlayerControl.nameText => nameText;
            IntPtr PlayerControl.LightPrefab => LightPrefab;
            UInt32 PlayerControl.myLight_ => myLight_;
            IntPtr PlayerControl.Collider => Collider;
            IntPtr PlayerControl.MyPhysics => MyPhysics;
            IntPtr PlayerControl.NetTransform => NetTransform;
            IntPtr PlayerControl.CurrentPet => CurrentPet;
            IntPtr PlayerControl.myTasks => myTasks;
            IntPtr PlayerControl.closest_ => closest_;
            byte PlayerControl.isNew_ => isNew_;
            IntPtr PlayerControl.cache_ => cache_;
            IntPtr PlayerControl.itemsInRange_ => itemsInRange_;
            IntPtr PlayerControl.newItemsInRange_ => newItemsInRange_;
            byte PlayerControl.scannerCount_ => scannerCount_;
            bool PlayerControl.infectedSet => infectedSet;
        }
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
    */
}
