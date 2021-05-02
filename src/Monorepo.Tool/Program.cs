using McMaster.Extensions.CommandLineUtils;
using System;

namespace Monorepo.Tool
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "dotnet-monorepo",
                Description = "Tools for managing a dotnet monorepo",
            };

            app.HelpOption(inherited: true);

            app.Command("version", VersionCommand.Configure);

            app.OnExecute(() =>
            {
                Console.WriteLine("Specify a subcommand");
                app.ShowHelp();
                return 1;
            });

            return app.Execute(args);
        }
    }
}
