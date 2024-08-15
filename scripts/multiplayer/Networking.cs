using Godot;

public static class Networking
{
    public static string DefaultIP = "127.0.0.1";
    public static int DefaultPort = 44220;

    public static string ValidateIP(string ip)
	{
		if (ip != "")
        {
            return ip;
        }

        return DefaultIP;
	}

	public static int ValidatePort(string port)
	{
		try
		{
			if (port != "")
			{
				return port.ToInt();
			}
		}
		catch
		{
			ToastNotification.Notify($"Could not set port, defaulting to {DefaultPort}", 2);
		}
		
		return DefaultPort;
	}
}