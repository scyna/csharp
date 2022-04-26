namespace dao;

class UserDAO : IUserDAO
{
    public void Create(scyna.Logger LOG, User user)
    {

        throw new DBException(scyna.Error.BAD_REQUEST);
    }

    public User Get(scyna.Logger LOG, ulong userID)
    {
        throw new DBException(scyna.Error.BAD_REQUEST);
    }

    public bool Exist(scyna.Logger LOG, string email)
    {
        return false;
    }

    public void Update(scyna.Logger LOG, User user)
    {

    }

    public void AddFriend(scyna.Logger LOG, ulong userID, ulong friendID)
    {

    }

    public List<User> ListFriend(scyna.Logger LOG, ulong userID)
    {
        throw new DBException(scyna.Error.BAD_REQUEST);
    }
}