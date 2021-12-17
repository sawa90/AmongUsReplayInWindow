using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AmongUsCapture
{
    public static class ReadSpace
    {
        public static bool ExistReadSpace = false;
        public static IntPtr readSpacePtr;
        static byte MeetingNum = 0;
        static int ChatNum = 0;
        static int ReadChatNum = 0;
        public static bool chatExist { get { return ChatNum != ReadChatNum; } }


        [StructLayout(LayoutKind.Explicit)]
        public struct Space
        {
            [FieldOffset(0x00)] public byte MeetingNum; 
            [FieldOffset(0x01)] public sbyte Reporter; 
            [FieldOffset(0x02)] public sbyte Target; 
            [FieldOffset(0x03)] public byte ChatNum; 
        }

        public static bool setEmergency(ref int reporter, ref int target, in int[] IdList)
        {
            if (ExistReadSpace)
            {
                var space = ProcessMemory.getInstance().Read<Space>(readSpacePtr);
                if (IdInRange(space.Reporter)) reporter = IdList[space.Reporter];
                else reporter = -1;
                if(IdInRange(space.Target)) target = IdList[space.Target];
                else target = -1;
                ChatNum = space.ChatNum % 15;
                if (MeetingNum != space.MeetingNum)
                {
                    MeetingNum = space.MeetingNum;
                    return true;
                }
            }
            return false;
        }

        public static ChatMessageEventArgs readChat(int gameTimeMilli, in int[] IdList, in string[] PlayerNames)
        {
            if (!chatExist || !ExistReadSpace) return null;
            IntPtr chatPtr = readSpacePtr + 8 + 0x200 * ReadChatNum;
            ReadChatNum = (ReadChatNum + 1) % 15;
            ChatMessageEventArgs chat = new ChatMessageEventArgs() { time = gameTimeMilli };
            byte[] bytes = ProcessMemory.getInstance().ReadByteArray(chatPtr, 0x8);
            if (bytes != null) {
                int id = (sbyte)bytes[0];
                if (IdInRange(id)) chat.Sender = PlayerNames[IdList[id]];
                else chat.Sender = null;
                int len = BitConverter.ToInt32(bytes, 4);
                if (len > 0x1F8) len = 0x1F8;
                bytes = ProcessMemory.getInstance().ReadByteArray(chatPtr + 0x8, len);
                if (bytes != null) chat.Message = Encoding.UTF8.GetString(bytes);
            }
            return chat;
        }

        public static int getMurder(int target)
        {
            if (!ExistReadSpace || !IdInRange(target))
            {
                var targetPlayerId = GameMemReader.getInstance()?.PlayerIdList[target] ?? -1;
                if (IdInRange(targetPlayerId))
                {
                    int murder = ProcessMemory.getInstance().Read<sbyte>(readSpacePtr + 0x1E08 + targetPlayerId);
                    if (IdInRange(murder)) return GameMemReader.getInstance()?.IdList[murder] ?? -1;
                }
            }
            return -1;
        }

        static bool IdInRange(int id)
        {
            return id >= 0 && id < PlayerData.MaxPlayerNum;
        }
    }
}
