﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Lab14Space {

  class PerfectFormCalculator {
    private string _expression;

    public string PDNF { get; private set; }
    public string PCNF { get; private set; }
    public string TruthTable { get; private set; }


    public PerfectFormCalculator(string expression) => 
      _expression = expression;


    public void Process() {
      string rpn = PostfixNotation(_expression);

      var pdnf = new List<string>();
      var pcnf = new List<string>();
      var arguments = new Dictionary<char, bool>();

      List<char> variablesNames = new List<char>(GetVariables(_expression));

      var truthTableBuilder = new StringBuilder();

      foreach (var variable in variablesNames)
        truthTableBuilder.Append($"{variable}\t");
      truthTableBuilder.Append($"{_expression}{Environment.NewLine}");

      int variablesCount = variablesNames.Count;
      int truthTableRows = (int)Math.Pow(2, variablesCount);

      for (int i = 0; i < truthTableRows; i++) {       
        int bits = i;
        int mask = 1 << variablesCount - 1;

        for (int j = 0; j < variablesCount; j++) { 
          arguments[(char)variablesNames[j]] = (bits & mask) != 0;
          bits <<= 1;
        }
        var fx = Calculate(rpn, arguments);

        StringBuilder formBuilder = new StringBuilder();
        string form;

        // PDNF
        if (fx == true) {
          foreach (var val in arguments)
            formBuilder.Append($"{(val.Value == false ? "¬" : "")}{val.Key} ∧ ");
          form = formBuilder.ToString();
          if (form.Length != 0)
            pdnf.Add(form[0..^3]);
        }
        // PCNF
        else {
          foreach (var val in arguments)
            formBuilder.Append($"{(val.Value == true ? "¬" : "")}{val.Key} ∨ ");
          form = formBuilder.ToString();
          if (form.Length != 0)
            pcnf.Add(form[0..^3]);
        }
        foreach (var value in arguments.Values)
          truthTableBuilder.Append($"{(value ? '1' : '0')}\t");
        truthTableBuilder.Append($"{(fx == true ? '1' : '0')}{Environment.NewLine}");
      }

      TruthTable = truthTableBuilder.ToString();
      PerfectForm("PDNF", pdnf);
      PerfectForm("PCNF", pcnf);
    }


    private void PerfectForm(string formType, List<string> pxnf) {
      if (pxnf.Count == 0)
        return;

      var sb = new StringBuilder();

      char operation = formType == "PDNF" ? '∨' : '∧';

      for (int i = 0; i < pxnf.Count; i++)
        sb.AppendFormat($"({pxnf[i]}) {operation} ");

      if (formType == "PDNF")
        PDNF = sb.ToString()[0..^3];
      else if (formType == "PCNF")
        PCNF = sb.ToString()[0..^3];
    }

    // Возвращает обратную польскую нотацию
    private static string PostfixNotation(string expr) {
      var stack = new Stack<char>();
      var result = new StringBuilder();

      for (int i = 0; i < expr.Length; ++i) {

        // Убирает скобки
        if (expr[i] == ')') {
          stack.Pop();
        }
        // Если символ -- буква, то дописываем его
        else if (char.IsLetterOrDigit(expr[i])) {
          if (char.IsLetter(expr[i]) && i != expr.Length - 1 && char.IsLetter(expr[i + 1])) 
            throw new NotSupportedException("String variables are not supported, please use symbols instead");

          if ((char.IsDigit(expr[i]) && expr[i] - '0' > 1)
              || (i != expr.Length - 1
                && char.IsDigit(expr[i])
                && char.IsDigit(expr[i + 1])))
            throw new FormatException("Constants must be either 1 or 0");

          result.Append(expr[i]);
        }
        // Если символ не буква - значит операция
        // Положить в стек операций
        else {          
          stack.Push(expr[i]);
          continue;
        }

      }
      // Возвращает все кроме скобок
      while (stack.Count > 0 && stack.Peek() != '(')
        result.Append(stack.Pop());

      return result.ToString();
    }


    private static bool Calculate(string postfix, Dictionary<char, bool> variableValue) {

      var stack = new Stack<bool>();

      foreach (var token in postfix) {
        switch (token) {
          case '¬': stack.Push(!stack.Pop()); break;
          case '∨': stack.Push(stack.Pop() | stack.Pop()); break;
          case '∧': stack.Push(stack.Pop() & stack.Pop()); break;
          case '→': stack.Push(stack.Pop() | !stack.Pop()); break;
          case '⇿': stack.Push(stack.Pop() == stack.Pop()); break;
          case '⊕': stack.Push(stack.Pop() != stack.Pop()); break;
          case '↓': stack.Push(!stack.Pop() & !stack.Pop()); break;
          case '|': stack.Push(!(stack.Pop() & stack.Pop())); break;
          case '0': stack.Push(false); break;
          case '1': stack.Push(true); break;
          default: 
            if (!Char.IsLetter(token)) 
              throw new FormatException("Invalid symbol");
            stack.Push(variableValue[token] == true);
            break;
        }
      }
      return stack.Pop();
    }


    private static SortedSet<char> GetVariables(string expression) {
      var variables = new SortedSet<char>();
      foreach (var symbol in expression) 
        if (Char.IsLetter(symbol))
          variables.Add(symbol);

      return variables;
    }

  }
}
