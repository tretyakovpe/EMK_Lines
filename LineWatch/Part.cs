using System.Text.Json;

namespace LineWatch
{
    internal record Part
    {
        public bool PartOk { get; init; }
        public bool PartNok { get; init; }
        public bool TestStarted { get; init; }
        public bool TestFinished { get; init; }
        public bool Positive_edge { get; init; }
        public bool Conf_active { get; init; }
        public bool Server_ack { get; init; }
        public bool Server_error { get; init; }
        public int RET_VALUE { get; init; }
        public DateTime DATE_TIME { get; init; }
        public string EMC_number { get; set; }
        public bool Reserve_01 { get; init; }
        public bool Reserve_02 { get; init; }
        public bool[] MKM { get; init; }
        public int Counter { get; init; }

        // Конструктор для инициализации массива MKM
        public Part()
        {
            MKM = new bool[32]; // Инициализация массива MKM
        }

        public Part(bool partOK, 
                    bool partNOK, 
                    bool testStarted, 
                    bool testFinished, 
                    bool v1, 
                    bool v2, 
                    bool v3, 
                    bool v4, 
                    int v5, 
                    DateTime dT, 
                    bool v6, 
                    bool v7, 
                    int counter):this()
        {
            PartOk = partOK;
            PartNok = partNOK;
            TestStarted = testStarted;
            TestFinished = testFinished;
            Counter = counter;
        }

        // Метод для сериализации объекта в JSON
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}