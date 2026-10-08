namespace TitanFitness.Domain.Entities;

public class Member
{
    public int MemberId { get; private set; }

    public string MembershipNumber { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? Address { get; private set; }

    public DateOnly JoinedDate { get; private set; }

    public string? Photo { get; private set; }

    public int HomeBranchId { get; private set; }

    private Member()
    {
    }

    public Member(
        string membershipNumber,
        string fullName,
        string? email,
        string? phone,
        string? address,
        DateOnly joinedDate,
        string? photo,
        int homeBranchId)
    {
        SetMembershipNumber(membershipNumber);
        SetFullName(fullName);
        SetEmail(email);
        SetPhone(phone);
        SetAddress(address);

        JoinedDate = joinedDate;

        SetPhoto(photo);
        SetHomeBranch(homeBranchId);
    }

    public void UpdateProfile(
        string fullName,
        string? email,
        string? phone,
        string? address,
        DateOnly joinedDate,
        string? photo,
        int homeBranchId)
    {
        SetFullName(fullName);
        SetEmail(email);
        SetPhone(phone);
        SetAddress(address);

        JoinedDate = joinedDate;

        SetPhoto(photo);
        SetHomeBranch(homeBranchId);
    }

    private void SetMembershipNumber(string membershipNumber)
    {
        if (string.IsNullOrWhiteSpace(membershipNumber))
            throw new ArgumentException(
                "Membership number is required.",
                nameof(membershipNumber));

        membershipNumber = membershipNumber.Trim();

        if (membershipNumber.Length > 10)
            throw new ArgumentException(
                "Membership number cannot exceed 10 characters.",
                nameof(membershipNumber));

        MembershipNumber = membershipNumber;
    }

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "Full name is required.",
                nameof(fullName));

        fullName = fullName.Trim();

        if (fullName.Length > 100)
            throw new ArgumentException(
                "Full name cannot exceed 100 characters.",
                nameof(fullName));

        FullName = fullName;
    }

    private void SetEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            Email = null;
            return;
        }

        email = email.Trim();

        if (email.Length > 100)
            throw new ArgumentException(
                "Email cannot exceed 100 characters.",
                nameof(email));

        Email = email;
    }

    private void SetPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            Phone = null;
            return;
        }

        phone = phone.Trim();

        if (phone.Length > 20)
            throw new ArgumentException(
                "Phone cannot exceed 20 characters.",
                nameof(phone));

        Phone = phone;
    }

    private void SetAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            Address = null;
            return;
        }

        address = address.Trim();

        if (address.Length > 200)
            throw new ArgumentException(
                "Address cannot exceed 200 characters.",
                nameof(address));

        Address = address;
    }

    private void SetPhoto(string? photo)
    {
        Photo = string.IsNullOrWhiteSpace(photo)
            ? null
            : photo.Trim();
    }

    private void SetHomeBranch(int homeBranchId)
    {
        if (homeBranchId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(homeBranchId),
                "Home branch ID must be greater than zero.");

        HomeBranchId = homeBranchId;
    }
}