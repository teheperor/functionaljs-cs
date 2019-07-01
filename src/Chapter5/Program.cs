using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chapter5
{
    class Program
    {
        static void Main(string[] args)
        {
            // 5.12 代数的データ構造によるリスト
            dynamic empty = (Func<Func<dynamic, dynamic>>)(() => _pattern => _pattern.empty());
            dynamic cons = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                (_value, _list) => _pattern => _pattern.cons(_value, _list));


            //  5.13 代数的データ構造のmatch関数
            dynamic match = (Func<dynamic, dynamic, dynamic>)((_data, _pattern) => _data(_pattern));


            // 5.14 リストの関数定義
            dynamic isEmpty = (Func<dynamic, dynamic>)(_alist => match(_alist, new
            {
                empty = (Func<dynamic>)(() => true),
                cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => false),
            }));
            dynamic head = (Func<dynamic, dynamic>)(_alist => match(_alist, new
            {
                empty = (Func<dynamic>)(() => null),
                cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => _head),
            }));
            dynamic tail = (Func<dynamic, dynamic>)(_alist => match(_alist, new
            {
                empty = (Func<dynamic>)(() => null),
                cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => _tail),
            }));


            // 5.15 代数的データ構造のリストの関数のテスト
            Debug.Assert((bool)isEmpty(empty()));
            Debug.Assert(!(bool)isEmpty(cons(1, empty())));
            Debug.Assert((int)head(cons(1, empty())) == 1);
            Debug.Assert((int)head(tail(cons(1, cons(2, empty())))) == 2);


            // 5.19 複利の計算
            dynamic compoundInterest = null;
            compoundInterest = (Func<dynamic, dynamic, dynamic, dynamic>)(
                (_a, _r, _n) => _n == 0 ? _a : compoundInterest(_a, _r, _n - 1) * (1 + _r));

            Debug.Assert((int)compoundInterest(100_000, 0.02, 2) == 104_040);


            // 5.20 infiniteLoop関数
            dynamic infiniteLoop = null;
            infiniteLoop = (Func<dynamic>)(() => infiniteLoop());

            //infiniteLoop();


            // 5.21 再帰によるmap関数
            dynamic map = null;
            map = (Func<dynamic, Func<dynamic, dynamic>, dynamic>)((_alist, _transform) => match(_alist, new
            {
                empty = (Func<dynamic>)(() => empty()),
                cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => cons(_transform(_head), map(_tail, _transform))),
            }));


            // 5.22 再帰によるtoArray関数
            dynamic toArray = (Func<dynamic, dynamic>)(_alist =>
            {
                dynamic toArrayHelper = null;
                toArrayHelper = (Func<dynamic, dynamic, dynamic>)((__alist, _accumulator) => match(__alist, new
                {
                    empty = (Func<dynamic>)(() => _accumulator),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) =>
                        toArrayHelper(_tail, ((List<dynamic>)_accumulator).Concat(new List<dynamic> { _head, }).ToList())),
                }));
                return toArrayHelper(_alist, new List<dynamic> { });
            });

            Debug.Assert(((List<dynamic>)toArray(cons(1, cons(2, empty())))).SequenceEqual(new List<dynamic> { 1, 2, }));


            // 5.23 リストの再帰的データ構造
            // LIST[T] = empty()
            //         | cons(T, LIST[T])


            // 5.25 再帰によるlength関数
            dynamic length = null;
            length = (Func<dynamic, dynamic>)(_list => match(_list, new
            {
                empty = (Func<dynamic>)(() => 0),
                cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => 1 + length(_tail)),
            }));

            Debug.Assert((int)length(cons(1, cons(2, empty()))) == 2);


            // 5.26 再帰によるappend関数
            dynamic append = null;
            append = (Func<dynamic, dynamic, dynamic>)((_xs, _ys) => match(_xs, new
            {
                empty = (Func<dynamic>)(() => _ys),
                cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => cons(_head, append(_tail, _ys))),
            }));

            Debug.Assert(((List<dynamic>)toArray(append(cons(1, cons(2, empty())), cons(3, cons(4, empty()))))).SequenceEqual(
                new List<dynamic> { 1, 2, 3, 4, }));


            // 5. 27 再帰によるreverse関数
            dynamic reverse = (Func<dynamic, dynamic>)(_list =>
            {
                dynamic reverseHelper = null;
                reverseHelper = (Func<dynamic, dynamic, dynamic>)((__list, _accumulator) => match(__list, new
                {
                    empty = (Func<dynamic>)(() => _accumulator),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => reverseHelper(_tail, cons(_head, _accumulator))),
                }));
                return reverseHelper(_list, empty());
            });

            Debug.Assert(((List<dynamic>)toArray(reverse(cons(1, cons(2, cons(3, empty())))))).SequenceEqual(
                new List<dynamic> { 3, 2, 1, }));


            // 5.28 代数的データ構造による数式
            dynamic num = (Func<dynamic, Func<dynamic, dynamic>>)(_n => _pattern => _pattern.num(_n));
            dynamic add = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)((_exp1, _exp2) => _pattern => _pattern.add(_exp1, _exp2));
            dynamic mul = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)((_exp1, _exp2) => _pattern => _pattern.mul(_exp1, _exp2));


            // 5.30 数式を再帰的に計算する
            dynamic calculate = null;
            calculate = (Func<dynamic, dynamic>)(_exp => match(_exp, new
            {
                num = (Func<dynamic, dynamic>)(_n => _n),
                add = (Func<dynamic, dynamic, dynamic>)((_expL, _expR) => calculate(_expL) + calculate(_expR)),
                mul = (Func<dynamic, dynamic, dynamic>)((_expL, _expR) => calculate(_expL) * calculate(_expR)),
            }));

            dynamic expression = add(num(1), mul(num(2), num(3)));
            Debug.Assert((int)calculate(expression) == 7);

        }
    }
}
