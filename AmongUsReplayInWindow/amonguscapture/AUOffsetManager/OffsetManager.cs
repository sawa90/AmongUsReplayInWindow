using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AUOffsetManager
{
    public class OffsetManager
    {
        public static int GameMemReaderVersion = 1; //GameMemReader should update this.
        private Dictionary<string, GameOffsets> OffsetIndex = new Dictionary<string, GameOffsets>();
        private Dictionary<string, GameOffsets> LocalOffsetIndex = new Dictionary<string, GameOffsets>();
        public string indexURL;
        private string StorageLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\index.json");
        private string StorageLocationCache = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\indexCache.json");
        string hash;
        public Task indexTask;
        public OffsetManager()
        {
            if (File.Exists(StorageLocation))
            {
                 LocalOffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocation));
                 if (LocalOffsetIndex is null)
                 {
                     LocalOffsetIndex = new Dictionary<string, GameOffsets>();
                 }
            }
            
            if (File.Exists(StorageLocationCache))
            {
                OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocationCache));
            }
            else
            {
                OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(AmongUsReplayInWindow.Properties.Resources.Offsets);
            }

        }
        public async Task RefreshIndex()
        {
            if (indexURL == "")
            {
                OffsetIndex = new Dictionary<string, GameOffsets>();
                return;
            }

            using var httpClient = new HttpClient();
            try
            {
                this.indexURL = "https://raw.githubusercontent.com/sawa90/AmongUsReplayInWindow/master/AmongUsReplayInWindow/amonguscapture/Offsets.txt";
                var json = await httpClient.GetStringAsync(indexURL);
                OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(json);
                await using StreamWriter sw = File.CreateText(StorageLocationCache);
                await sw.WriteAsync(JsonConvert.SerializeObject(OffsetIndex, Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                try
                {
                    OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(AmongUsReplayInWindow.Properties.Resources.Offsets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
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

                if (OffsetIndex == null || !OffsetIndex.ContainsKey(hash))
                {
                    OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(AmongUsReplayInWindow.Properties.Resources.Offsets);
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
                if (OffsetIndex == null || !OffsetIndex.ContainsKey(hash) || OffsetIndex[hash].ShipStatusPtr == null) 
                {
                    indexTask = RefreshIndex();
                    indexTask.Wait();
                }
                var offsets = OffsetIndex.ContainsKey(sha256Hash) ? OffsetIndex[sha256Hash] : null;
                if (offsets is not null)
                {
                    Console.WriteLine($"Loaded offsets: {OffsetIndex[sha256Hash].Description}");
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
        public void AddToLocalIndex(string gameHash,GameOffsets offset)
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
        public int[] ShipStatusPtr { get; set; }


    }

}
