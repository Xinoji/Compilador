using compilador;
using System.Text.RegularExpressions;

internal class Program
{
    private static void Main(string[] args)
    {
        do
        {
            automotaCompilador a = new automotaCompilador();

            Console.WriteLine("Linea a evaluar:");
            bool v = a.evaluate(input: Console.ReadLine());
            Console.WriteLine("\n");
            Console.WriteLine(a.msg);
            automataSintactico b = new automataSintactico(a.tokens);
            v = b.analizar();
            Console.WriteLine(b.msg);
            Console.WriteLine($" Es {(v ? "Correcto":"Incorrecto")} Sintacticamente");
            Console.ReadLine();
            Console.Clear();
        } while (true);

    }
}