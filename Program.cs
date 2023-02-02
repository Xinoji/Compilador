using System.Text.RegularExpressions;

internal class Program
{
    private static void Main(string[] args)
    {
        do{
            automotaCompilador a = new automotaCompilador();
            Console.WriteLine("Linea a evaluar:");
            bool v = a.evaluate(input: Console.ReadLine());
            Console.WriteLine("\n");
            Console.WriteLine(a.msg);
            Console.ReadLine();
            Console.Clear();
        }while(true);

    }
}