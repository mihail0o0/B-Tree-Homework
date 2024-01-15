namespace B__Stablo;
class Program
{
    static void Main(string[] args)
    {
        BPlusTree drvce = new(3);

        drvce.Insert(3, 3);
        drvce.Insert(5, 5);
        drvce.Insert(7, 7);
        drvce.Insert(2, 2);
        drvce.Insert(1, 1);
        drvce.Insert(12, 12);
        drvce.Insert(13, 13);
        drvce.Insert(6, 6);
        drvce.Insert(10, 10);
        drvce.Insert(4, 4);
        drvce.Insert(5, 5);
        drvce.Print();
        drvce.BTreePrint();
        drvce.PrintLeaves();
    }
}
