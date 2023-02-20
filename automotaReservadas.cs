using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using compilador;

internal class automotaCompilador
  {
    #region reservada-caracter
    //lista de operadores aritmeticos
    private static string[] Entrada = {
        "readChar", "readLine"
    };

    private static string[] Salida = {
        "write", "writeLine"
    };

    private static string[] aritmeticos = {
        "+"  , "-"  , "*"  , "/"  , "%"  , "**" , "//", "="
    };

    private static string[] operadorBloque = { 
        "(", ")", "{", "}", "[", "]"
    };

    //lista de selectiva simple
    private static string[] selectiva_simple = {
        "if", "else"
    };

    private static string[] selectiva_multiple = {
        "switch", "case", "select", "default"
    };

    private static string[] ciclos = {
        "for", "while", "foreach"
    };

    private static string[] booleanos = {
        "True", "False", "true", "false"
    };

    private static string[] operadores_relacion = {
        "<<"  , ">>" , "<"  , ">"  , "<="  , ">=" , "==", "!="
    };

    private static string[] tipo = {
        "bool","int", "char", "void"
    };
    //lista de palabras reservadas
    private static string[] reservadas = 
    {
        "asm", "auto", "bool", "break", "case", "catch", "char", "class", "const", "const_cast",
        "continue", "default", "delete", "do", "double", "dynamic_cast", "else", "enum", "explicit",
        "export", "extern", "false", "float", "for", "friend", "goto", "if", "inline", "int", "long",
        "mutable", "namespace", "new", "operator", "private", "protected", "public", "register", "reinterpret_cast",
        "return", "short", "signed", "sizeof", "static", "static_cast", "struct", "switch", "template", "this",
        "throw", "true", "try", "typedef", "typeid", "typename", "union", "unsigned", "using", "virtual", "volatile", "wchar_t", "while"
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
    #endregion
    //lista dinamica de los ID que se va a encontrar
    private List<string> id = new List<string>();
    public Queue<(byte token, string valor)> tokens { get; }

    public automotaCompilador() 
    {
        tokens = new Queue<(byte, string)>();
    }

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
        if (tipo.Contains(input))
            return TknType.DataType;

        if (Entrada.Contains(input))
            return TknType.Entrada;

        if (Salida.Contains(input))
            return TknType.Salida;

        if (operadorBloque.Contains(input))
            return TknType.Bloque;
        //comprueba si es un signo de los registrados
        if (signos.Contains(input))
            return TknType.Simbolo;

        if (aritmeticos.Contains(input))
            return TknType.OpArit;

        if (selectiva_simple.Contains(input))
            return TknType.SimIf;
        
        if (selectiva_multiple.Contains(input))
            return TknType.MulIf;

        if (ciclos.Contains(input))
            return TknType.Loop;

        if (booleanos.Contains(input))
            return TknType.OpBool;

        if (operadores_relacion.Contains(input))
            return TknType.OpRelacion;

        //comprueba si empieza por caracter para ver si es reservada, ID nuevo o ID registrado 
        if (new Regex(@"^[a-zA-Z_]$").IsMatch(input[0].ToString()))
        {
            //verifica si es una palabra reservada
            if (reservadas.Contains(input))
                return TknType.Reservada;

            //verfica si cumple con el formato de ID para ser id nuevo
            if (new Regex(@"^\w{0,64}$").IsMatch(input))
            {
                id.Add(input);
                return TknType.ID;
            }


        }
        //verifica si es una constante numerica
        if (new Regex(@"^\d(x[a-fA-f0-9]*|\.?)[0-9]*$").IsMatch(input))
            return TknType.Constante;

        //verifica si es una cadena que se encuentra entre comillas
        if (new Regex($@"\{'"'}.*\{'"'}").IsMatch(input.ToString()))
            return TknType.Cadena;   

        //verifica si es un encabezado
        if (new Regex($@"<.*>").IsMatch(input[0].ToString()))
            return TknType.Encabezado;
        
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

                    queue.Enqueue(repeatUntil('"', ref input));
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
                    case TknType.Reservada: filtro = "Reservada";               break;
                    case TknType.ID:        filtro = "ID";                      break;
                    case TknType.Constante: filtro = "Constante";               break;
                    case TknType.Cadena:    filtro = "Cadena";                  break;
                    case TknType.Simbolo:   filtro = "Simbolo";                 break;
                    case TknType.Encabezado:filtro = "Encabezado";              break;
                    case TknType.OpArit:    filtro = "Operador aritmetico";     break;
                    case TknType.SimIf:     filtro = "Selectiva simple";        break;
                    case TknType.MulIf:     filtro = "Selectiva multiple";      break;
                    case TknType.Loop:      filtro = "Ciclo";                   break;
                    case TknType.OpBool:    filtro = "Operador booleano";       break;
                    case TknType.OpRelacion:filtro = "Operador de relación";    break;
                    case TknType.Bloque:     filtro = "Bloque"; break;
                    case TknType.Desconocido:
                    default:                filtro = "Desconocido";             break;

                }
            msg += $"El Valor \"{buffer}\" es {filtro}\n";
            tokens.Enqueue((tipo, buffer));
           // state = transition[state, tipo];
        }
        tokens.Enqueue((TknType.EOF, "\0"));
        return accepted[state];
        
        }

    
    public void reset() 
    {
        
        state = 0;
        msg = "";
        tokens.Clear();
        id.Clear();
    }
  }

