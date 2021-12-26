using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Reflection;

namespace AUOffsetManager
{
    public class OffsetManager
    {
        public static int GameMemReaderVersion = 1; //GameMemReader should update this.
        private Dictionary<string, GameOffsets> OffsetIndex = new Dictionary<string, GameOffsets>();
        private Dictionary<string, GameOffsets> LocalOffsetIndex = new Dictionary<string, GameOffsets>();
        private Dictionary<string, GameOffsets> ReplayOffsetIndex = null;
        public string indexURL;
        private string StorageFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsReplayInWindow");
        private string StorageLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsReplayInWindow\\index.json");
        private string StorageLocationCache = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsReplayInWindow\\indexCache.json");
        string hash;
        public Task indexTask;
        public OffsetManager()
        {
            if (!Directory.Exists(StorageFolder))
            {
                Directory.CreateDirectory(StorageFolder);
            }

            if (File.Exists(StorageLocation))
            {
                LocalOffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocation));
                if (LocalOffsetIndex is null)
                {
                    LocalOffsetIndex = new Dictionary<string, GameOffsets>();
                }
            }
        }
        public async Task RefreshIndex()
        {
            if (indexURL == "")
            {
                OffsetIndex = new Dictionary<string, GameOffsets>();
                return;
            }
            ReplayOffsetIndex = null;
            using var httpClient = new HttpClient();
            try
            {
                this.indexURL = "https://raw.githubusercontent.com/sawa90/AmongUsReplayInWindow/master/AmongUsReplayInWindow/amonguscapture/Offsets.txt";
                var json = await httpClient.GetStringAsync(indexURL);
                OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(json);
                ReplayOffsetIndex = OffsetIndex;
               await using StreamWriter sw = File.CreateText(StorageLocationCache);
                await sw.WriteAsync(JsonConvert.SerializeObject(OffsetIndex, Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (File.Exists(StorageLocationCache))
                {
                    OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocationCache));
                    ReplayOffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(AmongUsReplayInWindow.Properties.Resources.Offsets);
                }
            }
            if (OffsetIndex == null || !OffsetIndex.ContainsKey(hash))
            {
                OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(AmongUsReplayInWindow.Properties.Resources.Offsets);
            }
            if (OffsetIndex == null || !OffsetIndex.ContainsKey(hash))
            {
                try
                {
                    indexURL = "https://raw.githubusercontent.com/denverquane/amonguscapture/master/Offsets.json";
                    var json = await httpClient.GetStringAsync(indexURL);
                    OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(json);
                    await using StreamWriter sw = File.CreateText(StorageLocationCache);
                    await sw.WriteAsync(JsonConvert.SerializeObject(OffsetIndex, Formatting.Indented));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("If you are reading this that means that the site is down. If github still exists in the future, try again in 30 minutes.");
                }
            }
        }

        public GameOffsets FetchForHash(string sha256Hash)
        {
            hash = sha256Hash;
            if (LocalOffsetIndex.ContainsKey(sha256Hash))
            {
                Console.WriteLine($"Loaded offsets: {LocalOffsetIndex[sha256Hash].Description}");
                return LocalOffsetIndex[sha256Hash];
            }
            else
            {
                indexTask = RefreshIndex();
                indexTask.Wait();
                var offsets = OffsetIndex.ContainsKey(sha256Hash) ? OffsetIndex[sha256Hash] : null;
                if (offsets is not null)
                {
                    Console.WriteLine($"Loaded offsets: {OffsetIndex[sha256Hash].Description}");
                    if (offsets.StructVersion == 0 && ReplayOffsetIndex != null)
                    {
                        GameOffsets newest_offsets = offsets;
                        float newest_date = 0;
                        foreach (var sets in ReplayOffsetIndex.Values)
                        {
                            if (sets != offsets)
                            {
                                var datelist = Regex.Matches(sets.Description, "[0-9]+");
                                if (datelist != null && datelist.Count >= 3)
                                {
                                    float date = int.Parse(datelist[0].Value) * 10000 + int.Parse(datelist[1].Value) * 100 + int.Parse(datelist[2].Value);
                                    if (datelist.Count > 3) date += int.Parse(datelist[3].Value) * 0.1f;
                                    if (date >= newest_date && sets.StructVersion != 0)
                                    {
                                        newest_offsets = sets;
                                        newest_date = date;
                                    }
                                }
                            }
                        }

                        PropertyInfo[] properties = offsets.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        foreach (PropertyInfo info in properties)
                        {
                            if (info.GetValue(offsets) == null) 
                                info.SetValue(offsets, info.GetValue(newest_offsets));
                        }
                        Console.WriteLine($"Complemented the missing offsets with {newest_offsets.Description}");
                    }
                }
                return offsets;
            }

        }

        public void refreshLocal()
        {
            if (File.Exists(StorageLocation))
            {
                LocalOffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocation));
            }
        }
        public void AddToLocalIndex(string gameHash, GameOffsets offset)
        {
            using StreamWriter sw = File.CreateText(StorageLocation);
            LocalOffsetIndex[gameHash] = offset;
            var serialized = JsonConvert.SerializeObject(LocalOffsetIndex, Formatting.Indented);
            sw.Write(serialized);
        }

    }

    public class GameOffsets
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Description = "";

        public int offsetListVersion { get; set; }

        public int StructVersion { get; set; }

        public int AmongUsClientOffset { get; set; }

        public int GameDataOffset { get; set; }

        public int MeetingHudOffset { get; set; }

        public int GameStartManagerOffset { get; set; }

        public int HudManagerOffset { get; set; }

        public int ServerManagerOffset { get; set; }

        public int TempDataOffset { get; set; }

        public int GameOptionsOffset { get; set; }

        public int[] MeetingHudPtr { get; set; }
        public int[] MeetingHudCachePtrOffsets { get; set; }
        public int[] MeetingHudStateOffsets { get; set; }
        public int[] GameStateOffsets { get; set; }
        public int[] AllPlayerPtrOffsets { get; set; }
        public int[] AllPlayersOffsets { get; set; }
        public int[] PlayerCountOffsets { get; set; }
        public int[] ExiledPlayerIdOffsets { get; set; }
        public int[] RawGameOverReasonOffsets { get; set; }
        public int[] WinningPlayersPtrOffsets { get; set; }
        public int[] WinningPlayersOffsets { get; set; }
        public int[] WinningPlayerCountOffsets { get; set; }
        public int[] GameCodeOffsets { get; set; }
        public int[] PlayRegionOffsets { get; set; }
        public int[] PlayMapOffsets { get; set; }
        public int[] StringOffsets { get; set; }




        public bool isEpic { get; set; }
        public int AddPlayerPtr { get; set; }
        public int PlayerListPtr { get; set; }

        public PlayerInfoStructOffsets PlayerInfoStructOffsets { get; set; }
        public WinningPlayerDataStructOffsets WinningPlayerDataStructOffsets { get; set; }
        public PlayerOutfitStructOffsets PlayerOutfitStructOffsets { get; set; }

        public VoteAreaOffsets VoteAreaOffsets { get; set; }
        public NetTransformOffsets NetTransformOffsets { get; set; }
        public ChatOffsets ChatOffsets { get; set; }
        public PlayerControlOffsets PlayerControlOffsets { get; set; }
        public TaskInfoOffsets TaskInfoOffsets { get; set; }

        public ShipStatusOffsets ShipStatusOffsets;
        public MeetingRoomManagerOffsets MeetingRoomManagerOffsets;
    }

    public class PlayerInfoStructOffsets
    {
        public int PlayerIDOffset { get; set; }
        public int[] OutfitsOffset { get; set; }
        public int PlayerLevelOffset { get; set; }
        public int DisconnectedOffset { get; set; }
        public int[] RoleTypeOffset { get; set; }
        public int[] RoleTeamTypeOffset { get; set; }
        public int TasksOffset { get; set; }
        public int IsDeadOffset { get; set; }
        public int ObjectOffset { get; set; }
    }

    public class WinningPlayerDataStructOffsets
    {
        public int IsYouOffset { get; set; }
        public int IsImposterOffset { get; set; }
        public int IsDeadOffset { get; set; }
    }

    public class PlayerOutfitStructOffsets
    {
        public int dontCensorNameOffset { get; set; }
        public int ColorIDOffset { get; set; }
        public int HatIDOffset { get; set; }
        public int PetIDOffset { get; set; }
        public int SkinIDOffset { get; set; }
        public int VisorIDOffset { get; set; }
        public int NamePlateIDOffset { get; set; }
        public int PlayerNameOffset { get; set; }
        public int PreCensorNameOffset { get; set; }
        public int PostCensorNameOffset { get; set; }
    }

    public class VoteAreaOffsets
    {
        public int PlayerVoteAreaListPtr { get; set; }
        public int _Id { get; set; }
        public int didReport { get; set; }
        public int votedFor { get; set; }
    }

    public class NetTransformOffsets
    {
        public int targetSyncPosition { get; set; }
        public int prevPosSent { get; set; }
    }

    public class ShipStatusOffsets
    {
        public int[] ShipStatusPtr { get; set; }
        public int AllDoors { get; set; }
        public int EmergencyCooldown { get; set; }
        public int GapPlatform { get; set; }
    }

    public class ChatOffsets
    {
        public int[] chatControllerPtr { get; set; }
        public int chatPoolPtr { get; set; }
        public int chatBubblesPtr { get; set; }
        public int chatBubblesAddrPtr { get; set; }
        public int numChatBubblesPtr { get; set; }
        public int chatBubsVersionPtr { get; set; }
        public int TextMeshPtr { get; set; }
        public int ChatText { get; set; }
    }

    public class PlayerControlOffsets
    {
        public int NetId { get; set; }
        public int PlayerId { get; set; }
        public int inVent { get; set; }
        public int protectedByGuardian { get; set; }
        public int RemainingEmergencies { get; set; }
        public int nameText { get; set; }
        public int myLight_ { get; set; }
        public int NetTransform { get; set; }
        public int myTasks { get; set; }
        public int roleAssigned { get; set; }
    }

    public class TaskInfoOffsets
    {
        public int StartAt;
        public int TaskType;
        public int MinigamePrefab;
        public int HasLocatin;
        public int LocatinDirty;
        public int TaskStep;
        public int AllStepNum;
    }

    public class MeetingRoomManagerOffsets
    {
        public int[] MeetingRoomManager_Offsets { get; set; }
        public int reporter { get; set; }
        public int target { get; set; }
    }
}
