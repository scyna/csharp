namespace ex.account;

public class Error
{
    public static scyna.Error ACCOUNT_EXISTS = new scyna.Error(100, "Account Exists");
    public static scyna.Error ACCOUNT_NOT_FOUND = new scyna.Error(101, "Account Not Found");
    public static scyna.Error BAD_EMAIL = new scyna.Error(102, "Bad Email");
    public static scyna.Error BAD_NAME = new scyna.Error(103, "Bad Name");
    public static scyna.Error BAD_PASSWORD = new scyna.Error(104, "Bad Password");
}
