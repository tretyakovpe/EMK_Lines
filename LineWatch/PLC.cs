using Microsoft.Identity.Client.NativeInterop;
using Sharp7;
using System.Text;

namespace LineWatch
{
    /// <summary>
    /// Сборочная линия
    /// </summary>
    /// <param name="Name">Название</param>
    /// <param name="IP">Адрес</param>
    /// <param name="Printer">Принтер, на который отправляется бирка</param>
    /// <param name="Label">Нужно ли печатать бирку</param>
    public class PLC(string Name, string IP, string Printer, bool Label) : Sharp7.S7Client
    {
        public new string Name { get; set; } = Name;
        public string IP { get; set; } = IP;
        public string Printer { get; set; } = Printer;
        public bool Label { get; set; } = Label;
        public bool IsOKbefore { get; set; } = false;

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
                IsOKbefore = true;
            }
            else
            {
                if (IsOKbefore == true)
                {
                    File.AppendAllText(@"./failures/" + Name + ".log", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ") + Name + " " + ErrorText(res) + "\n\r");
                    Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ") + Name + " " + ErrorText(res));
                    IsOKbefore = false;
                }
                return;
            }

        }
        /// <summary>
        /// Опрашивает станцию. Если видит флаг законченной коробки - печатает бирку.
        /// </summary>
        public async void Poll()
        {
            DateTime DT = DateTime.Now;
            Console.WriteLine(DT.ToString("dd.MM.yyyy HH:mm:ss") + " Зарегистрирована линия " + Name);
            byte[] db = new byte[64];
            byte[] partdata = new byte[34];
            while (true)
            {
                if (IsOKbefore == false)
                {
                    Connect();
                }
                else
                {
                    if (Connected)
                    {
                        IsOKbefore = true;
                        //Посылаем лайвбит
                        byte[] flag = new byte[1];
                        S7.SetBitAt(flag, 0, 0, true);
                        int result = DBWrite(1012, 0, 1, flag);
                        if (result == 0)
                        {
                            DT = DateTime.Now;
                            //Читаем блок данных о собранной детали
                            result = DBRead(1013, 0, 34, partdata);
                            if (result == 0)
                            {
                                string partMaterial = S7.GetStringAt(partdata, 14);
                                bool partReady = S7.GetBitAt(partdata, 2, 2);
                                int counter = S7.GetIntAt(partdata, 32);
                                bool partOK = S7.GetBitAt(partdata, 0, 0);
                                bool partNOK = S7.GetBitAt(partdata, 0, 1);
                                bool testStarted = S7.GetBitAt(partdata, 0, 2);
                                bool testFinished = S7.GetBitAt(partdata, 0, 3);
                                bool[] MKM = new bool[32];

                                if (partReady)
                                {
                                    //Обнуляем на линии флаг собранной детали
                                    byte[] f = new byte[1];
                                    S7.SetBitAt(f, 0, 2, false);
                                    DBWrite(1013, 2, 1, f);
                                    Console.WriteLine(partdata.ToString());
                                    Console.WriteLine("\t" + DT.ToString() + "\t" + Name + "\t" + partMaterial + "\t" + partOK + "\t" + partNOK + "\t" + counter.ToString());

                                    Part newPart = new(partOK, partNOK, testStarted, testFinished, false, false, false, false, 0, DT, false, false, counter);
                                    Buffer.BlockCopy(MKM,0,newPart.MKM,0,32);
                                    newPart.EMC_number = partMaterial;
                                    string json = newPart.ToJson();
                                    //TODO Доделать сохранение в БД кадой детали. Нужно сделать в БД хранимую процедуру AddPart.
                                }
                            }

                            //Читаем бит готовности ящика
                            result = DBRead(1012, 0, 64, db);
                            if (result == 0)
                            {
                                bool boxIsReady = S7.GetBitAt(db, 1, 0);
                                string Material = S7.GetStringAt(db, 2);

                                //=======Это извращение для чтения названия продукции. Она прилетает в ASCII
                                string Material_Description = Encoding.GetEncoding(1251).GetString(db, 28, 36);
                                /*
                                byte[] temp_material_data = new byte[36];
                                System.Array.ConstrainedCopy(db, 28, temp_material_data, 0, 36);
                                string Material_Description = Encoding.GetEncoding(1251).GetString(temp_material_data);
                                */
                                //===================

                                double Amount = S7.GetRealAt(db, 22);
                                if (boxIsReady)
                                {
                                    //Обнуляем на линии флаг собранного ящика
                                    S7.SetBitAt(flag, 0, 0, false);
                                    DBWrite(1012, 1, 1, flag);
                                    //Уникальный номер контейнера (12 символов - первые 3 символа - идентификатор станции, остальные 9 - сквозной номер, централизованный)
                                    string tempNumber = Name.ToString().PadLeft(2, '0') + DT.ToString("yyMMddHHmm");
                                    Console.WriteLine(DT.ToString() + "\t" + tempNumber + "\t" + Name + "\t" + Material + "\t" + Material_Description + "\t" + Amount.ToString());
                                    //если количество ноль, то в базу не пишем, только на экран.
                                    if (Amount > 0)
                                    {
                                        HandlingUnit box = new(Convert.ToInt64(tempNumber), Material, (int)Amount, Material_Description);
                                        //Если для станции установлен признак печати, печатаем бирку
                                        if (Label)
                                        {
                                            var labelFile = LabelGenerator.MakeLabel(box);
                                            File.Copy(labelFile, @"\\NAS\" + Printer, true);
                                        }

                                        //Сохраняем в базу
                                        /*string query = "INSERT INTO prod VALUES ('"
                                            + DT.ToString("yyyy-MM-dd") + "', '"
                                            + DT.ToString("HH:mm:ss.F") + "', '"
                                            + tempNumber + "', '"
                                            + Name + "', '"
                                            + Material + "', "
                                            + Amount.ToString() + ");";
                                        DataAccess.Execute(query);*/
                                        await DataAccess.AddBoxAsync(DT.ToString("yyyy-MM-dd"), DT.ToString("HH:mm:ss.F"), tempNumber, Name, Material, (int)Amount);

                                    }
                                }
                            }
                            else
                            {
                                if (IsOKbefore == true)
                                {
                                    Console.WriteLine(DateTime.Now.ToString() + " " + Name + " Ошибка чтения блока 1012: " + ErrorText(result));
                                    File.AppendAllText(@"./failures/" + Name + ".log", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ") + Name + "  Ошибка чтения блока 1012: " + ErrorText(result) + "\n\r");
                                    IsOKbefore = false;
                                }
                            }
                        }
                        else
                        {
                            if (IsOKbefore == true)
                            {
                                Console.WriteLine(DateTime.Now.ToString() + " " + Name + " Ошибка записи лайвбита: " + ErrorText(result));
                                File.AppendAllText(@"./failures/" + Name + ".log", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ") + Name + "  Ошибка записи лайвбита: " + ErrorText(result) + "\n\r");
                                IsOKbefore = false;
                            }
                        }

                    }
                    else
                    {
                        Connect();
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
