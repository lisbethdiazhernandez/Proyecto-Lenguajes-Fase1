using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Laboratorio1.ArbolHuffman;
using Laboratorio1.ArbolHuffman.GraphClass;

namespace Laboratorio1.ArbolHuffman
{
    public class ArbolHuff
    {
       
        public void prueba()
        {

        }
        public   int NoTerminales = 1;
        public  Dictionary<int, List<int>> Follows = new Dictionary<int, List<int>>();
      
        public Graph CreateTree(List<string> expresion,int nVertices, int nEdges)
        {
            Dictionary<int, vertice> vertices = new Dictionary<int, vertice>();
            Dictionary<int, Edge> edges = new Dictionary<int, Edge>();
            //Graph graph = new Graph();
           
            if (expresion.Count == 0)
            {
                Graph graph = new Graph(vertices, edges, 0);
                return graph;
            }
            List<string> temp = new List<string>();
            string valor = string.Empty; bool bandera = false;
            //if (expresion[expresion.Count-1] != "(")
            //{
            //    expresion.RemoveAt(expresion.Count - 1);
            //    temp = expresion;
            //    bandera =  temp.TrueForAll(x => x == "(");
            //}
            if (expresion.Count == 1 || bandera == true)
            {
                nVertices++;
                vertice vert = new vertice();
                vert.value = (bandera== true)? valor : expresion[0];
                if (expresion[0] != "?" && expresion[0] != "+" && expresion[0] != "*" && expresion[0] != "|" && expresion[0] != ".")
                {
                    vert.First = new List<int>();
                    vert.First.Add(NoTerminales);
                    vert.Last = new List<int>();
                    vert.Last.Add(NoTerminales);
                    NoTerminales++;
                    vert.nullable = false;
                }
                else if (expresion[0] == "?"  || expresion[0] == "*")
                {
                    vert.nullable = true;
                }
                else 
                {
                    vert.nullable = false;
                }
                vertices.Add(nVertices, vert);
                Graph graph = new Graph(vertices, edges, nVertices);
                return graph;
            }
            List<string> A = expresion;
            List<string> B = expresion;
            int parentesiscount = 0; bool expIsConcat = false;
            for (int i = 0; i < expresion.Count - 1 && !expIsConcat; i++)
            {
                if (expresion[i] == "(") { parentesiscount++; }
                if (expresion[i] == ")") { parentesiscount--; }
                if (expresion[i + 1] == "." && parentesiscount == 0)
                {
                    expIsConcat = true;
                    if (expresion[i] == "(")
                    {
                        A = expresion.GetRange(i, i + 1);
                    }
                    else
                    {
                        A = expresion.GetRange(0, i + 1);
                    }
                    B = expresion.GetRange(i + 2, expresion.Count - i - 2);
                }
            }
            // Si la expresion es de la forma a|b
            if (expIsConcat)
            {
                // Crear nodo de |
                nVertices++;
                vertice vert = new vertice();
                vert.value = ".";
                vert.nullable = false;
                vertices.Add(nVertices, vert);
                // Crear el subtree de las expresiones
                Graph subTreeA = CreateTree(A, nVertices, nEdges);
                Graph subTreeB = CreateTree(B, nVertices + subTreeA.vertices.Count, nEdges + subTreeA.edges.Count);
                // Agregar los vertices del subtree
                subTreeA.vertices.ToList().ForEach(x => vertices.Add(x.Key, x.Value));
                subTreeB.vertices.ToList().ForEach(x => vertices.Add(x.Key, x.Value));
                // Agregar los edges del subtree
                subTreeA.edges.ToList().ForEach(x => edges.Add(edges.Count+1, x.Value));
                subTreeB.edges.ToList().ForEach(x => edges.Add(edges.Count + 1, x.Value));
                // Agregar el edge que une al nodo y al subtree
                nEdges = edges.Count + 1;
                Edge newEdgeA = new Edge(nVertices, subTreeA.head);
                edges.Add(nEdges, newEdgeA);
                nEdges = edges.Count + 1;
                Edge newEdgeB = new Edge(nVertices, subTreeB.head);
                edges.Add(nEdges, newEdgeB);
                // Retornar el grafo
                Graph graph = new Graph(vertices, edges, nVertices);
                return graph;
            }
            // Revisar que la expresion sea de la forma a|b
            List<string> a = expresion;
            List<string> b = expresion;
            int parentesisCount = 0;
            bool expIsOr = false;
            for (int i = 0; i < expresion.Count - 1 && !expIsOr; i++)
            {
                if (expresion[i] == "(") { parentesisCount++; }
                if (expresion[i] == ")") { parentesisCount--; }
                if (parentesisCount == 0 && expresion[i + 1] == "|")
                {
                    expIsOr = true;
                    a = expresion.GetRange(0, i + 1);
                    b = expresion.GetRange(i + 2, expresion.Count - i - 2);
                }
            }
            // Si la expresion es de la forma a|b
            if (expIsOr)
            {
                // Crear nodo de |
                nVertices++;
                vertice vert = new vertice();
                vert.value = "|";
                vert.nullable = false;
                vertices.Add(nVertices, vert);
                // Crear el subtree de las expresiones
                Graph subTreeA = CreateTree(a, nVertices, nEdges);
                Graph subTreeB = CreateTree(b, nVertices + subTreeA.vertices.Count, nEdges + subTreeA.edges.Count);
                // Agregar los vertices del subtree
                subTreeA.vertices.ToList().ForEach(x => vertices.Add(x.Key, x.Value));
                subTreeB.vertices.ToList().ForEach(x => vertices.Add(x.Key, x.Value));
                // Agregar los edges del subtree
                subTreeA.edges.ToList().ForEach(x => edges.Add(edges.Count + 1, x.Value));
                subTreeB.edges.ToList().ForEach(x => edges.Add(edges.Count+1, x.Value));
                // Agregar el edge que une al nodo y al subtree
                nEdges = edges.Count + 1;
                Edge newEdgeA = new Edge(nVertices, subTreeA.head);
                edges.Add(nEdges, newEdgeA);
                nEdges = edges.Count + 1;
                Edge newEdgeB = new Edge(nVertices, subTreeB.head);
                edges.Add(nEdges, newEdgeB);
                // Retornar el grafo
                Graph graph = new Graph(vertices, edges, nVertices);
                return graph;
            }
            // Si la expresion es del tipo (s)
            else if (expresion.Count >= 2 && expresion[0] == "(" && expresion[expresion.Count - 1] == ")")
            {
                // Devuelve el arbol de s
                return CreateTree(expresion.GetRange(1,expresion.Count-2),nVertices,nEdges);
            }
            // Si la expresion es de la forma s*
            else if ((expresion.Count ==2 && expresion[1] == "*") || (expresion.Count > 2 && expresion[0] == "(" && expresion[expresion.Count-2] == ")" && expresion[expresion.Count - 1] == "*"))
            {
                // Crear el nodo del *
                nVertices++;
                vertice vert = new vertice();
                vert.value = "*";
                vert.nullable = true;
                vertices.Add(nVertices,vert);
                // Crear el subtree de la expresion
                Graph subTree = CreateTree(expresion.GetRange(0, expresion.Count-1), nVertices, nEdges);
                // Agregar los vertices del subtree
                subTree.vertices.ToList().ForEach(x => vertices.Add(x.Key, x.Value));
                // Agregar los edges del subtree
                subTree.edges.ToList().ForEach(x => edges.Add(x.Key,x.Value));
                // Agregar el edge que une al nodo y al subtree
                nEdges = edges.Count+1;
                Edge newEdge = new Edge(nVertices,subTree.head);
                edges.Add(nEdges,newEdge);
                // Retornar el grafo
                Graph graph = new Graph(vertices, edges, nVertices);
                return graph;
            }
            else if ((expresion.Count == 2 && expresion[1] == "?") || (expresion.Count > 2 && expresion[0] == "(" && expresion[expresion.Count - 2] == ")" && expresion[expresion.Count - 1] == "?"))
            {
                // Crear el nodo del *
                nVertices++;
                vertice vert = new vertice();
                vert.value = "?";
                vert.nullable = true;
                vertices.Add(nVertices, vert);
                // Crear el subtree de la expresion
                Graph subTree = CreateTree(expresion.GetRange(0, expresion.Count - 1), nVertices, nEdges);
                // Agregar los vertices del subtree
                subTree.vertices.ToList().ForEach(x => vertices.Add(x.Key, x.Value));
                // Agregar los edges del subtree
                subTree.edges.ToList().ForEach(x => edges.Add(x.Key, x.Value));
                // Agregar el edge que une al nodo y al subtree
                nEdges = edges.Count + 1;
                Edge newEdge = new Edge(nVertices, subTree.head);
                edges.Add(nEdges, newEdge);
                // Retornar el grafo
                Graph graph = new Graph(vertices, edges, nVertices);
                return graph;
            }
            else if ((expresion.Count == 2 && expresion[1] == "+") || (expresion.Count > 2 && expresion[0] == "(" && expresion[expresion.Count - 2] == ")" && expresion[expresion.Count - 1] == "+"))
            {
                // Crear el nodo del *
                nVertices++;
                vertice vert = new vertice();
                vert.value = "+";
                vert.nullable = false;
                vertices.Add(nVertices, vert);
                // Crear el subtree de la expresion
                Graph subTree = CreateTree(expresion.GetRange(0, expresion.Count - 1), nVertices, nEdges);
                // Agregar los vertices del subtree
                subTree.vertices.ToList().ForEach(x => vertices.Add(x.Key, x.Value));
                // Agregar los edges del subtree
                subTree.edges.ToList().ForEach(x => edges.Add(x.Key, x.Value));
                // Agregar el edge que une al nodo y al subtree
                nEdges = edges.Count+1;
                Edge newEdge = new Edge(nVertices, subTree.head);
                edges.Add(nEdges, newEdge);
                // Retornar el grafo
                Graph graph = new Graph(vertices, edges, nVertices);
                return graph;
            }
            
            
            Graph graphtemp = new Graph(vertices, edges, 0);
            return graphtemp;
        }

