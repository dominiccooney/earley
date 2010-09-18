// Copyright 2004 Dominic Cooney. All Rights Reserved.

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Earley
{
	public sealed class Terminal: Symbol
	{
		public static readonly Terminal Eof = new Terminal(-1);

		private static int[] CharVToIntV(char[] cs)
		{
			return cs == null
				? null
				: Array.ConvertAll<char, int>(
					cs,
					delegate(char ch) { return ch; });
		}

		readonly IDictionary<int, object> cs = new Dictionary<int, object>();

		private Terminal(params int[] cs)
		{
			if (cs == null) throw new ArgumentNullException();

			foreach (int c in cs)
			{
				this.cs[c] = null;
			}
		}

		public Terminal(params char[] cs) : this(CharVToIntV(cs)) { }

		public override bool Equals(object obj)
		{
			Terminal other = obj as Terminal;

			if (other != null && other.Count == Count)
			{
				foreach (int c in cs.Keys)
				{
					if (!other.Contains(c)) return false;
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			int sum = 0;

			foreach (int c in cs.Keys)
			{
				unchecked { sum += c; }
			}

			return sum;
		}

		internal bool Contains(int ch)
		{
			return cs.ContainsKey(ch);
		}

		internal int Count
		{
			get { return cs.Count; }
		}

		public override string ToString()
		{
			StringBuilder accum = new StringBuilder();

			List<int> ics = new List<int>(cs.Keys);

			if (ics.Contains(-1))
			{
				accum.Append("(EOF)");
				ics.Remove(-1);
			}

			List<char> ccs =
				ics.ConvertAll<char>(delegate(int x) { return (char)x; });
			ccs.Sort();
			ccs.ForEach(delegate(char ch) { accum.Append(ch); });
			
			return accum.ToString();
		}
	}
}