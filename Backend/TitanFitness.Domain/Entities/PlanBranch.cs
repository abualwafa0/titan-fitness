namespace TitanFitness.Domain.Entities;

public class PlanBranch
{
    public int PlanId { get; private set; }

    public int BranchId { get; private set; }

    private PlanBranch()
    {
    }

    internal PlanBranch(int branchId)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(branchId),
                "Branch id must be greater than zero.");
        }

        BranchId = branchId;
    }
}