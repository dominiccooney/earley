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
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Earley
{
	public class Reduction: Production
	{
		private static string MacroExpand(string code)
		{
			return Regex.Replace(code, "\\$(?<index>[0-9]+)", delegate(Match m)
			{
				return string.Format("args[{0}]", m.Groups["index"].Value);
			});
		}

		readonly string code;
		MethodInfo compiledMethod;

		public Reduction(string code, params Symbol[] syms) : base(syms)
		{
			if (code == null)
			{
				throw new ArgumentNullException();
			}

			this.code = code;
		}

		internal object Apply(object[] args)
		{
			if (compiledMethod == null) throw new InvalidOperationException();

			return compiledMethod.Invoke(null, new object[] { args });
		}

		internal CodeStatement CreateStatement()
		{
			return new CodeSnippetStatement(MacroExpand(code));
		}

		internal void SetCompiledMethod(MethodInfo m)
		{
			if (compiledMethod != null) throw new InvalidOperationException();
			if (m == null) throw new ArgumentNullException();

			compiledMethod = m;
		}
	}
}
