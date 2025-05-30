using SimpleRequest.Models.Attributes;
using WebLambdaProject.Models;

namespace WebLambdaProject.Handler;

public class NotesHandler {
    [Get("/notes")]
    public async Task<List<NotesModel>> GetAll() {
        return new List<NotesModel> {
            new NotesModel(1, "Hello World!"),
        };
    }
    
    [Get("/notes/{id}")]
    public async Task<NotesModel> GetNote(int id) {
        return new NotesModel(id, "Hello World");
    }
}