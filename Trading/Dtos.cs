using System.ComponentModel.DataAnnotations;

namespace Trading
{
    public record SubmitPurchaseDto(
        [Required] Guid? ItemId, 
        [Range(1, 100)] int Quantity
    );
}
