namespace scyna;
using Google.Protobuf;

public interface ISettings
{
    bool Write(string key, string value);
    bool Write<T>(string key, T value) where T : IMessage<T>, new();
    string? Read(string key);
    T? Read<T>(string key) where T : IMessage<T>, new();
    bool Remove(string key);
    void update(string key, string value);
    void remove(string key);
}

public class Settings : ISettings
{
    Dictionary<string, string> data = new Dictionary<string, string>();
    JsonFormatter formater = new JsonFormatter(new JsonFormatter.Settings(false));

    public bool Write(string key, string value)
    {
        var response = Service.SendRequest(Path.SETTING_WRITE_URL, new proto.WriteSettingRequest
        {
            Module = Engine.Instance.Module,
            Key = key,
            Value = value
        });

        if (response != null && response.Code == 200)
        {
            update(key, value);
            return true;
        }
        return false;
    }
    public bool Write<T>(string key, T value) where T : IMessage<T>, new()
    {
        var s = formater.Format(value);
        return Write(key, s);
    }
    public string? Read(string key)
    {
        /*from cache*/
        lock (this)
        {
            var ret = data[key];
            if (ret != null) return ret;
        }

        var response = Service.SendRequest<proto.ReadSettingResponse>(Path.SETTING_READ_URL, new proto.ReadSettingRequest
        {
            Module = Engine.Instance.Module,
            Key = key
        });

        if (response != null)
        {
            update(key, response.Value);
            return response.Value;
        }
        return null;
    }

    public T? Read<T>(string key) where T : IMessage<T>, new()
    {
        var s = Read(key);
        if (s != null)
        {
            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseJson(s);
        }
        return default(T);
    }

    public bool Remove(string key)
    {
        var response = Service.SendRequest(Path.SETTING_REMOVE_URL, new proto.RemoveSettingRequest
        {
            Module = Engine.Instance.Module,
            Key = key,
        });

        if (response != null && response.Code == 200)
        {
            remove(key);
            return true;
        }
        return false;
    }

    public void update(string key, string value)
    {
        lock (this)
        {
            data.Add(key, value);
        }
    }

    public void remove(string key)
    {
        lock (this)
        {
            data.Remove(key);
        }
    }

    public class UpdatedSignal : Signal.Handler<proto.SettingUpdatedSignal>
    {
        public override void Execute()
        {
            if (data.Module == Engine.Instance.Module)
            {
                Engine.Instance.Settings.update(data.Key, data.Value);
            }
        }
    }

    public class RemovedSignal : Signal.Handler<proto.SettingRemovedSignal>
    {
        public override void Execute()
        {
            if (data.Module == Engine.Instance.Module)
            {
                Engine.Instance.Settings.remove(data.Key);
            }
        }
    }
}