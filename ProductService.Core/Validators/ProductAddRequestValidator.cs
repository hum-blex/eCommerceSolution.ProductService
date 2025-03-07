using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;
public class ProductAddRequestValidator : AbstractValidator<ProductAddRequest>
{
    public ProductAddRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().WithMessage("Product Name can't be blank");
        RuleFor(x => x.Category).IsInEnum().WithMessage("Category isn't valid");
        RuleFor(x => x.UnitPrice).InclusiveBetween(0, double.MaxValue).WithMessage($"Unit Price should be between 0 to {double.MaxValue}");
        RuleFor(x => x.QuantityInStock).InclusiveBetween(0, int.MaxValue).WithMessage($"Quantity in Stock should be between 0 to {int.MaxValue}");
    }
}
