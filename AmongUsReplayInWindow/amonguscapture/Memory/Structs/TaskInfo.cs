using System;
using System.Runtime.InteropServices;

namespace AmongUsCapture
{
    
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
    }

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
        EmptyGarbage = 10,
        AlignEngineOutput = 11,
        FixWiring = 12,
        CalibrateDistributor = 13,
        DivertPower = 14,
        UnlockManifolds = 15,
        ResetReactor = 16,
        FixLights = 17,
        CleanO2Filter = 18,
        FixComms = 19,
        RestoreOxy = 20,
        StabilizeSteering = 21,
        AssembleArtifact = 22,
        SortSamples = 23,
        MeasureWeather = 24,
        EnterIdCode = 25,
        BuyBeverage = 26,
        ProcessData = 27,
        RunDiagnostics = 28,
        WaterPlants = 29,
        MonitorOxygen = 30,
        StoreArtifacts = 31,
        FillCanisters = 32,
        ActivateWeatherNodes = 33,
        InsertKeys = 34,
        ResetSeismic = 35,
        ScanBoardingPass = 36,
        OpenWaterways = 37,
        ReplaceWaterJug = 38,
        RepairDrill = 39,
        AlignTelescope = 40,
        RecordTemperature = 41,
        RebootWifi = 42,
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
        MedBay = 10,
        Security = 11,
        Weapons = 12,
        LowerEngine = 13,
        Comms = 14,
        ShipTasks = 15,
        Doors = 16,
        Sabotage = 17,
        Decontamination = 18,
        Launchpad = 19,
        LockerRoom = 20,
        Laboratory = 21,
        Balcony = 22,
        Office = 23,
        Greenhouse = 24,
        Dropship = 25,
        Decontamination2 = 26,
        Outside = 27,
        Specimens = 28,
        BoilerRoom = 29,
    }
}
