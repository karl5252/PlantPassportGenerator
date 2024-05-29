using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WpfApp1.Model;

namespace WpfApp1.Util
{
    public class JsonDatabaseService
    {
        private readonly string _filePath;

        public JsonDatabaseService(string filePath)
        {
            _filePath = filePath;
        }

        public List<PlantPassport> LoadData()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<PlantPassport>>(json) ?? new List<PlantPassport>();
            }

            return new List<PlantPassport>();
        }

        public void SaveData(List<PlantPassport> data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
