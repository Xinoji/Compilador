using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace compilador
{
    internal class ErrorSyn
    {
        public const byte EOF = 0;
        public const byte Sentence = 1;
        public const byte SemiColon = 3;
        public const byte simElse = 2;
        public const byte Asignacion = 4;
        public const byte Variable = 5;
        public const byte Parentesis = 6;
        public const byte Case = 7;
        public const byte OpPare = 8;
        public const byte ClPare = 9;
        public const byte Bool = 10;
        public const byte Constante = 11;  

        public static string errorSyn(byte error) 
        {
            string msg = "Error: ";
            switch (error)
            {
                case EOF: return msg + "No hay codigo que ejecutar";
                case Sentence: return msg + "Sentencia Invalida";
                case simElse: return msg + "Falta de if en else";
                case SemiColon: return msg + "Falta de ;";
                case Asignacion: return msg + "Falta de '=' para asignar";
                case Variable: return msg + "Se esperaba un Identificador / Variable";
                case Parentesis: return msg + "Se esperaba un Cierre de parentesis";
                case Case: return msg + "se esperaba : en case";
                case OpPare: return msg + "se esperaba '('";
                case ClPare: return msg + "se esperaba un ')'";
                case Bool: return msg + "Se esperaba un booleano";
                case Constante: return msg +"Se esperaba un constante numerico";
            }


            return msg + "Desconocido";
        }
    }
    internal class TknType
    {
        //Principales Lexico
        public const byte Reservada     = 0; 
        public const byte ID            = 1; 
        public const byte Constante     = 2; 
        public const byte Cadena        = 3; 
        public const byte Simbolo       = 4; 
        public const byte Encabezado    = 5; 
        public const byte OpArit        = 6; 
        public const byte SimIf         = 7; 
        public const byte MulIf         = 8; 
        public const byte Loop          = 9; 
        public const byte OpBool        = 10; 
        public const byte OpRelacion    = 11; //ver si es necesario que este reptido
        public const byte Bloque        = 12; 
        public const byte EOF           = 13; 
        public const byte Entrada       = 14; 
        public const byte Salida        = 15;
        public const byte DataType      = 16;
        public const byte BOOL = 16;
        public const byte INT = 17;
        public const byte CHAR = 18;
        
        public const byte Desconocido   = 255;
    }

    public class SintacticTree
    {

        public SintacticNode Root { get; }
        public SintacticTree()
        {
            Root = new SintacticNode();
        }
        public void print()
        {
            Console.Write(Root.value);
            foreach (SintacticNode child in Root.childrens)
                print(child);
        }

        private void print(SintacticNode Padre)
        {
            Console.Write(Padre.value);
            if (Padre.childrens == null)
                return;

            foreach (SintacticNode child in Padre.childrens)
                print(child);
        }
        public bool analiceSemantic()
        {
            try
            {
                analiceSemantic(Root);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // 1 Verificar que las operaciones usen mismos operandos o operandos validos (Subir token tipo).
        // 2 Verificar no se use una variable previamente sin estar inicializada.
        // 3 Verificar que una asignacion sea de tipo correcto.
        private void analiceSemantic(SintacticNode Actual)
        {

            //Console.WriteLine(Actual.value);

            switch (Actual.value)
            {
                case "<Declaracion>":
                    Declare(Actual);
                    if (Actual.childrens.Count == 3)
                        analiceSemantic(Actual.childrens[2]);
                    return;

                case "<Asignacion>":
                    analiceSemantic(Actual.childrens[1]);

                    Asign(Actual.childrens[0].value, Actual.childrens[1].value, Actual);
                    return;
                    break;
            }

            if (Actual.childrens != null)    
                foreach (SintacticNode child in Actual.childrens)
                    analiceSemantic(child);

            if (Actual.token == TknType.ID)
                if(!Asigned(Actual.value, Actual))
                    throw new Exception("Variable usada sin asignar");
            
            //Si es una expresion toma el token de su ultimo hijo
            if (Actual.value == "<Expresion>" |
                Actual.value == "<Factor>" |
                Actual.value == "<Termino>")
                Actual.value = Actual.childrens[0].value;

            //Si es Operador Verificar que ambos lados sean compatibles.    
            if (Actual.token == TknType.OpArit |
                Actual.token == TknType.OpRelacion)
                if (!IsCompatible(Actual.childrens[0].value, Actual.childrens[1].value))
                    throw new Exception("Operacion con tipos incompatibles");
                else
                    Actual.value = Actual.childrens[0].value;

            switch (Actual.token) 
            {
                case TknType.Constante: Actual.value = "int"; break;
                case TknType.Cadena: Actual.value = "char"; break;
                case TknType.ID: Actual.value = Declared(Actual.value, Actual); break;
            }
        }

        private bool Asigned(string value, SintacticNode actual)
        {
            if (actual.SimboleTable != null)
                if (actual.SimboleTable.ContainsKey(value))
                    return actual.SimboleTable[value].asigned;

            if (actual.Father != null)
                return Asigned(value, actual.Father);

            throw new Exception("Var");
        }

        private bool IsCompatible(string type1, string type2)
        {
            switch (type1) 
            {
                case "int":
                    if (type2 == "int" | type2 == "float")
                        return true;
                    break;
                case "char":
                    if (type2 == "int" | type2 == "float")
                        return false;
                    break;
                default:
                    break;
            }

            return false;
        }

        private void Asign(string value, string type, SintacticNode actual)
        {
            if (actual.SimboleTable != null)
                if (actual.SimboleTable.ContainsKey(value))
                    if (actual.SimboleTable[value].type != type)
                        throw new Exception("Tipo incorrecto");
                    else
                    {
                        actual.SimboleTable[value].asigned = true;
                        return;
                    }


            if (actual.Father == null)
                throw new Exception("Variable no declarada");

            Asign(value, type, actual.Father);
        }

        private void Declare(SintacticNode actual)
        {
            string value = actual.childrens[1].value;
            if (Declared(value, actual) != null)
                throw new Exception("Declare: Variable Previamente Declarada");

            actual.Father.SimboleTable.Add(value,new Value(actual.childrens[0].value, false));
        }

        private string? Declared(string value, SintacticNode Actual)
        {
            if (Actual.SimboleTable != null)
                if (Actual.SimboleTable.ContainsKey(value))
                    return Actual.SimboleTable[value].type;

            if (Actual.Father != null)
                return Declared(value, Actual.Father);

            return null;
        }
    }

    public class Value 
    { 
        public string type { get; set; }
        public bool asigned { get; set; }

        public Value(string type, bool asigned)
        {
            this.type = type;
            this.asigned = asigned;
        }   

    }
    public class SintacticNode
    {
        public SintacticNode Father { get; }
        public string value { get; set; }
        public byte token;
        public List<SintacticNode> childrens { get; set; }
        public Dictionary<string, Value> SimboleTable { get; set; }
        public SintacticNode(SintacticNode Father)
        {
            childrens = new List<SintacticNode>();
            this.Father = Father;
            SimboleTable = new Dictionary<string, Value>();
            token = TknType.Desconocido;

        }
        public SintacticNode(SintacticNode Father, string value)
        {
            this.Father = Father;
            this.value = value;
            token = TknType.Desconocido;
            childrens = new List<SintacticNode>();
            SimboleTable = new Dictionary<string, Value>();

        }
        public SintacticNode(SintacticNode Father, string value, byte token)
        {
            this.Father = Father;
            this.value = value;
            this.token = token;
        }

        public SintacticNode()
        {
            childrens = new List<SintacticNode>();
            this.Father = null;
            token = TknType.Desconocido;
            SimboleTable = new Dictionary<string, Value>();
        }

        public void NewFather(SintacticNode Father) 
        {
            this.Father.childrens.Remove(this);
            Father.childrens.Add(this);
        }
    }
}
