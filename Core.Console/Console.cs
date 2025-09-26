using Core.Console;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

AnsiConsole.WriteLine("Starting Thermodynamicist.Core testing console...");
var dirConsole = Environment.CurrentDirectory;
dirConsole = "Y:\\Repos\\faielgila\\Thermodynamicist\\Core.Console\\csv\\";
AnsiConsole.MarkupLine($"CSV output directory is [blue]{dirConsole}[/]");

AnsiConsole.WriteLine();
//var font = FigletFont.Load("Y:\\Repos\\faielgila\\Thermodynamicist\\Core.Console\\slant.flf");
//var figlet = new FigletText(font, "Troposoft");
//figlet.Color(Color.OrangeRed1);
//AnsiConsole.Write(figlet);
var troposoft = "  ______                                         ____ __\r\n /_  __/_____ ____   ____   ____   _____ ____   / __// /_\r\n  / /  / ___// __ \\ / __ \\ / __ \\ / ___// __ \\ / /_ / __/\r\n / /  / /   / /_/ // /_/ // /_/ /(___ )/ /_/ // __// /_\r\n/_/  /_/    \\____// .___/ \\____//____/ \\____//_/   \\__/\r\n                 /_/";
AnsiConsole.Markup($"[orangered1]{troposoft}[/]");
AnsiConsole.MarkupLine("[orangered1]		  engineering software[/]");
AnsiConsole.WriteLine();


//TestHomogeneousMixture.Test(dirConsole);
//TestBinaryMultiphaseSystem.Test(dirConsole);
TestBinaryPhaseDiagram.Test(dirConsole);

AnsiConsole.MarkupLine("\n\n[skyblue1]Thermodynamicist.Core test completed.[/]");