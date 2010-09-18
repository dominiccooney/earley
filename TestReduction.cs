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
using System.Reflection;

namespace Earley
{
	[TestFixture]
	public class TestReduction
	{
		[Test]
		public void Create()
		{
			Production p = new Reduction(
				"return 42;",
				new Terminal('x'));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateNullCode()
		{
			Production p = new Reduction(
				null,
				new Terminal('x'));
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ApplyNotCompiled()
		{
			Reduction r = new Reduction("return 42;");
			r.Apply(null);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void SetMethodTwice()
		{
			Reduction r = new Reduction("return 42;");
			MethodInfo m = typeof(TestReduction).GetMethod("SetMethodTwice");
			r.SetCompiledMethod(m);
			r.SetCompiledMethod(m);
		}

		[Test]
		public void SetCompiledMethod()
		{
			Reduction r = new Reduction("return \"hi\";");
			r.SetCompiledMethod(typeof(TestReduction).GetMethod("MyMethod", BindingFlags.Static | BindingFlags.Public));
			Assert.AreEqual("there", r.Apply(null));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SetNullCompiledMethod()
		{
			Reduction r = new Reduction("return \"hi\";");
			r.SetCompiledMethod(null);
		}

		public static object MyMethod(object[] args)
		{
			return "there";
		}
	}
}