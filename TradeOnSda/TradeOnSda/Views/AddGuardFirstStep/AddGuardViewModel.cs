using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using SteamAuthentication.Exceptions;
using SteamAuthentication.GuardLinking;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;
using SteamKit2.Authentication;
using TradeOnSda.Data;
using TradeOnSda.ViewModels;
using TradeOnSda.Windows.ConfirmEmail;
using TradeOnSda.Windows.GuardAdded;
using TradeOnSda.Windows.NotificationMessage;

namespace TradeOnSda.Views.AddGuardFirstStep;

public class AddGuardViewModel : ViewModelBase
{
    #region FirstStep

    private string _proxyString;
    private string _login;
    private string _password;
    private bool _isEnabledLoginButton;
    private string _askStepAnswer;
    private string _askStepTitle;

    public bool IsFirstStep
    {
        get => _isFirstStep;
        set => RaiseAndSetIfPropertyChanged(ref _isFirstStep, value);
    }

    public string ProxyString
    {
        get => _proxyString;
        set => RaiseAndSetIfPropertyChanged(ref _proxyString, value);
    }

    public string Login
    {
        get => _login;
        set => RaiseAndSetIfPropertyChanged(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => RaiseAndSetIfPropertyChanged(ref _password, value);
    }

    public ICommand TryLoginCommand { get; }

    public bool IsEnabledLoginButton
    {
        get => _isEnabledLoginButton;
        set => RaiseAndSetIfPropertyChanged(ref _isEnabledLoginButton, value);
    }

    private IWebProxy? _proxy;

    #endregion

    #region AskStep

    public string? TextBoxWatermark
    {
        get => _textBoxWatermark;
        set => RaiseAndSetIfPropertyChanged(ref _textBoxWatermark, value);
    }

    public bool IsAskStep
    {
        get => _isAskStep;
        set => RaiseAndSetIfPropertyChanged(ref _isAskStep, value);
    }

    public string AskStepTitle
    {
        get => _askStepTitle;
        set => RaiseAndSetIfPropertyChanged(ref _askStepTitle, value);
    }

    public string AskStepAnswer
    {
        get => _askStepAnswer;
        set => RaiseAndSetIfPropertyChanged(ref _askStepAnswer, value);
    }

    private TaskCompletionSource<string>? _askStepTaskCompletionSource;
    private bool _isFirstStep;
    private bool _isAskStep;
    private bool _isAddGuardStep;

    public ICommand AskStepCommand { get; }

    #endregion

    #region AddGuardStep

    public string SteamIdString
    {
        get => _steamIdString;
        set => RaiseAndSetIfPropertyChanged(ref _steamIdString, value);
    }

    public string AccountName
    {
        get => _accountName;
        set => RaiseAndSetIfPropertyChanged(ref _accountName, value);
    }

    public bool IsAddGuardStep
    {
        get => _isAddGuardStep;
        set => RaiseAndSetIfPropertyChanged(ref _isAddGuardStep, value);
    }

    public bool IsSecondTry
    {
        get => _isSecondTry;
        set => RaiseAndSetIfPropertyChanged(ref _isSecondTry, value);
    }

    public string LastPhoneNumber
    {
        get => _lastPhoneNumber;
        set => RaiseAndSetIfPropertyChanged(ref _lastPhoneNumber, value);
    }

    public ICommand AddGuardCommand { get; }

    private AuthPollResult? _pollResult;
    private ulong? _steamId;
    private GuardLinker? _guardLinker;
    private string _steamIdString;
    private string _accountName;
    private string _smsCode;

    #endregion

    #region FinalizeStep

    public bool IsFinalizeStep
    {
        get => _isFinalizeStep;
        set => RaiseAndSetIfPropertyChanged(ref _isFinalizeStep, value);
    }

    public string SmsCode
    {
        get => _smsCode;
        set => RaiseAndSetIfPropertyChanged(ref _smsCode, value);
    }

    public ICommand FinalizeCommand { get; }

    private SteamMaFile? _maFile;
    private bool _isFinalizeStep;
    private string? _textBoxWatermark;
    private bool _isSecondTry;
    private string _lastPhoneNumber;

    #endregion

    public AddGuardViewModel(SdaManager sdaManager, Window ownerWindow)
    {
        _proxyString = "";
        _steamIdString = "";
        _accountName = "";
        _login = "";
        _password = "";
        _askStepAnswer = "";
        _askStepTitle = "";
        _isEnabledLoginButton = true;
        _isFirstStep = true;
        _smsCode = "";
        _lastPhoneNumber = "";

        TryLoginCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IWebProxy? proxy;

            try
            {
                proxy = ProxyLogic.ParseWebProxy(ProxyString);
            }
            catch (Exception e)
            {
                await NotificationsMessageWindow.ShowWindow(e.Message, ownerWindow);
                return;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                await NotificationsMessageWindow.ShowWindow("Login is empty", ownerWindow);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                await NotificationsMessageWindow.ShowWindow("Password is empty", ownerWindow);
                return;
            }

            _proxy = proxy;

            _guardLinker = new GuardLinker(new UIPhoneProvider(this), proxy, new SimpleSteamTime(), Login, Password,
                new UIAuthenticator(this));

            IsEnabledLoginButton = false;

            var _ = Task.Run(async () =>
            {
                try
                {
                    var linkingResult = await _guardLinker.StartLinkingGuardAsync();

                    AccountName = linkingResult.pollResponse.AccountName;
                    SteamIdString = linkingResult.steamId.ToString();

                    _pollResult = linkingResult.pollResponse;
                    _steamId = linkingResult.steamId;

                    IsAskStep = false;
                    IsFirstStep = false;
                    IsAddGuardStep = true;
                }
                catch (NotSupportedException)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await NotificationsMessageWindow.ShowWindow($"This steam account is not supported",
                            ownerWindow);
                    });
                }
                catch (UserCancelException)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await NotificationsMessageWindow.ShowWindow($"Error while linking guard. Message: {e.Message}",
                            ownerWindow);
                    });
                }
                finally
                {
                    IsEnabledLoginButton = true;
                }
            });
        });

        AskStepCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (string.IsNullOrWhiteSpace(AskStepAnswer))
            {
                await NotificationsMessageWindow.ShowWindow("Code is empty",
                    ownerWindow);
                return;
            }

            IsAskStep = false;

            _askStepTaskCompletionSource?.SetResult(AskStepAnswer);
        });

        AddGuardCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var maFile = await _guardLinker!.SendAddGuardRequestAsync(_steamId!.Value, _pollResult!);

                _maFile = maFile;

                IsFinalizeStep = true;
                IsAskStep = false;
                IsFirstStep = false;
                IsAddGuardStep = false;
            }
            catch (UserCancelException)
            {
            }
            catch (PhoneNumberException ex)
            {
                if (ex.Email != null)
                {
                    await ConfirmEmailWindow.ShowWindow(ex.Email, ownerWindow);

                    try
                    {
                        await _guardLinker!.ConfirmPhoneNumberAsync(_pollResult!.AccessToken);
                        
                        IsSecondTry = true;
                    }
                    catch (RequestException e)
                    {
                        await NotificationsMessageWindow.ShowWindow(
                            $"Error while confirm phone number, code: {e.HttpStatusCode}, message: {e.Content}",
                            ownerWindow);
                        return;
                    }
                    catch (Exception e)
                    {
                        await NotificationsMessageWindow.ShowWindow(
                            $"Error while confirm phone number, message: {e.Message}", ownerWindow);
                        return;
                    }
                    
                    return;
                }

                IsSecondTry = true;
            }
            catch (RequestException e)
            {
                await NotificationsMessageWindow.ShowWindow(
                    $"Error while adding guard, code: {e.HttpStatusCode}, message: {e.Content}",
                    ownerWindow);
            }
            catch (Exception e)
            {
                await NotificationsMessageWindow.ShowWindow(
                    $"Error while adding guard, message: {e.Message}", ownerWindow);
            }
        });

        FinalizeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (string.IsNullOrWhiteSpace(SmsCode))
            {
                await NotificationsMessageWindow.ShowWindow(
                    $"Please, enter the SMS code",
                    ownerWindow);
                return;
            }

            try
            {
                await _guardLinker!.FinalizeAddGuardAsync(SmsCode, _maFile!, _pollResult!);
            }
            catch (RequestException e)
            {
                await NotificationsMessageWindow.ShowWindow(
                    $"Error while finializing guard, message: {e.Content}",
                    ownerWindow);
            }
            catch (Exception e)
            {
                await NotificationsMessageWindow.ShowWindow(
                    $"Error while finializing guard, message: {e.Message}", ownerWindow);
            }

            var steamGuardAccount = new SteamGuardAccount(_maFile!, new SteamRestClient(_proxy), new SimpleSteamTime(),
                NullLogger<SteamGuardAccount>.Instance);

            await steamGuardAccount.TryLoginAgainAsync(_login, _password);

            var credentials = new MaFileCredentials(_proxy, _proxyString, _password);

            await GuardAddedWindow.ShowWindow(steamGuardAccount, credentials, ownerWindow);
            
            await sdaManager.AddAccountAsync(steamGuardAccount, credentials,
                new SdaSettings(sdaManager.GlobalSettings.DefaultEnabledAutoConfirm, TimeSpan.FromMinutes(60)));
            
            ownerWindow.Close();
        });
    }

    public AddGuardViewModel()
    {
        _proxyString = "";
        _login = "";
        _password = "";
        _askStepAnswer = "";
        _askStepTitle = "";
        _isEnabledLoginButton = true;
        TryLoginCommand = null!;
        AskStepCommand = null!;
        AddGuardCommand = null!;
        _steamIdString = null!;
        _accountName = null!;
        _smsCode = null!;
        _lastPhoneNumber = null!;
        FinalizeCommand = null!;
    }

    public void WindowClose() => _askStepTaskCompletionSource?.TrySetException(new UserCancelException());

    public Task<string> AskUserAsync(string title, string? watermark = null)
    {
        AskStepTitle = title;
        TextBoxWatermark = watermark;
        AskStepAnswer = "";
        IsAskStep = true;

        _askStepTaskCompletionSource = new TaskCompletionSource<string>();

        return _askStepTaskCompletionSource.Task;
    }
}