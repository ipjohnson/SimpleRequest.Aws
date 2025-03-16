using LambdaFunctionProject.Models;
using LambdaFunctionProject.Services;
using SimpleRequest.Functions.Runtime.Attributes;

namespace LambdaFunctionProject.Handlers;

public class GetNote(IFolderManager folderManager) {
    public record Request(string FolderName, string NoteId);
    
    public record Response(Note? Note);

    [Function(Name = "get-note")]
    public Response Invoke(Request request) {
        var folder = folderManager.GetFolder(request.FolderName);

        if (folder == null) {
            return new Response(null);
        }
        
        return new Response(
            folder.Notes.FirstOrDefault(n => n.NoteId == request.NoteId));
    }
}