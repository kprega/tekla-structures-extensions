using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Tekla.Structures.OpenApi
{
    public static class Extensions
    {
        /// <summary>
        /// Creates list of objects with given type from object implementing IEnumerator interface.
        /// </summary>
        /// <typeparam name="T">Type of objects in the list.</typeparam>
        /// <param name="enumerator">Object implementing IEnumerator interface.</param>
        /// <returns>List of objects of given type.</returns>
        public static List<T> ToList<T>(this IEnumerator enumerator)
        {
            var list = new List<T>();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T) list.Add((T)enumerator.Current);
            }
            return list;
        }

        /// <summary>
        /// Gets given object's phase.
        /// </summary>
        /// <param name="modelObject">Model object to read phase from.</param>
        /// <returns>Model object's phase.</returns>
        public static Phase GetPhase(this ModelObject modelObject)
        {
            var phase = new Phase();
            modelObject.GetPhase(out phase);
            return phase;
        }
    }
}
