using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.IO;
using Laboratorio1.ArbolHuffman;
using System.Text.RegularExpressions;
using Laboratorio1.ArbolHuffman.GraphClass;
using System.Linq;

namespace Laboratorio1.Controllers
{
    public class ReadTextController : Controller
    {
        public string FilePath = "";
        public string cadena = string.Empty;
        string newstring = string.Empty;
        string Content = string.Empty; string temporal = string.Empty;
        public List<string> error = new List<string>();
        public List<string> sets = new List<string>();
        string siguiente = "="; bool set = false; bool tok = false; bool act = false;
        public List<string> tokens = new List<string>();
        public List<string> actions = new List<string>();
        int linenum = 0; static Graph graph;
        public Dictionary<string, string> SetsDic = new Dictionary<string, string>();
        public Dictionary<string, List<string>> TokenDic = new Dictionary<string, List<string>>();
        public Dictionary<string, string> ActionsDic = new Dictionary<string, string>();
        static  ArbolHuff arbol = new ArbolHuff();
        static Dictionary<int, List<int>> Follows = new Dictionary<int, List<int>>();
        string validarcaracteres = string.Empty;
        static Dictionary<string, List<List<string>>> transitions = new Dictionary<string, List<List<string>>>();
        static Dictionary<int, List<int>> P = new Dictionary<int, List<int>>();
       
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MostrarFollow()
        {
            return View("~/Views/ArbolHuff/MostrarFollow.cshtml", Follows);
        }

        public ActionResult Transiciones()
        {
            string[,] tablatransiciones = new string[transitions.Count+1, P.Count+1];
            List<string> temp = new List<string>();
            int contador = -1;
            for(int j=0; j<= P.Values.Count; j++)
            {
                for (int i = 0; i <= transitions.Count; i ++)
                {
                    if(i==0 &&j>=1)
                    {
                        tablatransiciones[i, j] = string.Join(",", P.Values.ElementAt(j-1).ToArray());
                    }
                    else if(j>=1 && i>=1)
                    {
                        if (transitions.Values.ElementAt(i - 1).Count > contador)
                        {
                            tablatransiciones[i, j] = string.Join(",", transitions.Values.ElementAt(i - 1).ElementAt(contador).ToArray());
                        }
                    }
                    else if(j ==0 && i>=1)
                    {
                        tablatransiciones[i, j] =  transitions.Keys.ElementAt(i-1);
                    }
                }
                contador++;
            }
            return View("~/Views/ArbolHuff/MostrarTransiciones.cshtml", tablatransiciones);
        }
        public ActionResult MostrarAsignaciones()
        {
            return View("~/Views/ArbolHuff/MostrarArbol.cshtml", graph.vertices);
        }
        public Dictionary<string, int> Diccionario_CaracteresHuff = new Dictionary<string, int>();

