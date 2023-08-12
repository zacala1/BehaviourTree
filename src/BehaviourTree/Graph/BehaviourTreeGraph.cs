using System.IO;

namespace BehaviourTree.Graph
{
    public static class BehaviourTreeGraph
    {
        public enum FormatOptions
        {
            Plantuml,
            DanAbad
        }

        public static string Format<TContext>(IBehaviour<TContext> bt, FormatOptions option) where TContext : IClock
        {
            switch (option)
            {
                case FormatOptions.Plantuml:
                    return BehaviourTreeGraphPlantuml.Format(bt);

                case FormatOptions.DanAbad:
                    return BehaviourTreeGraphDanAbad.Format(bt);

                default:
                    return null;
            }
        }

        public static void Save<TContext>(IBehaviour<TContext> bt, string directoryPath, string fileName, FormatOptions option) where TContext : IClock
        {
            switch (option)
            {
                case FormatOptions.Plantuml:
                    Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(Path.Combine(directoryPath, fileName + ".puml"), Format(bt, option));
                    break;

                case FormatOptions.DanAbad:
                    Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(Path.Combine(directoryPath, fileName + ".tree"), Format(bt, option));
                    break;
            }
        }
    }
}