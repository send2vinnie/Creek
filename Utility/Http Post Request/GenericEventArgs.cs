namespace System.Net
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Value { get; private set; }

        public GenericEventArgs(T value)
        {
            Value = value;
        }
    }
}