        public vertice AssignFirst(Graph graph, int position)
        {
            vertice first = new vertice();
            vertice vert = graph.vertices.FirstOrDefault(x => x.Key == position).Value;
            if (vert.First == null)
            {
                if(vert.value == "|" || vert.value == ".")
                {
                    List<int> hijos = new List<int>();
                    vertice v1 = new vertice();
                    vertice v2 = new vertice();
                    List<int> tempvert2 = new List<int>();
                    hijos.Add(graph.edges.FirstOrDefault(x => x.Value.Start == position).Value.End);
                    v1 = (AssignFirst(graph, hijos[0]));
                    hijos.Add(graph.edges.LastOrDefault(x => x.Value.Start == position).Value.End);
                    v2= AssignFirst(graph, hijos[1]);
                    if(vert.value == "|")
                    {
                        vert.First = v1.First.Union(v2.First).ToList();
                    }
                    else if (vert.value == ".")
                    {
                        if(v1.nullable == true)
                        {
                            vert.First = v1.First.Union(v2.First).ToList();
                        }
                        else
                        {
                            vert.First = v1.First;
                        }
                    }
                    return vert;
                }
                else
                {
                    int pos = graph.edges.FirstOrDefault(x => x.Value.Start == position).Value.End;
                    vert.First = AssignFirst(graph, pos).First;
                }
            }
            else if (vert.value != "*" || vert.value != "+" || vert.value != "?" || vert.value != "|")
            {
                return vert;
            }
            return vert;
        }

