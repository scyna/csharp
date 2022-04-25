namespace scyna;

public class Error
{
    public static proto.Error OK = new proto.Error { Code = 0, Message = "Success" };
    public static proto.Error SERVER_ERROR = new proto.Error { Code = 1, Message = "Server Error" };
    public static proto.Error BAD_REQUEST = new proto.Error { Code = 2, Message = "Bad Request" };
    public static proto.Error PERMISSION_ERROR = new proto.Error { Code = 4, Message = "Permission Error" };
    public static proto.Error REQUEST_INVALID = new proto.Error { Code = 5, Message = "Request Invalid" };
    public static proto.Error MODULE_NOT_EXIST = new proto.Error { Code = 6, Message = "Module Not Exist" };
}
