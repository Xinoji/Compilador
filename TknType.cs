using System;
using System.Collections.Generic;
using System.Linq;
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
        public const byte Desconocido   = 255;
    }
}
