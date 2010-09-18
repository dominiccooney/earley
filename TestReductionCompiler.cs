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
	public class TestReductionCompiler
	{
		[Test]
		public void Trival()
		{
			ReductionCompiler comp = new ReductionCompiler();
			Reduction r = new Reduction("return 42;");
			comp.Add(r);
			comp.Compile();
			Assert.AreEqual(42, r.Apply(null));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddNull()
		{
			new ReductionCompiler().Add(null);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void AddAfterCompile()
		{
			ReductionCompiler comp = new ReductionCompiler();
			comp.Compile();
			comp.Add(new Reduction("return 42;"));
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void CompileTwice()
		{
			ReductionCompiler comp = new ReductionCompiler();
			comp.Compile();
			comp.Compile();
		}

		[Test, ExpectedException(typeof(ReductionCompilerException))]
		public void CompileWithErrors()
		{
			ReductionCompiler comp = new ReductionCompiler();
			comp.Add(new Reduction("foobaz-- this isn't valid C#!"));
			comp.Compile();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddNullAssembly()
		{
			ReductionCompiler comp = new ReductionCompiler();
			comp.AddReference(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddNullUsing()
		{
			ReductionCompiler comp = new ReductionCompiler();
			comp.AddUsing(null);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void AddReferenceAfterCompile()
		{
			ReductionCompiler comp = new ReductionCompiler();
			comp.Compile();
			comp.AddReference(typeof(object).Assembly);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void AddUsingAfterCompile()
		{
			ReductionCompiler comp = new ReductionCompiler();
			comp.Compile();
			comp.AddUsing("System");
		}

		[Test]
		public void AddAssembly()
		{
			ReductionCompiler comp = new ReductionCompiler();
			Reduction r = new Reduction("return new Earley.ReductionCompiler();");
			comp.Add(r);
			comp.AddReference(typeof(ReductionCompiler).Assembly);
			comp.Compile();
			Assert.AreEqual(typeof(ReductionCompiler), r.Apply(new object[] {}).GetType());
		}

		[Test]
		public void AddUsing()
		{
			ReductionCompiler comp = new ReductionCompiler();
			Reduction r = new Reduction("return new ReductionCompiler();");
			comp.Add(r);
			comp.AddReference(typeof(ReductionCompiler).Assembly);
			comp.AddUsing("Earley");
			comp.Compile();
			Assert.AreEqual(typeof(ReductionCompiler), r.Apply(new object[] {}).GetType());
		}
	}
}
