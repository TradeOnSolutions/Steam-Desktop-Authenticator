using TradeOnSda.ViewModels;
using TradeOnSda.Views.ImportAccounts;

namespace TradeOnSda.Windows.ImportAccounts;

public class ImportAccountsWindowViewModel : ViewModelBase
{
    public ImportAccountsViewModel ImportAccountsViewModel { get; }

    public ImportAccountsWindowViewModel(ImportAccountsViewModel importAccountsViewModel)
    {
        ImportAccountsViewModel = importAccountsViewModel;
    }

    public ImportAccountsWindowViewModel() => ImportAccountsViewModel = new ImportAccountsViewModel();
}