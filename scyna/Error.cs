namespace scyna;

public class Error : Exception
{
    public static readonly Error OK = new(0, "Success");
    public static readonly Error SERVER_ERROR = new(1, "Server Error");
    public static readonly Error BAD_REQUEST = new(2, "Bad Request");
    public static readonly Error PERMISSION_ERROR = new(4, "Permission Error");
    public static readonly Error REQUEST_INVALID = new(5, "Request Invalid");
    public static readonly Error MODULE_NOT_EXIST = new(6, "Module Not Exist");
    public static readonly Error BAD_DATA = new(7, "Bad Data");
    public static readonly Error STREAM_ERROR = new(8, "Stream Error");

    public static readonly Error OBJECT_NOT_FOUND = new(9, "Object Not Found");
    public static readonly Error OBJECT_EXISTS = new(10, "Object Exists");

    public static readonly Error COMMAND_NOT_COMPLETED = new(11, "Command Not Completed");
    public static readonly Error EVENT_STORE_NULL = new(12, "EventStore Is Null");
    public static readonly Error API_CALL_ERROR = new(13, "Api Call Error");

    private readonly int code;
    public Error(int code, string message) : base(message) { this.code = code; }
    public int Code() { return code; }
    public proto.Error ToProto() { return new proto.Error { Code = code, Message = this.Message }; }
}