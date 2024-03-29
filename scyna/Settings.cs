namespace scyna;
using Google.Protobuf;

public class Settings
{
    readonly Dictionary<string, string> data = new();

    public bool Write(string key, string value)
    {
        var response = Request.Send(Path.SETTING_WRITE_URL, new proto.WriteSettingRequest
        {
            Module = Engine.Module,
            Key = key,
            Value = value
        }, null);

        if (response != null && response.Code == 200)
        {
            Update(key, value);
            return true;
        }
        return false;
    }
    public bool Write<T>(string key, T value) where T : IMessage<T>, new()
    {
        var s = JsonFormatter.Default.Format(value);
        return Write(key, s);
    }
    public string? Read(string key)
    {
        /*from cache*/
        lock (this)
        {
            try
            {
                var ret = data[key];
                if (ret != null) return ret;
            }
            catch (Exception)
            {
            }
        }

        var response = Request.Send<proto.ReadSettingResponse>(Path.SETTING_READ_URL, new proto.ReadSettingRequest
        {
            Module = Engine.Module,
            Key = key
        });

        if (response != null)
        {
            Update(key, response.Value);
            return response.Value;
        }
        return null;
    }

    public T? Read<T>(string key) where T : IMessage<T>, new()
    {
        var s = Read(key);
        if (s != null)
        {
            MessageParser<T> parser = new(() => new T());
            return parser.ParseJson(s);
        }
        return default;
    }

    public bool Remove(string key)
    {
        var response = Request.Send(Path.SETTING_REMOVE_URL, new proto.RemoveSettingRequest
        {
            Module = Engine.Module,
            Key = key,
        }, null);

        if (response != null && response.Code == 200)
        {
            remove(key);
            return true;
        }
        return false;
    }

    private void Update(string key, string value)
    {
        lock (this)
        {
            data.Add(key, value);
        }
    }

    private void remove(string key)
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
            if (data != null && data.Module == Engine.Module)
            {
                Engine.Settings.Update(data.Key, data.Value);
            }
        }
    }

    public class RemovedSignal : Signal.Handler<proto.SettingRemovedSignal>
    {
        public override void Execute()
        {
            if (data != null && data.Module == Engine.Module)
            {
                Engine.Settings.remove(data.Key);
            }
        }
    }
}