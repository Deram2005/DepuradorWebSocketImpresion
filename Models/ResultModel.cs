using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketC_.Models
{
    public class ResultModel<T>
    {
        public string Action { get; set; }
        public int? StatusCode { get; set; }
        public string Message { get; set; }
        public T ResponseModel { get; set; }
    }
}
