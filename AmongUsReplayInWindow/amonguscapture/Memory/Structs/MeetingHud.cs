using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AmongUsCapture
{
    //OOCJALPKPEP
    [StructLayout(LayoutKind.Explicit)]
    public struct MeetingHud
    {
        [FieldOffset(0x08)] public UInt32 m_CachedPtr;
        [FieldOffset(0x0C)] public uint SpawnId;
        [FieldOffset(0x10)] public uint NetId;
        [FieldOffset(0x14)] public uint DirtyBits;
        [FieldOffset(0x18)] public uint SpawnFlags;
        [FieldOffset(0x19)] public uint sendMode;
        [FieldOffset(0x1C)] public uint OwnerId;
        [FieldOffset(0x20)] public byte NOGGNBJBALN;
        [FieldOffset(0x24)] public UInt32 ButtonParent;
        [FieldOffset(0x28)] public UInt32 TitleText;
        [FieldOffset(0x2C)] public uint VoteOrigin;
        [FieldOffset(0x38)] public uint VoteButtonOffsets;
        [FieldOffset(0x44)] public uint FGJMDFDIKEK;
        [FieldOffset(0x50)] public uint IOHLPLMJHIB;
        [FieldOffset(0x5C)] public UInt32 SkipVoteButton;
        [FieldOffset(0x60)] public UInt32 VoteInfoList;
        [FieldOffset(0x64)] public UInt32 PlayerButtonPrefab;
        [FieldOffset(0x68)] public UInt32 PlayerVotePrefab;
        [FieldOffset(0x6C)] public UInt32 CrackedGlass;
        [FieldOffset(0x70)] public UInt32 Glass;
        [FieldOffset(0x74)] public UInt32 ProceedButton;
        [FieldOffset(0x78)] public UInt32 VoteSound;
        [FieldOffset(0x7C)] public UInt32 VoteLockinSound;
        [FieldOffset(0x80)] public UInt32 VoteEndingSound;
        [FieldOffset(0x84)] public uint DCCFKHIPIOF;
        [FieldOffset(0x88)] public UInt32 SkippedVoting;
        [FieldOffset(0x8C)] public UInt32 HostIcon;
        [FieldOffset(0x90)] public UInt32 KillBackground;
        [FieldOffset(0x94)] public UInt32 LCJLLGKMINO;
        [FieldOffset(0x98)] public byte GEJDOOANNJD;
        [FieldOffset(0x9C)] public UInt32 TimerText;
        [FieldOffset(0xA0)] public float discussionTimer;
        [FieldOffset(0xA4)] public byte EANIGGGNMAF;
        [FieldOffset(0xA5)] public byte PIKBAKKGJEA;
        [FieldOffset(0xA8)] public float EKGJAHLFJFP;
        [FieldOffset(0xAC)] public uint NHPLGFPGAJL;
    }

    //HDJGDMFCHDN
    [StructLayout(LayoutKind.Explicit)]
    public struct VoteInfo
    {
        [FieldOffset(0x08)] public UInt32 m_CachedPtr;
        [FieldOffset(0x10)] public sbyte Id_;
        [FieldOffset(0x14)] public UInt32 Buttons;
        [FieldOffset(0x18)] public UInt32 PlayerIcon;
        [FieldOffset(0x1C)] public UInt32 Background;
        [FieldOffset(0x20)] public UInt32 Flag;
        [FieldOffset(0x24)] public UInt32 Megaphone;
        [FieldOffset(0x28)] public UInt32 Overlay;
        [FieldOffset(0x2C)] public UInt32 NameText;
        [FieldOffset(0x30)] public bool isDead;
        [FieldOffset(0x31)] public bool didVote;
        [FieldOffset(0x32)] public bool didReport;
        [FieldOffset(0x33)] public sbyte votedFor;
        [FieldOffset(0x34)] public bool voteComplete;
        [FieldOffset(0x35)] public bool resultsShowing;

    }
}
