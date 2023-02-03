namespace scyna;

public class Error : Exception
{
    public static Error OK = new Error(0, "Success");
    public static Error SERVER_ERROR = new Error(1, "Server Error");
    public static Error BAD_REQUEST = new Error(2, "Bad Request");
    public static Error PERMISSION_ERROR = new Error(4, "Permission Error");
    public static Error REQUEST_INVALID = new Error(5, "Request Invalid");
    public static Error MODULE_NOT_EXIST = new Error(6, "Module Not Exist");

    private int code;
    public Error(int code, string message) : base(message) { this.code = code; }
    public int Code() { return code; }
    public proto.Error ToProto() { return new proto.Error { Code = code, Message = this.Message }; }
}