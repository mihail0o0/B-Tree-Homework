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
        curr.Length = Degree / 2;
        BPlusNode splitedRight = new(Degree, true);

        Array.ConstrainedCopy(curr.Keys, curr.Length, splitedRight.Keys, 0, curr.Length);
        if (splitedRight.DataPointers == null) throw new Exception("Nema data pointers be");
        Array.ConstrainedCopy(curr.DataPointers!, curr.Length, splitedRight.DataPointers, 0, curr.Length);
        splitedRight.Length = curr.Length;

        int k;
        for (k = splitedRight.Length; k > 0 && splitedRight.Keys[k - 1] > newKey; k--) {
            splitedRight.Keys[k] = splitedRight.Keys[k - 1];
            splitedRight.DataPointers[k] = splitedRight.DataPointers[k - 1];
        }
        splitedRight.Keys[k] = newKey;
        splitedRight.DataPointers[k] = newData;

        splitedRight.Length++;

        splitedRight.NextLeaf = curr.NextLeaf;
        curr.NextLeaf = splitedRight;

        UpTree(curr, splitedRight);

        return curr;
    }

    public BPlusNode FindNode(int keyToFind) {
        BPlusNode curr = Root;

        // nadjem ga
        while (curr.IsLeaf == false) {
            foreach ((int index, int nodeKey) in curr.IterateKeys()) {
                if (keyToFind < nodeKey) {
                    System.Console.WriteLine(index + " " + nodeKey);
                    curr = curr.Children![index];
                    break;
                }
                curr = curr.Children![curr.Length];
            }
        }

        return curr;
    }


    // 3 slucaja: parrent ne postoji, 
    // parrent postoji ali je nepun,
    // ili parrent je pun (rekurzivno zovem)
    public void UpTree(BPlusNode left, BPlusNode right) {
        BPlusNode? leftParrent = left.Parent;
        if (leftParrent == null) {
            BPlusNode noviRoot = new(Degree, false);
            noviRoot.Keys[0] = right.Keys[0];
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
            for (j = leftParrent.Length; j > 0 && leftParrent.Keys[j - 1] > right.Keys[0]; j--) {
                leftParrent.Keys[j] = leftParrent.Keys[j - 1];
                if (leftParrent.Children == null) throw new Exception("Nema dijece");
                leftParrent.Children[j + 1] = leftParrent.Children[j];
            }

            if (leftParrent.Children == null) throw new Exception("Nema dijece");
            leftParrent.Children[j + 1] = right;

            return;
        }

        // ako jeste
        leftParrent.Length = Degree / 2;
        BPlusNode splitedRight = new(Degree, false);

        Array.ConstrainedCopy(leftParrent.Keys, leftParrent.Length, splitedRight.Keys, 0, leftParrent.Length);
        if (splitedRight.Children == null) throw new Exception("Nema children be");
        if (leftParrent.Children == null) throw new Exception("Nema children be");
        Array.ConstrainedCopy(leftParrent.Children, leftParrent.Length, splitedRight.Children, 0, leftParrent.Length);
        splitedRight.Length = leftParrent.Length;


        //rightKeys[curr.Length] = newKey;
        int k;
        for (k = splitedRight.Length; k > 0 && splitedRight.Keys[k - 1] > right.Keys[0]; k--) {
            splitedRight.Keys[k] = splitedRight.Keys[k - 1];
            splitedRight.Children[k] = splitedRight.Children[k - 1];
        }
        splitedRight.Keys[k] = right.Keys[0];
        splitedRight.Children[k + 1] = right;

        UpTree(leftParrent, splitedRight);
    }

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

                if (curr.IsLeaf == false && qu.Peek() != null) {
                    qu.Enqueue(null);
                }
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