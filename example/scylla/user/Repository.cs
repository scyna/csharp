namespace ex.User;

class ScyllaRepository : IUserRepository
{
    public void Create(scyna.Logger LOG, User user)
    {
        try
        {
            var mapper = scyna.Engine.DB.Mapper;
            mapper.Insert(user);
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e);
            throw new Exception(scyna.Error.SERVER_ERROR);
        }
    }

    public User Get(scyna.Logger LOG, ulong userID)
    {
        try
        {
            var mapper = scyna.Engine.DB.Mapper;
            return mapper.Single<User>("WHERE id = ?", userID);
        }
        catch (System.Exception)
        {
            throw new Exception(Error.USER_NOT_EXIST);
        }
    }

    public User Get(scyna.Logger LOG, string email)
    {
        try
        {
            var mapper = scyna.Engine.DB.Mapper;
            return mapper.Single<User>("WHERE email = ?", email);
        }
        catch (System.Exception)
        {
            throw new Exception(Error.USER_NOT_EXIST);
        }
    }

    public bool Exist(scyna.Logger LOG, string email)
    {
        try
        {
            var mapper = scyna.Engine.DB.Mapper;
            var user = mapper.Single<User>("WHERE email = ?", email);
            return true;
        }
        catch (System.Exception) { }
        return false;
    }

    public void Update(scyna.Logger LOG, User user)
    {
        /*TODO*/
    }

    public void AddFriend(scyna.Logger LOG, ulong userID, ulong friendID)
    {
        /*TODO*/
    }

    public IEnumerable<User> ListFriend(scyna.Logger LOG, ulong userID)
    {
        try
        {
            var mapper = scyna.Engine.DB.Mapper;
            IEnumerable<User> users = mapper.Fetch<User>("FROM users WHERE name = ?", "aaa"); //FIXME
            return users;
        }
        catch (System.Exception)
        {
            throw new Exception(scyna.Error.BAD_REQUEST);
        }
    }
}