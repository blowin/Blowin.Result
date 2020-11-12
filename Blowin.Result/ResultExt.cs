using System;

namespace Blowin.Result
{
    public static class ResultExt
    {
        public static void ThrowIfFail(this Result self)
        {
            if (self.IsFail)
                throw self.Error;
        }

        public static void ThrowIfFail<T>(this Result<T> self)
        {
            if (self.IsFail)
                throw self.Error;
        }

        public static void ThrowIfFail<TO, TF>(this Result<TO, TF> self)
            where TF : Exception
        {
            if (self.IsFail)
                throw self.Error;
        }
        
        public static T Unwrap<T>(this Result<T> self)
        {
            if (self.IsOk)
                return self.Value;

            throw self.Error;
        }
     
        public static TSuccess Unwrap<TSuccess, TFail>(this Result<TSuccess, TFail> self)
        {
            if (self.IsOk)
                return self.Value;

            if (self.Error is Exception)
                throw (Exception)(object)self.Error;

            throw new InvalidOperationException();
        }

        public static string FailMessage<T, TFail>(this Result<T, TFail> self)
            where TFail : Exception => self.Error?.Message;
        
        internal static ResultFail<Exception> AsExceptionFail(this ResultFail<string> self)
            => new ResultFail<Exception>(new ResultException(self.Value));
    }
}