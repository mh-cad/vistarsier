using System;

namespace CAPI.UI.Models
{
    public class Response
    {
        public object Data { get; set; }
        public Exception Exception { get; set; }
        
        public Response()
        {
            Data = new object();
        }

        public Response(object data)
        {
            Data = data;
        }

        public object GetViewModel()
        {
            return new
            {
                Data,
                Success = Exception == null
            };
        }
    }
}
