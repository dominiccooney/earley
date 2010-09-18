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

namespace Earley
{
	internal sealed class Item
	{
		readonly Production production;
		readonly int index;
		readonly State parent;
		readonly IList<object> derivations;
		readonly Item prevItem;

		internal Item(Production production, State parent)
			: this(production, 0, parent, null) { }

		private Item(Production production, int index, State parent, Item prevItem)
		{
			if (production == null || parent == null)
			{
				throw new ArgumentNullException();
			}

			if (index < 0 || index > production.Symbols.Count)
			{
				throw new ArgumentOutOfRangeException();
			}

			if (index == 0 && prevItem != null)
			{
				throw new ArgumentException();
			}

			if (index > 0 && prevItem == null)
			{
				throw new ArgumentNullException();
			}

			this.production = production;
			this.index = index;
			this.parent = parent;
			this.prevItem = prevItem;
			this.derivations = index == 0 ? null : new List<object>();
		}

		public override bool Equals(object obj)
		{
			Item other = obj as Item;

			return other != null &&
				   other.production == production &&
				   other.index == index &&
				   other.parent == parent &&
				   other.prevItem == prevItem;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (((parent.GetHashCode() * 31)
						 + production.GetHashCode()) * 31)
					    + (prevItem == null ? 0 : prevItem.GetHashCode()) * 32
					   + index;
			}
		}

		internal void Add(int ch)
		{
			if (AtStart || !(prevItem.Symbol is Terminal))
			{
				throw new InvalidOperationException();
			}

			if (!((Terminal)prevItem.Symbol).Contains(ch))
			{
				throw new ArgumentException();
			}

			derivations.Add(ch);
		}

		internal void Add(Item it)
		{
			if (AtStart || !(prevItem.Symbol is Nonterminal))
			{
				throw new InvalidOperationException();
			}

			if (!((Nonterminal)prevItem.Symbol).Contains(it.production))
			{
				throw new ArgumentException();
			}

			derivations.Add(it);
		}

		private bool AtStart
		{
			get { return index == 0; }
		}

		internal bool AtEnd
		{
			get { return index == production.Symbols.Count; }
		}

		internal Item NextItem
		{
			get
			{
				if (AtEnd) throw new InvalidOperationException();
				return new Item(production, index + 1, parent, this);
			}
		}

		internal State Parent
		{
			get { return parent; }
		}

		internal Production Production
		{
			get { return production; }
		}

		internal IList<object> Reduce()
		{
			Reduction reduction = production as Reduction;

			if (!AtEnd || reduction == null)
			{
				throw new InvalidOperationException();
			}

			IList<object> result = new List<object>();

			foreach (object[] args in ReduceWorker())
			{
				result.Add(reduction.Apply(args));
			}

			return result;
		}

		// reduces all the derivations for *this* symbol
		private IList<object> ReduceSymbol()
		{
			if (AtStart) throw new InvalidOperationException();

			List<object> result = new List<object>();

			if (prevItem.Symbol is Terminal)
			{
				result.AddRange(derivations);
			}
			else if (prevItem.Symbol is Nonterminal)
			{
				foreach (Item item in derivations)
				{
					result.AddRange(item.Reduce());
				}
			}

			return result;
		}

		private IList<object[]> ReduceWorker()
		{
			IList<object[]> result = new List<object[]>();

			if (production.Symbols.Count == 0)
			{
				result.Add(new object[0]);
			}
			else if (prevItem.AtStart)
			{
				foreach (object value in ReduceSymbol())
				{
					object[] args = new object[production.Symbols.Count];
					args[0] = value;
					result.Add(args);
				}
			}
			else
			{
				IList<object> symbolReductions = ReduceSymbol();

				foreach (object[] prefix in prevItem.ReduceWorker())
				{
					foreach (object value in symbolReductions)
					{
						object[] args = prefix.Clone() as object[];
						args[prevItem.index] = value;
						result.Add(args);
					}
				}
			}

			return result;
		}

		internal Symbol Symbol
		{
			get
			{
				if (AtEnd) throw new InvalidOperationException();
				return production.Symbols[index];
			}
		}

		// whether the other item is empty and just being used as a key,
		// or whether it has a different previous item, already has
		// derivations, etc. See State::Import.
		internal bool IsImportCompatible(Item other)
		{
			if (other == null) throw new ArgumentNullException();

			return Equals(other) &&
				   ((other.derivations == null && derivations == null) ||
				    other.derivations.Count == 0);
		}
	}
}
