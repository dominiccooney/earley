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
	public class TestParser
	{
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateNullStart()
		{
			new Parser(null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void CreateStartWithoutEof()
		{
			new Parser(new Reduction("", new Terminal('x')));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ParseNull()
		{
			Parser parser = new Parser(new Reduction("", Terminal.Eof));
			parser.Parse(null);
		}

		[Test]
		public void ParseTrivial()
		{
			Reduction start = new Reduction("return 123;", Terminal.Eof);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(start);
			comp.Compile();

			Parser parser = new Parser(start);
			IList<object> results = parser.Parse("");

			Assert.AreEqual(1, results.Count);
			Assert.AreEqual(123, results[0]);
		}

		[Test]
		public void ParseTrivialNullable()
		{
			Reduction nullable = new Reduction("return \"Hello, world!\";");
			Reduction start = new Reduction("return $0;", new Nonterminal(nullable), Terminal.Eof);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(nullable);
			comp.Add(start);
			comp.Compile();

			Parser parser = new Parser(start);
			IList<object> results = parser.Parse("");

			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("Hello, world!", results[0]);
		}

		[Test]
		public void ParseIndirectNullable()
		{
			Reduction hello = new Reduction("return \"Hello\";");
			Reduction world = new Reduction("return \"world\";");
			Reduction nullable = new Reduction("return string.Format(\"{0}, {1}!\", $0, $1);", new Nonterminal(hello), new Nonterminal(world));
			Reduction start = new Reduction("return string.Format(\"{0}{1}\", $0, $1);", new Nonterminal(nullable), new Nonterminal(nullable), Terminal.Eof);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(hello);
			comp.Add(world);
			comp.Add(nullable);
			comp.Add(start);
			comp.Compile();

			Parser parser = new Parser(start);

			IList<object> results = parser.Parse("");
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("Hello, world!Hello, world!", results[0]);

			results = parser.Parse("xxx");
			Assert.AreEqual(0, results.Count);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ParseUncompiled()
		{
			Reduction start = new Reduction("return 123;", Terminal.Eof);
			Parser parser = new Parser(start);
			parser.Parse("");
		}

		[Test]
		public void ParseCountRR()
		{
			// E ::= a
			//     | a E

			Reduction single = new Reduction("return 1;", new Terminal('a'));

			Nonterminal e = new Nonterminal();
			
			Reduction rec = new Reduction(
				"return 1 + (int)$1;",
				new Terminal('a'),
				e);
			
			e.Add(single);
			e.Add(rec);

			Reduction start = new Reduction("return $0;", e, Terminal.Eof);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(single);
			comp.Add(rec);
			comp.Add(start);
			comp.Compile();

			Parser parser = new Parser(start);
			IList<object> result = parser.Parse("");
			Assert.AreEqual(0, result.Count);

			result = parser.Parse("a");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(1, result[0]);

			result = parser.Parse("aa");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(2, result[0]);

			result = parser.Parse("aaa");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(3, result[0]);

			result = parser.Parse("aaaaaaaaaa");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(10, result[0]);
		}

		[Test]
		public void ParseCountLR()
		{
			// E ::= empty
			//     | E a

			Reduction empty = new Reduction("return 0;");

			Nonterminal e = new Nonterminal();

			Reduction rec = new Reduction(
				"return 1 + (int)$0;",
				e,
				new Terminal('a'));

			e.Add(empty);
			e.Add(rec);

			Reduction start = new Reduction("return $0;", e, Terminal.Eof);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(empty);
			comp.Add(rec);
			comp.Add(start);
			comp.Compile();

			Parser parser = new Parser(start);
			IList<object> result = parser.Parse("");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(0, result[0]);

			result = parser.Parse("a");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(1, result[0]);

			result = parser.Parse("aa");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(2, result[0]);

			result = parser.Parse("aaa");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(3, result[0]);

			result = parser.Parse("aaaaaaaaaa");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(10, result[0]);
		}

		[Test]
		public void ParseAmbiguousExpressions()
		{
			// E ::= E + E
			//     | E * E
			//     | ( E )
			//     | [0-9]+

			Nonterminal e = new Nonterminal();

			Reduction add = new Reduction(
				"return (int)$0 + (int)$2;",
				e,
				new Terminal('+'),
				e);

			Reduction mul = new Reduction(
				"return (int)$0 * (int)$2;",
				e,
				new Terminal('*'),
				e);

			Reduction brack = new Reduction(
				"return $1;",
				new Terminal('('),
				e,
				new Terminal(')'));

			Nonterminal anyLit = new Nonterminal();
			Terminal digit = new Terminal('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

			Reduction litBase = new Reduction(
				"return (int)$0 - (int)'0';",
				digit);

			Reduction litRec = new Reduction(
				"return 10 * (int)$0 + (int)$1 - (int)'0';",
				anyLit,
				digit);

			anyLit.Add(litBase);
			anyLit.Add(litRec);

			e.Add(add);
			e.Add(mul);
			e.Add(brack);
			e.Add(litRec);
			e.Add(litBase);

			Reduction start = new Reduction("return $0;", e, Terminal.Eof);

			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(add);
			comp.Add(mul);
			comp.Add(brack);
			comp.Add(litRec);
			comp.Add(litBase);
			comp.Add(start);
			comp.Compile();

			Parser parser = new Parser(start);
			IList<object> result = parser.Parse("4");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(4, result[0]);

			result = parser.Parse("1023");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(1023, result[0]);

			result = parser.Parse("12*7");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(84, result[0]);

			result = parser.Parse("(((12))*(7))");
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(84, result[0]);

			result = parser.Parse("2*20+3");
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.Contains(43) && result.Contains(46));

			result = parser.Parse("1+2+3");
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result[0].Equals(6) && result[1].Equals(6));
		}
	}
}
