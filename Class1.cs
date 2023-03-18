using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compilador
{
    internal class AutomataSemantico
    {
        #region atributos
        SintacticTree Arbol;
        

        #endregion
        public AutomataSemantico(SintacticTree Arbol) 
        {
            this.Arbol = Arbol;
        }

        public bool analizar() 
        {
            try
            {
                Arbol.analiceSemantic();
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }



    }
}
