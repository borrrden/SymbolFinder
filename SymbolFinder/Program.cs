// 
//  Program.cs
// 
//  Copyright (c) 2018 Couchbase, Inc All rights reserved.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using ConsoleTables;

using McMaster.Extensions.CommandLineUtils;

namespace SymbolFinder
{
    [HelpOption]
    class Program
    {
        #region Variables

        private readonly IConsole _console;

        #endregion

        #region Properties

        [Option(Description = "The list of symbols to check for non-existence")]
        [FileExists]
        [Required]
        public string SymbolFile { get; set; }

        [Option(Description = "The amount of logging to perform while running")]
        public (bool HasValue, TraceLevel Level) Trace { get; set; }

        #endregion

        #region Constructors

        public Program(IConsole console)
        {
            _console = console;
        }

        #endregion

        #region Private Methods

        private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private void Log(TraceLevel level, string message)
        {
            if (!Trace.HasValue || level > Trace.Level) {
                return;
            }

            _console.WriteLine(message);
        }

        private void Log(TraceLevel level, ConsoleTable table)
        {
            if (!Trace.HasValue || level > Trace.Level) {
                return;
            }

            _console.WriteLine(table);
        }

        private int OnExecute()
        {
            Couchbase.Lite.Support.NetDesktop.Activate();

            var foundCount = 0;
            var symbols = File.ReadAllLines(SymbolFile);
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(x => x.FullName.StartsWith("Couchbase.Lite,"));

            
            var itemTable = new ConsoleTable("Item", "Exists");
            foreach (var symbol in symbols) {
                var split = symbol.Split(':');
                if (split.Length == 1 || split[0] == "c") {
                    var exists = LookupClass(assembly, split.Last());
                    if (exists) {
                        foundCount++;
                    }

                    itemTable.AddRow(split.Last().Split('.').Last(), exists ? "O" : "X");
                } else if (split[0] == "p") {
                    var components = split[1].Split(".");
                    var exists = LookupProperty(assembly, String.Join(".", components.Take(components.Length - 1)),
                        components.Last());

                    if (exists) {
                        foundCount++;
                    }

                    itemTable.AddRow(String.Join(".", components.TakeLast(2)), exists ? "O" : "X");
                }
            }

            Log(TraceLevel.Info, itemTable);

            return foundCount;
        }

        private static bool LookupClass(Assembly assembly, string name)
        {
            var type = assembly.GetType(name);
            return type?.IsClass == true;
        }

        private static bool LookupProperty(Assembly assembly, string className, string name)
        {
            var type = assembly.GetType(className);
            if (type?.IsClass == false) {
                return false;
            }

            return type.GetProperty(name) != null;
        }

        #endregion
    }
}
