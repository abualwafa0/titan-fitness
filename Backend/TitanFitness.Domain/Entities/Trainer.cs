namespace TitanFitness.Domain.Entities;

public class Trainer
{
    public int TrainerId { get; private set; }

    public string TrainerNumber { get; private set; } = null!;

    public string TrainerName { get; private set; } = null!;

    public string? Specialty { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public int BranchId { get; private set; }

    public bool IsActive { get; private set; }

    private Trainer()
    {
    }

    public Trainer(
        string trainerNumber,
        string trainerName,
        string? specialty,
        string? email,
        string? phone,
        int branchId,
        bool isActive)
    {
        SetTrainerNumber(trainerNumber);
        SetTrainerName(trainerName);
        SetSpecialty(specialty);
        SetEmail(email);
        SetPhone(phone);
        SetBranch(branchId);

        IsActive = isActive;
    }

    public void Update(
        string trainerName,
        string? specialty,
        string? email,
        string? phone,
        int branchId,
        bool isActive)
    {
        SetTrainerName(trainerName);
        SetSpecialty(specialty);
        SetEmail(email);
        SetPhone(phone);
        SetBranch(branchId);

        IsActive = isActive;
    }

    private void SetTrainerNumber(string trainerNumber)
    {
        if (string.IsNullOrWhiteSpace(trainerNumber))
            throw new ArgumentException(
                "Trainer number is required.",
                nameof(trainerNumber));

        TrainerNumber = trainerNumber.Trim();
    }

    private void SetTrainerName(string trainerName)
    {
        if (string.IsNullOrWhiteSpace(trainerName))
            throw new ArgumentException(
                "Trainer name is required.",
                nameof(trainerName));

        trainerName = trainerName.Trim();

        if (trainerName.Length > 100)
            throw new ArgumentException(
                "Trainer name cannot exceed 100 characters.",
                nameof(trainerName));

        TrainerName = trainerName;
    }

    private void SetSpecialty(string? specialty)
    {
        Specialty = string.IsNullOrWhiteSpace(specialty)
            ? null
            : specialty.Trim();
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

    private void SetBranch(int branchId)
    {
        if (branchId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(branchId),
                "Branch ID must be greater than zero.");

        BranchId = branchId;
    }
}