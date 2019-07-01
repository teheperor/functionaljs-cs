using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chapter2
{
    class Program
    {
        static void Main(string[] args)
        {
            // 2.6 関数型プログラミングによる足し算の定義
            dynamic add = null;
            add = (Func<dynamic, dynamic, dynamic>)((_x, _y) => _y < 1 ? _x : add(_x + 1, _y - 1));

            Debug.Assert((int)add(2, 3) == 5);


            // reduce関数
            dynamic reduce = (Func<dynamic, Func<dynamic, dynamic, dynamic>, dynamic, dynamic>)(
                (_array, _callback, _initialValue) =>
                {
                    dynamic result = _initialValue;
                    foreach (dynamic x in _array)
                        result = _callback(result, x);
                    return result;
                });


            // 2.9 sum関数の定義
            dynamic sum = (Func<dynamic, dynamic>)(
                _array => reduce(_array, (Func<dynamic, dynamic, dynamic>)((_accumurator, _item) => _accumurator + _item), 0));

            Debug.Assert((int)(sum(new[] { 1, 2, 3, })) == 6);


            // 2.10 product関数の定義
            dynamic product = (Func<dynamic, dynamic>)(
                _array => reduce(_array, (Func<dynamic, dynamic, dynamic>)((_accumurator, _item) => _accumurator * _item), 1));

            Debug.Assert((int)(sum(new[] { 1, 2, 3, })) == 6);


            // 2.11 map関数の定義
            dynamic map = (Func<dynamic, Func<dynamic, dynamic>>)(
                _transform => _array => reduce(_array, (Func<dynamic, dynamic, dynamic>)(
                    (_accumulator, _item) => ((List<dynamic>)_accumulator).Concat(
                        new List<dynamic> { _transform(_item), }).ToList()), new List<dynamic> { }));


            // 2.12 map関数のテスト
            dynamic succ = (Func<dynamic, dynamic>)(_n => _n + 1);

            Debug.Assert(((List<dynamic>)map(succ)(new int[] { 1, 3, 5, })).SequenceEqual(new List<dynamic> { 2, 4, 6, }));


            // 2.13 constant関数の定義
            dynamic constant = (Func<dynamic, Func<dynamic, dynamic>>)(_any => _ => _any);
            dynamic alwaysOne = (Func<dynamic, dynamic>)(constant(1));


            // 2.14 map(alwaysOne)で配列の全要素を1に変える
            Debug.Assert(((List<dynamic>)map(alwaysOne)(new List<dynamic> { 1, 2, 3, })).SequenceEqual(new List<dynamic> { 1, 1, 1, }));


            // 2.15 関数適用によるlength関数の定義
            dynamic length = (Func<dynamic, dynamic>)(_array => sum(map(alwaysOne)(_array)));

            Debug.Assert((int)length(new List<dynamic> { 1, 2, 3, }) == 3);


            // 2.16 関数の合成
            dynamic compose = (Func<Func<dynamic, dynamic>, Func<dynamic, dynamic>, Func<dynamic, dynamic>>)(
                (_f, _g) => _arg => _f(_g(_arg)));


            // 2.17 関数合成によるlength関数の定義（ポイントフリースタイル）
            length = compose(sum, map(alwaysOne));

            Debug.Assert((int)length(new List<dynamic> { 1, 2, 3, }) == 3);


            // 2.18 関数合成によるlength関数の定義（引数を明示したスタイル）
            length = (Func<dynamic, dynamic>)(_array => compose(sum, map(alwaysOne))(_array));

            Debug.Assert((int)length(new List<dynamic> { 1, 2, 3, }) == 3);


            // 2.19 正格評価の例
            Debug.Assert((int)length(new[] { 1, 1 + 1, }) == 2);


            // 2.20 遅延評価の例
            Debug.Assert((int)length(new List<dynamic> { 1, (Func<dynamic>)(() => 1 + 1), }) == 2);


            // 2.21 ストリームの例
            dynamic aStream = new List<dynamic> { 1, (Func<dynamic>)(() => 2), };

            Debug.Assert((int)aStream[0] == 1);
            Debug.Assert((int)aStream[1]() == 2);


            // 2.22 enumFrom関数
            dynamic enumFrom = null;
            enumFrom = (Func<dynamic, dynamic>)(_n => new List<dynamic> { _n, (Func<dynamic>)(() => enumFrom(succ(_n))), });

            Debug.Assert((int)enumFrom(1)[0] == 1);
            Debug.Assert((int)enumFrom(1)[1]()[0] == 2);
            Debug.Assert((int)enumFrom(1)[1]()[1]()[0] == 3);


            // 2.23 無限の偶数列を作る
            dynamic evenFrom = null;
            evenFrom = (Func<dynamic, dynamic>)(_n => new List<dynamic> { _n, (Func<dynamic>)(() => evenFrom(_n + 2)), });
            dynamic evenStream = evenFrom(2);

            Debug.Assert((int)evenStream[0] == 2);
            Debug.Assert((int)evenStream[1]()[0] == 4);
            Debug.Assert((int)evenStream[1]()[1]()[0] == 6);


            // 2.24 iterate関数
            dynamic iterate = null;
            iterate = (Func<dynamic, Func<dynamic, dynamic>>)(
                _init => _step => new List<dynamic> { _init, (Func<dynamic>)(() => iterate(_step(_init))(_step)), });


            // 2.25 無限ストリームの例
            enumFrom = (Func<dynamic, dynamic>)(_n => iterate(_n)(succ));
            dynamic naturals = enumFrom(1);
            dynamic twoStep = (Func<dynamic, dynamic>)(_n => _n + 2);
            evenStream = iterate(2)(twoStep);

            Debug.Assert((int)naturals[0] == 1);
            Debug.Assert((int)naturals[1]()[0] == 2);
            Debug.Assert((int)naturals[1]()[1]()[0] == 3);

            Debug.Assert((int)evenStream[0] == 2);
            Debug.Assert((int)evenStream[1]()[0] == 4);
            Debug.Assert((int)evenStream[1]()[1]()[0] == 6);


            // 2.26 ストリームのfilter関数
            dynamic filter = null;
            filter = (Func<dynamic, Func<dynamic, dynamic>>)(_predicate => _aStream =>
            {
                dynamic head = _aStream[0];
                return _predicate(head) ?
                    new List<dynamic> { head, (Func<dynamic>)(() => filter(_predicate)(_aStream[1]())), } :
                    filter(_predicate)(_aStream[1]());
            });


            // 2.27 filter関数で無限の偶数列を作る
            dynamic even = (Func<dynamic, dynamic>)(_n => _n % 2 == 0);
            evenStream = filter(even)(enumFrom(1));

            Debug.Assert((int)evenStream[0] == 2);
            Debug.Assert((int)evenStream[1]()[0] == 4);
            Debug.Assert((int)evenStream[1]()[1]()[0] == 6);


            // 2.28 ストリームのelemAt関数
            dynamic elemAt = null;
            elemAt = (Func<dynamic, Func<dynamic, dynamic>>)(
                _n => _aStream => _n == 1 ? _aStream[0] : elemAt(_n - 1)(_aStream[1]()));


            // 2.29 3番目の偶数を求める
            Debug.Assert((int)elemAt(3)(evenStream) == 6);


            // 2.39 プロパティテストのための関数
            dynamic map_ = null;
            map_ = (Func<dynamic, Func<dynamic, dynamic>>)(_transform => _aStream =>
            {
                dynamic head = _aStream[0];
                return new List<dynamic> { _transform(head), (Func<dynamic>)(() => map_(_transform)(_aStream[1]())), };
            });
            dynamic take = null;
            take = (Func<dynamic, Func<dynamic, dynamic>>)(_n => _aStream =>
                _n == 0 ? null : new List<dynamic> { _aStream[0], (Func<dynamic>)(() => take(_n - 1)(_aStream[1]())), });
            dynamic all = (Func<dynamic, dynamic>)(_aStream =>
            {
                dynamic allHelper = null;
                allHelper = (Func<dynamic, dynamic, dynamic>)((__aStream, _accumulator) =>
                {
                    dynamic head = __aStream[0];
                    dynamic newAccumulator = _accumulator && head;
                    return __aStream[1]() == null ? newAccumulator : allHelper(__aStream[1](), newAccumulator);
                });
                return allHelper(_aStream, true);
            });
            dynamic proposistion = (Func<dynamic, dynamic>)(_n => succ(0) + succ(_n) == succ(succ(_n)));


            // 2.40 succ関数のプロパティテスト
            Debug.Assert((bool)all(take(100)(map_(proposistion)(enumFrom(0)))));

        }
    }
}
