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
using System.Collections;

namespace Earley
{
	[TestFixture]
	public class TestNonterminal
	{
		[Test]
		public void CreateEmpty()
		{
			new Nonterminal();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateNullProduction()
		{
			new Nonterminal(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateNullProductions()
		{
			new Nonterminal((Production[]) null);
		}

		[Test]
		public void Create()
		{
			Production p1 = new Production();
			Production p2 = new Production();
			Nonterminal nt = new Nonterminal(p1, p2);
			Assert.AreEqual(2, nt.Count);
			Assert.IsTrue(nt.Contains(p1) && nt.Contains(p2));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ContainsNull()
		{
			new Nonterminal().Contains(null);
		}

		[Test]
		public void Add()
		{
			Nonterminal nt = new Nonterminal();
			Production p = new Production();
			nt.Add(p);
			Assert.IsTrue(nt.Contains(p));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddNull()
		{
			new Nonterminal().Add(null);
		}
	}
}