        public vertice AssignLast(Graph graph, int position)
        {
            vertice vert = graph.vertices.FirstOrDefault(x => x.Key == position).Value;
            if (vert.Last == null)
            {
                if (vert.value == "|" || vert.value == ".")
                {
                    List<int> hijos = new List<int>();
                    vertice v1 = new vertice();
                    vertice v2 = new vertice();
                    List<int> tempvert2 = new List<int>();
                    hijos.Add(graph.edges.FirstOrDefault(x => x.Value.Start == position).Value.End);
                    v1 = (AssignLast(graph, hijos[0]));
                    hijos.Add(graph.edges.LastOrDefault(x => x.Value.Start == position).Value.End);
                    v2 = AssignLast(graph, hijos[1]);
                    if (vert.value == "|")
                    {
                        vert.Last = v1.Last.Union(v2.Last).ToList();
                    }
                    else if (vert.value == ".")
                    {
                        if (v2.nullable == true)
                        {
                            vert.Last = v1.Last.Union(v2.Last).ToList();
                        }
                        else
                        {
                            vert.Last = v2.Last;
                        }
                    }
                    return vert;
                }
                else
                {
                    int pos = graph.edges.FirstOrDefault(x => x.Value.Start == position).Value.End;
                    vert.Last = AssignLast(graph, pos).Last;
                }
            }
            else if (vert.value != "*" || vert.value != "+" || vert.value != "?" || vert.value != "|")
            {
                return vert;
            }
            return vert;
        }