        public ActionResult Read(string filename)
        {
            List<string> Text_archivo = new List<string>();
            var path = Path.Combine(Server.MapPath("~/Archivo"), filename);
            FilePath = Server.MapPath("~/Archivo");
            string line = string.Empty; bool errorfound = false;

            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream && !errorfound)
                    {
                        line = reader.ReadLine();
                        linenum++;
                        if (error.Count == 0)
                        {
                            if (!set)
                            {
                                line = ValidacionesGeneral(line);
                            }
                            if (set && line != "" && !tok)
                            {
                                string replacement = Regex.Replace(line, @"\t|\n|\r| ", "");
                                ValidacionesSets(replacement);
                            }
                            else if (tok)
                            {
                                string replacement = Regex.Replace(line, @"\t|\n|\r| ", "");
                                ValidacionesTokens(replacement);
                            }
                        }
                        else
                        {
                            
                            errorfound = true;
                            if (error.ElementAt(0) != "fin")
                            {
                                ViewBag.Message = error.ElementAt(0);
                                return View();
                            }
                            else {
                               return RedirectToAction("MostrarAsignaciones");
                            }
                            
                        }
                    }
                }
            }
            return View();
        }
        public ActionResult Error()
        {
            ViewBag.Message = error.ElementAt(0);
            return View();
        }
        public string ValidacionesGeneral(string linea)
        {
            string replacement = Regex.Replace(linea, @"\t|\n|\r| ", "");
            if (replacement.Substring(0, 4).ToLower() == "sets")
            {
                set = true;
                return "";
            }
            else
            {
                error.Add("Error en linea: " + linenum.ToString() + ". Debe de iniciar con indicar los SETS colocando y definiendolos");
            }
            return replacement;
        }
        //Tomo caracteres especiales, cadena y hasta encontrar el caracter esperado
        public string ValidarCaracteres(List<string> specialCharacters, string cadena, string esperado)
        {
            int inicio = 0; int length = 1; int cadenalength = 1;
            cadenalength = cadena.Length;
            for (int i = 0; i < cadenalength; i++)
            {
                if (cadena.Length >= 6)
                {
                    if (cadena.Substring(0, i) == "TOKENS")
                    {
                        return "TOKENS";
                    }
                    else if (cadena.Substring(0, 6) == "TOKENS")
                    {
                        return "TOKENS";
                    }

                }
                if (esperado == "." && cadena.Length >= (length + 1))
                {
                    if (cadena.Substring(i, length + 1) == "..")
                    {
                        string palabra = "..";
                        return palabra;
                    }
                    else if (cadena.Substring(i, length) == "+")
                    {
                        string palabra = "+";
                        return palabra;
                    }
                    else if (cadena.Substring(i, length) == "." && cadena.Substring((i + 1), length) != ".")
                    {
                        error.Add("Error en linea: " + linenum.ToString() + " se esperaban 2 puntos consecutivos. Se obtuvo " + cadena.Substring(i, length + 1));
                        return "error";
                    }
                    else if (!specialCharacters.Contains(cadena.Substring(i, length)))
                    {
                        if (cadena.Contains("="))
                        { return "="; }
                        else { temporal = cadena; return ""; }
                    }

                }
                if (cadena.Substring(0, 1) == "'")
                {
                    abre = true;
                    return cadena.Substring(0, 1);
                }

                else if (cadena.Substring(i, length) == esperado)
                {
                    if (!abre && i > 0 && esperado == "'")
                    {
                        error.Add("Error en linea: " + linenum.ToString() + ". la comilla ' se esperaba antes de  " + cadena.Substring(0, i));
                        return "error";
                    }
                    else
                    {
                        string palabra = cadena.Substring(0, i);
                        return palabra;
                    }

                }
                if (specialCharacters.Contains(cadena.Substring(i, length)) && cadena.Substring(i, length) != esperado)
                {
                    error.Add("Error en linea: " + linenum.ToString() + ". Se esperaba el caracter " + esperado);
                    return "error";
                }
            }
            return "";
        }
        bool abre = false;
        public List<string> ValidacionesSets(string cadena)
        {
            //cadena += Cadena;
            SpecialCharacters specialCharacters = new SpecialCharacters();
            string nuevacadena = string.Empty;
            nuevacadena += cadena;
            string respuesta = string.Empty;
            string nombre = string.Empty;
            bool terminar = false;
            if (cadena == "=")
            {
                sets.Add(temporal);
                temporal = ""; terminar = true; Content = ""; siguiente = "'";
            }
            while (!terminar)
            {
                switch (siguiente)
                {
                    case "=":
                        nombre = ValidarCaracteres(specialCharacters.SpecialOnSets, nuevacadena, "=");
                        if (nombre == "TOKENS")
                        {
                            terminar = true; tok = true;
                            siguiente = "=";
                            nuevacadena = nuevacadena.Length > 0 ? nuevacadena.Substring(nombre.Length, nuevacadena.Length - nombre.Length) : "";
                        }
                        else
                        {
                            sets.Add(nombre);
                            nuevacadena = nuevacadena.Length > 0 ? nuevacadena.Substring(nombre.Length + 1, nuevacadena.Length - nombre.Length - 1) : "";
                            siguiente = "'";
                        }
                        break;
                    case "'":
                        respuesta = ValidarCaracteres(specialCharacters.SpecialOnSets, nuevacadena, "'");
                        if (respuesta == "TOKENS")
                        {
                            siguiente = "="; terminar = true; tok = true;
                            if (abre) { nuevacadena = nuevacadena.Length > 0 ? nuevacadena.Substring(nombre.Length, nuevacadena.Length - nombre.Length) : ""; }
                            else
                            {
                                error.Add("Error en linea: " + linenum);
                            }
                        }
                        if (respuesta != "error")
                        {
                            if (abre)
                            {
                                Content += "'";
                                respuesta = nuevacadena.Length > 0 ? ValidarCaracteres(specialCharacters.SpecialOnSets, nuevacadena.Substring(1, nuevacadena.Length - 1), "'") : "";
                                if (respuesta.Length == 0)
                                {
                                    error.Add("Error en linea: " + linenum + " no se encontro la ' la cual fue abierta pero no cerrada, ambas comillas deben de encontrarse en la misma linea"); terminar = true;
                                }
                                else
                                {
                                    Content += respuesta + "'";
                                }
                            }
                            else
                            {
                                error.Add("Error en linea: " + linenum + "se encontro la ' la cual nunca fue abierta");
                            }

                        }
                        else
                        {
                            terminar = true;
                        }
                        nuevacadena = nuevacadena.Length > 0 && respuesta != "error" ? nuevacadena.Substring(respuesta.Length + 2, nuevacadena.Length - respuesta.Length - 2) : "";
                        if (nuevacadena.Length == 0)
                        { terminar = true; }
                        siguiente = ".";
                        break;
                    case ".":
                        respuesta = ValidarCaracteres(specialCharacters.SpecialOnSets, nuevacadena, ".");
                        if (respuesta == "TOKENS")
                        {
                            siguiente = "=";
                            terminar = true; tok = true;
                            nuevacadena = nuevacadena.Length > 0 ? nuevacadena.Substring(nombre.Length, nuevacadena.Length - nombre.Length) : "";
                        }
                        else if (respuesta == "error")
                        {
                            terminar = true;
                        }
                        else
                        {
                            if (respuesta == "=")
                            { siguiente = "="; }
                            else if (respuesta == "")
                            { terminar = true; }
                            else if (respuesta != "error" && respuesta != "=")
                            {
                                Content += respuesta;
                                nuevacadena = nuevacadena.Length > 0 ? nuevacadena.Substring(respuesta.Length, nuevacadena.Length - respuesta.Length) : "";
                                siguiente = "'";
                            }

                        }
                        if (nuevacadena.Length == 0)
                        { terminar = true; }
                        break;

                }
            }
            return sets;
        }

        public string ValidarCaracteresTokens(List<string> specialCharacters, string cadena, string esperado)
        {
            int length = 1; int cadenalength = 1; string contenido = string.Empty;
            cadenalength = cadena.Length;

            for (int i = 0; i <= cadenalength; i++)
            {
                if (cadena.Substring(0, i).ToUpper() == "TOKEN")
                {
                    return "TOKEN";
                }
                if (cadena.Substring(0, i).ToUpper() == "ACTIONS")
                {
                    return "ACTIONS";
                }
                if (sets.Contains(cadena.Substring(0, i)))
                {
                    tokens.Add(cadena.Substring(0, i));
                    return cadena.Substring(0, i);
                }
                if (specialCharacters.Contains(cadena.Substring(0, 1)))
                {
                    tokens.Add(cadena.Substring(0, 1));
                    return cadena.Substring(0, 1);
                }
                else if (cadena.Substring(i, length) == "=" && esperado != "'")
                {
                    string palabra = cadena.Substring(0, i);
                    return palabra;
                }
                else if (cadena.Length >= i + length)
                {
                    if (cadena.Substring(i, length) == esperado)
                    {
                        if (cadena.Substring(0, 1) == "'")
                        {
                            return "'";
                        }
                        else
                        {
                            tokens.Add(cadena.Substring(0, i));
                            return cadena.Substring(0, i);
                        }
                    }
                }
            }
            return "";
        }
        string numero = string.Empty;
        public List<string> ValidacionesTokens(string cadena)
        {
            SpecialCharacters specialCharacters = new SpecialCharacters();

            newstring += cadena;
            string respuesta = string.Empty;
            string nombre = string.Empty;
            string Content = string.Empty; bool terminar = false;
            if (!newstring.Contains("TOKEN") && !newstring.Contains(siguiente))
            {
                if (newstring.Contains("ACTIONS"))
                { siguiente = "'"; }
                else { terminar = true; }
            }
            else if (newstring.Contains("TOKEN") && !newstring.Contains(siguiente))
            {
                if (!newstring.Contains("="))
                {
                    terminar = true;
                }
                else if (newstring.Contains("ACTIONS"))
                { siguiente = "'"; }
            }
            while (!terminar && newstring.Length > 0)
            {
                switch (siguiente)
                {
                    case "=":
                        if (numero != "")
                        {
                            if (!TokenDic.Keys.Contains(numero))
                            {
                                TokenDic.Add(numero, tokens.GetRange(0, tokens.Count));
                            }
                            else
                            {
                                error.Add("El numero de token " + numero + " ya existe, por lo cual no puede ser creado");
                            }
                            tokens.Clear();
                        }
                        nombre = ValidarCaracteresTokens(specialCharacters.SpecialOnTokens, newstring, "=");
                        if (nombre.ToUpper() == "TOKEN")
                        {
                            siguiente = "=";
                            newstring = newstring.Length > 0 ? newstring.Substring(nombre.Length, newstring.Length - nombre.Length) : "";
                            numero = newstring.Length > 0 ? ValidarCaracteresTokens(specialCharacters.SpecialOnTokens, newstring, "=") : "";
                            newstring = newstring.Length > 0 ? newstring.Substring(numero.Length + 1, newstring.Length - numero.Length - 1) : "";
                        }
                        siguiente = "'";
                        break;
                    case "'":
                        respuesta = ValidarCaracteresTokens(specialCharacters.SpecialOnTokens, newstring, "'");
                        if (newstring.Substring(0, 1) == "'")
                        {
                            newstring = newstring.Length > 0 ? newstring.Substring(1, newstring.Length - 1) : "";
                            respuesta = ValidarCaracteresTokens(specialCharacters.SpecialOnTokens, newstring, "'");
                            newstring = newstring.Length > 0 ? newstring.Substring(respuesta.Length + 1, newstring.Length - (respuesta.Length + 1)) : "";
                            siguiente = "'";
                        }
                        else if (sets.Contains(respuesta))
                        {
                            newstring = newstring.Length > 0 ? newstring.Substring(respuesta.Length, newstring.Length - respuesta.Length) : "";
                        }
                        else if (respuesta.ToUpper() == "TOKEN")
                        {
                            TokenDic.Add(numero, tokens.GetRange(0, tokens.Count));
                            tokens.Clear(); numero = "";
                            siguiente = "=";
                        }
                        else if (respuesta.ToUpper() == "ACTIONS")
                        {
                            if (!TokenDic.Keys.Contains(numero))
                            {
                                TokenDic.Add(numero, tokens.GetRange(0, tokens.Count));
                            }
                            else
                            {
                                error.Add("El numero de token " + numero + " ya existe, por lo cual no puede ser creado");
                            }
                            tokens.Clear(); numero = "";
                            terminar = true;
                            ValidarActions();
                        }
                        else
                        {
                            newstring = newstring.Length > 0 ? newstring.Substring(respuesta.Length, newstring.Length - respuesta.Length) : "";
                        }

                        break;
                }
            }

            return sets;
        }

        public List<string> ValidarActions()
        {
            bool concatenacion = true; int count = 0;
            List<string> listadeNodostemp = new List<string>();
            List<List<string>> listaexpresionestemp = new List<List<string>>();
            List<string> listadeNodos = new List<string>();
           // List<string> listadeNodos = new List<string>{ "(", "(", "a", "|", "b", ")", "+", "|", "c", "*", ")", "+" };
            foreach (var item in TokenDic.Values)
            {
                listadeNodos.Clear();
                listadeNodostemp.Clear();
                foreach (var dato in item)
                {
                    if (dato != "(" && dato != "*" && dato != "?" && dato != "+" && dato != ")" && dato != "|")
                    {
                        if (concatenacion == true)
                        {
                            listadeNodostemp.Clear();
                            listadeNodostemp.AddRange(listadeNodos);
                            listadeNodos.Clear();
                            if (listadeNodostemp.Count > 0)
                            {
                                listadeNodos.Add("(");
                                listadeNodos = listadeNodos.Concat(listadeNodostemp).ToList();
                                listadeNodos.Add(".");
                                listadeNodos.Add(dato);
                                listadeNodos.Add(")");
                            }
                            else
                            {
                                listadeNodos.Add(dato);
                            }
                            concatenacion = true;
                        }
                        else
                        {
                            listadeNodostemp.Clear();
                            listadeNodostemp.AddRange(listadeNodos);
                            listadeNodos.Clear();
                            listadeNodos.Add("(");
                            listadeNodos = listadeNodos.Concat(listadeNodostemp).ToList();
                            listadeNodos.Add(dato);
                            listadeNodos.Add(")");
                            concatenacion = true;
                        }
                    }
                    else if (dato == "|")
                    {

                        listadeNodos.Add(dato);
                        concatenacion = false;
                    }
                    else
                    {
                        listadeNodos.Add(dato);
                    }
                }
                count++;
                if (count <= TokenDic.Values.Count)
                {
                    listadeNodostemp.Clear();
                    listadeNodostemp.AddRange(listadeNodos);
                    listadeNodos.Clear();
                    if (listadeNodostemp.ElementAt(listadeNodostemp.Count - 1) != ")")
                    {
                        listadeNodos.Add("(");
                        listadeNodos.AddRange(listadeNodostemp);
                        listadeNodos.Add(")");
                    }
                    else { listadeNodos.AddRange(listadeNodostemp); }
                    List<string> temp = new List<string>();
                    temp.AddRange(listadeNodos);
                    listaexpresionestemp.Add(temp);
                    concatenacion = true;
                }
            }
            listadeNodos.Clear();
            foreach (var expresion in listaexpresionestemp)
            {
                listadeNodostemp.Clear();
                listadeNodostemp.AddRange(listadeNodos);
                listadeNodos.Clear();
                if (listadeNodostemp.Count > 0)
                {
                    listadeNodos.Add("(");
                    listadeNodos = listadeNodos.Concat(listadeNodostemp).ToList();
                    listadeNodos.Add("|");
                    listadeNodos.AddRange(expresion);
                    listadeNodos.Add(")");
                }
                else
                {
                    listadeNodos.AddRange(expresion);
                }
            }
            //List<string> listadeNodos = new List<string> {  "a" };
            // arbol.prueba();
             graph = arbol.CreateTree(listadeNodos, 0, 0);
            //Asigno first
            vertice graph1 = arbol.AssignFirst(graph, graph.head);
            //Asigno Last
            vertice graph2 = arbol.AssignLast(graph, graph.head);
            int nt = arbol.NoTerminales;
            Follows = arbol.AssignFollow(graph);
           
            foreach (var vertu in graph.vertices)
            {
                List<List<string>> TempData = new List<List<string>>();
                if (vertu.Value.value != "*" && vertu.Value.value != "+" && vertu.Value.value != "?" && vertu.Value.value != "|" && vertu.Value.value != "." && transitions.Keys.Contains(vertu.Value.value) == false)
                {
                    transitions.Add(vertu.Value.value, TempData);
                }
            }

            
            P.Add(1, graph.vertices.ToList().FirstOrDefault(x => x.Key == 1).Value.First);
            try
            {
                P = arbol.CreateTransitions(graph, transitions, P, 1);
            }
            catch
            {
                string p = string.Empty;
            }
            error.Add("fin");
            return actions;
        }
        private List<string> FilesUploaded()
        {
            var dir = new System.IO.DirectoryInfo(Server.MapPath("~/Archivo"));
            System.IO.FileInfo[] fileNames1 = dir.GetFiles("*.huff");
            List<string> filesupld = new List<string>();
            foreach (var file1 in fileNames1)
            {
                filesupld.Add(file1.Name);
            }
            return filesupld;
        }

        public FileResult Download(string TxtName)
        {
            var FileVirtualPath = "Archivo/" + TxtName;
            return File(FileVirtualPath, "application/force- download", Path.GetFileName(FileVirtualPath));
        }
    }
}
