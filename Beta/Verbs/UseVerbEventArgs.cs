using System;
using Beta.Actors;

namespace Beta.Verbs;

public class UseVerbEventArgs : EventArgs
{
    public Actor Item { get; }

    public UseVerbEventArgs(Actor item)
    {
        Item = item;
    }
}