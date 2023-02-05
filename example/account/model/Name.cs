namespace ex.account;

public class Name
{
    string value;
    private Name(string name) { this.value = name; }

    public static Name Create(string name)
    {
        if (name == null || name.Length == 0) throw Error.BAD_NAME;
        return new Name(name);
    }

    public override string ToString() { return value; }
}
