namespace Pharmacy.DTO;

public class IdTenDTO
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;

    public override string ToString() => Ten;
}
