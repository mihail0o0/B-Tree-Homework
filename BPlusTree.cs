using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace B__Stablo;

class BPlusTree {
    public int Degree { get; set; }
    public BPlusNode Root { get; set; }

    public BPlusTree(int degree = 3) {
        Degree = degree;
        Root = new BPlusNode(degree, true);
    }

    public BPlusNode Insert(int newKey, object newData) {
        BPlusNode curr = Root;

        while (curr.IsLeaf == false) {
            foreach ((int index, int nodeKey) in curr.IterateKeys()) {
                if (newKey < nodeKey) {
                    curr = curr.Children![index];
                    break;
                }
                curr = curr.Children![curr.Length - 1];
            }
        }

        if (curr.Length < Degree) {
            curr.Keys[curr.Length] = newKey;
            if(curr.DataPointers != null){
                curr.DataPointers[curr.Length++] = newData;
            }
            else{
                curr.Length++;
            }
            return curr;
        }


        // ako nema mesta



        return curr;
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

            foreach ((int index, int key) in curr.IterateKeys()) {
                Console.Write($"{key} ");
                if (curr.Children != null) {
                    qu.Enqueue(curr.Children[index]);
                }

                if (curr.IsLeaf == false && qu.Peek() == null) {
                    qu.Enqueue(null);
                }
            }
        }
    }





}