using System;
using System.Runtime.InteropServices;
using AUOffsetManager;

namespace AmongUsCapture
{
    public class TaskInfo
    {
        public SystemTypes StartAt;
        public TaskTypes TaskType;
        public UInt32 MinigamePrefab;
        public bool HasLocatin;
        public bool LocatinDirty;
        public byte TaskStep;
        public byte AllStepNum;
        public bool set(IntPtr _objectPtr, ProcessMemory MemInstance, GameOffsets CurrentOffsets)
        {
            if (_objectPtr == IntPtr.Zero) return false;
            TaskInfoOffsets offsets = CurrentOffsets.TaskInfoOffsets;
            var intPtrSize = MemInstance.is64Bit ? 8 : 4;
            int size = ((int)Math.Ceiling((decimal)((intPtrSize + offsets.AllStepNum) / 8.0))) * 8; //Find the nearest multiple of 8
            byte[] buffer = MemInstance.ReadByteArray(_objectPtr, size);
            if (buffer != null)
            {
                StartAt = (SystemTypes)buffer[offsets.StartAt];
                TaskType = (TaskTypes)buffer[offsets.TaskType];
                MinigamePrefab = BitConverter.ToUInt32(buffer, offsets.MinigamePrefab);
                HasLocatin = BitConverter.ToBoolean(buffer, offsets.HasLocatin);
                LocatinDirty = BitConverter.ToBoolean(buffer, offsets.LocatinDirty);
                TaskStep = buffer[offsets.TaskStep];
                AllStepNum = buffer[offsets.AllStepNum];
                return true;
            }
            return false;
        }
        public bool IsSabotage()
        {
            switch (this.TaskType)
            {
                case TaskTypes.ResetReactor:
                case TaskTypes.FixLights:
                case TaskTypes.RestoreOxy:
                case TaskTypes.StopCharles:
                case TaskTypes.FixComms:
                case TaskTypes.ResetSeismic:
                    return true;
                default:
                    return false;
            }
        }
    }
    /*
    [StructLayout(LayoutKind.Explicit)]

    public struct TaskInfo
    {
        [FieldOffset(0x18)] public SystemTypes StartAt;
        [FieldOffset(0x1C)] public TaskTypes TaskType;
        [FieldOffset(0x20)] public UInt32 MinigamePrefab;
        [FieldOffset(0x24)] public bool HasLocatin;
        [FieldOffset(0x25)] public bool LocatinDirty;
        [FieldOffset(0x28)] public byte TaskStep;
        [FieldOffset(0x2C)] public byte AllStepNum;
    }*/

    public enum TaskTypes : byte
    {
        SubmitScan = 0, // [marker]
        PrimeShields = 1,
        FuelEngines = 2,
        ChartCourse = 3,
        StartReactor = 4,
        SwipeCard = 5,
        ClearAsteroids = 6,
        UploadData = 7,
        InspectSample = 8,
        EmptyChute = 9,
        EmptyGarbage = 0xA,
        AlignEngineOutput = 0xB,
        FixWiring = 0xC,
        CalibrateDistributor = 0xD,
        DivertPower = 0xE,
        UnlockManifolds = 0xF,
        ResetReactor = 0x10,
        FixLights = 0x11,
        CleanO2Filter = 0x12,
        FixComms = 0x13,
        RestoreOxy = 0x14,
        StabilizeSteering = 0x15,
        AssembleArtifact = 0x16,
        SortSamples = 0x17,
        MeasureWeather = 0x18,
        EnterIdCode = 0x19,
        BuyBeverage = 0x1A,
        ProcessData = 0x1B,
        RunDiagnostics = 0x1C,
        WaterPlants = 0x1D,
        MonitorOxygen = 0x1E,
        StoreArtifacts = 0x1F,
        FillCanisters = 0x20,
        ActivateWeatherNodes = 0x21,
        InsertKeys = 0x22,
        ResetSeismic = 0x23,
        ScanBoardingPass = 0x24,
        OpenWaterways = 0x25,
        ReplaceWaterJug = 0x26,
        RepairDrill = 0x27,
        AlignTelescope = 0x28,
        RecordTemperature = 0x29,
        RebootWifi = 0x2A,
        PolishRuby = 0x2B,
        ResetBreakers = 0x2C,
        Decontaminate = 0x2D,
        MakeBurger = 0x2E,
        UnlockSafe = 0x2F,
        SortRecords = 0x30,
        PutAwayPistols = 0x31,
        FixShower = 0x32,
        CleanToilet = 0x33,
        DressMannequin = 0x34,
        PickUpTowels = 0x35,
        RewindTapes = 0x36,
        StartFans = 0x37,
        DevelopPhotos = 0x38,
        GetBiggolSword = 0x39,
        PutAwayRifles = 0x3A,
        StopCharles = 0x3B
    }

    public enum SystemTypes : byte
    {
        Hallway = 0, // [marker]
        Storage = 1,
        Cafeteria = 2,
        Reactor = 3,
        UpperEngine = 4,
        Nav = 5,
        Admin = 6,
        Electrical = 7,
        LifeSupp = 8,
        Shields = 9,
        MedBay = 0xA,
        Security = 0xB,
        Weapons = 0xC,
        LowerEngine = 0xD,
        Comms = 0xE,
        ShipTasks = 0xF,
        Doors = 0x10,
        Sabotage = 0x11,
        Decontamination = 0x12,
        Launchpad = 0x13,
        LockerRoom = 0x14,
        Laboratory = 0x15,
        Balcony = 0x16,
        Office = 0x17,
        Greenhouse = 0x18,
        Dropship = 0x19,
        Decontamination2 = 0x1A,
        Outside = 0x1B,
        Specimens = 0x1C,
        BoilerRoom = 0x1D,
        VaultRoom = 0x1E,
        Cockpit = 0x1F,
        Armory = 0x20,
        Kitchen = 0x21,
        ViewingDeck = 0x22,
        HallOfPortrait = 0x23,
        CargoBay = 0x24,
        Ventilation = 0x25,
        Showers = 0x26,
        Engine = 0x27,
        Brig = 0x28,
        MeetingRoom = 0x29,
        Records = 0x2A,
        Lounge = 0x2B,
        GapRoom = 0x2C,
        MainHall = 0x2D
    }
}
