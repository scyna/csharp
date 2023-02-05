namespace ex.account;

using System.Text.RegularExpressions;

public class EmailAddress
{
    private static Regex emailPattern = new Regex("^(.+)@(\\S+)$", RegexOptions.Compiled);  // FIXME: do not use this pattern in production
    string value;

    private EmailAddress(string email) { this.value = email; }

    public static EmailAddress Parse(string email)
    {
        if (email == null || email.Length == 0) throw Error.BAD_EMAIL;
        if (!emailPattern.IsMatch(email)) throw Error.BAD_EMAIL;
        return new EmailAddress(email);
    }

    public override string ToString() { return value; }
}
