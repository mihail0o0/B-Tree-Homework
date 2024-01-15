using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Transactions;

namespace B__Stablo;

class BPlusTree {
    public int Degree { get; set; }
    public BPlusNode Root { get; set; }

    public BPlusTree(int degree = 3) {
        Degree = degree;
        Root = new BPlusNode(degree, true);
    }

    public BPlusNode Insert(int newKey, object newData) {
        BPlusNode curr = FindNode(newKey);

        // ako mogu dodam
        if (curr.Length < Degree - 1) {
            int j;
            for (j = curr.Length; j > 0 && curr.Keys[j - 1] > newKey; j--) {
                curr.Keys[j] = curr.Keys[j - 1];
                if (curr.DataPointers != null) {
                    curr.DataPointers[j] = curr.DataPointers[j - 1];
                }
            }

            curr.Keys[j] = newKey;
            if (curr.DataPointers != null) {
                curr.DataPointers[j] = newData;
            }

            curr.Length++;
            return curr;
        }

        // ako nema mesta, preselim
        // sortiram, pa podelim 
        int mostRightKey = newKey;
        object mostRightData = newData;

        if (curr.DataPointers == null) throw new Exception("Nema data pointers");

        int k;
        for (k = curr.Length - 1; k > 0 && curr.Keys[k] > newKey; k--) {
            if (k == curr.Length - 1) {
                mostRightKey = curr.Keys[k];
                mostRightData = curr.DataPointers[k];
            }

            curr.Keys[k] = curr.Keys[k - 1];
            curr.DataPointers[k] = curr.DataPointers[k - 1];
        }

        if (k != curr.Length - 1) {
            curr.Keys[k] = newKey;
            curr.DataPointers[k] = newData;
        }

        curr.Length = Degree / 2;
        BPlusNode splitedRight = new(Degree, true);
        if (splitedRight.DataPointers == null) throw new Exception("Nema data pointers be");


        Array.ConstrainedCopy(curr.Keys, curr.Length, splitedRight.Keys, 0, curr.Length);
        Array.ConstrainedCopy(curr.DataPointers!, curr.Length, splitedRight.DataPointers, 0, curr.Length);
        splitedRight.Length = curr.Length;


        splitedRight.Keys[splitedRight.Length] = mostRightKey;
        splitedRight.DataPointers[splitedRight.Length] = mostRightData;

        splitedRight.Length++;

        splitedRight.NextLeaf = curr.NextLeaf;
        curr.NextLeaf = splitedRight;

        UpTree(curr, splitedRight, splitedRight.Keys[0]);

        return curr;
    }

    public BPlusNode FindNode(int keyToFind) {
        BPlusNode curr = Root;

        // nadjem ga
        while (curr.IsLeaf == false) {
            bool found = false;
            foreach ((int index, int nodeKey) in curr.IterateKeys()) {
                if (keyToFind <= nodeKey) {
                    found = true;
                    curr = curr.Children![index];
                    break;
                }
            }
            if (found == false) {
                curr = curr.Children![curr.Length];
            }
        }

        return curr;
    }


