namespace SimoraUnity.Http
{
    public class Result<T>
    {
        public bool Success;

        public T Data;

        public string Message;
    }
}