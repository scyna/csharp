namespace ex.account;

using scyna;

public class Account
{
    private scyna.Context context;

    public Account(Context context)
    {
        this.context = context;
    }

    public long ID;
    public Name? name;
    public EmailAddress? email;
    public Password? password;

}