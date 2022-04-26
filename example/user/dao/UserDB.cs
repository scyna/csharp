namespace dao;
using scyna;

class UserDB : IUserDB
{
    public void Create(scyna.Logger LOG, User user)
    {
        try
        {
            var mapper = Engine.DB.Mapper;
            user.ID = Engine.ID.Next();
            mapper.Insert(user);
        }
        catch (Exception)
        {
            throw new DBException(scyna.Error.SERVER_ERROR);
        }
    }

    public User Get(scyna.Logger LOG, ulong userID)
    {
        try
        {
            var mapper = Engine.DB.Mapper;
            return mapper.Single<User>("WHERE id = ?", userID);
        }
        catch (Exception)
        {
            throw new DBException(dao.Error.USER_NOT_EXIST);
        }
    }

    public bool Exist(scyna.Logger LOG, string email)
    {
        try
        {
            var mapper = Engine.DB.Mapper;
            var user = mapper.Single<User>("WHERE email = ?", email);
            return true;
        }
        catch (Exception) { }
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