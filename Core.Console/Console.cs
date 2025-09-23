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
AnsiConsole.MarkupLine("[orangered1]Created by[/]");
var font = FigletFont.Load("Y:\\Repos\\faielgila\\Thermodynamicist\\Core.Console\\slant.flf");
var figlet = new FigletText(font, "Troposoft");
figlet.Color(Color.OrangeRed1);
AnsiConsole.Write(figlet);
AnsiConsole.MarkupLine("[orangered1]						engineering software[/]");
AnsiConsole.WriteLine();


TestBinaryMultiphaseSystem.Test(dirConsole);
//TestHomogeneousMixture.Test(dirConsole);

AnsiConsole.MarkupLine("\n\n[skyblue1]Thermodynamicist.Core test completed.[/]");