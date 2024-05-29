using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using WpfApp1.Model;

namespace WpfApp1
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
            if (!File.Exists(_filePath))
            {
                return new List<PlantPassport>();
            }

            var jsonData = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<PlantPassport>>(jsonData);
        }

        public void SaveData(List<PlantPassport> data)
        {
            var jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_filePath, jsonData);
        }
    }
}