    // 3 slucaja: parrent ne postoji, 
    // parrent postoji ali je nepun,
    // ili parrent je pun (rekurzivno zovem)
    public void UpTree(BPlusNode left, BPlusNode right, int newKey) {
        BPlusNode? leftParrent = left.Parent;
        if (leftParrent == null) {
            BPlusNode noviRoot = new(Degree, false);
            noviRoot.Keys[0] = newKey;
            left.Parent = noviRoot;
            right.Parent = noviRoot;
            if (noviRoot.Children == null) throw new Exception("Kako?");
            noviRoot.Children[0] = left;
            noviRoot.Children[1] = right;
            noviRoot.Length = 1;
            Root = noviRoot;
            return;
        }

        // ako niz nije prepucan
        if (leftParrent.Length < Degree - 1) {
            int j;
            if (leftParrent.Children == null) throw new Exception("Nema dijece");

            for (j = leftParrent.Length; j > 0 && leftParrent.Keys[j - 1] > newKey; j--) {
                leftParrent.Keys[j] = leftParrent.Keys[j - 1];
                leftParrent.Children[j + 1] = leftParrent.Children[j];
            }

            leftParrent.Keys[j] = newKey;
            leftParrent.Children[j + 1] = right;

            // za levog je vec postavljen parrent
            right.Parent = leftParrent;

            leftParrent.Length++;
            return;
        }

        // ako je prepucalo
        if (leftParrent.Children == null) throw new Exception("Nema children be");

        int[] keyDup = new int[leftParrent.Length + 1];
        BPlusNode[] childDup = new BPlusNode[leftParrent.Length + 2];

        Array.Copy(leftParrent.Keys, keyDup, leftParrent.Length);
        Array.Copy(leftParrent.Children, childDup, leftParrent.Length + 1);

        int k;
        for (k = leftParrent.Length; k > 0 && keyDup[k - 1] > newKey; k--) {
            keyDup[k] = keyDup[k - 1];
            childDup[k + 1] = childDup[k];
        }

        keyDup[k] = newKey;
        childDup[k + 1] = right;

        int midKey;
        int midIndex;
        if (keyDup.Length % 2 == 0) {
            midIndex = (keyDup.Length / 2) + 1;
        }
        else {
            midIndex = (keyDup.Length / 2);
        }

        midKey = keyDup[midIndex];

        leftParrent.Length = midIndex;
        BPlusNode splitedRight = new(Degree, false);

        if (splitedRight.Children == null) throw new Exception("Nema children be");

        Array.ConstrainedCopy(keyDup, 0, leftParrent.Keys, 0, midIndex);
        Array.ConstrainedCopy(keyDup, midIndex + 1, splitedRight.Keys, 0, keyDup.Length - 1 - midIndex);

        Array.ConstrainedCopy(childDup, 0, splitedRight.Children, 0, midIndex + 1);
        Array.ConstrainedCopy(childDup, midIndex + 1, splitedRight.Children, 0, childDup.Length - 1 - midIndex);
        splitedRight.Length = keyDup.Length - 1 - midIndex;

        right.Parent = splitedRight;

        UpTree(leftParrent, splitedRight, midKey);
    }

    public void BTreePrint() {
        TestPrint(Root, 0);
    }
    public void TestPrint(BPlusNode node, int level) {

        Console.Write($"Level {level}, Leaf: {node.IsLeaf}, Keys: ");

        for (int i = 0; i < node.Length; i++) {
            Console.Write($"{node.Keys[i]} ");
        }

        Console.WriteLine();

        if (node.IsLeaf != true) {
            level++;

            for (int i = 0; i < node.Length + 1; i++) {
                TestPrint(node.Children![i], level);
            }
        }
    }

    // public void PrintTree(BPlusNode? root = null, string indent = "", bool last = true) {
    // if (Root == null) return;
    // root ??= Root;

    // Console.Write(indent);
    // if (last) {
    // Console.Write("\\-");
    // indent += "  ";
    // }
    // else {
    // Console.Write("|-");
    // indent += "| ";
    // }

    // Console.WriteLine($"{root.key} {root.value!.Value}");

    // for (int i = 0; i < root.children.Count; i++) {
    // PrintTree(root.children[i], indent, i == root.children.Count - 1);
    // }

    // }
    public void Print() {
        Queue<BPlusNode?> qu = new();
        qu.Enqueue(Root);
        qu.Enqueue(null);

        while (qu.TryDequeue(out BPlusNode? curr)) {

            if (curr == null) {
                System.Console.WriteLine();
                continue;
            }
            for (int i = 0; i < curr.Length + 1; i++) {
                if (i < curr.Length) {
                    Console.Write($"{curr.Keys[i]} ");
                }

                if (curr.Children == null) continue;

                qu.Enqueue(curr.Children[i]);

            }

            if (curr.IsLeaf == false && qu.Peek() == null) {
                qu.Enqueue(null);
            }

            System.Console.Write("-> ");
        }
        System.Console.WriteLine();
    }

    public void PrintLeaves() {
        BPlusNode? curr = Root;
        // nadjem najlevlji root
        while (curr.IsLeaf == false) {
            if (curr.Children == null) throw new Exception("Cannot itereate leaves");
            curr = curr.Children[0];
        }

        while (curr != null) {
            foreach ((int index, int key) in curr.IterateKeys()) {
                System.Console.Write($"{key} ");
            }
            System.Console.Write(" | ");
            curr = curr.NextLeaf;
        }
        System.Console.WriteLine();
    }
}