namespace LambdaFunctionProject.Models;

public record OperationResult(bool Success, string Message = "");

public record Note(string NoteId, string Content);

public record Folder(string FolderId, string Name, IReadOnlyList<Note> Notes);