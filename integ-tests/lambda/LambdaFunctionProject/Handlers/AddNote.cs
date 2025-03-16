using LambdaFunctionProject.Models;
using LambdaFunctionProject.Services;
using SimpleRequest.Functions.Runtime.Attributes;

namespace LambdaFunctionProject.Handlers;


public class AddNote(IFolderManager folderManager) {
    public record Request(Note Note, string FolderName);
    
    [Function(Name = "add-note")]
    public OperationResult Invoke(Request request) {
        var folder = folderManager.GetFolder(request.FolderName);

        if (folder == null) {
            return new OperationResult(false, "Folder not found");
        }

        folderManager.AddNoteToFolder(request);
        
        return new OperationResult(true);
    }
}