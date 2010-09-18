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

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Earley
{
	[TestFixture]
	public class TestState
	{
		[Test]
		public void Create()
		{
			State state = new State();
		}

		[Test]
		public void Add()
		{
			State state = new State();
			Item item = new Item(new Production(), state);
			state.Add(item);
			Assert.AreEqual(1, state.Count);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddNull()
		{
			new State().Add(null);
		}

		[Test]
		public void Enumerate()
		{
			State state = new State();
			Item item = new Item(new Production(), state);
			state.Add(item);
			Assert.AreEqual(item, state[0]);

			for (int i = 0; i < state.Count; i++)
			{
				item = state[i];
			}
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ItemNegativeIndex()
		{
			Item item = new State()[-1];
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ItemPastEnd()
		{
			Item item = new State()[0];
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void AddDuplicate()
		{
			State state = new State();
			Production p = new Production(new Terminal('x', 'y'));
			Item itemA = new Item(p, state);
			itemA.Add('x');
			state.Add(itemA);

			Item itemB = new Item(p, state);
			itemB.Add('y');
			state.Add(itemB);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void GetItemsNullProduction()
		{
			new State().GetItems(null);
		}

		[Test]
		public void GetItems()
		{
			State state = new State();
			Nonterminal nt = new Nonterminal();
			Production p = new Production(nt);
			nt.Add(p);
			Item item = new Item(p, state);
			state.Add(item);
			state.Add(new Item(new Production(), state));

			IList<Item> items = state.GetItems(p);

			Assert.AreEqual(1, items.Count);
			Assert.AreSame(item, items[0]);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ImportNull()
		{
			new State().Import(null);
		}

		[Test]
		public void Import()
		{
			Production p = new Production(new Terminal('a', 'b'));
			Item item = new Item(p, new State());

			State s = new State();

			Item next = item.NextItem;
			next.Add('a');
			s.Add(next);

			Item imported = s.Import(item.NextItem);
			Assert.AreSame(next, imported);

			item = new Item(new Production(), new State());
			imported = s.Import(item);
			Assert.AreSame(item, imported);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ImportIncompatible()
		{
			Production p = new Production(new Terminal('a', 'b'));
			Item item = new Item(p, new State());

			State s = new State();

			Item next = item.NextItem;
			next.Add('a');
			s.Add(next);

			Item newNext = item.NextItem;
			newNext.Add('b');
			s.Import(newNext);
		}
	}
}