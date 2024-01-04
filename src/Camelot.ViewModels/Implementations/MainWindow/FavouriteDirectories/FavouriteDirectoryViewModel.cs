using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using Camelot.Extensions;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;
using Camelot.ViewModels.Interfaces.MainWindow.Directories;
using Camelot.ViewModels.Services.Interfaces;
using ReactiveUI;

namespace Camelot.ViewModels.Implementations.MainWindow.FavouriteDirectories;

public class FavouriteDirectoryViewModel : ViewModelBase, IFavouriteDirectoryViewModel
{
    private readonly IFilesOperationsMediator _filesOperationsMediator;
    private readonly IFavouriteDirectoriesService _favouriteDirectoriesService;

    public string FullPath { get; }
    private string _directoryDisplayName;
    public string DirectoryDisplayName
    {
        get => _directoryDisplayName;
        private set
        {
            if (_directoryDisplayName == value)
                return;

            _directoryDisplayName = value;

            // X7X7X7
            // WIP... not working BUGBUG
            // ASK IGOR ???
            // next, try to tell view to refresh.  not working
            // so index will be update if a favoirite is rmeoved not fro end
            this.RaiseAndSetIfChanged(ref _directoryDisplayName, value);
        }
    }

    private int GetIndexInFavourites()
    {
        var favorites = _favouriteDirectoriesService.FavouriteDirectories.ToList();
        for (int i = 0; i < favorites.Count; i++)
        {
            if (favorites[i] == FullPath)
            {
                return i;
            }
        }
        return -1;
    }
    public string DirectoryName { get; }

    public event EventHandler<FavouriteDirectoryMoveRequestedEventArgs> MoveRequested;

    public ICommand OpenCommand { get; }

    public ICommand RemoveCommand { get; }

    public ICommand RequestMoveCommand { get; }

    public FavouriteDirectoryViewModel(
        IFilesOperationsMediator filesOperationsMediator,
        IFavouriteDirectoriesService favouriteDirectoriesService,
        DirectoryModel directoryModel)
    {
        _filesOperationsMediator = filesOperationsMediator;
        _favouriteDirectoriesService = favouriteDirectoriesService;
        // why next was commented out in branch of wip1 ?
        //X7X7X7
        _favouriteDirectoriesService.DirectoryAdded += (s, e) => 
        {
            // adding call in UIThred didnt help
            Dispatcher.UIThread.Post(() => UpdateDisplayName(),
                                        DispatcherPriority.MaxValue);
            //UpdateDisplayName(); 
        };
        _favouriteDirectoriesService.DirectoryRemoved += (s, e) => 
        {
            // adding call in UIThred didnt help
            Dispatcher.UIThread.Post(() => UpdateDisplayName(),
                                        DispatcherPriority.MaxValue);
        };
        FullPath = directoryModel.FullPath;
        DirectoryName = directoryModel.Name;
        UpdateDisplayName();

        OpenCommand = ReactiveCommand.Create(Open);
        RemoveCommand = ReactiveCommand.Create(Remove);
        RequestMoveCommand = ReactiveCommand.Create<IFavouriteDirectoryViewModel>(RequestMoveTo);
    }

    private void UpdateDisplayName()
    {
        // X7X7X7
        // TODO - get from settings of "ui"
        bool _UiSettings_ShowNumbersForFavourites = true;
        if (_UiSettings_ShowNumbersForFavourites)
        {
            var index = GetIndexInFavourites();
            if (index == -1)
                return; // not found, this is the one that was removed.
            DirectoryDisplayName = $"{index + 1}. {DirectoryName}";
        }
        else
        {
            DirectoryDisplayName = DirectoryName;
        }
    }

    private void Open() =>
        _filesOperationsMediator.ActiveFilesPanelViewModel.CurrentDirectory = FullPath;

    private void Remove() => _favouriteDirectoriesService.RemoveDirectory(FullPath);

    private void RequestMoveTo(IFavouriteDirectoryViewModel target) =>
        MoveRequested.Raise(this, new FavouriteDirectoryMoveRequestedEventArgs(target));
}