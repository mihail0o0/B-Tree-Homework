using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;

namespace B__Stablo;

class BPlusNode {
    /// <summary>
    /// Odgovara broju ubacenih kljuceva i podataka.
    /// Broj dece je za 1 veci od ovog broja (ukoliko nod ima dece).
    /// </summary>
    public int Length { get; set; }
    public int[] Keys { get; set; }
    public BPlusNode[]? Children { get; set; }
    public BPlusNode? NextLeaf { get; set; }
    public BPlusNode? Parent { get; set; }
    public bool IsLeaf { get; set; }
    public object[]? DataPointers { get; set; }

    public BPlusNode(int degree, bool isLeaf) {
        Length = 0;
        Keys = new int[degree - 1];
        NextLeaf = null;
        Parent = null;
        IsLeaf = isLeaf;

        if (isLeaf) {
            Children = null;
            DataPointers = new object[degree - 1];
        }
        else {
            Children = new BPlusNode[degree];
            DataPointers = null;
        }
    }

    public void InsertLeaf(int[] keysToIns, object[]? dataPointersToIns = null, BPlusNode[]? childrenToIns = null) {
        int size = 0;
        int j;
        for (j = size; j > 0 && this.Keys[j - 1] > keysToIns[i]; j--) {
            Keys[j] = Keys[j - 1];
            if (dataPointersToIns != null) {
                DataPointers![j] = DataPointers[j - 1];
            }
        }

        Keys[j] = keysToIns[i];
        if (dataPointersToIns != null) {
            DataPointers![j] = dataPointersToIns[i];
        }
        size++;
        Length = size;

        // this.Print();
    }


    public void Print() {
        if (IsLeaf == true) {
            foreach ((int index, object obj) in this.IterateData()) {
                System.Console.Write($"K: {Keys[index]} ");
                System.Console.WriteLine($"O: {obj} ");
            }
            System.Console.WriteLine();

            return;
        }

        foreach ((int index, int key) in this.IterateKeys()) {
            System.Console.Write($"K: {key} ");
        }
        System.Console.WriteLine();

        return;
    }

    public IEnumerable<(int index, int key)> IterateKeys() {
        for (int i = 0; i < Length; i++) {
            yield return (i, Keys[i]);
        }
    }

    public IEnumerable<(int index, BPlusNode node)> IterateChildren() {
        if (Children == null) yield break;

        for (int i = 0; i <= Length; i++) {
            yield return (i, Children[i]);
        }
    }

    public IEnumerable<(int index, object data)> IterateData() {
        if (DataPointers == null) yield break;

        for (int i = 0; i < Length; i++) {
            yield return (i, DataPointers[i]);
        }
    }
}