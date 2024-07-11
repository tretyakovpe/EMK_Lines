namespace LineWatch
{
    /// <summary>
    /// Параметры привязанные к типу продукции.
    /// </summary>
    internal class Material
    {
        public string Internal { get; set; }
        public string Customer { get; set; }
        public string Dock { get; set; }
        public string Package { get; set; }
        public float Weight { get; set; }
        public float PackWeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intern">Внутреннее название</param>
        /// <param name="customer">Клиент</param>
        /// <param name="dock">Адрес доставки</param>
        /// <param name="package">Упаковка</param>
        /// <param name="weight">Вес изделия</param>
        /// <param name="packWeight">Вес упаковки</param>
        public Material(string intern, string customer, string dock, string package, float weight, float packWeight)
        {
            Internal = intern;
            Customer = customer;
            Dock = dock;
            Package = package;
            Weight = weight;
            PackWeight = packWeight;
        }
    }
}
