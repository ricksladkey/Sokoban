/*
 * Copyright (c) 2010 by Rick Sladkey
 * 
 * This program is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban.Engine.Core
{
    public class MutipleEnumerable<T> : IEnumerable<T>
    {
        private List<IEnumerable<T>> enumerators;

        public MutipleEnumerable()
        {
            enumerators = new List<IEnumerable<T>>();
        }

        public MutipleEnumerable(IEnumerable<T> enumerator)
            : this()
        {
            enumerators.Add(enumerator);
        }

        public MutipleEnumerable(IEnumerable<T> enumerator1, IEnumerable<T> enumerator2)
            : this()
        {
            enumerators.Add(enumerator1);
            enumerators.Add(enumerator2);
        }

        public void AddEnumerator(IEnumerable<T> enumerator)
        {
            enumerators.Add(enumerator);
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            foreach (IEnumerable<T> enumerator in enumerators)
            {
                foreach (T value in enumerator)
                {
                    yield return value;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
