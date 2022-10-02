using Dragonhill.DevOps.Cli;
using System.CommandLine;

var rootCommand = new RootCommand("Dragonhill devops tool");
rootCommand.AddCommand(MetadataCommands.CreateCommand());

return rootCommand.InvokeAsync(args).Result;
