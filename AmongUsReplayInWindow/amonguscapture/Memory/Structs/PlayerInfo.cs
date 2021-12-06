using System;
using System.Runtime.InteropServices;
using AUOffsetManager;

namespace AmongUsCapture
{
    public class PlayerInfo
    {
        public byte PlayerId;

        public string PlayerName;
        public PlayerData.PlayerColor ColorId;
        public uint HatId;
        public uint PetId;
        public uint SkinId;

        public uint PlayerLevel;
        public bool Disconnected;

        public RoleTypes RoleType;
        public RoleTeamTypes RoleTeamType;

        public IntPtr Tasks;
        public bool IsImpostor;
        public bool IsDead;

        public IntPtr _object; //Assume this always has largest offset

        public bool shaping;
        public PlayerData.PlayerColor shapeColorId;

        public PlayerInfo(IntPtr baseAddr, ProcessMemory MemInstance, GameOffsets CurrentOffsets)
        {
            unsafe
            {
                var baseAddrCopy = baseAddr;
                int last = MemInstance.OffsetAddress(ref baseAddrCopy, 0, 0);
                var intPtrSize = MemInstance.is64Bit ? 8 : 4;
                int size = ((int)Math.Ceiling((decimal)((intPtrSize + CurrentOffsets.PlayerInfoStructOffsets.ObjectOffset) / 8))) * 8; //Find the nearest multiple of 8
                byte[] buffer = MemInstance.ReadByteArray(baseAddrCopy + last, size);
                PlayerInfoStructOffsets pOf = CurrentOffsets.PlayerInfoStructOffsets;
                PlayerOutfitStructOffsets oOf = CurrentOffsets.PlayerOutfitStructOffsets;
                var outfit = MemInstance.Read<IntPtr>(baseAddrCopy, pOf.OutfitsOffset);
                if (buffer != null)
                {
                    fixed (byte* ptr = buffer)
                    {
                        var buffptr = (IntPtr)ptr;
                        PlayerId = Marshal.ReadByte(buffptr, pOf.PlayerIDOffset);
                        Disconnected = Marshal.ReadByte(buffptr, pOf.DisconnectedOffset) > 0;
                        Tasks = Marshal.ReadIntPtr(buffptr, pOf.TasksOffset);
                        IsDead = Marshal.ReadByte(buffptr, pOf.IsDeadOffset) > 0;
                        _object = Marshal.ReadIntPtr(buffptr, pOf.ObjectOffset);

                        // Read from Role
                        RoleType = (RoleTypes)MemInstance.Read<int>(baseAddrCopy, pOf.RoleTypeOffset);
                        RoleTeamType = (RoleTeamTypes)MemInstance.Read<int>(baseAddrCopy, pOf.RoleTeamTypeOffset);
                        IsImpostor = RoleTeamType == RoleTeamTypes.Impostor;

                        // Read from PlayerOutfit
                        PlayerName = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.PlayerNameOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                        ColorId = (PlayerData.PlayerColor)(uint)MemInstance.Read<int>(outfit, oOf.ColorIDOffset);
                        // TODO: Since IDs are changed from enum to string like "hat_police", renaming or mapping existing svgs to string is required
                        // TODO: As a workaround just fill with 0 as IDs
                        //HatId = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.HatIDOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                        //PetId = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.PetIDOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                        //SkinId = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.SkinIDOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                        HatId = 0;
                        PetId = 0;
                        SkinId = 0;
                    }
                    if (RoleType == RoleTypes.Shapeshifter) {
                        var outfitPtr = MemInstance.Read<IntPtr>(baseAddrCopy, pOf.OutfitsOffset[0], pOf.OutfitsOffset[1]);
                        var shape = MemInstance.ReadByteArray(outfitPtr + pOf.OutfitsOffset[2] + intPtrSize, intPtrSize * 4);
                        if (shape != null)
                        {
                            fixed (byte* ptr = shape)
                            {
                                var buffptr = (IntPtr)ptr;
                                var hash = Marshal.ReadIntPtr(buffptr, 0);
                                var key = Marshal.ReadIntPtr(buffptr, intPtrSize * 2);
                                shaping = (hash == key && hash == (IntPtr)1);
                                if (shaping)
                                {
                                    var outfit2 = Marshal.ReadIntPtr(buffptr, intPtrSize * 3);
                                    shapeColorId = (PlayerData.PlayerColor)(uint)MemInstance.Read<int>(outfit2, oOf.ColorIDOffset);
                                }
                                else
                                {
                                    shapeColorId = ColorId;
                                }
                            }
                        }
                    }
                }
            }
        }
        public string GetPlayerName()
        {
            return PlayerName;
        }
        public bool GetIsDead()
        {
            return IsDead;
        }

