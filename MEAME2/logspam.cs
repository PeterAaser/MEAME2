using System;
using System.Text;
using System.IO;

namespace MEAME2
{
		public static class log {

				public static void err(String s, String id = ""){
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"[{id}Error]: {s}\n");
						Console.ResetColor();
				}

				public static void info(String s, String id = ""){
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine($"[{id}Info]: {s}");
						Console.ResetColor();
				}

				public static void ok(String s, String id = ""){
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"[{id}Info]: {s}");
						Console.ResetColor();
				}

				public static void msg(String s){
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.WriteLine($"[Info]: {s}");
						Console.ResetColor();
				}

				static int hasLogged = 0;

				public static void infoOnce(String s, String id = ""){
						if(hasLogged == 0){
								Console.ForegroundColor = ConsoleColor.Yellow;
								Console.WriteLine($"[{id}Info]: {s}");
								Console.ResetColor();
								hasLogged = 1;
						}
				}
		}
}
