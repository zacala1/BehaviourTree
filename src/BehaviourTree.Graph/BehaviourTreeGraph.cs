using System.IO;

namespace BehaviourTree.Graph
{
    /// <summary>
    /// Provides methods for formatting and saving behavior tree visualizations.
    /// </summary>
    public static class BehaviourTreeGraph
    {
        /// <summary>
        /// Supported output formats for behavior tree visualization.
        /// </summary>
        public enum FormatOptions
        {
            /// <summary>
            /// PlantUML mindmap diagram format.
            /// </summary>
            Plantuml,

            /// <summary>
            /// Dan Abad text format for behavior trees.
            /// </summary>
            DanAbad
        }

        /// <summary>
        /// Formats a behavior tree into a string representation.
        /// Note: IClock constraint removed - graph generation doesn't require time information.
        /// </summary>
        /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
        /// <param name="bt">Behavior tree to format</param>
        /// <param name="option">Output format option</param>
        /// <returns>Formatted string representation of the tree</returns>
        public static string? Format<TContext>(IBehaviour<TContext> bt, FormatOptions option)
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
        /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
        /// <param name="bt">Behavior tree to save</param>
        /// <param name="directoryPath">Directory path to save the file</param>
        /// <param name="fileName">File name without extension</param>
        /// <param name="option">Output format option</param>
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