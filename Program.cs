using TeleBot;
using Telegram.Bot;
using TeleBotConfiguration;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace TeleBotRun;

public static class Program
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;


    private static TelegramBotClient? Bot;

    public static async Task Main()
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        Bot = new TelegramBotClient(Configuration.BotToken);

        User me = await Bot.GetMeAsync();
        Console.Title = me.Username ?? "BOT";

        using var cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
        Bot.StartReceiving(BotHandlers.HandleUpdateAsync,
                           BotHandlers.HandleErrorAsync,
                           receiverOptions,
                           cts.Token);

        try
        {
            await Bot.SendTextMessageAsync(Configuration.AccesschatID, "🖥 PC is online!");
        }
        catch (Exception)
        {
            MessageBox.Show($"Invalid chat id : {Configuration.AccesschatID}");
        }

        Console.ReadLine();

        cts.Cancel();
    }
}