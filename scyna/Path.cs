namespace scyna;

class Path
{
    public const string GEN_GET_ID_URL = "/scyna2/generator/id";
    public const string GEN_GET_SN_URL = "/scyna2/generator/sn";
    public const string SESSION_CREATE_URL = "/scyna2/session/create";
    public const string SESSION_UPDATE_CHANNEL = "scyna2.session.update";
    public const string SESSION_END_CHANNEL = "scyna2.session.end";
    public const string LOG_CREATED_CHANNEL = "scyna2.log";
    public const string SETTING_WRITE_URL = "/scyna2/setting/write";
    public const string SETTING_READ_URL = "/scyna2/setting/read";
    public const string SETTING_REMOVE_URL = "/scyna2/setting/remove";
    public const string SETTING_UPDATE_CHANNEL = "scyna2.setting.updated.";
    public const string SETTING_REMOVE_CHANNEL = "scyna2.setting.removed.";
    public const string CALL_WRITE_CHANNEL = "scyna2.call.write";
    public const string APP_UPDATE_CHANNEL = "scyna2.application.updated";
    public const string CLIENT_UPDATE_CHANNEL = "scyna2.client.updated";
    public const string AUTH_CREATE_URL = "/scyna2/auth/create";
    public const string AUTH_GET_URL = "/scyna2/auth/get";
    public const string AUTH_LOGOUT_URL = "/scyna2/auth/logout";
    public const string TRACE_CREATED_CHANNEL = "scyna2.trace";
    public const string START_TASK_URL = "/scyna2/task/start";
    public const string STOP_TASK_URL = "/scyna2/task/stop";
    public const string ENDPOINT_DONE_CHANNEL = "scyna2.tag.endpoint";
}
