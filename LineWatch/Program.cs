using LineWatch;

DataAccess.InitDB();
Directory.CreateDirectory(@"./conf/");
Directory.CreateDirectory(@"./pdf/");
Directory.CreateDirectory(@"./failures/");

List<Task> tasks = [];
List<PLC> plcList = DataAccess.GetPLCList();

foreach (var elem in plcList)
{
    // Создаем задачу для каждого PLC
    tasks.Add(Task.Run(async () =>
    {
        try
        {
            await elem.Poll(); // Вызываем асинхронный метод Poll()
        }
        catch (Exception ex)
        {
            // Логируем ошибку
            Console.WriteLine($"Ошибка в задаче {elem.Name}: {ex.Message}");
            File.AppendAllText(@"./failures/ErrorLog.txt", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ") + $"Ошибка в задаче {elem.Name}: {ex.Message}" + "\n\r");
        }
    }));
}
await Task.WhenAll(tasks); // Ожидаем завершения всех задач