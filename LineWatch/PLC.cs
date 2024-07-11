using Sharp7;

namespace LineWatch
{
    /// <summary>
    /// Сборочная линия
    /// </summary>
    /// <param name="Name">Название</param>
    /// <param name="IP">Адрес</param>
    /// <param name="Port">Порт, не используется</param>
    /// <param name="Printer">Принтер, на который отправляется бирка</param>
    /// <param name="Label">Нужно ли печатать бирку</param>
    public class PLC(string Name, string IP, int Port, string Printer, bool Label) : Sharp7.S7Client
    {
        public new string Name { get; set; } = Name;
        public string IP { get; set; } = IP;
        public int Port { get; set; } = Port;
        public string Printer { get; set; } = Printer;
        public bool Label { get; set; } = Label;
        public bool lastStatus { get; set; } = false;

        /// <summary>
        /// Подключается к станции по IP.
        /// </summary>
        private new void Connect()
        {
            DateTime DT = DateTime.Now;
            int res = ConnectTo(IP, 0, 2);
            if (res == 0)
            {
                Console.WriteLine(DT.ToString("dd.MM.yyyy HH:mm:ss ") + " Станция " + Name + " подключена.");
                GetPlcDateTime(ref DT);
                Console.WriteLine(Name + " Текущее время на станции " + DT.ToString());
                lastStatus = true;
            }
            else
            {
                if (lastStatus == true)
                {
                    File.AppendAllText(@"./failures/"+Name+".log",DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ") + Name + " " + ErrorText(res));
                    Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ") + Name + " " + ErrorText(res));
                    lastStatus = false;
                    return;
                }
            }

        }
        /// <summary>
        /// Опрашивает станцию. Если видит флаг законченной коробки - печатает бирку.
        /// </summary>
        public void Poll()
        {
            DateTime DT = DateTime.Now;
            Console.WriteLine(DT.ToString("dd.MM.yyyy HH:mm:ss") + " Зарегистрирована линия " + Name);
            byte[] db = new byte[26];
            while (true)
            {
                if (Connected)
                {
                    //Посылаем лайвбит
                    byte[] flag = new byte[1];
                    S7.SetBitAt(flag, 0, 0, true);
                    int result = DBWrite(1012, 0, 1, flag);
                    if (result == 0)
                    {
                        DT = DateTime.Now;
                        //Читаем бит готовности ящика
                        result = DBRead(1012, 0, 26, db);
                        if (result == 0)
                        {
                            bool boxIsReady = S7.GetBitAt(db, 1, 0);
                            string Material = S7.GetStringAt(db, 2);
                            double Amount = S7.GetRealAt(db, 22);
                            if (boxIsReady)
                            {
                                //Обнуляем на линии флаг собранного ящика
                                S7.SetBitAt(flag, 0, 0, false);
                                DBWrite(1012, 1, 1, flag);

                                //Уникальный номер контейнера (12 символов - первые 3 символа - идентификатор станции, остальные 9 - сквозной номер, централизованный)
                                string tempNumber = Name.ToString().PadLeft(2, '0') + DT.ToString("yyMMddHHmm");
                                Console.WriteLine(DT.ToString() + "\t" + tempNumber + "\t" + Name + "\t" + Material + "\t" + Amount.ToString());
                                HandlingUnit box = new(Convert.ToInt64(tempNumber), Material, (int)Amount);



                                //Если для станции установлен признак печати, печатаем бирку
                                if (Label)
                                {
                                    var labelFile = LabelGenerator.MakeLabel(box);
                                    File.Copy(labelFile, @"\\NAS\" + Printer, true);
                                }

                                //Сохраняем в файл
                                //using StreamWriter w = File.AppendText("_data.csv");
                                //await w.WriteLineAsync(DT.ToString("dd.MM.yyyy,HH:mm:ss") + "," + tempNumber + "," + Name + "," + Material + "," + Amount.ToString());
                                //w.Flush();

                                //Сохраняем в базу
                                string query = "INSERT INTO prod VALUES ('"
                                    + DT.ToString("yyyy-MM-dd") + "', '"
                                    + DT.ToString("HH:mm:ss.F") + "', '"
                                    + tempNumber + "', '"
                                    + Name + "', '"
                                    + Material + "', "
                                    + Amount.ToString() + ");";
                                DataAccess.Execute(query); 
                            }
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " " + Name + " Ошибка чтения блока 1012: " + ErrorText(result));
                        }
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now.ToString() + " " + Name + " Ошибка записи лайвбита: " + ErrorText(result));
                    }

                }
                else
                {
                    Connect();
                }
                Thread.Sleep(10);
            }

        }

    }
}
