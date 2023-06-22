using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Binding;
using TradeOnSda.Data;
using TradeOnSda.ViewModels;
using TradeOnSda.Views.Account;

namespace TradeOnSda.Views.AccountList;

public class AccountListViewModel : ViewModelBase
{
    public SdaManager SdaManager { get; }

    private readonly ReadOnlyObservableCollection<AccountViewModel> _visibleAccounts;
    private AccountViewModel? _selectedAccountViewModel;
    private string _searchText = null!;
    private ReadOnlyObservableCollection<AccountViewModel> _allAccounts;

    public ReadOnlyObservableCollection<AccountViewModel> VisibleAccounts => _visibleAccounts;

    public ReadOnlyObservableCollection<AccountViewModel> AllAccounts => _allAccounts;

    public string SearchText
    {
        get => _searchText;
        set => RaiseAndSetIfPropertyChanged(ref _searchText, value);
    }

    public AccountViewModel? SelectedAccountViewModel
    {
        get => _selectedAccountViewModel;
        set => RaiseAndSetIfPropertyChanged(ref _selectedAccountViewModel, value);
    }

    public AccountListViewModel(SdaManager sdaManager, Window ownerWindow)
    {
        SdaManager = sdaManager;
        SearchText = "";

        SdaManager.ItemsConnection
            .Select(t => new AccountViewModel(t, SdaManager, ownerWindow))
            .Bind(out _allAccounts)
            .Subscribe();

        AllAccounts
            .ToObservableChangeSet()
            .FilterOnObservable(t =>
                t.WhenPropertyChanged(vm => vm.IsVisible).Select(propertyValue => propertyValue.Value))
            .Bind(out _visibleAccounts)
            .Subscribe();
        
        this.WhenPropertyChanged(t => t.SearchText)
            .Subscribe(valueWrapper =>
            {
                var searchText = valueWrapper.Value;
                
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    foreach (var account in AllAccounts) 
                        account.IsVisible = true;
                    return;
                }

                foreach (var account in AllAccounts)
                    account.IsVisible = account.AccountName.ToLower().Contains(searchText!.ToLower());
            });
    }

    public AccountListViewModel()
    {
        SdaManager = new SdaManager();
        SearchText = "";

        _visibleAccounts = null!;
    }
}