        public Dictionary<int, List<int>> AssignFollow(Graph graph)
        {
            List<int> templist = new List<int>();

            foreach (var item in graph.vertices)
            {
                if (item.Value.value == "*" || item.Value.value == "+" || item.Value.value == "?")
                {
                    Edge edge  = graph.edges.Values.FirstOrDefault(x => x.Start == item.Key);
                    vertice vertice = graph.vertices.FirstOrDefault(x => x.Key ==edge.End).Value;
                    foreach (var follow in vertice.Last)
                    {
                        if(Follows.ContainsKey(follow))
                        {
                            Follows[follow].AddRange(vertice.First.FindAll(x => Follows[follow].Contains(x) == false));
                        }
                        else
                        {
                            Follows.Add(follow, vertice.First);
                        }
                    }
                }
                else  if (item.Value.value == ".")
                {
                    Edge edge = graph.edges.Values.FirstOrDefault(x => x.Start == item.Key);
                    Edge edge2 = graph.edges.Values.LastOrDefault(x => x.Start == item.Key);

                    vertice vertice = graph.vertices.FirstOrDefault(x => x.Key == edge.End).Value;
                    vertice vertice2 = graph.vertices.FirstOrDefault(x => x.Key == edge2.End).Value;
                    
                   foreach (var follow in vertice.Last)
                    {
                        if (Follows.ContainsKey(follow))
                        {
                            Follows[follow].AddRange(vertice2.First.FindAll(x => Follows[follow].Contains(x) == false));
                        }
                        else
                        {
                            Follows.Add(follow, vertice2.First);
                        }
                    }
                }
            }
            return Follows;
        }

        static List<List<int>> faltantes = new List<List<int>>();
        static Dictionary<int, List<int>> pr = new Dictionary<int, List<int>>();
        bool exit = false;
        


        public Dictionary<int, List<int>> CreateTransitions(Graph graph, Dictionary<string, List<List<string>>> transiciones, Dictionary<int, List<int>> p, int inicio)
        {
              int count = 1; bool existlist = false;
            if (!exit)
            {
                foreach (var item in p[inicio])
                {
                    string tempNT = graph.vertices.ToList().FirstOrDefault(x => x.Value.Last.ElementAt(0) == item && x.Value.Last.Count == 1 && x.Value.value != "*" && x.Value.value != "?" && x.Value.value != "." && x.Value.value != "+").Value.value;
                    foreach (var noter in transiciones)
                    {
                        if (noter.Key == tempNT)
                        {
                            if (noter.Value.Count == inicio - 1 || noter.Value.Count == 0)
                            {
                                if (Follows.Count > item)
                                {
                                    noter.Value.Add(Follows[item].ConvertAll<string>(x => x.ToString()));
                                    faltantes.Add(Follows[item]);
                                }
                            }
                            else
                            {
                                noter.Value[inicio - 1].AddRange(Follows[item].ConvertAll<string>(x => x.ToString()));
                                faltantes.Add(Follows[item]);
                            }
                        }
                        else
                        {
                            List<string> temp = new List<string>();
                            temp.Add("");
                            noter.Value.Add(temp);
                        }
                    }
                }
                foreach (var conjunto in faltantes)
                {
                    if (!p.Values.Contains(conjunto))
                    {
                        exit = false;
                        p.Add(p.Count + 1, conjunto);
                        p = CreateTransitions(graph, transiciones, p, p.Count);
                    }
                    else
                    {
                        exit = true;
                    }
                }
            }
            return p;
        }
    }
}
/////

