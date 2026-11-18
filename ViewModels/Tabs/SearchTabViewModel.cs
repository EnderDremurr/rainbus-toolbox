using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RainbusToolbox.ViewModels.Tabs;

/*

basically what i need:

- field to enter what to find at the top + toggle button to enable regex mode

- search through all the files in /localize/
the porblem with search is that my files are serialized, so first i won't be able to process new filetypes the game adds without updating my model list
second it would search only file's data instead of whole json like in vscode

good things is that i can easily make "jump to" button to open needed file at needed place right away
i also can ignore ids, but for this it's probably better to add a toggle if you'd need to find something by id

next what, next i need to see how to make this not lag during the search itself, + properly fill the view so it won't lag too

i think i'd make search execute on focus loss, like i did with navigation manual input
realtime search like in vscode i feel will bee too sluggish due to serializations


plans for future:
- maybe make a replace field like in vscode (with force ignore for id fields)
- if i'd make a replace field, i can add a "create quick regex" button that'd copy current replacement into regex list


TODO: also overview icon fits search more, move it and make a new icon for the overview!!!
*/
public partial class SearchTabViewModel : ObservableObject, IMainWindowTab
{
    [ObservableProperty]
    private string _replaceToText;

    [ObservableProperty]
    private string _searchStatus = "No searches yet...";

    [ObservableProperty]
    private string _textToSearch;

    public Task OnTabOpened()
    {
        // something something, i guess add an automatic search refresh here + discord rpc as usual
        return Task.CompletedTask;
    }


    [RelayCommand]
    public void ProcessSearch()
    {
    }

    public void CleanPreviousResults()
    {
    }
}