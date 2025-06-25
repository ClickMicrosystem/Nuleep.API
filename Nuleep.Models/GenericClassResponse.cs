using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class GenericClassResponse<T>
    {
        public T Data { get; set; }
        public int Code { get; set; }

        public GenericClassResponse(T data, int code)
        {
            Data = data;
            Code = code;
        }

        public static GenericClassResponse<T> Create(T data, int code)
        {
            return new GenericClassResponse<T>(data, code);
        }
    }
}
