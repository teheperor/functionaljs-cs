using System;

namespace Chapter3
{
    class Program
    {
        static void Main(string[] args)
        {
            // reduce関数
            dynamic reduce = (Func<dynamic, Func<dynamic, dynamic, dynamic>, dynamic, dynamic>)(
                (_array, _callback, _initialValue) =>
                {
                    dynamic result = _initialValue;
                    foreach (dynamic x in _array)
                        result = _callback(result, x);
                    return result;
                });


            // 3.3 DRYなtimes関数
            dynamic times = null;
            times = (Func<dynamic, dynamic, dynamic, Func<dynamic, dynamic, dynamic>, dynamic>)(
                (_count, _arg, _memo, _fun) => _count > 1 ? times(_count - 1, _arg, _fun(_arg, _memo), _fun) : _fun(_arg, _memo));


            // 3.4 DRYなかけ算関数とべき乗関数
            dynamic add = (Func<dynamic, dynamic, dynamic>)((_n, _m) => _n + _m);
            dynamic multiiply = (Func<dynamic, dynamic, dynamic>)((_n, _m) => times(_m, _n, 0, add));
            dynamic exponential = (Func<dynamic, dynamic, dynamic>)((_n, _m) => times(_m, _n, 1, multiiply));


            // 3.8 reduceによるsum関数
            dynamic sum = (Func<dynamic, dynamic>)(
                _array => reduce(_array, (Func<dynamic, dynamic, dynamic>)((_x, _y) => _x + _y), 0));

        }
    }
}
