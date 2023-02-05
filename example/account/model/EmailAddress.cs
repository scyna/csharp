namespace ex.account;

public class EmailAddress
{
    private const string emailPattern = "^(.+)@(\\S+)$"; // FIXME: do not use this pattern in production
    string value;

    private EmailAddress(string email) { this.value = email; }

    public static EmailAddress Parse(string email)
    {
        if (email == null || email.Length == 0) throw Error.BAD_EMAIL;

        // if (!Pattern.compile(emailPattern).matcher(email).matches())
        // {
        //     throw Error.BAD_EMAIL;
        // }

        return new EmailAddress(email);
    }

    public override string ToString() { return value; }
}
