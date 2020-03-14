using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Laboratorio1.ArbolHuffman.GraphClass
{
    public class Edge
    {
        public int Start;
        public int End;
        public Edge(int a, int b)
        {
            Start = a;
            End = b;
        }
    }
    public class vertice
    {
        public string value { get; set; }
        public List<int> First { get; set; }
        public List<int> Last { get; set; }
        public bool nullable { get; set; }

    }
    public class Graph
    {
        public Dictionary<int, vertice> vertices = new Dictionary<int, vertice>();
        public Dictionary<int, Edge> edges = new Dictionary<int, Edge>();

        public int head = new int();

        public Graph(Dictionary<int, vertice> v, Dictionary<int, Edge> e, int h)
        {
            vertices = v;
            edges = e;
            head = h;
        }


    }
   
}