        public bool GetIsImposter()
        {
            return IsImpostor;
        }

        public PlayerData.PlayerColor GetPlayerColor()
        {
            return ColorId;
        }

        public bool GetIsDisconnected()
        {
            return Disconnected;
        }
    }
    /*
    public interface PlayerInfo
    {
        public abstract byte PlayerId { get; }
        public abstract IntPtr PlayerName { get; }
        public abstract byte ColorId { get; }
        public abstract uint HatId { get; }
        public abstract uint PetId { get; }
        public abstract uint SkinId { get; }
        public abstract byte Disconnected { get; }
        public abstract IntPtr Tasks { get; }
        public abstract byte IsImpostor { get; }
        public abstract byte IsDead { get; }
        public abstract IntPtr _object { get; }

        public bool GetIsDead()
        {
            return this.IsDead > 0;
        }

        public string GetPlayerName()
        {
            return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName, 0x10, 0x14);
        }

        public bool GetIsImposter()
        {
            return this.IsImpostor == 1;
        }

        public PlayerData.PlayerColor GetPlayerColor()
        {
            return (PlayerData.PlayerColor)this.ColorId;
        }

        public bool GetIsDisconnected()
        {
            return this.Disconnected > 0;
        }
    }
    static partial class Struct_2020_12_9s
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct v_PlayerInfo : PlayerInfo
        {
            [FieldOffset(0x08)] public byte PlayerId;
            [FieldOffset(0x0C)] public uint PlayerName;
            [FieldOffset(0x10)] public byte ColorId;
            [FieldOffset(0x14)] public uint HatId;
            [FieldOffset(0x18)] public uint PetId;
            [FieldOffset(0x1C)] public uint SkinId;
            [FieldOffset(0x20)] public byte Disconnected;
            [FieldOffset(0x24)] public IntPtr Tasks;
            [FieldOffset(0x28)] public byte IsImpostor;
            [FieldOffset(0x29)] public byte IsDead;
            [FieldOffset(0x2C)] public IntPtr _object;

            byte PlayerInfo.PlayerId => PlayerId;

            IntPtr PlayerInfo.PlayerName => (IntPtr)PlayerName;

            byte PlayerInfo.ColorId => ColorId;

            uint PlayerInfo.HatId => HatId;

            uint PlayerInfo.PetId => PetId;

            uint PlayerInfo.SkinId => SkinId;

            byte PlayerInfo.Disconnected => Disconnected;

            IntPtr PlayerInfo.Tasks => Tasks;

            byte PlayerInfo.IsImpostor => IsImpostor;

            byte PlayerInfo.IsDead => IsDead;

            IntPtr PlayerInfo._object => _object;

            public bool GetIsDead()
            {
                return this.IsDead > 0;
            }

            public string GetPlayerName()
            {
                if (this.PlayerName == 0) return null;
                return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName);
            }

            public PlayerData.PlayerColor GetPlayerColor()
            {
                return (PlayerData.PlayerColor)this.ColorId;
            }

            public bool GetIsDisconnected()
            {
                return this.Disconnected > 0;
            }
        }
    }


    static partial class Struct_2021_3_5s
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct v_PlayerInfo : PlayerInfo
        {
            [FieldOffset(0x08)] public byte PlayerId;
            [FieldOffset(0x0C)] public uint PlayerName;
            [FieldOffset(0x11)] public byte ColorId;
            [FieldOffset(0x14)] public uint HatId;
            [FieldOffset(0x18)] public uint PetId;
            [FieldOffset(0x1C)] public uint SkinId;
            [FieldOffset(0x20)] public byte Disconnected;
            [FieldOffset(0x24)] public IntPtr Tasks;
            [FieldOffset(0x28)] public byte IsImpostor;
            [FieldOffset(0x29)] public byte IsDead;
            [FieldOffset(0x2C)] public IntPtr _object;

            byte PlayerInfo.PlayerId => PlayerId;

            IntPtr PlayerInfo.PlayerName => (IntPtr)PlayerName;

            byte PlayerInfo.ColorId => ColorId;

            uint PlayerInfo.HatId => HatId;

            uint PlayerInfo.PetId => PetId;

            uint PlayerInfo.SkinId => SkinId;

            byte PlayerInfo.Disconnected => Disconnected;

            IntPtr PlayerInfo.Tasks => Tasks;

            byte PlayerInfo.IsImpostor => IsImpostor;

            byte PlayerInfo.IsDead => IsDead;

            IntPtr PlayerInfo._object => _object;


            public string GetPlayerName()
            {
                if (this.PlayerName == 0) return null;
                return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName);
            }

        }
    }

    static partial class Struct_2021_3_31_3s
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct v_PlayerInfo : PlayerInfo
        {
            [FieldOffset(0x08)] public byte PlayerId;
            [FieldOffset(0x0C)] public uint PlayerName;
            [FieldOffset(0x10)] public bool unknown;
            [FieldOffset(0x14)] public byte ColorId;
            [FieldOffset(0x18)] public uint HatId;
            [FieldOffset(0x1C)] public uint PetId;
            [FieldOffset(0x20)] public uint SkinId;
            [FieldOffset(0x24)] public byte Disconnected;
            [FieldOffset(0x28)] public IntPtr Tasks;
            [FieldOffset(0x2C)] public byte IsImpostor;
            [FieldOffset(0x2D)] public byte IsDead;
            [FieldOffset(0x30)] public IntPtr _object;

            byte PlayerInfo.PlayerId => PlayerId;

            IntPtr PlayerInfo.PlayerName => (IntPtr)PlayerName;

            byte PlayerInfo.ColorId => ColorId;

            uint PlayerInfo.HatId => HatId;

            uint PlayerInfo.PetId => PetId;

            uint PlayerInfo.SkinId => SkinId;

            byte PlayerInfo.Disconnected => Disconnected;

            IntPtr PlayerInfo.Tasks => Tasks;

            byte PlayerInfo.IsImpostor => IsImpostor;

            byte PlayerInfo.IsDead => IsDead;

            IntPtr PlayerInfo._object => _object;


            public string GetPlayerName()
            {
                if (this.PlayerName == 0) return null;
                return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName);
            }

        }
    }

    static partial class Struct_2021_6_15s
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct v_PlayerInfo : PlayerInfo
        {
            [FieldOffset(0x08)] public byte PlayerId;
            [FieldOffset(0x0C)] public uint PlayerName;
            [FieldOffset(0x10)] public bool unknown;
            [FieldOffset(0x14)] public byte ColorId;
            [FieldOffset(0x18)] public uint HatId;
            [FieldOffset(0x1C)] public uint PetId;
            [FieldOffset(0x20)] public uint SkinId;
            [FieldOffset(0x24)] public byte Disconnected;
            [FieldOffset(0x28)] public IntPtr Tasks;
            [FieldOffset(0x2C)] public byte IsImpostor;
            [FieldOffset(0x2D)] public byte IsDead;
            [FieldOffset(0x30)] public IntPtr _object;

            byte PlayerInfo.PlayerId => PlayerId;

            IntPtr PlayerInfo.PlayerName => (IntPtr)PlayerName;

            byte PlayerInfo.ColorId => ColorId;

            uint PlayerInfo.HatId => HatId;

            uint PlayerInfo.PetId => PetId;

            uint PlayerInfo.SkinId => SkinId;

            byte PlayerInfo.Disconnected => Disconnected;

            IntPtr PlayerInfo.Tasks => Tasks;

            byte PlayerInfo.IsImpostor => IsImpostor;

            byte PlayerInfo.IsDead => IsDead;

            IntPtr PlayerInfo._object => _object;


            public string GetPlayerName()
            {
                if (this.PlayerName == 0) return null;
                return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName);
            }

        }
    }*/
    public enum RoleTypes: uint
    {
        Crewmate = 0,
        Impostor = 1,
        Scientist = 2,
        Engineer = 3,
        GuardianAngel = 4,
        Shapeshifter = 5
    }

    public enum RoleTeamTypes : uint
    {
        Crewmate = 0,
        Impostor = 1
    }

}
