using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace META_FA.Assets
{
    public class Asset
    {
        public string Text { get; set; }
        public bool ExpectedResult { get; set; }

        public static Dictionary<string, bool> FromFile(string path)
        {
            using var assetsFile = File.OpenText("assets.json");
            return JsonSerializer.Deserialize<List<Asset>>(assetsFile.ReadToEnd())
                                 .ToDictionary(asset => asset.Text, asset => asset.ExpectedResult);
        }
    }
}