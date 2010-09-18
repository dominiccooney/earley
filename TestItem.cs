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
	public class TestItem
	{
		[Test]
		public void Create()
		{
			new Item(new Production(), new State());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateNullProduction()
		{
			new Item(null, new State());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateNullParent()
		{
			new Item(new Production(), null);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ShiftAtStart()
		{
			Production p = new Production(new Terminal('x', 'y'));
			Item item = new Item(p, new State());
			item.Add('x');
		}

		[Test]
		public void ShiftTerminal()
		{
			Production p = new Production(new Terminal('x', 'y'));
			Item item = new Item(p, new State());
			item.NextItem.Add('x');
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShiftUnrelatedTerminal()
		{
			Production p = new Production(new Terminal('x', 'y'));
			Item item = new Item(p, new State());
			item.NextItem.Add('a');
		}

		[Test]
		public void ShiftNonterminal()
		{
			Production p = new Production();
			Production q = new Production(new Nonterminal(p));
			Item item = new Item(q, new State());
			item.NextItem.Add(new Item(p, new State()));
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ShiftTerminalForNonterminal()
		{
			Production p = new Production();
			Production q = new Production(new Nonterminal(p));
			Item item = new Item(q, new State());
			item.NextItem.Add('x');
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ShiftNonterminalForTerminal()
		{
			Production p = new Production(new Terminal('x', 'y'));
			Item item = new Item(p, new State());
			item.NextItem.Add(new Item(p, new State()));
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShiftUnrelatedNonterminal()
		{
			Production p = new Production();
			Production q = new Production(new Nonterminal(p));
			Item item = new Item(q, new State());
			item.NextItem.Add(new Item(new Production(), new State()));
		}

		[Test]
		public void Equals()
		{
			Production p = new Production(Terminal.Eof);
			State s = new State();
			Item item = new Item(p, s);
			Assert.IsFalse(item.Equals(null));
			Assert.IsFalse(item.Equals(new object()));
			Assert.IsTrue(item.Equals(item), "Identity");
			Assert.IsTrue(item.Equals(new Item(p, s)), "Value equality");
			Assert.IsFalse(item.Equals(new Item(p, new State())), "Vary by state");
			Assert.IsFalse(item.Equals(new Item(new Production(), s)), "Vary by production");
			Assert.IsFalse(item.Equals(item.NextItem), "Vary by index");

			Item another = new Item(p, s);
			Assert.IsFalse(item.NextItem.Equals(another.NextItem), "Vary by previous item");
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void NextItemAtEnd()
		{
			Item item = new Item(new Production(), new State());
			item = item.NextItem;
		}

		[Test]
		public void NextItem()
		{
			Item item = new Item(new Production(Terminal.Eof), new State());
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void SymbolAtEnd()
		{
			Item item = new Item(new Production(), new State());
			Symbol sym = item.Symbol;
		}

		[Test]
		public void Symbol()
		{
			Item item = new Item(new Production(new Terminal('x'), Terminal.Eof), new State());
			Assert.IsTrue(((Terminal)item.Symbol).Contains('x'));
			Assert.AreEqual(item.NextItem.Symbol, Terminal.Eof);
		}

		[Test]
		public void AtEnd()
		{
			Assert.IsTrue(new Item(new Production(), new State()).AtEnd);

			Item item = new Item(new Production(Terminal.Eof), new State());
			Assert.IsFalse(item.AtEnd);
			Assert.IsTrue(item.NextItem.AtEnd);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ContainsNull()
		{
			new State().Contains(null);
		}

		[Test]
		public void Contains()
		{
			State s = new State();
			Item item = new Item(new Production(), s);
			Assert.IsFalse(s.Contains(item));
			s.Add(item);
			Assert.IsTrue(s.Contains(item));
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ReduceBeforeEnd()
		{
			Reduction r = new Reduction("return \"hello\";", Terminal.Eof);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(r);
			comp.Compile();

			new Item(r, new State()).Reduce();
		}

		[Test]
		public void TrivalReduce()
		{
			Reduction reduction = new Reduction("return 42;");
			
			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(reduction);
			comp.Compile();

			Item item = new Item(reduction, new State());
			IList<object> results = item.Reduce();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual(42, results[0]);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ReduceProduction()
		{
			new Item(new Production(), new State()).Reduce();
		}

		[Test]
		public void Reduce()
		{
			Reduction n1 = new Reduction("return 1;");
			Reduction n2 = new Reduction("return 2;");
			Reduction n10 = new Reduction("return 10;");
			Nonterminal exp = new Nonterminal();
			Reduction plus = new Reduction("return (int)$0 + (int)$1;", exp, exp);
			
			exp.Add(n1);
			exp.Add(n2);
			exp.Add(n10);
			exp.Add(plus);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(n1);
			comp.Add(n2);
			comp.Add(n10);
			comp.Add(plus);
			comp.Compile();

			//     +
			//    / \ 
			//   +   \ 
			//  / \   \ 
			//  ?  1   ?
			// / \    / \ 
			// 1 2   +  10 
			//      / \  
			//      ?  1 
			//	   / \ 	
			//	   1 2 	

			Item inner = new Item(plus, new State());
			inner = inner.NextItem;
			inner.Add(new Item(n1, new State()));
			inner.Add(new Item(n2, new State()));

			inner = inner.NextItem;
			inner.Add(new Item(n1, new State()));

			Item item = new Item(plus, new State());
			item = item.NextItem;
			item.Add(inner);

			item = item.NextItem;
			item.Add(inner);
			item.Add(new Item(n10, new State()));

			IList<object> result = item.Reduce();

			int[] precomputed = new int[] {
				4, 5, 12, 5, 6, 13
			};

			Assert.AreEqual(precomputed.Length, result.Count);

			for (int i = 0; i < precomputed.Length; i++)
			{
				Assert.AreEqual(precomputed[i], result[i]);
			}
		}

		[Test]
		public void ReduceTerminals()
		{
			Reduction r = new Reduction(
				"return string.Format(\"{0}{1}{2}\", (char)(int)$0, (char)(int)$1, (char)(int)$2);",
				new Terminal('f', 'b'),
				new Terminal('o'),
				new Terminal('o'));

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(r);
			comp.Compile();

			Item item = new Item(r, new State());
			item = item.NextItem;
			item.Add('f');
			item.Add('b');

			item = item.NextItem;
			item.Add('o');

			item = item.NextItem;
			item.Add('o');

			IList<object> results = item.Reduce();

			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("foo", results[0]);
			Assert.AreEqual("boo", results[1]);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void IsImportCompatibleNull()
		{
			new Item(new Production(), new State()).IsImportCompatible(null);
		}

		[Test]
		public void IsImportCompatible()
		{
			Item item = new Item(new Production(new Terminal('x')), new State());
			Assert.IsTrue(item.IsImportCompatible(item));

			// unrelated items are not import compatible
			Assert.IsFalse(item.IsImportCompatible(item.NextItem));

			// related items with no derivations are import compatible
			Item nextItem = item.NextItem;
			Assert.IsTrue(nextItem.IsImportCompatible(nextItem));

			// calling x.IsImportCompatible(x) is unusual, but is false
			// below because item has derivation now!
			nextItem.Add('x');
			Assert.IsFalse(nextItem.IsImportCompatible(nextItem));

			// related item with no derivations is compatible with one
			// *with* derivations
			Assert.IsTrue(nextItem.IsImportCompatible(item.NextItem));
		}
	}
}
