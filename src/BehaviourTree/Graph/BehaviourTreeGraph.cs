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

        /// <summary>
        /// Formats a behavior tree into a string representation.
        /// Note: IClock constraint removed - graph generation doesn't require time information.
        /// </summary>
        public static string Format<TContext>(IBehaviour<TContext> bt, FormatOptions option)
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

        /// <summary>
        /// Saves a behavior tree graph to a file.
        /// Note: IClock constraint removed - graph generation doesn't require time information.
        /// </summary>
        public static void Save<TContext>(IBehaviour<TContext> bt, string directoryPath, string fileName, FormatOptions option)
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