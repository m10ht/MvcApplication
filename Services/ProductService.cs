using App.Models;

namespace App.Services
{
    public class ProductService : List<ProductModel> {
        public ProductService() {
            this.AddRange(new ProductModel[] {
                new ProductModel() {Id = 1, Name = "IPhoneX", Price = 1000},
                new ProductModel() {Id = 1, Name = "SamsungGalaxy", Price = 9000},
                new ProductModel() {Id = 1, Name = "NokiaLumia", Price = 7000},
                new ProductModel() {Id = 1, Name = "GooglePixel", Price = 8000},
            });
        }
    }
}
