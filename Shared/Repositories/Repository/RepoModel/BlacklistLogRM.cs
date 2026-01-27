  public class BlacklistLogRM
  {
      public Guid Id { get; set; }
      public string? Type { get; set; } // "Member" / "Visitor"
      public string? Name { get; set; }
      public string? CardNumber { get; set; }
      public string? PersonId { get; set; }   // Bisa null untuk visitor
  }
