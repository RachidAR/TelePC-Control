using PC_Control;
using TeleBotConfiguration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace TeleBot;

public class BotHandlers
{
    public static bool isOnline = true;
    public static bool isLockPC = false;


    public static InlineKeyboardMarkup MainKeyboard = new(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("PC","PCControl"),
                });

    public static InlineKeyboardMarkup PCKeyboard = new(
               new[]
               {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("PowerControl","PowerControl"),
                        InlineKeyboardButton.WithCallbackData("TaskList", "TaskList"),
                        InlineKeyboardButton.WithCallbackData("ScreenShot", "ScreenShot"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("PC Info", "PCInfo"),
                        InlineKeyboardButton.WithCallbackData("Back", "BackToMain"),
                    },
               });

    public static InlineKeyboardMarkup PowerControls = new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Shutdown", "ShutdownPC"),
                        InlineKeyboardButton.WithCallbackData("Lock", "LockPC"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Reboot", "RebootPC"),
                        InlineKeyboardButton.WithCallbackData("Sleep", "SleepPC"),
                        InlineKeyboardButton.WithCallbackData("Back", "BackToControl"),
                    },
                });

    public static string mainText = $"🖥 Your PC is Online\n" + $"OS : {HardwareInfo.GetOSInformation()}\n" + $"Name : {HardwareInfo.GetComputerName()}";

    public static string minfoPC =
        $"⬇️ PC Configuration\n" +
        $"🖥 Name : {HardwareInfo.GetComputerName()}\n" +
        $"🖥 Cpu : {HardwareInfo.GetProcessorInformation()}\n" +
        $"💾 Ram : {HardwareInfo.GetPhysicalMemory()}\n";

    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!, cancellationToken),
            UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/mypc" => SendInlineKeyboard(botClient, message),
            _ => Usage(botClient, message)
        };
        Message sentMessage = await action;
        Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

        static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            await Task.Delay(5);

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: mainText,
                                                        replyMarkup: MainKeyboard);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
        {
            const string usage = $"Usage /mypc (working only Windows)";

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: usage,
                                                        replyMarkup: new ReplyKeyboardRemove());
        }
    }

    private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message.Chat.Id == Configuration.AccesschatID)
        {
            switch (callbackQuery.Data)
            {
                case "PCControl":
                    await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    minfoPC,
                    replyMarkup: PCKeyboard);
                    break;

                case "PowerControl":
                    await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    minfoPC,
                    replyMarkup: PowerControls);
                    break;

                //Power Control
                case "LockPC":
                    PC.Lock();
                    break;
                case "ShutdownPC":
                    PC.ShutDown();
                    break;
                case "RestartPC":
                    PC.Restart();
                    break;
                case "Sleep":
                    PC.SetSuspendState(false, true, true);
                    break;

                //Other info
                case "ScreenShot":
                    await botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto);
                    PC.Screenshot();
                    {
                        using FileStream fileStream = new(PC.filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var fileName = PC.filePath.Split(Path.DirectorySeparatorChar).Last();
                        await botClient.SendPhotoAsync(chatId: callbackQuery.Message.Chat.Id,
                                                              photo: new InputOnlineFile(fileStream, fileName),
                                                              caption: DateTime.Now.ToString("dd MMMM hh:mm:ss"));
                    }
                    break;

                case "PCInfo":
                    InlineKeyboardMarkup inlinkeyboard = new(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Back","BackToControl"),
                    });
                    await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    $"=============================\n" +
                    $"📌 Uptime : {HardwareInfo.GetSystemUpTimeInfo()}\n" +
                    $"📌 Active window : {HardwareInfo.GetActiveWindowTitle()}\n" +
                    $"=============================\n",
                    replyMarkup: inlinkeyboard);
                    break;

                //Back
                case "BackToMain":
                    await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    mainText,
                    replyMarkup: MainKeyboard);
                    break;
                case "BackToControl":
                    await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    minfoPC,
                    replyMarkup: PCKeyboard);
                    break;

                //Task list
                case "TaskList":
                    PC.ListProcesses();
                    {
                        using FileStream fileStream = new(PC.filePathListProc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        var fileName = PC.filePathListProc.Split(Path.DirectorySeparatorChar).Last();
                        await botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadDocument);
                        await botClient.SendDocumentAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            document: new InputOnlineFile(fileStream, fileName),
                            caption: "<b>Task List</b>",
                            parseMode: ParseMode.Html,
                            cancellationToken: cancellationToken);
                    }
                    break;
                default:
                    break;
            }
        }
        else await botClient.EditMessageTextAsync(
               callbackQuery.Message.Chat.Id,
               callbackQuery.Message.MessageId,
               $"Access is denied\n" +
               $"Chat ID: {callbackQuery.Message.Chat.Id}",
               replyMarkup: MainKeyboard);
    }

    private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
    {
        Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                )
            )
        };

        await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                               results: results,
                                               isPersonal: true,
                                               cacheTime: 0);
    }

    private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
    {
        Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        return Task.CompletedTask;
    }

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}