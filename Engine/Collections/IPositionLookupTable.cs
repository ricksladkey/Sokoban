using System;
using System.Collections.Generic;
using System.Text;

using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;

namespace Sokoban.Engine.Collections
{
    public interface IPositionLookupTable<TValue> : IDictionary<HashKey, TValue>
    {
        bool Validate { get; set; }
    }
}
