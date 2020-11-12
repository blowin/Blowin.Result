using System;
using System.Collections.Generic;

namespace Blowin.Result
{
    public readonly struct Result : IEquatable<Result>
    {
        public readonly Exception Error;

        public string FailMessage => Error?.Message;
        
        public bool IsFail => Error != null;
        public bool IsOk => !IsFail;

        public Result(Exception error)
        {
            Error = error;
        }

        public bool Equals(Result other) => Equals(Error, other.Error);

        public override bool Equals(object obj) => obj is Result other && Equals(other);

        public override int GetHashCode() => (Error != null ? Error.GetHashCode() : 0);

        public override string ToString() => IsOk ? "Success" : $"Fail({FailMessage})";

        public void Deconstruct(out bool isOk, out Exception fail)
        {
            isOk = IsOk;
            fail = Error;
        }

        public static implicit operator Result(ResultFail<Exception> fail) => new Result(fail.Value);
        
        public static implicit operator Result(ResultFail<string> fail) => new Result(fail.AsExceptionFail().Value);

        #region Factory methods

        public static ResultFail<T> Fail<T>(T exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new ResultFail<T>(exception);
        }

        public static Result Success() => new Result();
        
        public static ResultOk<T> Success<T>(T value) => new ResultOk<T>(value);
        
        public static Result<TRes> Wrap<TRes>(Func<Result<TRes>> f)
        {
            try
            {
                return f();
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }

        public static Result<TRes> Wrap<TRes>(Func<TRes> f)
        {
            try
            {
                return Success(f());
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }

        public static Result<TRes> Wrap<T, TRes>(Func<T, Result<TRes>> f, T param)
        {
            try
            {
                return f(param);
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }

        public static Result<TRes> Wrap<T, TRes>(Func<T, TRes> f, T param)
        {
            try
            {
                return Success(f(param));
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }

        #endregion
    }
    
    public readonly struct Result<T> : IEquatable<Result<T>>
    {
        /// <summary>
        /// Результат если IsOk == True
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// Результат если IsFail == True
        /// </summary>
        public readonly Exception Error;

        public bool IsOk => Error == null;

        public bool IsFail => !IsOk;
        
        public string FailMessage => Error?.Message;

        internal Result(T success, Exception failure)
        {
            Value = success;
            Error = failure;
        }
        
        public TRes Map<TRes>(Func<T, TRes> okMap, Func<Exception, TRes> failMap) => IsOk ? okMap(Value) : failMap(Error);

        public Result<TRes> Map<TRes>(Func<T, TRes> transform) => IsOk ? Result.Wrap(transform, Value) : Result.Fail(Error);
        
        public Result<TRes> FlatMap<TRes>(Func<T, Result<TRes>> transform) => IsOk ? Result.Wrap(transform, Value) : Result.Fail(Error);

        public Result<T> MapFail(Func<Exception, T> failMap) => IsOk ? this : Result.Wrap(failMap, Error);

        public Result<T> FlatMapFail(Func<Exception, Result<T>> failMap) => IsOk ? this : Result.Wrap(failMap, Error);
        
        public void Deconstruct(out bool isOk, out T value, out Exception error)
        {
            isOk = IsOk;
            value = Value;
            error = Error;
        }

        public override string ToString()
        {
            var status = IsOk ? "Success(" + Value + ")" : "Fail(" + FailMessage + ")";

            return "Result." + status;
        }

        public bool Equals(Result<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value) && Equals(Error, other.Error);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Result<T> && Equals((Result<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Value) * 397) ^
                       (Error != null ? Error.GetHashCode() : 0);
            }
        }

        public Result<TResult> SelectMany<TSecond, TResult>(Func<T, Result<TSecond>> opSelector, Func<T, TSecond, TResult> projection)
        {
            if(IsFail)
                return Result.Fail(Error);

            var sec = opSelector(Value);
            if(sec.IsFail)
                return Result.Fail(sec.Error);

            var result = projection(Value, sec.Value);
            return Result.Success(result);
        }

        public static implicit operator Result<T>(ResultOk<T> val) => new Result<T>(val.Value, null);
        
        public static implicit operator Result<T>(ResultFail<Exception> fail) => new Result<T>(default, fail.Value);
        
        public static implicit operator Result<T>(ResultFail<string> fail) => new Result<T>(default, fail.AsExceptionFail().Value);

        public static implicit operator Result(Result<T> r) => r.IsOk ? Result.Success() : Result.Fail(r.Error);
        
        public static implicit operator Result<T>(T value) => Result.Success(value);
        
        public static implicit operator Result<T>(Exception ex) => Result.Fail(ex);
        
        public static implicit operator Result<T>(string msg) => Result.Fail(msg);
        
        public static implicit operator Result<T>(Result<T, Exception> me) => new Result<T>(me.Value, me.Error);

        public static implicit operator Result<T, Exception>(Result<T> me) => new Result<T, Exception>(me.Value, me.Error, me.IsOk);

        public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

        public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);
    }

    public readonly struct Result<TSuccess, TFailure> : IEquatable<Result<TSuccess, TFailure>>
    {
        /// <summary>
        /// Результат если IsOk == True
        /// </summary>
        public readonly TSuccess Value;

        /// <summary>
        /// Результат если IsOk == False
        /// </summary>
        public readonly TFailure Error;

        public readonly bool IsOk;

        public bool IsFail => !IsOk;

        internal Result(TSuccess success, TFailure error, bool isOk)
        {
            IsOk = isOk;
            Value = success;
            Error = error;
        }
        
        public void Deconstruct(out bool isOk, out TSuccess value, out TFailure error)
        {
            isOk = IsOk;
            value = Value;
            error = Error;
        }

        public override string ToString()
        {
            var status = IsOk ? "Success(" + Value + ")" : $"Fail({Error})";
            return "Result." + status;
        }
        
        public bool Equals(Result<TSuccess, TFailure> other)
        {
            return EqualityComparer<TSuccess>.Default.Equals(Value, other.Value) &&
                   EqualityComparer<TFailure>.Default.Equals(Error, other.Error);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Result<TSuccess, TFailure> && Equals((Result<TSuccess, TFailure>) obj);
        }

        public Result<TResult, TFailure> SelectMany<TSecond, TResult>(Func<TSuccess, Result<TSecond, TFailure>> opSelector, Func<TSuccess, TSecond, TResult> projection)
        {
            if(IsFail)
                return Result.Fail(Error);

            var sec = opSelector(Value);
            if(sec.IsFail)
                return Result.Fail(sec.Error);

            var result = projection(Value, sec.Value);
            return Result.Success(result);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<TSuccess>.Default.GetHashCode(Value);
                hashCode = (hashCode * 397) ^ EqualityComparer<TFailure>.Default.GetHashCode(Error);
                return hashCode;
            }
        }

        public static implicit operator Result<TSuccess, TFailure>(ResultOk<TSuccess> val) => new Result<TSuccess, TFailure>(val.Value, default, true);
        
        public static implicit operator Result<TSuccess, TFailure>(ResultFail<TFailure> fail) => new Result<TSuccess, TFailure>(default, fail.Value, false);
        
        public static implicit operator Result<TSuccess, TFailure>(TSuccess success) => Result.Success(success);
        
        public static implicit operator Result<TSuccess, TFailure>(TFailure fail) => Result.Fail(fail);

        public static bool operator ==(Result<TSuccess, TFailure> left, Result<TSuccess, TFailure> right) => left.Equals(right);

        public static bool operator !=(Result<TSuccess, TFailure> left, Result<TSuccess, TFailure> right) => !left.Equals(right);
    }
}