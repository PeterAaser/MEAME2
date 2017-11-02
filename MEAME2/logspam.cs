using System;
using System.Text;
using System.IO;

namespace MEAME2
{
  public static class log {

    public static void err(String s, String id = ""){
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($"[{id}Error]: {s}");
      Console.ResetColor();
    }

    public static void info(String s, String id = ""){
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine($"[{id}Info]: {s}");
      Console.ResetColor();
    }

    public static void ok(String s, String id = ""){
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"[{id}Info]: {s}\n\n");
      Console.ResetColor();
    }


  }
}
