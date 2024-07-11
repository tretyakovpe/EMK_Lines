using LineWatch;

DataAccess.InitDB();
Directory.CreateDirectory(@"./conf/");
Directory.CreateDirectory(@"./pdf/");
Directory.CreateDirectory(@"./failures/");
List<PLC> plcList =
    [
        new PLC("73","10.149.50.21",0,"togp0018",true),
        new PLC("74","10.149.50.23",0,"togp0019",true),
        new PLC("79","10.149.50.25",0,"togp0020",true),
        new PLC("37","10.149.50.92",0,"togp0021",false),
    ];
//HandlingUnit box = new(01010101010101001, "LL1200-100", 100);
//string filename = LabelGenerator.MakeLabel(box);

foreach (var elem in plcList)
{
    Thread plcThread = new(() => elem.Poll())
    {
        Name = elem.Name
    };
    plcThread.Start();
}
//await DataAccess.connection.CloseAsync();