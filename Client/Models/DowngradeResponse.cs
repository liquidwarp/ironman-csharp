namespace IronManClient.Models;

public class DowngradeResponse(ProfileType profileType, long downgradeLastOfferedAt) {
    public ProfileType ProfileType { get; } = profileType;
    public long DowngradeLastOfferedAt { get; } = downgradeLastOfferedAt;
}
