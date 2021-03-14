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
                if (OffsetIndex == null || !OffsetIndex.ContainsKey(hash) || OffsetIndex[hash].offsetListVersion < 1) 
                {
                    indexTask = RefreshIndex();
                    indexTask.Wait();
                }
                var offsets = OffsetIndex.ContainsKey(sha256Hash) ? OffsetIndex[sha256Hash] : null;
                if (offsets is not null)
                {
                    Console.WriteLine($"Loaded offsets: {OffsetIndex[sha256Hash].Description}");
                    OffsetIndex[sha256Hash].before20201209s = HashList_until20201209s.Contains(sha256Hash);
                    if (OffsetIndex[sha256Hash].PlayerVoteAreaListPtr == 0) OffsetIndex[sha256Hash].PlayerVoteAreaListPtr = 0x60;
                    if (OffsetIndex[sha256Hash].before20201209s)
                    {
                        if (OffsetIndex[sha256Hash].DoorsPtr == 0) OffsetIndex[sha256Hash].DoorsPtr = 0x7c;
                    }
                    else
                    {
                        if (OffsetIndex[sha256Hash].DoorsPtr == 0) OffsetIndex[sha256Hash].DoorsPtr = 0x84;  
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
        public void AddToLocalIndex(string gameHash,GameOffsets offset)
        {
            using StreamWriter sw = File.CreateText(StorageLocation);
            LocalOffsetIndex[gameHash] = offset;
            var serialized = JsonConvert.SerializeObject(LocalOffsetIndex, Formatting.Indented);
            sw.Write(serialized);
        }

        public static List<string> HashList_until20201209s = new List<string>()
        {
            "FF1DAE62454312FCE09A39061999C26FD26440FDA5F36C1E6424290A34D05B08",
            "38119B8551718D9016BAFEEDC105610D5B3AED5B0036D1A6060B8E2ABE523C02",
            "5AB7B3419ED29AF0728E66AE8F1A207AEDD6456280128060FEDF74621B287BE6",
            "9BD96553424D3313700E4CB06F0FFECA346B96F731DA57C00F6B78BC3CE81902",
            "1393240B74D9E27741E7FBA13D67C843E5CE0C3D0EF7ABBEE24179CB97C29918",
            "0B010BD3195D39C089DC018D834B2EBD26BA67D2F49C4EBEA608A804FC0975B7",
            "4BFEB19A37634C94017824F0D71B1C4651173C4B9242FF4EF6FAFFA593DFD91D",
            "windows_store"
        };
    }

    public class GameOffsets
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Description = "";

        public int offsetListVersion { get; set; }

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
        public bool before20201209s { get; set; }
        public int DoorsPtr { get; set; }
        public int PlayerVoteAreaListPtr { get; set; }
        public int[] ChatControllerPtr { get; set; }
    }

}
