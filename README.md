# EMK_Lines
Модуль представляет из себя программу на C#

Программа формирует pdf файлы бирок и кладёт их в папку программы.
После этого она отправляет их на принтер, который назначен соответствующей линии.

Кроме этого, программа сохраняет считанные данные в базу данных MSSQL, расположенную на этом же компьютере.

Для работы необходимы файлы конфигурации линий и материалов. Они должны находиться в папке conf, в папке программы.

  lines.txt - конфигурация линий. Данные вводятся простым текстом, разделяются запятыми. 
  Порядок полей: название станции, IP, имя принтера, нужно ли печатать бирку (true/false)
  Пример для 73 станции: 73,10.149.50.21,togp0018,true

  materials.txt - конфигурация материалов. Данные вводятся простым текстом, разделяются запятыми.
  Порядок полей: индекс ЭМК, индекс потребителя, склад доставки, тип тары, вес изделия, вес тары.
  Пример для LF1400-100: LF1400-100, 8450034709, 08329, 8USP0010, 2, 20
