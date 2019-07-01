using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Chapter7
{
    class Program
    {
        static void Main(string[] args)
        {
            // 7.1 multipleOf関数の定義
            dynamic multipleOf_ = (Func<dynamic, dynamic, dynamic>)((_n, _m) => _m % _n == 0);


            // 7.2 multipleOf関数のテスト
            Debug.Assert((bool)multipleOf_(2, 4));


            // 7.3 カリー化されたmultipleOf関数の定義
            dynamic multipleOf = (Func<dynamic, Func<dynamic, dynamic>>)(_n => _m => _m % _n == 0);


            // 7.4 カリー化されたmultipleOf関数のテスト
            Debug.Assert((bool)multipleOf(2)(4));


            // 7.5 multipleOf関数のテスト
            dynamic twoFold = multipleOf(2);
            Debug.Assert((bool)twoFold(4));


            // 7.6 指数関数の例
            dynamic exponential = null;
            exponential = (Func<dynamic, Func<dynamic, dynamic>>)(
                _base => _index => _index == 0 ? 1 : _base * exponential(_base)(_index - 1));
            Debug.Assert((int)exponential(2)(3) == 8);


            // 7.7 flip関数の定義
            dynamic flip = null;
            flip = (Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>)(_fun => _x => _y => _fun(_y)(_x));


            // 7.8 flip関数でexponential関数の引数の順序を変更する
            dynamic square = flip(exponential)(2);
            dynamic cube = flip(exponential)(3);
            Debug.Assert((int)square(2) == 4);
            Debug.Assert((int)cube(2) == 8);


            // チャーチによる自然数の定義
            dynamic zero = (Func<dynamic, Func<dynamic, dynamic>>)(_f => _x => _x);
            dynamic one = (Func<dynamic, Func<dynamic, dynamic>>)(_f => _x => _f(_x));
            dynamic two = (Func<dynamic, Func<dynamic, dynamic>>)(_f => _x => _f(_f(_x)));
            dynamic three = (Func<dynamic, Func<dynamic, dynamic>>)(_f => _x => _f(_f(_f(_x))));

            dynamic succ = (Func<dynamic, dynamic>)(_n => _n + 1);
            Debug.Assert((int)zero(succ)(0) == 0);
            Debug.Assert((int)one(succ)(0) == 1);
            Debug.Assert((int)two(succ)(0) == 2);
            Debug.Assert((int)three(succ)(0) == 3);


            // 7.10 multipleOf関数の再利用
            dynamic even = multipleOf(2);
            Debug.Assert((bool)even(2));


            // 7.11 !演算子によるnot関数
            dynamic not_ = (Func<dynamic, dynamic>)(_predicate => !_predicate);
            //dynamic not_ = (Func<Func<dynamic, dynamic>, Func<dynamic, dynamic>>)(_predicate => !_predicate);
            //dynamic odd_ = not_(even);


            // 7.12 !演算子はコンビネータではない
            //Debug.Assert((bool)odd_(3));


            // 7.13 notコンビネータ
            // not:: FUN[NUM => BOOL] => FUN[NUM => BOOL]
            dynamic not = (Func<Func<dynamic, dynamic>, Func<dynamic, dynamic>>)(_predicate => _arg => !_predicate(_arg));


            // 7.15 notコンビネータによるodd関数の定義
            dynamic odd = not(even);
            Debug.Assert((bool)odd(3));
            Debug.Assert(!(bool)odd(2));


            // 7.16 関数合成の定義
            dynamic compose = (Func<Func<dynamic, dynamic>, Func<dynamic, dynamic>, Func<dynamic, dynamic>>)(
                (_f, _g) => _arg => _f(_g(_arg)));


            // 7.17 関数合成のテスト
            dynamic f_ = (Func<dynamic, dynamic>)(_x => _x * _x + 1);
            dynamic g_ = (Func<dynamic, dynamic>)(_x => _x - 2);
            Debug.Assert((int)compose(f_, g_)(2) == (int)f_(g_(2)));


            // 7.18 反数関数の合成
            dynamic opposite = (Func<dynamic, dynamic>)(_n => -_n);

            Debug.Assert((int)compose(opposite, opposite)(2) == 2);


            // 7.19 合成が失敗する例
            dynamic add = (Func<dynamic, dynamic, dynamic>)((_x, _y) => _x + _y);
            //Debug.Assert((int)compose(opposite, add)(2, 3) == -5);


            // 7.20 カリー化による合成
            dynamic addCurried = (Func<dynamic, Func<dynamic, dynamic>>)(_x => _y => _x + _y);
            Debug.Assert((int)compose(opposite, addCurried(2))(3) == -5);


            // リスト型の定義
            dynamic list = null;
            list = new
            {
                match = (Func<dynamic, dynamic, dynamic>)((_data, _pattern) => _data(_pattern)),
                empty = (Func<Func<dynamic, dynamic>>)(() => _pattern => _pattern.empty()),
                cons = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_value, _list) => _pattern => _pattern.cons(_value, _list)),

                head = (Func<dynamic, dynamic>)(_alist => list.match(_alist, new
                {
                    empty = (Func<dynamic>)(() => null),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => _head),
                })),
                tail = (Func<dynamic, dynamic>)(_alist => list.match(_alist, new
                {
                    empty = (Func<dynamic>)(() => null),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => _tail),
                })),

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

                reverse_ = (Func<dynamic, dynamic>)(_alist =>
                {
                    dynamic reverseHelper = null;
                    reverseHelper = (Func<dynamic, dynamic, dynamic>)((__alist, _accumulator) => list.match(__alist, new
                    {
                        empty = (Func<dynamic>)(() => _accumulator),
                        cons = (Func<dynamic, dynamic, dynamic>)(
                            (_head, _tail) => reverseHelper(_tail, list.cons(_head, _accumulator))),
                    }));
                    return reverseHelper(_alist, list.empty());
                }),

                // 7.21 具体的なlast関数
                last_ = (Func<dynamic, dynamic>)(_alist => list.match(_alist, new
                {
                    empty = (Func<dynamic>)(() => null),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => list.match(_tail, new
                    {
                        empty = (Func<dynamic>)(() => _head),
                        cons = (Func<dynamic, dynamic, dynamic>)((_, __) => list.last(_tail)),
                    })),
                })),

                // 7.22 抽象的なlast関数
                last = (Func<dynamic, dynamic>)(_alist => compose(list.head, list.reverse_)(_alist)),

                // 7.49 リストのmap関数の定義
                // map: FUN[T => T] => LIST[T] => LIST[T}
                map = (Func<dynamic, Func<dynamic, dynamic>>)(_callback => _alist => list.match(_alist, new
                {
                    empty = (Func<dynamic>)(() => list.empty()),
                    cons = (Func<dynamic, dynamic, dynamic>)(
                        (_head, _tail) => list.cons(_callback(_head), list.map(_callback)(_tail))),
                })),

                // 7.51 sum関数の定義
                sum_ = (Func<dynamic, Func<dynamic, dynamic>>)(_alist => _accumulator => list.match(_alist, new
                {
                    empty = (Func<dynamic>)(() => _accumulator),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => list.sum_(_tail)(_accumulator + _head)),
                })),

                // 7.52 コールバック関数を用いたsum関数の再定義
                sumWithCallback = (Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>)(
                    _alist => _accumulator => _CALLBACK => list.match(_alist, new
                    {
                        empty = (Func<dynamic>)(() => _accumulator),
                        cons = (Func<dynamic, dynamic, dynamic>)(
                            (_head, _tail) => _CALLBACK(_head)(list.sumWithCallback(_tail)(_accumulator)(_CALLBACK))),
                    })),

                // 7.54 length関数の定義
                length_ = (Func<dynamic, Func<dynamic, dynamic>>)(_alist => _accumulator => list.match(_alist, new
                {
                    empty = (Func<dynamic>)(() => _accumulator),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => list.length_(_tail)(_accumulator + 1)),
                })),

                // 7.55 コールバックによるlength関数の再定義
                lengthWithCallback = (Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>)(
                    _alist => _accumulator => _CALLBACK => list.match(_alist, new
                    {
                        empty = (Func<dynamic>)(() => _accumulator),
                        cons = (Func<dynamic, dynamic, dynamic>)(
                            (_head, _tail) => _CALLBACK(_head)(list.lengthWithCallback(_tail)(_accumulator)(_CALLBACK))),
                    })),

                // 7.58 リストの畳み込み関数
                foldr = (Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>)(
                    _alist => _accumulator => _callback => list.match(_alist, new
                    {
                        empty = (Func<dynamic>)(() => _accumulator),
                        cons = (Func<dynamic, dynamic, dynamic>)(
                            (_head, _tail) => _callback(_head)(list.foldr(_tail)(_accumulator)(_callback))),
                    })),

                // 7.59 foldr関数によるsum関数とlength関数の定義
                sum = (Func<dynamic, dynamic>)(_alist =>
                    list.foldr(_alist)(0)((Func<dynamic, Func<dynamic, dynamic>>)(_item => _accumulator => _accumulator + _item))),
                length = (Func<dynamic, dynamic>)(_alist =>
                    list.foldr(_alist)(0)((Func<dynamic, Func<dynamic, dynamic>>)(_item => _accumulator => _accumulator + 1))),

                // 反復処理における蓄積編巣の初期値とコールバック関数の関係
                //sum = (Func<dynamic, dynamic>)(_alist =>
                //    list.foldr(_alist)(0)((Func<dynamic, Func<dynamic, dynamic>>)(_item => _accumulator => _item + _accumulator))),
                //length = (Func<dynamic, dynamic>)(_alist =>
                //    list.foldr(_alist)(0)((Func<dynamic, Func<dynamic, dynamic>>)(_item => _accumulator => 1 + _accumulator))),
                product = (Func<dynamic, dynamic>)(_alist =>
                    list.foldr(_alist)(1)((Func<dynamic, Func<dynamic, dynamic>>)(_item => _accumulator => _item * _accumulator))),
                all = (Func<dynamic, dynamic>)(_alist =>
                    list.foldr(_alist)(true)((Func<dynamic, Func<dynamic, dynamic>>)(_item => _accumulator => _item && _accumulator))),
                any = (Func<dynamic, dynamic>)(_alist =>
                    list.foldr(_alist)(false)((Func<dynamic, Func<dynamic, dynamic>>)(_item => _accumulator => _item || _accumulator))),

                // 7.60 foldr関数によるreverse関数の定義
                append = (Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>)(_xs => _ys => list.match(_xs, new
                {
                    empty = (Func<dynamic>)(() => _ys),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => list.cons(_head, list.append(_tail)(_ys))),
                })),
                reverse = (Func<dynamic, Func<dynamic, dynamic>>)(_alist =>
                    list.foldr(_alist)(list.empty())((Func<dynamic, Func<dynamic, dynamic>>)(
                        _item => _accumulator => list.append(_accumulator)(list.cons(_item, list.empty()))))),

                // 7.61 foldr関数によるfind関数の定義
                find = (Func<dynamic, Func<dynamic, dynamic>>)(_alist => _predicate =>
                    list.foldr(_alist)(null)((Func<dynamic, Func<dynamic, dynamic>>)(
                        _item => _accumulator => _predicate(_item) ? _item : _accumulator))),

            };

            // last関数のテスト
            dynamic alist_ = list.cons(1, list.cons(2, list.cons(3, list.empty())));
            Debug.Assert((int)list.last_(alist_) == 3);
            Debug.Assert((int)list.last(alist_) == 3);


            //// 7.23 Yコンビネータ
            //var Y = (F) => {
            //  return (((x) => {
            //    return F((y) => {
            //      return x(x)(y);
            //    });
            //  })((x) => {
            //    return F((y) => {
            //      return x(x)(y);
            //    });
            //  });
            //});
            //
            //// 7.24 Yコンビネータによるfactorial関数の実装
            //var factorial = Y((fact) => {
            //  return (n) => {
            //    if (n === 0) {
            //      return 1;
            //    } else {
            //      return n * fact(n - 1);
            //    }
            //  };
            //});
            //expect(
            //  factorial(3) // 3 * 2 * 1 = 6
            //).to.eql(
            //  6
            //);


            // 7.28 クロージャーとしてのcounter関数
            dynamic counter = (Func<dynamic, Func<dynamic>>)(_init =>
            {
                dynamic countingNumber = _init;
                return () => ++countingNumber;
            });


            // 7.29 counter関数の利用法
            dynamic counterFromZero = counter(0);
            Debug.Assert((int)counterFromZero() == 1);
            Debug.Assert((int)counterFromZero() == 2);


            // 7.31 カリー化された不変なオブジェクト型
            dynamic object_ = null;
            object_ = new
            {
                empty = (Func<dynamic>)(() => null),
                get = (Func<dynamic, Func<dynamic, dynamic>>)(_key => _obj => _obj(_key)),
                set = (Func<dynamic, dynamic, Func<dynamic, Func<dynamic, dynamic>>>)(
                    (_key, _value) => _obj => _queryKey => _key == _queryKey ? _value : object_.get(_queryKey)(_obj)),
            };


            // 7.32 カリー化された不変なオブジェクト型のテスト
            dynamic robots = compose(
                object_.set("C3PO", "Star Wars"),
                object_.set("HAL9000", "2001: a space oddesay")
                )(object_.empty);

            Debug.Assert((string)object_.get("HAL9000")(robots) == "2001: a space oddesay");


            // ストリーム型の定義
            dynamic stream = null;
            stream = new
            {
                match = (Func<dynamic, dynamic, dynamic>)((_data, _pattern) => _data(_pattern)),
                empty = (Func<dynamic, dynamic>)(_pattern => _pattern.empty()),
                cons = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_head, _tailThunk) => _pattern => _pattern.cons(_head, _tailThunk)),

                // 7.35 ストリームのfilter関数
                // filter:: FUN[T => BOOL] => STREAM[T] => STREAM[T]
                filter = (Func<Func<dynamic, dynamic>, Func<dynamic, dynamic>>)(
                    _predicate => (Func<dynamic, dynamic>)(_aStream => stream.match(_aStream, new
                    {
                        empty = (Func<dynamic>)(() => stream.empty()),
                        cons = (Func<dynamic, dynamic, dynamic>)((_head, _tailThunk) =>
                            _predicate(_head) ?
                                stream.cons(_head, (Func<dynamic>)(() => stream.filter(_predicate)(_tailThunk()))) :
                                stream.filter(_predicate)(_tailThunk())),
                    }))),

                // 7.36 ストリームのremove関数
                // remove:: FUN[T => BOOL] => STREAM[T] => STREAM[T]
                remove = (Func<Func<dynamic, dynamic>, Func<dynamic, dynamic>>)(
                    _predicate => _aStream => stream.filter(not(_predicate))(_aStream)),
            };


            // 7.33 ストリームからジェネレータを作る
            dynamic generate = (Func<dynamic, Func<dynamic>>)(_aStream =>
            {
                dynamic stream_ = _aStream;
                return (Func<dynamic>)(() => stream.match(stream_, new
                {
                    empty = (Func<dynamic>)(() => null),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tailThunk) =>
                    {
                        stream_ = _tailThunk();
                        return _head;
                    }),
                }));
            });


            // 無限に連続する整数列を生成するenumFrom関数
            dynamic enumFrom = null;
            enumFrom = (Func<dynamic, dynamic>)(_n => stream.cons(_n, (Func<dynamic>)(() => enumFrom(_n + 1))));


            // 7.34 整数列のジェネレータ
            dynamic integers = enumFrom(0);
            dynamic initGenerator = generate(integers);
            Debug.Assert((int)initGenerator() == 0);
            Debug.Assert((int)initGenerator() == 1);
            Debug.Assert((int)initGenerator() == 2);


            // 7.37 エラトステネスのふるいによる素数の生成
            dynamic sieve = null;
            sieve = (Func<dynamic, dynamic>)(_aStream => stream.match(_aStream, new
            {
                empty = (Func<dynamic>)(() => null),
                cons = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_head, _tailThunk) => stream.cons(_head, (Func<dynamic>)(() => sieve(
                        //stream.remove((Func<dynamic, dynamic>)(_item => multipleOf(_item)(_head)))(_tailThunk()))))),
                        stream.remove((Func<dynamic, dynamic>)(_item => multipleOf(_head)(_item)))(_tailThunk()))))),
            }));
            dynamic primes = sieve(enumFrom(2));


            // 7.39 素数のジェネレータ
            dynamic primeGenerator = generate(primes);
            Debug.Assert((int)primeGenerator() == 2);
            Debug.Assert((int)primeGenerator() == 3);
            Debug.Assert((int)primeGenerator() == 5);


            // 7.47 直接的な関数呼び出しの例
            //dynamic succ = (Func<dynamic, dynamic>)(_n => _n + 1);
            dynamic doCall = (Func<dynamic, dynamic>)(_arg => succ(_arg));
            Debug.Assert((int)doCall(2) == 3);


            // 7.48 単純なコールバックの例
            dynamic setupCallback = (Func<dynamic, Func<dynamic, dynamic>>)(_callback => _arg => _callback(_arg));
            dynamic doCallback = setupCallback(succ);
            Debug.Assert((int)doCallback(2) == 3);


            // 7.50 map関数のテスト
            dynamic numbers = list.cons(1, list.cons(2, list.cons(3, list.empty())));
            dynamic mapDouble = list.map((Func<dynamic, dynamic>)(_n => _n * 2));
            Debug.Assert(((List<dynamic>)compose(list.toArray, mapDouble)(numbers)).SequenceEqual(
                new List<dynamic> { 2, 4, 6, }));
            dynamic mapSquare = list.map((Func<dynamic, dynamic>)(n => n * n));
            Debug.Assert(((List<dynamic>)compose(list.toArray, mapSquare)(numbers)).SequenceEqual(
                new List<dynamic> { 1, 4, 9, }));


            // sum関数のテスト
            Debug.Assert((int)list.sum_(numbers)(0) == 6);

            // 7.53 sumWithCallback関数のテスト
            dynamic sumCallback = (Func<dynamic, Func<dynamic, dynamic>>)(_n => _m => _n + _m);
            Debug.Assert((int)list.sumWithCallback(numbers)(0)(sumCallback) == 6);


            // length関数のテスト
            Debug.Assert((int)list.length_(numbers)(0) == 3);

            // 7.54 lengthWithCallback関数でリストの長さをテストする
            dynamic lengthCallback = (Func<dynamic, Func<dynamic, dynamic>>)(_n => _m => 1 + _m);
            Debug.Assert((int)list.lengthWithCallback(numbers)(0)(lengthCallback) == 3);


            // sum関数のテスト
            Debug.Assert((int)list.sum(numbers) == 6);

            // length関数のテスト
            Debug.Assert((int)list.length(numbers) == 3);

            // 反復処理における蓄積変数の初期値とコールバック関数の関係のテスト
            Debug.Assert((int)list.product(numbers) == 6);
            Debug.Assert((bool)list.all(list.cons(true, list.cons(true, list.cons(true, list.empty())))));
            Debug.Assert(!(bool)list.all(list.cons(true, list.cons(false, list.cons(true, list.empty())))));
            Debug.Assert((bool)list.any(list.cons(false, list.cons(true, list.cons(false, list.empty())))));
            Debug.Assert(!(bool)list.any(list.cons(false, list.cons(false, list.cons(false, list.empty())))));


            // foldr関数によるreverse関数のテスト
            Debug.Assert(((List<dynamic>)list.toArray(list.reverse(numbers))).SequenceEqual(new List<dynamic> { 3, 2, 1, }));

            // foldr関数によるfind関数のテスト
            Debug.Assert((int)list.find(numbers)((Func<dynamic, dynamic>)(_n => _n % 2 == 0)) == 2);
            Debug.Assert((object)list.find(numbers)((Func<dynamic, dynamic>)(_n => _n < 0)) == null);


            // reduce関数
            dynamic reduce = (Func<dynamic, Func<dynamic, dynamic, dynamic>, dynamic, dynamic>)(
                (_array, _callback, _initialValue) =>
                {
                    dynamic result = _initialValue;
                    foreach (dynamic x in _array)
                        result = _callback(result, x);
                    return result;
                });


            // 7.62 reduceメソッドによるfromArray関数
            dynamic fromArray = (Func<dynamic, dynamic>)(_array =>
                reduce(_array, (Func<dynamic, dynamic, dynamic>)(
                    (_accumulator, _item) => list.append(_accumulator)(list.cons(_item, list.empty()))), list.empty()));
            dynamic theList = fromArray(new List<dynamic> { 0, 1, 2, 3, });
            Debug.Assert(((List<dynamic>)list.toArray(theList)).SequenceEqual(new List<dynamic> { 0, 1, 2, 3, }));


            // 7.64 tarai関数の定義
            dynamic tarai = null;
            tarai = (Func<dynamic, dynamic, dynamic, dynamic>)((_x, _y, _z) =>
                _x > _y ? tarai(tarai(_x - 1, _y, _z), tarai(_y - 1, _z, _x), tarai(_z - 1, _x, _y)) : _y);
            Debug.Assert((int)tarai(1 * 2, 1, 0) == 2);


            // 7.70 add(2, succ(3))の継続渡し
            dynamic identity = (Func<dynamic, dynamic>)(_any => _any);
            dynamic succ_ = (Func<dynamic, dynamic, dynamic>)((_n, _continues) => _continues(_n + 1));
            dynamic add_ = (Func<dynamic, dynamic, dynamic, dynamic>)((_n, _m, _continues) => _continues(_n + _m));
            Debug.Assert((int)succ_(3, (Func<dynamic, dynamic>)(succResult => add_(2, succResult, identity))) == 6);


            // 7.71 継続による反復処理からの脱出
            dynamic find_ = (Func<dynamic, dynamic, dynamic, dynamic, dynamic>)(
                (_aStream, _predicate, _continuesOnFailure, _continuesOnSuccess) => list.match(_aStream, new
                {
                    empty = (Func<dynamic>)(() => _continuesOnSuccess(null)),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tailThunk) =>
                        _predicate(_head) ?
                            _continuesOnSuccess(_head) :
                            _continuesOnFailure(_tailThunk(), _predicate, _continuesOnFailure, _continuesOnSuccess)),
                }));


            // 7.72 find関数に渡す2つの継続
            dynamic continuesOnSuccess = identity;
            dynamic continuesOnFailure = (Func<dynamic, dynamic, dynamic, dynamic, dynamic>)(
                (_aStream, _predicate, _continuesOnSuccess, _escapesFromRecursion) =>
                    find_(_aStream, _predicate, _continuesOnSuccess, _escapesFromRecursion));


            // 7.73 find関数のテスト
            Debug.Assert((int)find_(
                integers, (Func<dynamic, dynamic>)(item => item == 100), continuesOnFailure, continuesOnSuccess) == 100);


            // 7.74 決定性計算機
            dynamic exp_ = null;
            exp_ = new
            {
                //match = (Func<dynamic, dynamic, dynamic>)((_anExp, _pattern) => _anExp.call(exp_, _pattern)),
                match = (Func<dynamic, dynamic, dynamic>)((_anExp, _pattern) => _anExp(_pattern)),
                num = (Func<dynamic, Func<dynamic, dynamic>>)(_n => _pattern => _pattern.num(_n)),
                add = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)((_exp1, _exp2) => _pattern => _pattern.add(_exp1, _exp2)),
            };
            dynamic calculate_ = null;
            calculate_ = (Func<dynamic, dynamic>)(_anExp => exp_.match(_anExp, new
            {
                num = (Func<dynamic, dynamic>)(_n => _n),
                add = (Func<dynamic, dynamic, dynamic>)((_exp1, _exp2) => calculate_(_exp1) + calculate_(_exp2)),
            }));

            Debug.Assert((int)calculate_(exp_.num(2)) == 2);
            Debug.Assert((int)calculate_(exp_.add(exp_.num(2), exp_.num(3))) == 5);


            // 7.75 非決定性計算機の式
            dynamic exp = null;
            exp = new
            {
                //match = (Func<dynamic, dynamic, dynamic>)((_anExp, _pattern) => _anExp.call(exp, _pattern)),
                match = (Func<dynamic, dynamic, dynamic>)((_anExp, _pattern) => _anExp(_pattern)),
                num = (Func<dynamic, Func<dynamic, dynamic>>)(_n => _pattern => _pattern.num(_n)),
                add = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)((_exp1, _exp2) => _pattern => _pattern.add(_exp1, _exp2)),
                amb = (Func<dynamic, Func<dynamic, dynamic>>)(_alist => _pattern => _pattern.amb(_alist)),
            };


            // 7.76 非決定性計算機の評価関数の骨格
            dynamic calculate = null;
            calculate = (Func<dynamic, dynamic, dynamic, dynamic>)((_anExp, _continuesOnSuccess, _continuesOnFailure) => exp.match(_anExp, new
            {
                // 7.79 数値の評価
                num = (Func<dynamic, dynamic>)(_n => _continuesOnSuccess(_n, _continuesOnFailure)),

                // 7.80 足し算の評価
                add = (Func<dynamic, dynamic, dynamic>)(
                    (_x, _y) => calculate(
                        _x, (Func<dynamic, dynamic, dynamic>)(
                            (_resultX, _continuesOnFailureX) => calculate(
                                _y, (Func<dynamic, dynamic, dynamic>)(
                                    (_resultY, _continuesOnFailureY) => _continuesOnSuccess(_resultX + _resultY, _continuesOnFailureY)),
                                _continuesOnFailureX)),
                        _continuesOnFailure)),

                // 7.81 amb式の評価
                amb = (Func<dynamic, dynamic>)(_choices =>
                {
                    dynamic calculateAmb = null;
                    calculateAmb = (Func<dynamic, dynamic>)(__choices => list.match(__choices, new
                    {
                        empty = (Func<dynamic>)(() => _continuesOnFailure()),
                        cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) =>
                            calculate(_head, _continuesOnSuccess, (Func<dynamic>)(() => calculateAmb(_tail)))),
                    }));
                    return calculateAmb(_choices);
                }),
            }));


            // 7.82 非決定性計算機の駆動関数
            dynamic driver = (Func<dynamic, dynamic>)(_expression =>
            {
                dynamic suspendedComputation = null;
                dynamic continuesOnSuccess_ = (Func<dynamic, dynamic, dynamic>)((_anyValue, _continuesOnFailure) =>
                {
                    suspendedComputation = _continuesOnFailure;
                    return _anyValue;
                });
                dynamic continuesOnFailure_ = (Func<dynamic>)(() => null);
                return (Func<dynamic>)(() =>
                    suspendedComputation == null ?
                        calculate(_expression, continuesOnSuccess_, continuesOnFailure_) :
                        suspendedComputation());
            });


            // 7.83 非決定性計算機のテスト
            dynamic ambExp = exp.add(
                exp.amb(fromArray(new List<dynamic> { exp.num(1), exp.num(2), })),
                exp.amb(fromArray(new List<dynamic> { exp.num(3), exp.num(4), })));
            dynamic calculator = driver(ambExp);
            Debug.Assert((int)calculator() == 4);
            Debug.Assert((int)calculator() == 5);
            Debug.Assert((int)calculator() == 5);
            Debug.Assert((int)calculator() == 6);
            Debug.Assert((object)calculator() == null);


            // 7.84 モナド則
            // 右単位原則
            //  flatMap(instanceM)(unit) === instanceM
            // 左単位原則
            //  flatMap(unit(value))(f) === f(value)
            // 結合法則
            //  flatMap(flatMap(instanceM)(f))(g) === flatMap(instanceM)((value) => { return flatMap(f(value))(g));})


            // 7.85 恒等モナドの定義
            dynamic ID = new
            {
                // unit:: T => ID[T]
                unit = (Func<dynamic, dynamic>)(_value => _value),
                // flatMap:: ID[T] => FUN[T => ID[T]] => ID[T]
                flatMap = (Func<dynamic, Func<dynamic, dynamic>>)(_instanceM => _transform => _transform(_instanceM)),
            };


            // 7.86 恒等モナドのunit関数のテスト
            Debug.Assert((int)ID.unit(1) == 1);


            // 7.87 恒等モナドのflatMap関数のテスト
            Debug.Assert((int)ID.flatMap(ID.unit(1))((Func<dynamic, dynamic>)(_one => ID.unit(succ(_one)))) == (int)succ(1));


            // 7.88 flatMapと関数合成の類似性
            dynamic double_ = (Func<dynamic, dynamic>)(_n => _n * 2);
            Debug.Assert(
                (int)ID.flatMap(ID.unit(1))(
                    (Func<dynamic, dynamic>)(_one => ID.flatMap(ID.unit(succ(_one)))(
                        (Func<dynamic, dynamic>)(_two => ID.unit(double_(_two)))))) ==
                (int)compose(double_, succ)(1));


            // 7.89 恒等モナドのモナド則
            dynamic instanceM = ID.unit(1);
            dynamic f = (Func<dynamic, dynamic>)(_n => ID.unit(_n + 1));
            dynamic g = (Func<dynamic, dynamic>)(_n => ID.unit(-_n));
            Debug.Assert((int)ID.flatMap(instanceM)(ID.unit) == (int)instanceM);
            Debug.Assert((int)ID.flatMap(ID.unit(1))(f) == (int)f(1));
            Debug.Assert(
                (int)(ID.flatMap(ID.flatMap(instanceM)(f))(g)) ==
                (int)ID.flatMap(instanceM)((Func<dynamic, dynamic>)(_x => ID.flatMap(f(_x))(g))));


            // 7.91 Maybeの代数的構造
            dynamic maybe = new
            {
                //match = (Func<dynamic, dynamic, dynamic>)((_exp, _pattern) => _exp.call(_pattern, _pattern)),
                match = (Func<dynamic, dynamic, dynamic>)((_exp, _pattern) => _exp(_pattern)),
                just = (Func<dynamic, Func<dynamic, dynamic>>)(_value => _pattern => _pattern.just(_value)),
                nothing = (Func<Func<dynamic, dynamic>>)(() => _pattern => _pattern.nothing()),
            };


            // 7.92 Maybeモナドの定義
            dynamic MAYBE = new
            {
                // unit:: T => MAYBE[T]
                unit = (Func<dynamic, dynamic>)(_value => maybe.just(_value)),
                // flatMap:: MAYBE[T] => FUN[T => MAYBE[T]] => MAYBE[T]
                flatMap = (Func<dynamic, Func<dynamic, dynamic>>)(_instanceM => _transform => maybe.match(_instanceM, new
                {
                    just = (Func<dynamic, dynamic>)(_value => _transform(_value)),
                    nothing = (Func<dynamic>)(() => maybe.nothing()),
                })),

                getOrElse = (Func<dynamic, Func<dynamic, dynamic>>)(_instanceM => _alternate => maybe.match(_instanceM, new
                {
                    just = (Func<dynamic, dynamic>)(_value => _value),
                    nothing = (Func<dynamic>)(() => _alternate),
                })),
            };


            // 7.93 Maybeモナドの利用法
            add = (Func<dynamic, dynamic, dynamic>)((_maybeA, _maybeB) =>
                MAYBE.flatMap(_maybeA)((Func<dynamic, dynamic>)(_a =>
                    MAYBE.flatMap(_maybeB)((Func<dynamic, dynamic>)(_b =>
                        MAYBE.unit(_a + _b))))));
            dynamic justOne = maybe.just(1);
            dynamic justTwo = maybe.just(2);

            Debug.Assert((int)MAYBE.getOrElse(add(justOne, justOne))(null) == 2);
            Debug.Assert((object)MAYBE.getOrElse(add(justOne, maybe.nothing()))(null) == null);


            // 7.94 Pair型の定義
            dynamic pair = null;
            pair = new
            {
                cons = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_left, _right) => _pattern => _pattern.cons(_left, _right)),
                match = (Func<dynamic, dynamic, dynamic>)((_data, _pattern) => _data(_pattern)),
                right = (Func<dynamic, dynamic>)(_tuple => pair.match(_tuple, new
                { cons = (Func<dynamic, dynamic, dynamic>)((_left, _right) => _right), })),
                left = (Func<dynamic, dynamic>)(_tuple => pair.match(_tuple, new
                { cons = (Func<dynamic, dynamic, dynamic>)((_left, _right) => _left), })),
            };


            // 7.95 外界を明示したIOモナドの定義
            dynamic IO_ = null;
            IO_ = new
            {
                // unit:: T => IO[T]
                unit = (Func<dynamic, Func<dynamic, dynamic>>)(_any => _world => pair.cons(_any, _world)),
                // flatMap:: IO[A] => (A => IO[B]) => IO[B]
                flatMap = (Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>)(_instanceA => _actionAB => _world =>
                {
                    dynamic newPair = _instanceA(_world);
                    return pair.match(newPair, new
                    {
                        cons = (Func<dynamic, dynamic, dynamic>)((_value, _newWorld) => _actionAB(_value)(_newWorld)),
                    });
                }),

                // 7.96 IOモナドの補助関数
                // done:: T -> IO[T]
                done = (Func<dynamic, dynamic>)(_any => IO_.unit(null)),
                // run:: IO[A] => A
                run = (Func<dynamic, Func<dynamic, dynamic>>)(_instanceM => _world => pair.left(_instanceM(_world))),

                // 7.97 IOアクションへの変換
                // readFile:: STRING => IO[STRING]
                readFile = (Func<dynamic, Func<dynamic, dynamic>>)(_path => _world =>
                {
                    dynamic content = File.ReadAllText(_path, Encoding.UTF8);
                    return IO_.unit(content)(_world);
                }),
                // println:: STRING => IO[]
                println = (Func<dynamic, Func<dynamic, dynamic>>)(_message => _world =>
                {
                    Console.WriteLine((string)_message);
                    return IO_.unit(null)(_world);
                }),
            };


            // 7.98 run関数の利用法
            dynamic initialWorld = null;
            Debug.Assert((object)IO_.run(IO_.println("吾輩は猫である"))(initialWorld) == null);


            // 7.99 外界を明示しないIOモナドの定義
            dynamic IO = null;
            IO = new
            {
                // unit:: T => IO[T]
                unit = (Func<dynamic, Func<dynamic>>)(_any => () => _any),
                // flatMap:: IO[A] => FUN[A => IO[B]] => IO[B]
                flatMap = (Func<dynamic, Func<dynamic, dynamic>>)(_instanceA => _actionAB => _actionAB(IO.run(_instanceA))),
                // done:: T => IO[T]
                done = (Func<dynamic, dynamic>)(_any => IO.unit(null)),
                // run:: IO[A] => A
                run = (Func<dynamic, dynamic>)(_instance => _instance()),
                // readFile:: STRING => IO[STRING]
                readFile = (Func<dynamic, dynamic>)(_path => IO.unit(File.ReadAllText(_path, Encoding.UTF8))),
                // println:: STRING => IO[]
                println = (Func<dynamic, dynamic>)(_message =>
                {
                    Console.WriteLine((string)_message);
                    return IO.unit(null);
                }),

                // 7.101 putChar関数の定義
                // putChar: CHAR => IO[]
                putChar = (Func<dynamic, dynamic>)(_character =>
                {
                    Console.Write((char)_character);
                    return IO.unit(null);
                }),

                // 7.102 seq関数の定義
                // seq:: IO[T] => IO[U] => IO[U]
                seq = (Func<dynamic, Func<dynamic, dynamic>>)(_actionA => _actionB => IO.unit(IO.run(
                    IO.flatMap(_actionA)((Func<dynamic, dynamic>)(_ =>
                        IO.flatMap(_actionB)(((Func<dynamic, dynamic>)(__ => IO.done(null))))))))),

                // 7.104 putStr関数
                // putStr:: LIST[CHAR] => IO[]
                putStr = (Func<dynamic, dynamic>)(_alist => list.match(_alist, new
                {
                    empty = (Func<dynamic>)(() => IO.done(null)),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => IO.seq(IO.putChar(_head))(IO.putStr(_tail))),
                })),

                // 7.105 putStr関数とputStrLn関数
                // putStrLn:: LIST[CHAR] => IO[]
                putStrLn = (Func<dynamic, dynamic>)(_alist => IO.seq(IO.putStr(_alist))(IO.putChar('\n'))),
            };


            // 7.100 run関数の利用法
            Debug.Assert((object)IO.run(IO.println("名前はまだない")) == null);


            // 7.103 stringモジュール
            dynamic string_ = null;
            string_ = new
            {
                head = (Func<dynamic, dynamic>)(_str => _str[0]),
                tail = (Func<dynamic, dynamic>)(_str => _str.Substring(1)),
                isEmpty = (Func<dynamic, dynamic>)(_str => _str.Length == 0),
                toList = (Func<dynamic, dynamic>)(_str =>
                    string_.isEmpty(_str) ? list.empty() : list.cons(string_.head(_str), string_.toList(string_.tail(_str)))),
            };


            // 7.106 ファイルの内容を画面に出力するプログラム
            //dynamic path = args[2];
            dynamic path = @"..\..\..\dream.txt";

            dynamic cat = IO.flatMap(IO.readFile(path))((Func<dynamic, dynamic>)(_content =>
            {
                dynamic string_as_list = string_.toList(_content);
                return IO.flatMap(IO.putStrLn(string_as_list))((Func<dynamic, dynamic>)(_ => IO.done(null)));
            }));

            IO.run(cat);

        }
    }
}
