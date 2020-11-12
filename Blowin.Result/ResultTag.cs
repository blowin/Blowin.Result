namespace Blowin.Result
{
    public readonly struct ResultOk<T>
    {
        public readonly T Value;

        public ResultOk(T value) => Value = value;
    }

    public readonly struct ResultFail<T>
    {
        public readonly T Value;

        public ResultFail(T value) => Value = value;
    }
}