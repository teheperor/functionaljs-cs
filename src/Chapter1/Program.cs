using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chapter1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1.1 JavaScriptによるチューリング機械
            dynamic machine = (Func<dynamic, dynamic, dynamic, dynamic, dynamic>)((_program, _tape, _initState, _endState) =>
            {
                dynamic position = 0;
                dynamic state = _initState;
                dynamic currentInstruction = null;

                while (state != _endState)
                {
                    dynamic cell = null;
                    if (_tape.TryGetValue(position.ToString(), out cell))
                        currentInstruction = _program[state][cell];
                    else
                        currentInstruction = _program[state]["B"];
                    if (currentInstruction == null)
                        return null;
                    _tape[position.ToString()] = currentInstruction.write;
                    position += currentInstruction.move;
                    state = currentInstruction.next;
                }
                return _tape;
            });


            // 1.2 チューリング機械の実行例
            dynamic tape_ = new Dictionary<dynamic, dynamic>() {
                { "0", "1" },
                { "1", "0" },
            };
            dynamic program_ = new Dictionary<dynamic, dynamic>() {
                { "q0", new Dictionary<dynamic, dynamic>() {
                    { "1", new { write = "1", move = 1, next = "q0", } },
                    { "0", new { write = "0", move = 1, next = "q0", } },
                    { "B", new { write = "B", move = -1, next = "q1", } }, } },
                { "q1", new Dictionary<dynamic, dynamic>() {
                    { "1", new { write = "0", move = -1, next = "q1", } },
                    { "0", new { write = "1", move = -1, next = "q2", } },
                    { "B", new { write = "1", move = -1, next = "q3", } }, } },
                { "q2", new Dictionary<dynamic, dynamic>() {
                    { "1", new { write = "1", move = -1, next = "q2", } },
                    { "0", new { write = "0", move = -1, next = "q2", } },
                    { "B", new { write = "B", move = 1, next = "q4", } }, } },
                { "q3", new Dictionary<dynamic, dynamic>() {
                    { "1", new { write = "1", move = 1, next = "q4", } },
                    { "0", new { write = "0", move = 1, next = "q4", } },
                    { "B", new { write = "B", move = 1, next = "q4", } }, } },
            };


            // 1.3 1を加えるチューリング機械の実行
            Debug.Assert(((Dictionary<dynamic, dynamic>)machine(program_, tape_, "q0", "q4")).OrderBy(_x => _x.Key).SequenceEqual(
                new Dictionary<dynamic, dynamic>() {
                    { "-1", "B" },
                    { "0", "1" },
                    { "1", "1" },
                    { "2", "B" },
                }.OrderBy(_x => _x.Key)));


            // 1.5 add関数
            dynamic succ = (Func<dynamic, dynamic>)(_n => _n + 1);
            dynamic prev = (Func<dynamic, dynamic>)(_n => _n - 1);
            dynamic add = null;
            add = (Func<dynamic, dynamic, dynamic>)((_x, _y) => _y < 1 ? _x : add(succ(_x), prev(_y)));

            Debug.Assert((int)add(3, 2) == 5);


            // 1.6 漸化式の例
            dynamic a = null;
            a = (Func<dynamic, dynamic>)(_n => _n == 1 ? 1 : a(_n - 1) + 3);

            Debug.Assert((int)a(1) == 1);
            Debug.Assert((int)a(2) == 4);
            Debug.Assert((int)a(3) == 7);


            // 1.8 かけ算の定義
            dynamic times = null;
            times = (Func<dynamic, Func<dynamic, dynamic, dynamic>, dynamic, dynamic, dynamic>)(
                (_count, _fun, _arg, _memo) => _count > 1 ? times(_count - 1, _fun, _arg, _fun(_memo, _arg)) : _fun(_memo, _arg));
            dynamic multiply = (Func<dynamic, dynamic, dynamic>)((_n, _m) => times(_m, add, _n, 0));

            Debug.Assert((int)multiply(2, 3) == 6);


            // 1.9 べき乗の定義
            dynamic exponential = (Func<dynamic, dynamic, dynamic>)((_n, _m) => times(_m, multiply, _n, 1));

            Debug.Assert((int)exponential(2, 3) == 8);

        }
    }
}
