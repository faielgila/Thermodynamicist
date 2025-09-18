using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

Console.WriteLine("Starting Thermodynamicist.Core testing console...\n\n");
var dirConsole = Environment.CurrentDirectory;
Path.GetDirectoryName(dirConsole);

TestBinaryMultiphaseSystem.Test(dirConsole);
//TestHomogeneousMixture.Test(dirConsole);