namespace Araci.Applications.Abstractions
{
    public interface IProjectFileDialogService
    {
        string? ShowSaveDialog();

        string? ShowOpenDialog();
    }
}
