using System;
using System.Collections.Generic;
using System.Text;

namespace BehaviourTree.Serialization
{
    /// <summary>
    /// Serializes a behavior tree to a JSON string for visualization, debugging, or storage.
    /// </summary>
    public static class BehaviourTreeSerializer
    {
        /// <summary>
        /// Serializes a behavior tree to a JSON string.
        /// </summary>
        public static string ToJson<TContext>(IBehaviour<TContext> root, bool indented = true)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            var sb = new StringBuilder(1024);
            SerializeNode(root, sb, indented, 0);
            return sb.ToString();
        }

        private static void SerializeNode<TContext>(IBehaviour<TContext> node, StringBuilder sb, bool indented, int depth)
        {
            var indent = indented ? new string(' ', depth * 2) : "";
            var nl = indented ? "\n" : "";
            var childIndent = indented ? new string(' ', (depth + 1) * 2) : "";

            sb.Append(indent).Append('{').Append(nl);

            // Core properties
            AppendProperty(sb, childIndent, "id", node.Id.ToString(), nl);
            AppendProperty(sb, childIndent, "name", EscapeJson(node.Name), nl);

            var typeName = (node is BaseBehaviour bb) ? bb.TypeName : node.GetType().Name;
            AppendProperty(sb, childIndent, "type", EscapeJson(typeName), nl);

            // Node category
            var category = GetCategory(node);
            AppendProperty(sb, childIndent, "category", category, nl);

            // Children
            var children = GetChildren(node);
            if (children != null && children.Count > 0)
            {
                sb.Append(childIndent).Append("\"children\": [").Append(nl);
                for (int i = 0; i < children.Count; i++)
                {
                    SerializeNode(children[i], sb, indented, depth + 2);
                    if (i < children.Count - 1)
                        sb.Append(',');
                    sb.Append(nl);
                }
                sb.Append(childIndent).Append(']').Append(nl);
            }
            else
            {
                // Remove trailing comma from last property
                RemoveTrailingComma(sb, nl);
                sb.Append(nl);
            }

            sb.Append(indent).Append('}');
        }

        private static void AppendProperty(StringBuilder sb, string indent, string key, string value, string nl)
        {
            sb.Append(indent).Append('"').Append(key).Append("\": \"").Append(value).Append("\",").Append(nl);
        }

        private static void RemoveTrailingComma(StringBuilder sb, string nl)
        {
            // Find and remove the last comma before the newline
            var len = sb.Length;
            for (int i = len - 1; i >= 0; i--)
            {
                if (sb[i] == ',')
                {
                    sb.Remove(i, 1);
                    break;
                }
                if (sb[i] != '\n' && sb[i] != '\r' && sb[i] != ' ')
                    break;
            }
        }

        private static string GetCategory<TContext>(IBehaviour<TContext> node)
        {
            if (node is Composites.CompositeBehaviour<TContext>)
                return "composite";
            if (node is Decorators.DecoratorBehaviour<TContext>)
                return "decorator";
            return "leaf";
        }

        private static List<IBehaviour<TContext>>? GetChildren<TContext>(IBehaviour<TContext> node)
        {
            if (node is Composites.CompositeBehaviour<TContext> composite)
            {
                var list = new List<IBehaviour<TContext>>(composite.ChildNodes.Count);
                foreach (var child in composite.ChildNodes)
                    list.Add(child);
                return list;
            }

            if (node is Decorators.DecoratorBehaviour<TContext> decorator)
            {
                return new List<IBehaviour<TContext>> { decorator.Child };
            }

            return null;
        }

        private static string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
