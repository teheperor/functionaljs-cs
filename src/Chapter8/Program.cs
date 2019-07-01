using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chapter8
{
    class Program
    {
        static void Main(string[] args)
        {
            // 8.2 式の代数的データ構造
            dynamic exp = null;
            exp = new
            {
                match = (Func<dynamic, dynamic, dynamic>)((_data, _pattern) => _data(_pattern)),
                num = (Func<dynamic, Func<dynamic, dynamic>>)(_value => _pattern => _pattern.num(_value)),
                variable = (Func<dynamic, Func<dynamic, dynamic>>)(_name => _pattern => _pattern.variable(_name)),
                lambda = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_variable, _body) => _pattern => _pattern.lambda(_variable, _body)),
                app = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_lambda, _arg) => _pattern => _pattern.app(_lambda, _arg)),

                // 8.3 演算の定義
                add = (Func<dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_expL, _expR) => _pattern => _pattern.add(_expL, _expR)),

                // 8.20 ログ出力評価器の式
                log = (Func<dynamic, Func<dynamic, dynamic>>)(_anExp => _pattern => _pattern.log(_anExp)),
            };


            // 8.5 クロージャーによる「環境」の定義
            dynamic env = null;
            env = new
            {
                // empty:: STRING => VALUE
                empty = (Func<dynamic, dynamic>)(_variable => null),
                // lookup:: (STRING, ENV) => VALUE
                lookup = (Func<dynamic, dynamic, dynamic>)((_name, _environment) => _environment(_name)),
                // extend:: (STRING, VALUE, ENV) => ENV
                extend = (Func<dynamic, dynamic, dynamic, Func<dynamic, dynamic>>)(
                    (_identifier, _value, _environment) => _queryIdentifier =>
                        _identifier == _queryIdentifier ? _value : env.lookup(_queryIdentifier, _environment)),
            };


            // 8.7 変数バインディングにおける環境のセマンティクス
            Debug.Assert((int)((Func<dynamic>)(() =>
            {
                dynamic _newEnv = env.extend("a", 1, env.empty);
                return env.lookup("a", _newEnv);
            }))() == 1);


            // 8.8 クロージャーにおける「環境」の働き
            dynamic x = 1;
            dynamic closure = (Func<dynamic>)(() =>
            {
                dynamic y = 2;
                return x + y;
            });

            Debug.Assert((int)closure() == 3);


            // 8.9 クロージャーにおける環境のセマンティクス
            Debug.Assert((int)((Func<dynamic>)(() =>
            {
                dynamic initEnv = env.empty;
                dynamic outerEnv = env.extend("x", 1, initEnv);
                dynamic closureEnv = env.extend("y", 2, outerEnv);
                return env.lookup("x", closureEnv) + env.lookup("y", closureEnv);
            }))() == 3);


            // 恒等モナドの定義
            dynamic ID = new
            {
                // unit:: T => ID[T]
                unit = (Func<dynamic, dynamic>)(_value => _value),
                // flatMap:: ID[T] => FUN[T => ID[T]] => ID[T]
                flatMap = (Func<dynamic, Func<dynamic, dynamic>>)(_instanceM => _transform => _transform(_instanceM)),
            };


            // 8.10 恒等モナド評価器の定義
            // evaluate_:: (EXP, ENV) => ID[VALUE]
            dynamic evaluate_ = null;
            evaluate_ = (Func<dynamic, dynamic, dynamic>)((_anExp, _environment) => exp.match(_anExp, new
            {
                num = (Func<dynamic, dynamic>)(_numericValue => ID.unit(_numericValue)),
                variable = (Func<dynamic, dynamic>)(_name => ID.unit(env.lookup(_name, _environment))),
                lambda = (Func<dynamic, dynamic, dynamic>)((_variable, _body) => exp.match(_variable, new
                {
                    variable = (Func<dynamic, dynamic>)(
                            _name => ID.unit((Func<dynamic, dynamic>)(
                                _actualArg => evaluate_(_body, env.extend(_name, _actualArg, _environment))))),
                })),
                app = (Func<dynamic, dynamic, dynamic>)(
                    (_lambda, _arg) => ID.flatMap(evaluate_(_lambda, _environment))((Func<dynamic, dynamic>)(
                        _closure => ID.flatMap(evaluate_(_arg, _environment))((Func<dynamic, dynamic>)(
                            _actualArg => _closure(_actualArg)))))),
                add = (Func<dynamic, dynamic, dynamic>)(
                    (_expL, _expR) => ID.flatMap(evaluate_(_expL, _environment))(
                        (Func<dynamic, dynamic>)(_valueL => ID.flatMap(evaluate_(_expR, _environment))(
                            (Func<dynamic, dynamic>)(_valueR => ID.unit(_valueL + _valueR)))))),
            }));


            // 8.12 数値の評価のテスト
            Debug.Assert((int)evaluate_(exp.num(2), env.empty) == 2);


            // 8.14 変数の評価のテスト
            dynamic newEnv = env.extend("x", 1, env.empty);
            Debug.Assert((int)evaluate_(exp.variable("x"), newEnv) == (int)ID.unit(1));


            // 8.16 足し算の評価のテスト
            dynamic addition = exp.add(exp.num(1), exp.num(2));
            Debug.Assert((int)evaluate_(addition, env.empty) == (int)ID.unit(3));


            // 8.17 関数適用の評価のテスト
            dynamic expression_ = exp.app(exp.lambda(exp.variable("n"), exp.add(exp.variable("n"), exp.num(1))), exp.num(2));
            Debug.Assert((int)evaluate_(expression_, env.empty) == (int)ID.unit(3));


            //// 8.18 カリー化関数の例
            //((n) => {
            //  return (m) => {
            //    return n + m;
            //  };
            //})(2)(3)


            // 8.19 カリー化関数の評価
            dynamic expression = exp.app(exp.app(
                exp.lambda(exp.variable("n"),
                    exp.lambda(exp.variable("m"),
                        exp.add(exp.variable("n"), exp.variable("m")))),
                    exp.num(2)),
                exp.num(3));
            Debug.Assert((int)(evaluate_(expression, env.empty)) == (int)ID.unit(5));


            // Pair型の定義
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

                append = (Func<dynamic, Func<dynamic, dynamic>>)(_xs => _ys => list.match(_xs, new
                {
                    empty = (Func<dynamic>)(() => _ys),
                    cons = (Func<dynamic, dynamic, dynamic>)((_head, _tail) => list.cons(_head, list.append(_tail)(_ys))),
                })),

            };


            // 8.21 LOGモナドの定義
            // LOG:: PAIR[T, LIST[STRING]]
            dynamic LOG = new
            {
                // unit:: VALUE => LOG[VALUE]
                unit = (Func<dynamic, dynamic>)(_value => pair.cons(_value, list.empty())),
                // flatMap:: LOG[T] => FUN[T => LOG[T]] => LOG[T]
                flatMap = (Func<dynamic, Func<dynamic, dynamic>>)(_instanceM => _transform => pair.match(_instanceM, new
                {
                    cons = (Func<dynamic, dynamic, dynamic>)((_value, _log) =>
                    {
                        dynamic newInstance = _transform(_value);
                        return pair.cons(pair.left(newInstance), list.append(_log)(pair.right(newInstance)));
                    }),
                })),
                // output:: VALUE => LOG[()]
                output = (Func<dynamic, dynamic>)(_value => pair.cons(null, list.cons(_value.ToString(), list.empty()))),
            };


            // 8.22 LOGモナド評価器
            // evaluate:: (EXP, ENV) => LOG[VALUE]
            dynamic evaluate = null;
            evaluate = (Func<dynamic, dynamic, dynamic>)((_anExp, _environment) => exp.match(_anExp, new
            {
                log = (Func<dynamic, dynamic>)(
                    (Func<dynamic, dynamic>)(__anExp => LOG.flatMap(evaluate(__anExp, _environment))(
                        (Func<dynamic, dynamic>)(_value => LOG.flatMap(LOG.output(_value))(
                            (Func<dynamic, dynamic>)(_ => LOG.unit(_value))))))),

                num = (Func<dynamic, dynamic>)(_numericValue => LOG.unit(_numericValue)),
                variable = (Func<dynamic, dynamic>)(_name => LOG.unit(env.lookup(_name, _environment))),
                lambda = (Func<dynamic, dynamic, dynamic>)((_variable, _body) => exp.match(_variable, new
                {
                    variable = (Func<dynamic, dynamic>)(
                            _name => LOG.unit((Func<dynamic, dynamic>)(
                                _actualArg => evaluate(_body, env.extend(_name, _actualArg, _environment))))),
                })),
                app = (Func<dynamic, dynamic, dynamic>)(
                    (_lambda, _arg) => LOG.flatMap(evaluate(_lambda, _environment))((Func<dynamic, dynamic>)(
                        _closure => LOG.flatMap(evaluate(_arg, _environment))((Func<dynamic, dynamic>)(
                            _actualArg => _closure(_actualArg)))))),
                add = (Func<dynamic, dynamic, dynamic>)(
                    (_expL, _expR) => LOG.flatMap(evaluate(_expL, _environment))(
                        (Func<dynamic, dynamic>)(_valueL => LOG.flatMap(evaluate(_expR, _environment))(
                            (Func<dynamic, dynamic>)(_valueR => LOG.unit(_valueL + _valueR)))))),
            }));


            // 8.23 ログ出力の対象とする式
            // (n => 1 + n)(2)
            dynamic theExp_ = exp.app(
                exp.lambda(
                    exp.variable("n"),
                    exp.add(exp.num(1), exp.variable("n"))),
                exp.num(2));


            // 8.24 ログ出力のための式
            dynamic theExp = exp.log(exp.app(
                exp.lambda(
                    exp.variable("n"),
                    exp.add(exp.log(exp.num(1)), exp.variable("n"))),
                exp.log(exp.num(2))));


            // 8.25 ログ出力評価器による評価戦略の確認
            pair.match(evaluate(theExp, env.empty), new
            {
                cons = (Func<dynamic, dynamic, dynamic>)((_value, _log) =>
                {
                    Debug.Assert((int)_value == 3);
                    Debug.Assert(((List<dynamic>)list.toArray(_log)).SequenceEqual(new List<dynamic> { "2", "1", "3", }));
                    return null;
                }),
            });

        }
    }
}
