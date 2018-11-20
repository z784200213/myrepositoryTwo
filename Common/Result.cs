using System;

namespace Common
{
    public class Result {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public bool IsSuccess { get; set; }

        private Result(int code,string message,object data,bool issuccess)
        {
            this.Code = code;
            this.Message = message;
            this.Data = data;
            this.IsSuccess = issuccess;
        }
        public static Result Success(int code=1,object data=null,string message=null)
        {
            
            return new Result(code, message, data,true) ;
        }
        public static Result Fail(int code = -1, object data = null, string message = null)
        {

            return new Result(code, message, data, false);
        }
        public static Result Fail(int code = -1, object data = null, Exception ex = null)
        {

            string message = "";
            if (ex != null)
            {
                message = ex.Message;
            }
            return new Result(code, message, data, false);
        }
    }
}