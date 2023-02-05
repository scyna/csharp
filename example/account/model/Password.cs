namespace ex.account;

public class Password
{
    string value;

    private Password(string password) { this.value = password; }

    public static Password Create(string password)
    {
        if (password == null || password.Length == 0) throw Error.BAD_PASSWORD;
        /* TODO: some other rules here */
        return new Password(password);
    }

    public override string ToString() { return value; }
}
