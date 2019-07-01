using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chapter6
{
    class Program
    {
        static void Main(string[] args)
        {
            // 6.9 JavaScriptによる正格評価
            dynamic left = (Func<dynamic, dynamic, dynamic>)((_x, _y) => _x);
            dynamic infiniteLoop = null;
            infiniteLoop = (Func<dynamic>)(() => infiniteLoop());

            //Debug.Assert(left(1, infiniteLoop()) == 1);


            // 6.10 条件文と遅延評価
            dynamic conditional = (Func<dynamic, dynamic>)(_n =>
            {
                if (_n == 1)
                    return true;
                else
                    return infiniteLoop();
            });

            Debug.Assert((bool)conditional(1));


            // 6.11 遅延評価で定義したmultiply関数
            dynamic lazyMultiply = (Func<dynamic, dynamic, dynamic>)((_funX, _funY) =>
            {
                dynamic x = _funX();
                return x == 0 ? 0 : x * _funY;
            });


            // 6.12 遅延評価で定義したmultiply関数のテスト
            Debug.Assert((int)lazyMultiply((Func<dynamic>)(() => 0), (Func<dynamic>)(() => infiniteLoop())) == 0);


            // 6.13 ストリーム型のデータ構造
            // ストリーム型のデータ構造
            // STRAEM[T] = emtpy()
            //           | cons(T, FUN[() => STREAM[T]])
            // リスト型のデータ構造
            // LIST[T] = empty()
            //         | cons(T, LIST[T])


            // リスト型の定義
            dynamic list = null;
            list = new
            {
                match = (Func<dynamic, dynamic, dynamic>)((_data, _pattern) => _data(_pattern)),
                empty = (Func<Func<dynamic, dynamic>>)(() => _pattern => _pattern.empty()),
                cons = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_value, _list) => _pattern => _pattern.cons(_value, _list)),

                toArray = (Func<dynamic, dynamic>)(_alist =>
                {
                    dynamic toArrayHelper = null;
                    toArrayHelper = (Func<dynamic, dynamic, dynamic>)((__alist, _accumulator) => list.match(__alist, new
                    {
                        empty = (Func<dynamic>)(() => _accumulator),
                        cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) =>
                            toArrayHelper(_tail, ((List<dynamic>)_accumulator).Concat(new List<dynamic> { _head, }).ToList())),
                    }));
                    return toArrayHelper(_alist, new List<dynamic> { });
                }),
            };


            // 6.14 サンクによるストリーム型の定義
            dynamic stream = null;
            stream = new
            {
                match = (Func<dynamic, dynamic, dynamic>)((_data, _pattern) => _data(_pattern)),
                empty = (Func<dynamic, dynamic>)(_pattern => _pattern.empty()),
                cons = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_head, _tailThunk) => _pattern => _pattern.cons(_head, _tailThunk)),
                // head:: STREAM[T] => T
                head = (Func<dynamic, dynamic>)(_aStream => stream.match(_aStream, new
                {
                    empty = (Func<dynamic>)(() => null),
                    cons = (Func<dynamic, dynamic, dynamic>)((_value, _tailThunk) => _value),
                })),
                // tail:: STREAM[T] => STREAM[T]
                tail = (Func<dynamic, dynamic>)(_aStream => stream.match(_aStream, new
                {
                    empty = (Func<dynamic>)(() => null),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tailThunk) => _tailThunk()),
                })),

                // 6.21 ストリームのtake関数
                // take:: (STREAM[T], NUM) => LIST[T]
                take = (Func<dynamic, dynamic, dynamic>)((_aStream, _n) => stream.match(_aStream, new
                {
                    empty = (Func<dynamic>)(() => list.empty()),
                    cons = (Func<dynamic, dynamic, dynamic>)(
                        (_head, _tailThunk) => _n == 0 ? list.empty() : list.cons(_head, stream.take(_tailThunk(), _n - 1))),
                })),
            };


            // 6.16 ストリーム型のテスト
            dynamic theStream = stream.cons(1, (Func<dynamic>)(() => stream.cons(2, (Func<dynamic>)(() => stream.empty()))));
            Debug.Assert((int)stream.head(theStream) == 1);


            // 6.17 無限に1が続く数列
            dynamic ones = null;
            ones = (Func<dynamic, dynamic>)(stream.cons(1, (Func<dynamic>)(() => ones)));

            Debug.Assert((int)stream.head(ones) == 1);
            Debug.Assert((int)stream.head(stream.tail(ones)) == 1);
            Debug.Assert((int)stream.head(stream.tail(stream.tail(ones))) == 1);


            // 6.19 無限に連続する整数列を生成するenumFrom関数
            dynamic enumFrom = null;
            enumFrom = (Func<dynamic, dynamic>)(_n => stream.cons(_n, (Func<dynamic>)(() => enumFrom(_n + 1))));

            Debug.Assert((int)stream.head(enumFrom(1)) == 1);
            Debug.Assert((int)stream.head(stream.tail(enumFrom(1))) == 2);
            Debug.Assert((int)stream.head(stream.tail(stream.tail(enumFrom(1)))) == 3);


            // 6.23 無限の整数列をテストする
            Debug.Assert(((List<dynamic>)list.toArray(stream.take(enumFrom(1), 4))).SequenceEqual(
                new List<dynamic> { 1, 2, 3, 4, }));

        }
    }
}
