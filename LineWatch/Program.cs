using LineWatch;

DataAccess.InitDB();
Directory.CreateDirectory(@"./conf/");
Directory.CreateDirectory(@"./pdf/");
Directory.CreateDirectory(@"./failures/");

var filename = @"./conf/lines.txt";
List<PLC> plcList = new();
 if (File.Exists(@filename))
{
    var listFile = File.ReadLines(@filename);
    foreach (string line in listFile)
    {
        string[] items = line.Split(",");
        PLC p = new(items[0], items[1], items[2], items[3]=="true");
        plcList.Add(p);
    }
}
else
{
    plcList =
    [
        new PLC("73","10.149.50.21","togp0018",true),
        new PLC("74","10.149.50.23","togp0019",true),
        new PLC("79","10.149.50.25","togp0020",true),
        new PLC("37","10.149.50.92","togp0021",false),
    ];
}
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