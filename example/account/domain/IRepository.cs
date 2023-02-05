namespace ex.account;

public interface IRepository
{
    void createAccount(scyna.Command command, Account account);
    Account GetAccountByEmail(EmailAddress email);
    Account GetAccountByID(ulong ID);
}