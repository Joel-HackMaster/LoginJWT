namespace LoginAPI.Utilidad
{
    public class Response<T>
    {
        public bool status { get; set; }
        public T value { get; set; }
        public string msg { get; set; }
        public string token { get; set; }
        public string refreshtoken { get; set; }
    }
}
