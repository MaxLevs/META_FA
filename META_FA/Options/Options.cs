using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace META_FA.Options
{
    public class Options
    {
        public SMOptions Arch { get; set; }
        public List<Asset> Assets { get; set; }

        public static Options FromFile(string path)
        {
            using var optionsFile = File.OpenText(path);
            return JsonSerializer.Deserialize<Options>(optionsFile.ReadToEnd());
        }

        public string ToText()
        {
            return JsonSerializer.Serialize(this, GetType());
        }

        public void ToFile(string path)
        {
            using var optionsFile = File.OpenWrite(path);
            optionsFile.Write(JsonSerializer.SerializeToUtf8Bytes(this, GetType()));
            optionsFile.Close();
        }
    }
}