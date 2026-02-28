using System;
using System.Collections.Generic;

namespace Beta.Gui.Elements;

public class GuiSearch
{
    /// <summary>
    ///     Performs a breadth-first search on GUI, starting from a given element.
    ///     Returns the first element can satisfies the given predicate, or default if none did.
    /// </summary>
    /// <param name="root">GuiElement to begin from.</param>
    /// <param name="predicate">Predicate to match.</param>
    /// <returns>First satisfying element or default.</returns>
    public static GuiElement? FirstOrDefault(GuiElement root, Predicate<GuiElement> predicate)
    {
        var queue = new Queue<GuiElement>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            var node = queue.Dequeue();

            // See if the element matches, then we've found it.
            if (predicate.Invoke(node))
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                queue.Enqueue(child);
            }
        }
        return default;
    }

    /// <summary>
    ///     Performs a breadth-first search on GUI, starting from a given element.
    ///     Returns all elements that satisfy the given predicate, or empty list if none did.
    /// </summary>
    /// <param name="root">GuiElement to begin from.</param>
    /// <param name="predicate">Predicate to match.</param>
    /// <returns>All matching elements.</returns>
    public static List<GuiElement> Where(GuiElement root, Predicate<GuiElement> predicate)
    {
        List<GuiElement> matches = [];
        var queue = new Queue<GuiElement>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            var node = queue.Dequeue();

            // See if the element matches, add it to the list.
            if (predicate.Invoke(node))
            {
                matches.Add(node);
            }

            foreach (var child in node.Children)
            {
                queue.Enqueue(child);
            }
        }
        return matches;
    }

    /// <summary>
    ///     Performs a breadth-first search on GUI, starting from a given element.
    ///     Returns all elements.
    /// </summary>
    /// <param name="root">GuiElement to begin from.</param>
    /// <returns>All elements.</returns>
    public static List<GuiElement> All(GuiElement root)
    {
        List<GuiElement> matches = [];
        var queue = new Queue<GuiElement>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            var node = queue.Dequeue();

            matches.Add(node);

            foreach (var child in node.Children)
            {
                queue.Enqueue(child);
            }
        }
        return matches;
    }


    /// <summary>
    ///     Performs a breadth-first search on GUI, starting from a given element.
    ///     Applies a specified action on every node.
    /// </summary>
    /// <param name="root">GuiElement to begin from.</param>
    /// <param name="action">Action to apply.</param>
    public static void ForEach(GuiElement root, Action<GuiElement> action)
    {
        var queue = new Queue<GuiElement>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            var node = queue.Dequeue();

            // Invoke specified action on the node.
            action.Invoke(node);
            
            foreach (var child in node.Children)
            {
                queue.Enqueue(child);
            }
        }
    }
}
