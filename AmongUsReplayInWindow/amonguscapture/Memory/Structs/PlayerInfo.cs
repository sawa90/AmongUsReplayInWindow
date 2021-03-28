using System;
using System.Runtime.InteropServices;

namespace AmongUsCapture
{
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

}
