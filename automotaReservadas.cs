using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

  internal class automotaCompilador
  {
    //lista de operadores aritmeticos
    private static string[] aritmeticos = {
        "++", "--", "+"  , "-"  , "*"  , "/"  , "%"  , "**" , "//", "("  , ")", "="
    };

    //lista de selectiva simple
    private static string[] selectiva_simple = {
        "if", "else", "else if"
    };

    private static string[] selectiva_multiple = {
        "switch", "case", "select"
    };

    private static string[] ciclos = {
        "for", "while", "foreach", "dowhile"
    };

    private static string[] booleanos = {
        "True", "False", "true", "false"
    };

    private static string[] operadores_relacion = {
        "<<"  , ">>" , "<"  , ">"  , "<="  , ">=" , "==", "!="
    };

    //lista de palabras reservadas, falta acompletar palabras para ser las 30
    private static string[] reservadas = 
    {
        "asm", "auto", "bool", "break", "case", "catch", "char", "class", "const", "const_cast",
        "continue", "default", "delete", "do", "double", "dynamic_cast", "else", "enum", "explicit",
        "export", "extern", "false", "float", "for", "friend", "goto", "if", "inline", "int", "long",
        "mutable", "namespace", "new", "operator", "private", "protected", "public", "register", "reinterpret_cast",
        "return", "short", "signed", "sizeof", "static", "static_cast", "struct", "switch", "template", "this",
        "throw", "true", "try", "typedef", "typeid", "typename", "union", "unsigned", "using", "virtual", "void", "volatile", "wchar_t", "while"
    };

    //lista de signos permitidos
    private static string[] signos = 
    {
        "["  , "]"  , "{"  , "}"  , "."   , "->" ,
        "++" , "--" , "&"  , "~"  , "!"  , "%"   , 
        "!=" , "^"  , "|"  , "&&" , "||" , "?"  , ":"   , ";"  , "...",
        "*=" , "/=" , "%=" , "+=" , "-=" , "<<=" , ">>=",
        "&=" , "^=" , "|=" , ","  , "#"  , "##" ,
        "<:" , ":>" , "<%" , "%>" , "%:%:"
    };

    //lista dinamica de los ID que se va a encontrar
    private List<string> id = new List<string>();
    
    //repite al buffer hasta encontrar un caracter en especifico
    private string repeatUntil(char c, ref string input) {
        string buffer = "";
        do
        {
            buffer += input[0];
            input = input.Substring(1);
        } while (input[0] != c);
        return buffer + c;
    }

    //verifica si el buffer tiene contenido para agregarlo a la fila
    private void bufferCheck(Queue<string> queue, ref string buffer) {
        if (buffer.Length > 0)
        {
            queue.Enqueue(buffer);
        }
        buffer = "";

    }

    //comparar para saber que tipo es el signo usuado
    byte Compare(string input)
    {
        //comprueba si es un signo de los registrados
        if (signos.Contains(input))
            return 5;

        if (aritmeticos.Contains(input))
            return 7;

        if (selectiva_simple.Contains(input))
            return 8;
        
        if (selectiva_multiple.Contains(input))
            return 9;

        if (ciclos.Contains(input))
            return 10;

        if (booleanos.Contains(input))
            return 11;

        if (operadores_relacion.Contains(input))
            return 12;

        //comprueba si empieza por caracter para ver si es reservada, ID nuevo o ID registrado 
        if (new Regex(@"^[a-zA-Z_]$").IsMatch(input[0].ToString()))
        {
            //verifica si es una palabra reservada
            if (reservadas.Contains(input))
                return 0;
            
            //verifica si es un ID que aperecio previo
            if (id.Contains(input))
                return 2;

            //verfica si cumple con el formato de ID para ser id nuevo
            if (new Regex(@"^\w{0,64}$").IsMatch(input))
            {
                id.Add(input);
                return 1;
            }


        }
        //verifica si es una constante numerica
        if (new Regex(@"^\d(x[a-fA-f0-9]*|\.?)[0-9]*$").IsMatch(input))
            return 3;

        //verifica si es una cadena que se encuentra entre comillas
        if (new Regex($@"{"\""}.*{"\""}").IsMatch(input[0].ToString()))
            return 4;   

        //verifica si es un encabezado
        if (new Regex($@"<.*>").IsMatch(input[0].ToString()))
            return 6;
        
        //en caso contrario retorna desconocido
        return 255;
        }

    //estados para el automata, cuando se diseñe el Jflap se cambia la tabla
    private readonly int[,] transition = 
    { 
        //
        { 1, 1,-1,-1}, //q0
        { 2, 2, 6,-1}, //q1
        { 3, 3, 6,-1}, //q2
        { 4, 4, 6,-1}, //q3
        { 5, 5, 6,-1}, //q4
        { -1,-1,6,-1}, //q5
        { 1 ,1 ,0,-1}  //q6
                           
    };

    //aceptacion de estados del automata, cuando se diseñe el Jflap se cambia
    private readonly bool[] accepted = 
    {
            false,     //q0
            true,      //q1
            true,      //q2
            true,      //q3
            true,      //q4
            true,      //q5
            false      //q6
    };    

    //mensaje guardado del automata
    public string msg = "";

    //estado inicial
    private int state = 0;



    public bool evaluate(string input) 
    {
        //fila en la que estaran las entradas
        Queue<string> queue = new Queue<string>();

        //buffer para apilar caracteres
        string buffer = "";

        //usado cuando se necesita comprobar retorno
        int temp = 0;

        //separar la entrada, filtrando por estos criterios
        do
        {
            switch (input[0])
            {

                case '"' :
                case '\'':
                    bufferCheck(queue, ref buffer);

                    queue.Enqueue('"' + repeatUntil('"', ref input));
                    input = input.Substring(1);
                    break;

                case ' ':
                    bufferCheck(queue, ref buffer);
                    input = input.Substring(1);
                    break;
                case '{':
                case '}':
                case '(':
                case ')':
                case '*':
                case '+':
                case '-':
                case ';':
                    bufferCheck(queue, ref buffer);

                    queue.Enqueue(input[0].ToString());
                    input = input.Substring(1);
                    break;
                case '/':
                    bufferCheck(queue, ref buffer);
                    if (input[1] == '/')
                    {
                        //para comentarios
                    }
                    
                    break;
                case '!':
                    bufferCheck(queue, ref buffer);
                    input = input.Substring(1);
                    break;
                case '>':
                case '<':
                case '=':
                    bufferCheck(queue, ref buffer);
                    temp = input[1] == '=' ? 2 : 1 ;

                    queue.Enqueue(input.Substring(0, temp));
                    input = input.Substring(temp);
                    break;

                default:
                    buffer += input[0];
                    input = input.Substring(1);
                    break;
                        
            }
        }
        while (input.Count() > 0);

        bufferCheck(queue, ref buffer);

        byte tipo;
        string filtro;
        while (queue.Count() > 0)
        {
            buffer = queue.Dequeue();

            tipo = Compare(buffer);
                switch (tipo)
                {
                    case 0: filtro = "Reservada";               break;
                    case 1: filtro = "ID nuevo";                break;
                    case 2: filtro = "ID";                      break;
                    case 3: filtro = "Constante";               break;
                    case 4: filtro = "Operadores de relacion";  break;
                    case 5: filtro = "Simbolo";                 break;
                    case 6: filtro = "Encabezado";              break;
                    case 7: filtro = "Operador aritmetico";     break;
                    case 8: filtro = "Selectiva simple";        break;
                    case 9: filtro = "Selectiva multiple";      break;
                    case 10: filtro = "Ciclo";                  break;
                    case 11: filtro = "Operador booleano";      break;
                    case 12: filtro = "Operador de relación";   break;
                    default:filtro = "Desconocido";             break;

                }
            msg += $"El Valor \"{buffer}\" es {filtro}\n";
           // state = transition[state, tipo];
        }
        
        return accepted[state];
        
        }


    public void reset() 
    {
        state = 0;
        msg = "";
        id.Clear();
    }
  }

