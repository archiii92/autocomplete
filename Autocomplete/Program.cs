using System;

namespace Autocomplete
{
    class Program
    {
        static void Main()
        {
            new Autocomplete("Input.txt", "Output.txt").MakeAutocomplete();

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }
}
