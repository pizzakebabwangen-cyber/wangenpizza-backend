namespace WangenPizza.Helper
{
    public class UserManagerResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
       public IEnumerable<string> Errors { get; set; }
        public object Data { get; set; }

    }
}
