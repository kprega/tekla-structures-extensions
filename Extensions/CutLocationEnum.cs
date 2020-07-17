using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tekla.Structures.OpenApi
{
    /// <summary>
    /// Describes where the cut is located on its father part.
    /// </summary>
    public enum CutLocationEnum
    {
        /// <summary>
        /// Indicates that cut is not cutting through the part,
        /// </summary>
        Outside,
        /// <summary>
        /// Indicates that cut is located inside a part, i.e. doesn't cut any edge.
        /// </summary>
        Internal,
        /// <summary>
        /// Indicates that cut goes through an edge of the part.
        /// </summary>
        Edge,
        /// <summary>
        /// Indicates that cut goes through a corner of the part.
        /// </summary>
        Corner
    }
}
