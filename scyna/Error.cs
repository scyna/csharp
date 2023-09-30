namespace scyna;

public class Error : Exception
{
    public static readonly Error OK = new("OK", "Success");
    public static readonly Error SERVER_ERROR = new("ServerError", "Server Error");
    public static readonly Error BAD_REQUEST = new("BadRequest", "Bad Request");
    public static readonly Error PERMISSION_ERROR = new("PermissionError", "Permission Error");
    public static readonly Error REQUEST_INVALID = new("RequestInvalid", "Request Invalid");
    public static readonly Error MODULE_NOT_EXIST = new("ModuleNotExist", "Module Not Exist");
    public static readonly Error BAD_DATA = new("BadData", "Bad Data");
    public static readonly Error STREAM_ERROR = new("StreamError", "Stream Error");

    public static readonly Error OBJECT_NOT_FOUND = new("ObjectNotFound", "Object Not Found");
    public static readonly Error OBJECT_EXISTS = new("ObjectExists", "Object Exists");

    public static readonly Error COMMAND_NOT_COMPLETED = new("CommandNotCompleted", "Command Not Completed");
    public static readonly Error EVENT_STORE_NULL = new("EventStoreIsNull", "EventStore Is Null");
    public static readonly Error API_CALL_ERROR = new("ApiCallError", "Api Call Error");

    private readonly string code;
    public Error(string code, string message) : base(message) { this.code = code; }
    public string Code() { return code; }
    public proto.Error ToProto() { return new proto.Error { Code = code, Message = this.Message }; }
}