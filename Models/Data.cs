namespace SnowBackend.Models;
    public class DataRow
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int Value { get; set; }

        public DataRow (string name, string color, int value) {
            Name = name;
            Color = color;
            Value = value;
        }

    }

    public class UserConnection
    {
        public Guid UserId {get; set;}
        public string Hash {get; set;}

        public bool IsActive {get; set;}
        public List<DataRow> FileData {get;set;}

        public List<DataRow> CurrentData {get;set;}

        public void GenerateNewCurrentData(){
            var rnd = new Random();
            CurrentData = FileData
                .Select(row => new DataRow (row.Name, row.Color, rnd.Next(row.Value)))
                .ToList();
        }
    }
