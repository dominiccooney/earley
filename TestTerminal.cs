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

namespace Earley
{
	[TestFixture]
	public class TestTerminal
	{
		[Test]
		public void Create()
		{
			new Terminal('x');
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateNull()
		{
			new Terminal(null);
		}

		[Test]
		public void CreateWithDuplicate()
		{
			Assert.AreEqual(2, new Terminal('x', 'y', 'x').Count);
		}

		[Test]
		public void Contains()
		{
			Terminal t = new Terminal('x', 'y');
			Assert.AreEqual(true, t.Contains('x') && t.Contains('y'));
			Assert.AreEqual(false, t.Contains('z'));
		}

		[Test]
		public void Eof()
		{
			Assert.AreEqual(1, Terminal.Eof.Count);
			Assert.IsTrue(Terminal.Eof.Contains(-1));
		}

		[Test]
		public void Equals()
		{
			Assert.AreEqual(new Terminal('x'), new Terminal('x'));
			Assert.IsFalse(new Terminal('x').Equals(new Terminal('x', 'y')));
		}
	}
}
