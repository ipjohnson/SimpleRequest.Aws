using DependencyModules.Runtime.Attributes;
using LambdaFunctionProject.Handlers;
using LambdaFunctionProject.Models;

namespace LambdaFunctionProject.Services;

public interface IFolderManager {
    Folder? GetFolder(string name);

    void AddNoteToFolder(AddNote.Request request);
}

[SingletonService]
public class FolderManager : IFolderManager {
    private readonly Dictionary<string, Folder> _folders = new ();
    
    public Folder? GetFolder(string name) {
        return _folders.GetValueOrDefault(name);
    }

    public void AddNoteToFolder(AddNote.Request request) {
        var folder = GetFolder(request.FolderName);

        if (folder == null) {
            throw new Exception("Folder not found: " + request.FolderName);
        }
        
        // in real implementation this would be saved to 
        // persistent storage
        _folders[request.FolderName] = folder with {
            Notes = folder.Notes.Concat([request.Note]).ToList()
        };
    }
}