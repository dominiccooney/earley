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

using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Earley
{
	public sealed class ReductionCompiler
	{
		State state = State.Collecting;
		IDictionary<Reduction, string> rs = new Dictionary<Reduction, string>();
		IDictionary<Assembly, object> references = new Dictionary<Assembly, object>();
		IDictionary<string, object> usings = new Dictionary<string, object>();

		public ReductionCompiler() { }

		public void Add(Reduction r)
		{
			if (state != State.Collecting)
			{
				throw new InvalidOperationException();
			}

			if (r == null) throw new ArgumentNullException();

			rs[r] = null;
		}

		public void AddReference(Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException();

			if (state != State.Collecting)
			{
				throw new InvalidOperationException();
			}

			references[assembly] = null;
		}

		public void AddUsing(string @namespace)
		{
			if (@namespace == null) throw new ArgumentNullException();

			if (state != State.Collecting)
			{
				throw new InvalidOperationException();
			}

			usings[@namespace] = null;
		}

		public void Compile()
		{
			if (state == State.Compiled) throw new InvalidOperationException();

			CSharpCodeProvider provider = new CSharpCodeProvider();

			CodeCompileUnit cu = new CodeCompileUnit();

			foreach (Assembly assembly in references.Keys)
			{
				cu.ReferencedAssemblies.Add(assembly.Location);
			}

			CodeNamespace ns = new CodeNamespace();
			cu.Namespaces.Add(ns);

			foreach (string @namespace in usings.Keys)
			{
				ns.Imports.Add(new CodeNamespaceImport(@namespace));
			}

			CodeTypeDeclaration tydecl = new CodeTypeDeclaration("Reductions");
			ns.Types.Add(tydecl);
			tydecl.Attributes = MemberAttributes.Public |
								MemberAttributes.Final;

			CodeConstructor ctor = new CodeConstructor();
			tydecl.Members.Add(ctor);
			ctor.Attributes = MemberAttributes.Private;

			NameGenerator freshNames = new NameGenerator("Reduction");

			foreach (Reduction r in new List<Reduction>(rs.Keys))
			{
				CodeMemberMethod meth = new CodeMemberMethod();
				tydecl.Members.Add(meth);
				meth.Attributes = MemberAttributes.Public |
								  MemberAttributes.Static;
				meth.Name = freshNames.CreateName();
				meth.ReturnType = new CodeTypeReference(typeof(object));
				meth.Parameters.Add(
					new CodeParameterDeclarationExpression(
						typeof(object[]),
						"args"));
				meth.Statements.Add(r.CreateStatement());

				rs[r] = meth.Name;
			}

			CompilerParameters options = new CompilerParameters();
			ICodeCompiler compiler = provider.CreateCompiler();
			CompilerResults results =
				compiler.CompileAssemblyFromDom(options, cu);

			if (results.Errors.HasErrors)
			{
				StringBuilder accum = new StringBuilder();

				foreach (CompilerError error in results.Errors)
				{
					accum.AppendLine(error.ToString());
				}

				throw new ReductionCompilerException(accum.ToString());
			}
			else
			{
				Type ty =
					results.CompiledAssembly.GetType("Reductions", true, false);

				foreach (Reduction r in rs.Keys)
				{
					r.SetCompiledMethod(
						ty.GetMethod(
							rs[r],
							BindingFlags.Public | BindingFlags.Static));
				}

				state = State.Compiled;
			}
		}

		private enum State
		{
			Collecting,
			Compiled
		}
	}
}