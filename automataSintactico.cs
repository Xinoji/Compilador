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
    public class SintacticTree
    {

        public SintacticNode Root { get; }
        public SintacticTree()
        {
            Root = new SintacticNode();
        }

    }
    public class SintacticNode
    {
        public SintacticNode Father { get; }
        public string value { get; set; }
        public LinkedList<SintacticNode> childrens { get; set; }
        public SintacticNode(ref SintacticNode Father)
        {
            childrens = new LinkedList<SintacticNode>();
            this.Father = Father;
        }
        public SintacticNode()
        {
            childrens = new LinkedList<SintacticNode>();
            this.Father = null;

        }
    }

    internal class automataSintactico
    {
        
        #region subclases
        public struct Token
        {
            byte type { get; set; }
            string valor { get; set; }
        }

       
        #endregion
        SintacticTree tree;
        LinkedListNode<SintacticNode> actualNode;
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
            return valido;
        }

        void programa() 
        {
            msg += "<Programa>\n";
            if (tokens.Peek().token == TknType.EOF) 
            {
                
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.EOF));
            }
            while (tokens.Peek().token != TknType.EOF)
                sentencia();
        }
        void bloque() 
        {
            msg += "<Op_Llave>";
            if (tokens.Peek().token == TknType.EOF)
                return;

            sentencia();
            
            if (tokens.Peek().token == TknType.EOF)
                return;

            tokens.Dequeue();
            msg += "<Cl_Llave>";

        }
        void sentencia() 
        {
            if (tokens.Peek().token == TknType.EOF)
                throw new Exception ("Error: Se esperaba Sentencia");
            bool puntoComa = false;
            msg += "<Sentencia>\n";


            (byte token, string value) = tokens.Dequeue();
            if (value == ";")
                return;

            switch (token)
            {
                //try - catch
                case TknType.ID: asignacion(); break;
                case TknType.DataType: Declaracion(); break;
                case TknType.SimIf:
                    if (value != "if")
                        throw new Exception(ErrorSyn.errorSyn(ErrorSyn.simElse));
                    SimpleSelectiva();
                    puntoComa = true;
                    break;
                case TknType.MulIf: MultipleSelectiva();
                    puntoComa = true; 
                    break;
                case TknType.Reservada: Reservadas(value);  break;
                case TknType.Entrada: lectura(); break;
                case TknType.Salida: escritura();  break;

                case TknType.Loop: 
                    Bucle(value);
                    puntoComa = true;
                    break;
                case TknType.Bloque:
                    if (value != "{") 
                        throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Sentence));
                    puntoComa = true;
                    bloque();
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

        private void Declaracion()
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Declaracion>";
            variable();
            if (tokens.Peek().valor != "=")
                return;
            msg += "<Asignacion>";
        }

        private void Bucle(string value)
        {

            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Bucle>\n";
            switch (value) 
            {
                case "for": Parentesis(For); break;
                case "while":
                    Parentesis(expresion);
                    sentencia();
                    break;
                case "foreach":
                    Parentesis(variable);
                    sentencia();
                    break;   
            }
        }

        private void For()
        {
            sentencia();
            expresion();
            if (tokens.Dequeue().valor != ";")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.SemiColon));
            sentencia();
        }

        private void MultipleSelectiva()
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<SelectivaMultiple>\n";
            (byte token, string value) = tokens.Dequeue();
            Action evaluar;

            switch (token)
            {
                case TknType.ID: evaluar = variable; break;
                case TknType.Constante: evaluar = constante; break;
                default: throw new Exception();
            }

            Parentesis(evaluar);

            while (tokens.Peek().valor != "}") cases();
            
            
        }

        private void cases()
        {
            if (tokens.Peek().token == TknType.EOF)
                return;

            string valor = tokens.Dequeue().valor;
            msg += "<SelectivaCase>\n";

            if (valor != "case" & valor != "default")
                return;

            if (valor == "case")
                switch (tokens.Dequeue()) 
                { 
                    
                }
                variable();

            tokens.Dequeue();
            if (tokens.Dequeue().valor != ":")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Case));
            sentencia();
        }

        private void SimpleSelectiva()
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<SelectivaSimple>\n";


            Parentesis(expresion);
            sentencia();
            
            if (tokens.Peek().token == TknType.EOF)
                return;

            if (tokens.Peek().valor != "else")
                return;
            msg += "<SelectivaSimple_else>\n";
            (byte type, string value) token = tokens.Dequeue();
            sentencia();
        }

        private void Reservadas(string value)
        {
            //todas las palabras reservadas simples de (Accion)

            //si es parte de las reservadas pero no tiene alguna regla especifica es una
            //funcion Reservada(Identificador)

            //en caso de agregar funciones extra es poner cases para cada una.

            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<PalabraReservada>";
            switch (value) 
            {
                case "using": Parentesis(Declaracion); break;
                default: Parentesis(variable); break;

            }


                

            msg += "<Reservadas>\n";
            tokens.Dequeue();
            
        }

        void otra_sentencia() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<otra_sentencia>\n";
            if (tokens.Peek().valor == ";")
            {
                tokens.Dequeue();
                sentencia();
                otra_sentencia();
            }
            
        }
        void asignacion() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Asignacion>";

            msg += "<Variable>";
            if (tokens.Dequeue().valor != "=")
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Asignacion)); //errores

            expresion();
        }
        void lectura() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Lectura>\n";
            Parentesis(variable);
        }
        void escritura() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Escritura>\n";
            Parentesis(cadena);
        }

        private void cadena()
        {
            if (tokens.Peek().token == TknType.EOF)
                return;

            (byte token, string value) = tokens.Dequeue();

            if (token != TknType.Cadena)
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Parentesis));
            
            msg += "<Cadena>\n";

        }

        void variable() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            if((tokens.Dequeue().token != TknType.ID))
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Variable));
            msg += "<Variable>\n";
            

        }
        void expresion() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Expresion>\n";
            
            termino();
            mas_termino();
        }
        void termino() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Termino>\n";
            factor();
            mas_factores();
        }
        void mas_termino() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            
            msg += "<Mas_Terminos>";
            
            if (tokens.Peek().token != TknType.OpArit &
                tokens.Peek().token != TknType.OpRelacion )
                return;

            tokens.Dequeue();
            termino();
            mas_termino();

        }
        void factor() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Factor>";
            (byte token, string value) = tokens.Peek();
            switch (token) 
            {
                case TknType.ID: variable(); break;
                case TknType.Constante: constante(); break;
                case TknType.OpBool: booleanos(); break;
                case TknType.Bloque:
                    if (value == "(")
                        Parentesis(expresion);
                    break;
            }
        }

        private void booleanos()
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Booleano>";
            if (tokens.Peek().token != TknType.OpBool)
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Bool));//error
        }

        //pendiente
        void mas_factores() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            msg += "<Mas_factores>";
        
        }
        void constante() 
        {
            if (tokens.Peek().token == TknType.EOF)
                return;
            byte token = tokens.Peek().token;
            if (token != TknType.Constante)
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.Constante));

            tokens.Dequeue();
            msg += "<Constante>";
        }
        void Parentesis(Action Expresion) 
        {

            if (!(tokens.Dequeue().valor == "("))
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.OpPare));
            msg += "<Op_Paren>\n";

            Expresion();

            if (tokens.Peek().token == TknType.EOF)
                return;

            if (!(tokens.Dequeue().valor == ")"))
                throw new Exception(ErrorSyn.errorSyn(ErrorSyn.ClPare));
            msg += "<Cl_Paren>\n";

        }



    }
   
}
