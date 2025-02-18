using System.Text.Json.Serialization;

namespace MediHub.Web.HttpConfig
{
    public class ServiceResponse
    {
        public object Data { get; set; }
        public int StatusCode { get; set; }
        public bool Succeeded { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string AdditionalInfo { get; set; }

        [JsonIgnore]
        public byte[] FileDatas { get; set; }

        public ServiceResponse()
        {
        }

        public static ServiceResponse Succeed(int statusCode, object data = default, string code = "", string message = "") => new ServiceResponse
        {
            StatusCode = statusCode,
            Data = data,
            Code = code,
            Message = message,
            Succeeded = true
        };

        public static ServiceResponse Fail(int statusCode, string code, string message) => new ServiceResponse
        {
            StatusCode = statusCode,
            Code = code,
            Message = message,
            Succeeded = false
        };

        public T GetData<T>()
        {
            try
            {
                var result = Convert.ChangeType(Data, typeof(T));

                return result == null ? default : (T)result;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("Data type is not correct.");
            }
        }
    }
}
