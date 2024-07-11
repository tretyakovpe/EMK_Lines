namespace LineWatch
{
    /// <summary>
    /// Описание тарного места готовой продукции (ящика)
    /// </summary>
    /// <param name="Number">Серийный номер ящика</param>
    /// <param name="Type">Код продукции</param>
    /// <param name="Amount">Количество продукции</param>
    public class HandlingUnit(long Number, string Type, int Amount)
    {
        public long Number { get; set; } = Number;
        public string Type { get; set; } = Type;
        public int Amount { get; set; } = Amount;
    }
}
