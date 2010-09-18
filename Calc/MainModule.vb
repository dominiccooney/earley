Imports Earley
Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic

Module MainModule
    Sub Main()
        Console.WriteLine("Type an arithmetic expression. Type a blank line to quit.")

        Dim parser As Parser = CreateParser()

        While True
            Console.Write("> ")
            Dim input As String = Console.ReadLine()

            If input.Length = 0 Then
                Return
            Else
                Dim results As IList(Of Object) = parser.Parse(input)

                If results.Count = 0 Then
                    Console.WriteLine("Parse error")
                Else
                    ' Display results
                    For i As Integer = 0 To results.Count - 1
                        Dim exp As Exp = DirectCast(results(i), Exp)
                        Dim result As Integer = exp.Eval()
                        Console.WriteLine("Result {0,2}: {1} => {2,4}", i + 1, exp.ToString(), result)
                    Next
                End If
            End If
        End While
    End Sub

    Function CreateParser() As Parser
        ' Grammar:
        '
        ' Digit   ::= [0-9]
        '
        ' Literal ::= Digit
        '           | Literal Digit
        '
        ' Exp     ::= Literal
        '           | ( Exp )
        '           | Exp + Exp
        '           | Exp - Exp
        '           | Exp * Exp
        '           | Exp / Exp

        ' characters that can appear in a literal
        Dim digit As New Terminal("0123456789".ToCharArray())

        ' this is how we refer to literals (which can be either one digit or
        ' many digits long)
        Dim literal As New Nonterminal

        ' this is the base case for literals-- just one digit
        Dim literalBase As New Reduction( _
            "return (int)$0 - (int)'0';", _
            digit)

        ' this is the recursive case for literals-- a literal in front of
        ' another digit
        Dim literalRec As New Reduction( _
            "return 10 * (int)$0 + (int)$1 - (int)'0';", _
            literal, digit)

        ' add the actual rules that make up literal; we have to create literal
        ' and *then* add to it because literalRec refers to literal, and
        ' literal contains literalRec
        literal.Add(literalBase)
        literal.Add(literalRec)

        ' exp is how we refer to any kind of expression (like literal above)
        Dim exp As New Nonterminal()

        ' all the kinds of expressions

        Dim brack As New Reduction( _
            "return $1;", _
            New Terminal("("c), exp, New Terminal(")"c))

        Dim add As New Reduction( _
            "return new AddExp((Exp)$0, (Exp)$2);", _
            exp, New Terminal("+"c), exp)

        Dim [sub] As New Reduction( _
            "return new SubExp((Exp)$0, (Exp)$2);", _
            exp, New Terminal("-"c), exp)

        Dim mul As New Reduction( _
            "return new MulExp((Exp)$0, (Exp)$2);", _
            exp, New Terminal("*"c), exp)

        Dim div As New Reduction( _
            "return new DivExp((Exp)$0, (Exp)$2);", _
            exp, New Terminal("/"c), exp)

        Dim lit As New Reduction( _
            "return new LitExp((int)$0);", _
            literal)

        ' add all of the kinds of expressions to exp
        exp.Add(brack)
        exp.Add(add)
        exp.Add([sub])
        exp.Add(mul)
        exp.Add(div)
        exp.Add(lit)

        ' we need a special start production that ends with Terminal.Eof; it
        ' will just return the result of parsing one expression
        Dim start As New Reduction("return $0;", exp, Terminal.Eof)

        ' compile all the C# code in the reductions

        Dim comp As New ReductionCompiler
        comp.AddReference(GetType(Exp).Assembly)
        comp.AddUsing("Calc")
        comp.Add(literalBase)
        comp.Add(literalRec)
        comp.Add(brack)
        comp.Add(add)
        comp.Add([sub])
        comp.Add(mul)
        comp.Add(div)
        comp.Add(lit)
        comp.Add(start)
        comp.Compile()

        ' create and return the parser
        Return New Parser(start)
    End Function
End Module

Public MustInherit Class Exp
    Public MustOverride Function Eval() As Integer
    Public MustOverride Overrides Function ToString() As String
End Class

Public MustInherit Class BinExp
    Inherits Exp

    Protected Left As Exp
    Protected Right As Exp

    Protected Sub New(ByVal left As Exp, ByVal right As Exp)
        If left Is Nothing Or right Is Nothing Then
            Throw New ArgumentNullException
        End If

        Me.Left = left
        Me.Right = right
    End Sub
End Class

Public NotInheritable Class AddExp
    Inherits BinExp

    Public Sub New(ByVal left As Exp, ByVal right As Exp)
        MyBase.New(left, right)
    End Sub

    Public Overrides Function Eval() As Integer
        Return Left.Eval() + Right.Eval()
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("(+ {0} {1})", Left.ToString(), Right.ToString())
    End Function
End Class

Public NotInheritable Class SubExp
    Inherits BinExp

    Public Sub New(ByVal left As Exp, ByVal right As Exp)
        MyBase.New(left, right)
    End Sub

    Public Overrides Function Eval() As Integer
        Return Left.Eval() - Right.Eval()
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("(- {0} {1})", Left.ToString(), Right.ToString())
    End Function
End Class

Public NotInheritable Class MulExp
    Inherits BinExp

    Public Sub New(ByVal left As Exp, ByVal right As Exp)
        MyBase.New(left, right)
    End Sub

    Public Overrides Function Eval() As Integer
        Return Left.Eval() * Right.Eval()
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("(* {0} {1})", Left.ToString(), Right.ToString())
    End Function
End Class

Public NotInheritable Class DivExp
    Inherits BinExp

    Public Sub New(ByVal left As Exp, ByVal right As Exp)
        MyBase.New(left, right)
    End Sub

    Public Overrides Function Eval() As Integer
        Return Left.Eval() / Right.Eval()
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("(/ {0} {1})", Left.ToString(), Right.ToString())
    End Function
End Class

Public NotInheritable Class LitExp
    Inherits Exp

    Private ReadOnly value As Integer

    Public Sub New(ByVal value As Integer)
        Me.value = value
    End Sub

    Public Overrides Function Eval() As Integer
        Return value
    End Function

    Public Overrides Function ToString() As String
        Return value.ToString()
    End Function
End Class
