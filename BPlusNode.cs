namespace B__Stablo;

class BPlusNode {
    public int Length { get; set; }
    public int[] Keys { get; set; }
    public BPlusNode[]? Children { get; set; }
    public BPlusNode? NextLeaf { get; set; }
    public BPlusNode? Parent { get; set; }
    public bool IsLeaf { get; set; }
    public object[]? DataPointers { get; set; }

    public BPlusNode(int degree, bool isLeaf) {
        Length = 0;
        Keys = new int[degree];
        NextLeaf = null;
        Parent = null;
        IsLeaf = isLeaf;

        if (isLeaf) {
            Children = null;
            DataPointers = new object[degree];
        }
        else {
            Children = new BPlusNode[degree + 1];
            DataPointers = null;
        }
    }

    public IEnumerable<(int index, int key)> IterateKeys(){
        for(int i = 0; i < Keys.Length; i++){
            yield return (i, Keys[i]);
        }
    }

    public IEnumerable<(int index, BPlusNode node)> IterateChildren() {
        if(Children == null) yield break;

        for(int i = 0; i < Children.Length; i++){
            yield return (i, Children[i]);
        }
    }

    public IEnumerable<(int index, object data)> IterateData(){
        if(DataPointers == null) yield break;

        for(int i = 0; i < DataPointers.Length; i++){
            yield return (i, DataPointers[i]);
        }
    }
}