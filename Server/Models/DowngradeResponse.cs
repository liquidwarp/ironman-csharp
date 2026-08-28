namespace IronManServer.Models;

using Enums;

public class DowngradeResponse
{
    public ProfileType ProfileType { get; set; }
    public long DowngradeLastOfferedAt { get; set; }
}
