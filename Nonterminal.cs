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
using System.Diagnostics;
using System.Reflection;

namespace Earley
{
	public sealed class Nonterminal: Symbol
	{
		readonly IDictionary<Production, object> ps =
			new Dictionary<Production, object>();

		public Nonterminal(params Production[] ps)
		{
			if (ps == null) throw new ArgumentNullException();

			for (int i = 0; i < ps.Length; i++)
			{
				if (ps[i] == null) throw new ArgumentNullException();
			}

			for (int i = 0; i < ps.Length; i++)
			{
				this.ps[ps[i]] = null;
			}
		}

		public void Add(Production p)
		{
			if (p == null) throw new ArgumentNullException();

			ps[p] = null;
		}

		internal int Count
		{
			get { return ps.Count;  }
		}

		internal bool Contains(Production p)
		{
			if (p == null) throw new ArgumentNullException();

			return ps.ContainsKey(p);
		}

		internal ICollection<Production> Productions
		{
			get { return ps.Keys; }
		}
	}
}
