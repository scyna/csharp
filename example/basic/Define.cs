namespace ex.Basic;

public class Path
{
    public static String ECHO_USER_URL = "/example/basic/echo";
    public static String HELLO_USER_URL = "/example/basic/hello";
    public static String ADD_USER_URL = "/example/basic/add";
}

public class Error
{
    public static scyna.proto.Error TOO_BIG = new scyna.proto.Error { Code = 100, Message = "Result is too big" };
}