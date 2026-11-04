namespace KeylessCosmos.Api.Todos;

public sealed record Todo(string Id, string Title, bool IsComplete, DateTimeOffset CreatedAt);

public sealed record TodoInput(string Title, bool IsComplete);
