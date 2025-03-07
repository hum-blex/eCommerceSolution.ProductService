using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using FluentValidation;
using FluentValidation.Results;

namespace ProductService.API.APIEndpoints;

public static class ProductAPIEndpoints
{
    public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/products
        app.MapGet("/api/products", async (IProductService productService) =>
        {
            List<ProductResponse?> products = await productService.GetProducts();
            return Results.Ok(products);
        });

        // GET /api/products/search/product-id/{ProductID}
        app.MapGet("/api/products/search/product-id/{ProductID:guid}",
            async (IProductService productService, Guid ProductID) =>
        {
            ProductResponse? product = await productService.GetProductByCondition(temp => temp.ProductID == ProductID);
            return Results.Ok(product);
        });

        // GET /api/products/search/{SearchString}
        app.MapGet("/api/products/search/{SearchString}",
            async (IProductService productService, string SearchString) =>
        {
            List<ProductResponse?> productsByProductsName = await productService.GetProductsByCondition(temp => temp.ProductName != null &&
                                    temp.ProductName.Contains(SearchString, StringComparison.OrdinalIgnoreCase));

            List<ProductResponse?> productsByCategory = await productService.GetProductsByCondition(temp => temp.Category != null &&
                                    temp.Category.Contains(SearchString, StringComparison.OrdinalIgnoreCase));

            var products = productsByProductsName.Union(productsByCategory);

            return Results.Ok(products);

        });

        // POST /api/products
        app.MapPost("/api/products", async (IProductService productService,
                                            IValidator<ProductAddRequest> productAddRequestValidator,
                                            ProductAddRequest productAddRequest) =>
        {
            ValidationResult validationResult = await productAddRequestValidator.ValidateAsync(productAddRequest);
            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors =
                    validationResult.Errors
                    .GroupBy(temp => temp.PropertyName)
                    .ToDictionary(grp => grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());

                return Results.ValidationProblem(errors);
            }

            ProductResponse? addedProduct = await productService.AddProduct(productAddRequest);
            if (addedProduct != null)
                return Results.Created($"/api/products/search/product-id/{addedProduct.ProductID}", addedProduct);

            return Results.Problem("Error in adding product");
        });

        // PUT /api/products
        app.MapPut("/api/products", async (IProductService productService,
                                           IValidator<ProductUpdateRequest> productUpdateRequestValidator,
                                           ProductUpdateRequest productUpdateRequest) =>
        {
            ValidationResult validationResult = await productUpdateRequestValidator.ValidateAsync(productUpdateRequest);
            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors =
                    validationResult.Errors
                    .GroupBy(temp => temp.PropertyName)
                    .ToDictionary(grp => grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());

                return Results.ValidationProblem(errors);
            }

            ProductResponse? updatedProduct = await productService.UpdateProduct(productUpdateRequest);
            if (updatedProduct != null)
                return Results.Ok(updatedProduct);

            return Results.Problem("Error in updating product");
        });

        // DELETE /api/products/{productID}
        app.MapDelete("/api/products/{ProductID:guid}", async (IProductService productService, Guid ProductID) =>
        {
            bool isDeleted = await productService.DeleteProduct(ProductID);
            if (isDeleted)
                return Results.Ok(true);

            return Results.Problem("Error in deleting product");
        });

        return app;
    }
}
