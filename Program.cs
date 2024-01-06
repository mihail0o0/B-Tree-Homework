namespace B__Stablo;
class Program
{
    static void Main(string[] args)
    {
        BPlusTree drvce = new(3);

        drvce.Insert(10, 10);
        drvce.Print();
        drvce.Insert(20, 20);
        drvce.Print();
        drvce.Insert(30, 30);
        drvce.Print();
        drvce.Insert(30, 30);
        drvce.Print();
    }
}
