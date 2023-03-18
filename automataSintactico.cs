using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static compilador.automataSintactico;

namespace compilador
{
   
    internal class automataSintactico
    {
        public SintacticTree tree { get; }
        Queue<(byte token, string valor)> tokens;
        Stack<Action> pendientes;
        public string msg { get; private set; }
        bool valido = true;

        public automataSintactico(Queue<(byte, string)> tokens)
        {
            this.tokens = tokens;
            tree = new SintacticTree();
            pendientes = new Stack<Action>();
            pendientes.Push(programa);
        }

        public bool analizar() 
        {
            try
            {
                programa();
            }
            catch (Exception e)
            {
                msg += e.Message + "\n";
                valido = false;
            }

            Console.WriteLine();
            Console.WriteLine(tree.analiceSemantic() ? "Correcto Semantico": "Incorrecto Semtantico" );
            Console.WriteLine();
            return valido;
            
        }

        void programa() 
        {
            tree.Root.value = msg += "<Programa>\n";
            SintacticNode actual;
            if (tokens.Peek().token == TknType.EOF) 
            {
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.EOF));
            }
            while (tokens.Peek().token != TknType.EOF) 
                sentencia(tree.Root);
            
        }
        void bloque(SintacticNode Padre) 
        {

            msg += "<Op_Llave>";

            if (tokens.Peek().token == TknType.EOF)
                return;

            SintacticNode actual = new SintacticNode(Padre, "Sentencias");
            Padre.childrens.Add(actual);
            sentencia(actual);
            
            if (tokens.Peek().token == TknType.EOF)
                return;

            if (tokens.Peek().valor != "}")
                return;

            Padre.childrens.Add(new SintacticNode(Padre, "}"));
            
            tokens.Dequeue();
            
            msg += "<Cl_Llave>";

        }
        void sentencia(SintacticNode Padre) 
        {
            SintacticNode actual;
            if (tokens.Peek().token == TknType.EOF)
                throw new Exception ("Error: Se esperaba Sentencia");

            bool puntoComa = false;
            
            msg += "<Sentencia>\n";
            
            (byte token, string value) = tokens.Peek();

            if (value == "}") 
                return;

            actual = new SintacticNode(Padre);
            actual.value = value;

            Padre.childrens.Add(actual);

            tokens.Dequeue();
           
            if (value == ";")
                return;

            switch (token)
            {
                //try - catch
                case TknType.ID: 
                    asignacion(value,actual); break;
                case TknType.DataType: Declaracion(value, actual); break;
                case TknType.SimIf:
                    SimpleSelectiva(value, actual);
                    puntoComa = true;
                    break;
                case TknType.MulIf: MultipleSelectiva(value, actual);
                    puntoComa = true; 
                    break;
                case TknType.Reservada: Reservadas(value, actual);  break;
                case TknType.Loop: 
                    Bucle(value,Padre);
                    puntoComa = true;
                    break;
                case TknType.Bloque:
                    
                    if (value != "{") 
                        throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Sentence));
                    actual.childrens.Add(new SintacticNode(actual, "{"));
                    puntoComa = true;
                    bloque(actual);
                    break;
                default:
                    throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Sentence));
            }

            if (puntoComa)
                return;

            if (tokens.Dequeue().valor != ";")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.SemiColon));


            //otra_sentencia();
        }

        private void DeclaracionPre(SintacticNode Padre) 
        {
            var token = tokens.Dequeue();
            if (token.token != TknType.DataType)
                throw new Exception("se esperaba tipo de datos");

            Declaracion(token.valor, Padre);
        
        }
        private void Declaracion(string value, SintacticNode Padre)
        {
            SintacticNode actual = new SintacticNode(Padre);
            Padre.childrens.Add(actual);
            Padre.value = "<Declaracion>";
            actual.value = value;
            if (tokens.Peek().token == TknType.EOF)
                return;
            
            msg += "<Declaracion>";
            
            variable(Padre);

            if (tokens.Peek().valor != "=")
                return;
            
            asignacion(Padre.childrens.Last().value ,Padre);
        }

        private void Bucle(string value, SintacticNode Padre)
        {
            SintacticNode actual = new SintacticNode(Padre);
            actual.value = value;

            if (tokens.Peek().token == TknType.EOF)
                return;

            msg += "<Bucle>\n";
            
            switch (value) 
            { 
                case "for": 
                    Parentesis(For, actual);
                    sentencia(actual);
                    break;
                case "while":
                    Parentesis(expresion, actual);
                    sentencia(actual);
                    break;
            }
        }

        private void For(SintacticNode Padre)
        {
            SintacticNode actual = new SintacticNode(Padre);
            sentencia(actual);
            expresion(actual);
            if (tokens.Dequeue().valor != ";")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.SemiColon));
            sentencia(actual);
        }

        //No esta probado que funcione tanto en el sintactico como en el semantico
        private void MultipleSelectiva(string valor, SintacticNode Padre)
        {
            if (valor != "switch")
                throw new Exception("Se esperaba Switch");

            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<SelectivaMultiple>\n";
            (byte token, string value) = tokens.Dequeue();
            Action<SintacticNode> evaluar;
            SintacticNode actual = new SintacticNode(Padre);
            switch (token)
            {
                case TknType.ID: evaluar = variable; break;
                case TknType.Constante: evaluar = constante; break;
                default: throw new Exception();
            }

            Parentesis(evaluar,Padre);

            (token, value) = tokens.Dequeue();

            while (tokens.Peek().valor != "}") cases(Padre);
        }

        private void cases(SintacticNode Padre)
        {
            if (tokens.Peek().token == TknType.EOF)
                return;

            string valor = tokens.Dequeue().valor;
            
            SintacticNode actual = new SintacticNode(Padre);

            msg += "<SelectivaCase>\n";

            if (valor != "case" & valor != "default")
                return;

            Action<SintacticNode> evaluar;

            (byte token, string value) = tokens.Peek();
            actual.value = valor;

            if (valor == "case") 
            {
                switch (token)
                {
                    case TknType.ID: evaluar = variable; break;
                    case TknType.Constante: evaluar = constante; break;
                    default: throw new Exception();
                }
                tokens.Dequeue();
            }

            if (tokens.Dequeue().valor != ":")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Case));
            
            sentencia(actual);
        }

        private void SimpleSelectiva(string value, SintacticNode Padre)
        {
            SintacticNode actual = new SintacticNode(Padre);
            Padre.childrens.Add(actual);

            if (tokens.Peek().token == TknType.EOF)
                return;
            
            msg += "<SelectivaSimple>\n";
            
            if (value != "if")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.simElse));
            actual.value = "if";

            Parentesis(expresion,actual);
            
            sentencia(actual);
            
            if (tokens.Peek().token == TknType.EOF)
                return;

            if (tokens.Peek().valor != "else")
                return;

            msg += "<SelectivaSimple_else>\n";
            
            (byte type, string value) token = tokens.Dequeue();
            
            SintacticNode siNo = new SintacticNode(Padre);
            siNo.value = "else";
            Padre.childrens.Add(siNo);
            sentencia(siNo);
        }

        //todas las palabras reservadas simples de (Accion)

        //si es parte de las reservadas pero no tiene alguna regla especifica es una
        //funcion Reservada(Identificador)

        //en caso de agregar funciones extra es poner cases para cada una.

        private void Reservadas(string value, SintacticNode Padre)
        {
            SintacticNode actual = new SintacticNode(Padre);

            if (tokens.Peek().token == TknType.EOF)
                return;
            actual.value = value;
            msg += "<PalabraReservada>";
            
            switch (value) 
            {
                case "using":
                    Parentesis(DeclaracionPre, actual); sentencia(actual); break;
                 
                    break;
                default: Parentesis(variable, actual); break;

            }   
            msg += "<Reservadas>\n";   
        }

        void asignacion(string? ID, SintacticNode Padre) 
        {
            SintacticNode actual = new SintacticNode(Padre);
            Padre.childrens.Add(actual);
            actual.value = "<Asignacion>";
            
            if (tokens.Peek().token == TknType.EOF)
                return;

            if (ID != null)
                actual.childrens.Add(new SintacticNode(Padre, ID, TknType.ID));
            
            msg += "<Asignacion>";

            msg += "<Variable>";
            if (tokens.Dequeue().valor != "=")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Asignacion)); //errores
            
            expresion(actual);
        }


        void escrituraSwitch(SintacticNode Padre) 
        {
            var token = tokens.Peek();
            switch (token.token) 
            {
                
                case TknType.ID:
                case TknType.Constante:
                    tokens.Dequeue();
                    expresion(Padre);
                    break;
                case TknType.Cadena:
                    tokens.Dequeue();
                    break;
                default:
                    if (token.valor == ")")
                        return;
                    throw new Exception("se esperaba algo para imprimir");
            }
            Padre.childrens.Add(new SintacticNode(Padre, token.valor, token.token));
        }

        private void cadena(SintacticNode Padre)
        {            
            if (tokens.Peek().token == TknType.EOF)
                return;

            (byte token, string value) = tokens.Dequeue();

            if (token != TknType.Cadena)
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Parentesis));
            
            Padre.childrens.Add(new SintacticNode(Padre, value, token));
            
            msg += "<Cadena>\n";

        }

        void variable(SintacticNode Padre) 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            (byte token, string value) = tokens.Dequeue();
            if ((token != TknType.ID))
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Variable));
            Padre.childrens.Add(new SintacticNode(Padre, value, token));
            
            msg += "<Variable>\n";

        }

        void expresion(SintacticNode Padre) 
        {
            SintacticNode actual = new SintacticNode(Padre);
            Padre.childrens.Add(actual);
            actual.value = "<Expresion>";

            if (tokens.Peek().token == TknType.EOF)
                return;

            msg += "<Expresion>\n";
            
            termino(actual);
            mas_termino(actual);
        }
        void termino(SintacticNode Padre) 
        {
            SintacticNode actual = new SintacticNode(Padre);
            Padre.childrens.Add(actual);
            actual.value = "<Termino>";
            if (tokens.Peek().token == TknType.EOF)
                return;

            msg += "<Termino>\n";
            
            factor(actual);
        }
        void mas_termino(SintacticNode Padre) 
        {
            SintacticNode actual;
            SintacticNode term;
            var token = tokens.Peek();
            if (token.token == TknType.EOF)
                return;
            
             msg += "<Mas_Terminos>";
            
            if (token.token != TknType.OpArit &
                token.token != TknType.OpRelacion )
                return;
            actual = new SintacticNode(Padre);
            actual.value = token.valor;
            actual.token = token.token;
            Padre.childrens.Last().NewFather(actual);
            
            Padre.childrens.Add(actual);

            tokens.Dequeue();

            termino(actual);
            mas_termino(actual);

        }
        void factor(SintacticNode Padre) 
        {
            SintacticNode actual = new SintacticNode(Padre);
            actual.value = "<Factor>";
            Padre.childrens.Add(actual);
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Factor>";
            
            (byte token, string value) = tokens.Peek();


            switch (token) 
            {
                case TknType.ID: variable(actual);           break;
                case TknType.Constante: constante(actual);   break;
                case TknType.OpBool: booleanos(actual);      break;
                case TknType.Cadena: cadena(actual);         break;
                case TknType.Bloque:
                    if (value == "(")
                        Parentesis(expresion,actual);
                    break;
                default:
                    throw new Exception("Se esperaba Factor");
            }
            
        }

        private void booleanos(SintacticNode Padre)
        {
            var token = tokens.Dequeue();

            if (token.token == TknType.EOF)
                return;
            msg += "<Booleano>";
            if (token.token != TknType.OpBool)
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Bool));//error
            Padre.childrens.Add(new SintacticNode(Padre, token.valor, token.token));
        }

        void constante(SintacticNode Padre) 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            var token = tokens.Dequeue();
            if (token.token != TknType.Constante)
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Constante));

            Padre.childrens.Add(new SintacticNode(Padre, token.valor, token.token));

            msg += "<Constante>";
        }
        void Parentesis(Action<SintacticNode> Expresion,SintacticNode Padre) 
        {
            
            (byte token, string valor) = tokens.Dequeue();

            if (!(valor == "("))
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.OpPare));
            
            SintacticNode actual = new SintacticNode(Padre);
            Padre.childrens.Add(actual);

            actual.childrens.Add(new SintacticNode(actual,valor,token));

            msg += "<Op_Paren>\n";

            Expresion(actual);

            (token, valor) = tokens.Dequeue();

            if (token == TknType.EOF)
                return;

            if (valor != ")")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.ClPare));
            
            msg += "<Cl_Paren>\n";

            actual.childrens.Add(new SintacticNode(actual, valor, token));

        }

    }
   
}
