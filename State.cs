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

namespace Earley
{
	internal sealed class State
	{
		readonly IList<Item> items = new List<Item>();

		internal State() { }

		internal void Add(Item item)
		{
			if (item == null) throw new ArgumentNullException();
			if (Contains(item)) throw new InvalidOperationException();

			items.Add(item);
		}

		internal bool Contains(Item item)
		{
			if (item == null) throw new ArgumentNullException();
			return items.Contains(item);
		}

		internal int Count
		{
			get { return items.Count; }
		}

		// gets items in this state with the dot before non-terminals that
		// contain the specified production
		internal IList<Item> GetItems(Production production)
		{
			if (production == null) throw new ArgumentNullException();

			IList<Item> result = new List<Item>();

			foreach (Item item in items)
			{
				if (!item.AtEnd &&
					item.Symbol is Nonterminal &&
					((Nonterminal)item.Symbol).Contains(production))
				{
					result.Add(item);
				}
			}

			return result;
		}

		internal Item this[int index]
		{
			get { return items[index]; }
		}

		internal Item Import(Item item)
		{
			if (item == null) throw new ArgumentNullException();

			if (Contains(item))
			{
				Item existing = this[item];

				if (!existing.IsImportCompatible(item))
				{
					throw new ArgumentException();
				}

				return existing;
			}
			else
			{
				Add(item);
				return item;
			}
		}

		private Item this[Item item]
		{
			get
			{
				foreach (Item existing in items)
				{
					if (existing.Equals(item))
					{
						return existing;
					}
				}

				return null;
			}
		}
	}
}